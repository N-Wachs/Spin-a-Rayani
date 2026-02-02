/**
 * Spin a Rayan - Web Version
 * UI Components and Rendering
 */

// ========================================
// UI Manager
// ========================================
class UIManager {
    constructor(gameManager) {
        this.game = gameManager;
        this.currentModal = null;
        this.autoRollInterval = null;
        this.rollCooldown = false;
        
        this.initializeEventListeners();
    }

    initializeEventListeners() {
        // Roll button
        document.getElementById('rollBtn').addEventListener('click', () => this.handleRoll());
        
        // Auto Roll button
        document.getElementById('autoRollBtn').addEventListener('click', () => this.handleAutoRoll());
        
        // Auto Equip button
        document.getElementById('autoEquipBtn').addEventListener('click', () => this.handleAutoEquip());
        
        // View Inventory button
        document.getElementById('viewInventoryBtn').addEventListener('click', () => this.openModal('inventory'));
        
        // Merge All button
        document.getElementById('mergeAllBtn').addEventListener('click', () => this.handleMergeAll());
        
        // Dice selector
        document.getElementById('diceSelect').addEventListener('change', (e) => {
            this.game.selectDice(parseInt(e.target.value));
            this.updateDiceSelector();
        });
        
        // Navigation buttons
        document.querySelectorAll('.nav-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const panel = e.currentTarget.dataset.panel;
                this.openModal(panel);
            });
        });
        
        // Modal close buttons
        document.querySelectorAll('.modal-close').forEach(btn => {
            btn.addEventListener('click', () => this.closeModal());
        });
        
        // Modal overlay click to close
        document.getElementById('modalOverlay').addEventListener('click', (e) => {
            if (e.target === e.currentTarget) {
                this.closeModal();
            }
        });
        
        // Rebirth button
        document.getElementById('rebirthBtn').addEventListener('click', () => this.handleRebirth());
        
        // Upgrade tabs
        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const tab = e.target.dataset.tab;
                this.switchUpgradeTab(tab);
            });
        });
        
        // Inventory search
        document.getElementById('inventorySearch').addEventListener('input', () => this.renderFullInventory());
        document.getElementById('inventorySort').addEventListener('change', () => this.renderFullInventory());
        
        // Admin mode: Force event with 'E' key
        document.addEventListener('keydown', (e) => {
            // Force event with 'E' key in admin mode
            if (e.key === 'e' && this.game.adminMode) {
                this.game.forceEvent();
                this.showToast('Event erzwungen!', 'info');
            }
        });
    }

    // ========================================
    // Roll Handling
    // ========================================
    handleRoll() {
        if (this.rollCooldown) return;
        
        const rayan = this.game.roll();
        this.showRollResult(rayan);
        
        // Apply cooldown
        this.rollCooldown = true;
        const rollBtn = document.getElementById('rollBtn');
        rollBtn.disabled = true;
        
        const cooldownTime = this.game.getEffectiveRollCooldown() * 1000;
        setTimeout(() => {
            this.rollCooldown = false;
            rollBtn.disabled = false;
        }, cooldownTime);
        
        this.updateAll();
    }

    showRollResult(rayan) {
        const container = document.getElementById('rollResult');
        const rarityClass = getRarityClass(rayan.rarity);
        
        container.innerHTML = `
            <div class="roll-rayan-name ${rarityClass}">${rayan.fullName}</div>
            <div class="roll-rayan-value">💰 ${formatBigInt(rayan.totalValue)}/s</div>
            <div class="roll-rayan-rarity">${formatRarity(rayan.rarity)} • ${getRarityName(rayan.rarity)}</div>
        `;
        
        container.classList.add('highlight');
        setTimeout(() => container.classList.remove('highlight'), 500);
    }

    handleAutoRoll() {
        if (!this.game.stats.autoRollUnlocked) return;
        
        this.game.toggleAutoRoll();
        
        if (this.game.stats.autoRollActive) {
            this.startAutoRoll();
        } else {
            this.stopAutoRoll();
        }
        
        this.updateAutoRollButton();
    }

    startAutoRoll() {
        if (this.autoRollInterval) return;
        
        const rollWithCooldown = () => {
            if (!this.game.stats.autoRollActive) return;
            
            if (!this.rollCooldown) {
                this.handleRoll();
            }
        };
        
        this.autoRollInterval = setInterval(rollWithCooldown, 100);
    }

    stopAutoRoll() {
        if (this.autoRollInterval) {
            clearInterval(this.autoRollInterval);
            this.autoRollInterval = null;
        }
    }

    updateAutoRollButton() {
        const btn = document.getElementById('autoRollBtn');
        if (!this.game.stats.autoRollUnlocked) {
            btn.disabled = true;
            btn.textContent = '⚡ Auto Roll (Gesperrt)';
        } else if (this.game.stats.autoRollActive) {
            btn.disabled = false;
            btn.textContent = '⚡ Auto Roll: AN';
            btn.classList.add('btn-success');
            btn.classList.remove('btn-secondary');
        } else {
            btn.disabled = false;
            btn.textContent = '⚡ Auto Roll: AUS';
            btn.classList.remove('btn-success');
            btn.classList.add('btn-secondary');
        }
    }

    // ========================================
    // Auto Equip
    // ========================================
    handleAutoEquip() {
        this.game.autoEquipBest();
        this.updatePlots();
        this.showToast('Beste Rayans ausgerüstet!', 'success');
    }

    // ========================================
    // Merge
    // ========================================
    handleMergeAll() {
        const count = this.game.mergeAll();
        if (count > 0) {
            this.showToast(`${count} Merge(s) durchgeführt!`, 'success');
        } else {
            this.showToast('Keine Rayans zum Mergen (5+ gleiche benötigt)', 'info');
        }
        this.updateAll();
    }

    // ========================================
    // Modal Handling
    // ========================================
    openModal(type) {
        const overlay = document.getElementById('modalOverlay');
        
        // Hide all modals
        document.querySelectorAll('.modal').forEach(m => m.classList.add('hidden'));
        
        // Show overlay
        overlay.classList.remove('hidden');
        
        // Show specific modal
        switch (type) {
            case 'shop':
                document.getElementById('shopModal').classList.remove('hidden');
                this.renderDiceShop();
                break;
            case 'upgrades':
                document.getElementById('upgradesModal').classList.remove('hidden');
                this.renderUpgrades();
                break;
            case 'quests':
                document.getElementById('questsModal').classList.remove('hidden');
                this.renderQuests();
                break;
            case 'rebirth':
                document.getElementById('rebirthModal').classList.remove('hidden');
                this.renderRebirth();
                break;
            case 'stats':
                document.getElementById('statsModal').classList.remove('hidden');
                this.renderStats();
                break;
            case 'inventory':
                document.getElementById('inventoryModal').classList.remove('hidden');
                this.renderFullInventory();
                break;
        }
        
        this.currentModal = type;
    }

    closeModal() {
        document.getElementById('modalOverlay').classList.add('hidden');
        document.querySelectorAll('.modal').forEach(m => m.classList.add('hidden'));
        this.currentModal = null;
    }

    // ========================================
    // Dice Shop
    // ========================================
    renderDiceShop() {
        document.getElementById('shopMoneyDisplay').textContent = formatBigInt(this.game.stats.money);
        
        const container = document.getElementById('diceShopList');
        container.innerHTML = '';
        
        for (const template of DICE_TEMPLATES) {
            const owned = this.game.stats.ownedDices.find(d => d.name === template.name);
            const ownedQty = owned ? owned.quantity : BigInt(0);
            const canAfford = this.game.adminMode || this.game.stats.money >= template.cost;
            
            const item = document.createElement('div');
            item.className = 'shop-item';
            item.innerHTML = `
                <div class="shop-item-info">
                    <div class="shop-item-name">🎲 ${template.name}</div>
                    <div class="shop-item-desc">+${((template.luckMultiplier - 1) * 100).toFixed(0)}% Luck</div>
                    <div class="shop-item-owned">Besitzt: ${formatBigInt(ownedQty)}</div>
                </div>
                <div class="shop-item-actions">
                    <div class="shop-item-cost">💰 ${formatBigInt(template.cost)}</div>
                    <button class="btn btn-primary btn-small buy-btn" ${!canAfford ? 'disabled' : ''}>
                        ${this.game.adminMode ? 'FREE' : 'Kaufen'}
                    </button>
                    <button class="btn btn-accent btn-small buy-max-btn" ${!canAfford ? 'disabled' : ''}>
                        MAX
                    </button>
                </div>
            `;
            
            item.querySelector('.buy-btn').addEventListener('click', () => {
                if (this.game.buyDice(template)) {
                    this.renderDiceShop();
                    this.updateDiceSelector();
                    this.showToast(`${template.name} gekauft!`, 'success');
                }
            });
            
            item.querySelector('.buy-max-btn').addEventListener('click', () => {
                if (this.game.buyMaxDice(template)) {
                    this.renderDiceShop();
                    this.updateDiceSelector();
                    this.showToast(`${template.name} (MAX) gekauft!`, 'success');
                }
            });
            
            container.appendChild(item);
        }
    }

    // ========================================
    // Upgrades
    // ========================================
    renderUpgrades() {
        document.getElementById('upgradeGemsDisplay').textContent = this.game.stats.gems;
        document.getElementById('upgradeMoneyDisplay').textContent = formatBigInt(this.game.stats.money);
        
        this.renderGemUpgrades();
        this.renderMoneyUpgrades();
    }

    renderGemUpgrades() {
        const container = document.getElementById('gemsUpgrades');
        container.innerHTML = '';
        
        // Auto Roll Unlock
        const autoRollItem = document.createElement('div');
        autoRollItem.className = 'upgrade-item';
        
        if (this.game.stats.autoRollUnlocked) {
            autoRollItem.innerHTML = `
                <div class="upgrade-item-info">
                    <div class="upgrade-item-name">⚡ Auto Roll</div>
                    <div class="upgrade-item-desc">Automatisches Rollen</div>
                    <div class="upgrade-item-level">✓ Freigeschaltet</div>
                </div>
                <button class="btn ${this.game.stats.autoRollActive ? 'btn-success' : 'btn-secondary'} toggle-autoroll">
                    ${this.game.stats.autoRollActive ? 'AN' : 'AUS'}
                </button>
            `;
            autoRollItem.querySelector('.toggle-autoroll').addEventListener('click', () => {
                this.handleAutoRoll();
                this.renderUpgrades();
            });
        } else {
            const canAfford = this.game.adminMode || this.game.stats.gems >= 100;
            autoRollItem.innerHTML = `
                <div class="upgrade-item-info">
                    <div class="upgrade-item-name">⚡ Auto Roll</div>
                    <div class="upgrade-item-desc">Automatisches Rollen freischalten</div>
                </div>
                <button class="btn btn-primary" ${!canAfford ? 'disabled' : ''}>
                    ${this.game.adminMode ? 'FREE' : '💎 100 Gems'}
                </button>
            `;
            autoRollItem.querySelector('button').addEventListener('click', () => {
                if (this.game.unlockAutoRoll()) {
                    this.renderUpgrades();
                    this.updateAutoRollButton();
                    this.showToast('Auto Roll freigeschaltet!', 'success');
                }
            });
        }
        container.appendChild(autoRollItem);
        
        // Roll Cooldown
        const cooldownItem = document.createElement('div');
        cooldownItem.className = 'upgrade-item';
        
        const cooldownCost = this.game.getRollCooldownCost();
        const canAffordCooldown = (this.game.adminMode || this.game.stats.gems >= cooldownCost) && this.game.stats.rollCooldown > 0.5;
        const nextCooldown = Math.max(0.5, this.game.stats.rollCooldown - 0.2);
        
        cooldownItem.innerHTML = `
            <div class="upgrade-item-info">
                <div class="upgrade-item-name">⏱️ Roll Cooldown</div>
                <div class="upgrade-item-desc">Aktuell: ${this.game.stats.rollCooldown.toFixed(1)}s → ${nextCooldown.toFixed(1)}s</div>
                <div class="upgrade-item-level">Level ${this.game.stats.rollCooldownLevel} (Min: 0.5s)</div>
            </div>
            <button class="btn btn-primary" ${!canAffordCooldown ? 'disabled' : ''}>
                ${this.game.adminMode ? 'FREE' : `💎 ${cooldownCost} Gems`}
            </button>
        `;
        cooldownItem.querySelector('button').addEventListener('click', () => {
            if (this.game.buyRollCooldownUpgrade()) {
                this.renderUpgrades();
                this.showToast('Cooldown reduziert!', 'success');
            }
        });
        container.appendChild(cooldownItem);
    }

    renderMoneyUpgrades() {
        const container = document.getElementById('moneyUpgrades');
        container.innerHTML = '';
        
        // Luck Booster
        const luckItem = document.createElement('div');
        luckItem.className = 'upgrade-item';
        
        const luckCost = this.game.getLuckBoosterCost();
        const canAfford = this.game.adminMode || this.game.stats.money >= luckCost;
        
        luckItem.innerHTML = `
            <div class="upgrade-item-info">
                <div class="upgrade-item-name">🍀 Luck Booster</div>
                <div class="upgrade-item-desc">+25% Luck pro Level</div>
                <div class="upgrade-item-level">Level ${this.game.stats.luckBoosterLevel} (${(this.game.stats.luckMultiplier * 100 - 100).toFixed(0)}% Bonus)</div>
            </div>
            <button class="btn btn-success" ${!canAfford ? 'disabled' : ''}>
                ${this.game.adminMode ? 'FREE' : `💰 ${formatBigInt(luckCost)}`}
            </button>
        `;
        luckItem.querySelector('button').addEventListener('click', () => {
            if (this.game.buyLuckBooster()) {
                this.renderUpgrades();
                this.updateStats();
                this.showToast('Luck erhöht!', 'success');
            }
        });
        container.appendChild(luckItem);
        
        // Plot Slots Info
        const plotItem = document.createElement('div');
        plotItem.className = 'upgrade-item';
        plotItem.innerHTML = `
            <div class="upgrade-item-info">
                <div class="upgrade-item-name">📊 Plot Slots</div>
                <div class="upgrade-item-desc">Aktuell: ${this.game.stats.plotSlots}/10</div>
                <div class="upgrade-item-level">Nur durch Rebirth erhöhbar!</div>
            </div>
            <button class="btn btn-secondary" disabled>
                Nur via Rebirth
            </button>
        `;
        container.appendChild(plotItem);
    }

    switchUpgradeTab(tab) {
        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.classList.toggle('active', btn.dataset.tab === tab);
        });
        document.querySelectorAll('.tab-content').forEach(content => {
            content.classList.toggle('active', content.id === `${tab}Upgrades`);
        });
    }

    // ========================================
    // Quests
    // ========================================
    renderQuests() {
        const container = document.getElementById('questsList');
        container.innerHTML = '';
        
        for (const quest of this.game.questService.quests) {
            const item = document.createElement('div');
            item.className = `quest-item ${quest.isCompleted ? 'completed' : ''} ${quest.isClaimed && !quest.isRepeatable ? 'claimed' : ''}`;
            
            const progress = Math.min(quest.currentProgress, quest.goal);
            const percentage = quest.progressPercentage;
            
            item.innerHTML = `
                <div class="quest-header">
                    <span class="quest-name">${quest.description}</span>
                    <span class="quest-reward">💎 ${quest.rewardGems}</span>
                </div>
                <div class="quest-progress">
                    <div class="quest-progress-bar">
                        <div class="quest-progress-fill" style="width: ${percentage}%"></div>
                    </div>
                    <div class="quest-progress-text">${progress} / ${quest.goal} (${percentage.toFixed(1)}%)</div>
                </div>
                ${quest.isCompleted && !quest.isClaimed ? `
                    <button class="btn btn-success claim-btn">Belohnung abholen</button>
                ` : ''}
                ${quest.timesCompleted > 0 ? `
                    <div class="quest-progress-text">${quest.timesCompleted}x abgeschlossen</div>
                ` : ''}
            `;
            
            const claimBtn = item.querySelector('.claim-btn');
            if (claimBtn) {
                claimBtn.addEventListener('click', () => {
                    const reward = this.game.questService.claimReward(quest.id, this.game.stats);
                    if (reward > 0) {
                        this.showToast(`+${reward} Gems erhalten!`, 'success');
                        this.renderQuests();
                        this.updateStats();
                    }
                });
            }
            
            container.appendChild(item);
        }
    }

    // ========================================
    // Rebirth
    // ========================================
    renderRebirth() {
        document.getElementById('currentRebirths').textContent = this.game.stats.rebirths;
        document.getElementById('rebirthBonus').textContent = `+${((this.game.stats.moneyMultiplier - 1) * 100).toFixed(0)}%`;
        document.getElementById('rebirthPlots').textContent = `${this.game.stats.plotSlots}/10`;
        document.getElementById('rebirthCost').textContent = formatBigInt(this.game.stats.nextRebirthCost);
        
        const btn = document.getElementById('rebirthBtn');
        btn.disabled = !this.game.adminMode && this.game.stats.money < this.game.stats.nextRebirthCost;
        btn.textContent = this.game.adminMode ? '🔄 Rebirth (FREE)' : '🔄 Rebirth';
    }

    handleRebirth() {
        if (this.game.rebirth()) {
            this.showToast('Rebirth erfolgreich!', 'success');
            this.closeModal();
            this.updateAll();
        } else {
            this.showToast('Nicht genug Geld für Rebirth!', 'error');
        }
    }

    // ========================================
    // Stats
    // ========================================
    renderStats() {
        const container = document.getElementById('statsGrid');
        container.innerHTML = `
            <div class="stat-card">
                <div class="stat-card-label">💰 Gesamtes Geld verdient</div>
                <div class="stat-card-value">${formatBigInt(this.game.stats.totalMoneyEarned)}</div>
            </div>
            <div class="stat-card">
                <div class="stat-card-label">🎲 Rolls (Gesamt)</div>
                <div class="stat-card-value">${this.game.stats.totalRollsAllTime}</div>
            </div>
            <div class="stat-card">
                <div class="stat-card-label">🔄 Rebirths (Gesamt)</div>
                <div class="stat-card-value">${this.game.stats.totalRebirthsAllTime}</div>
            </div>
            <div class="stat-card">
                <div class="stat-card-label">⏱️ Spielzeit (Gesamt)</div>
                <div class="stat-card-value">${formatTime(this.game.stats.totalPlayTimeMinutes)}</div>
            </div>
            <div class="stat-card">
                <div class="stat-card-label">⭐ Bester Rayan</div>
                <div class="stat-card-value">${this.game.stats.bestRayanEverName || 'Keiner'}</div>
            </div>
            <div class="stat-card">
                <div class="stat-card-label">💎 Beste Seltenheit</div>
                <div class="stat-card-value">${this.game.stats.bestRayanEverRarity > 0 ? formatRarity(this.game.stats.bestRayanEverRarity) : '-'}</div>
            </div>
            <div class="stat-card">
                <div class="stat-card-label">📦 Inventar</div>
                <div class="stat-card-value">${this.game.stats.inventory.length} Rayans</div>
            </div>
            <div class="stat-card">
                <div class="stat-card-label">🎲 Aktiver Würfel</div>
                <div class="stat-card-value">${this.game.stats.getSelectedDice().name}</div>
            </div>
        `;
    }

    // ========================================
    // Full Inventory
    // ========================================
    renderFullInventory() {
        const container = document.getElementById('fullInventoryList');
        const searchTerm = document.getElementById('inventorySearch').value.toLowerCase();
        const sortBy = document.getElementById('inventorySort').value;
        
        // Group rayans by name
        const groups = new Map();
        for (let i = 0; i < this.game.stats.inventory.length; i++) {
            const rayan = this.game.stats.inventory[i];
            const name = rayan.fullName;
            
            if (!groups.has(name)) {
                groups.set(name, {
                    rayan: rayan,
                    count: 0,
                    indices: []
                });
            }
            groups.get(name).count++;
            groups.get(name).indices.push(i);
        }
        
        // Convert to array and filter/sort
        let items = Array.from(groups.values());
        
        // Filter by search
        if (searchTerm) {
            items = items.filter(item => 
                item.rayan.fullName.toLowerCase().includes(searchTerm)
            );
        }
        
        // Sort
        switch (sortBy) {
            case 'value':
                items.sort((a, b) => Number(b.rayan.totalValue - a.rayan.totalValue));
                break;
            case 'rarity':
                items.sort((a, b) => b.rayan.rarity - a.rayan.rarity);
                break;
            case 'name':
                items.sort((a, b) => a.rayan.fullName.localeCompare(b.rayan.fullName));
                break;
        }
        
        // Limit to 500 for performance
        items = items.slice(0, 500);
        
        container.innerHTML = '';
        
        for (const item of items) {
            const el = document.createElement('div');
            el.className = 'inventory-full-item';
            
            const rarityClass = getRarityClass(item.rayan.rarity);
            const canMerge = item.count >= 5;
            
            el.innerHTML = `
                <div class="inventory-full-item-header">
                    <span class="inventory-full-item-name ${rarityClass}">${item.rayan.fullName}</span>
                    <span class="inventory-full-item-count">x${item.count}</span>
                </div>
                <div class="inventory-full-item-details">
                    <span class="inventory-full-item-value">💰 ${formatBigInt(item.rayan.totalValue)}/s</span>
                    <span class="inventory-full-item-rarity">${formatRarity(item.rayan.rarity)} • ${getRarityName(item.rayan.rarity)}</span>
                </div>
                ${canMerge ? `
                    <div class="inventory-full-item-actions">
                        <button class="btn btn-accent btn-small merge-btn">Merge (5→1, 5x Wert)</button>
                    </div>
                ` : ''}
            `;
            
            const mergeBtn = el.querySelector('.merge-btn');
            if (mergeBtn) {
                mergeBtn.addEventListener('click', () => {
                    const merged = this.game.mergeRayans(item.rayan.fullName);
                    if (merged) {
                        this.showToast(`Merged: ${merged.fullName}`, 'success');
                        this.renderFullInventory();
                        this.updateAll();
                    }
                });
            }
            
            container.appendChild(el);
        }
        
        if (items.length === 0) {
            container.innerHTML = '<div class="text-center text-muted">Keine Rayans gefunden</div>';
        }
    }

    // ========================================
    // Update Methods
    // ========================================
    updateAll() {
        this.updateStats();
        this.updatePlots();
        this.updateInventoryPreview();
        this.updateDiceSelector();
        this.updateAutoRollButton();
        this.updateAdminBadge();
        this.updateEvents();
    }

    updateStats() {
        document.getElementById('moneyDisplay').textContent = formatBigInt(this.game.stats.money);
        document.getElementById('gemsDisplay').textContent = this.game.stats.gems;
        document.getElementById('luckDisplay').textContent = `+${this.game.getTotalLuckBonus().toFixed(0)}%`;
        document.getElementById('rebirthDisplay').textContent = `+${((this.game.stats.moneyMultiplier - 1) * 100).toFixed(0)}%`;
        document.getElementById('totalRolls').textContent = this.game.stats.totalRolls;
        document.getElementById('incomeDisplay').textContent = `${formatBigInt(this.game.getIncomePerSecond())}/s`;
    }

    updatePlots() {
        const container = document.getElementById('plotsContainer');
        document.getElementById('plotCount').textContent = `(${this.game.stats.equippedRayanIndices.length}/${this.game.stats.plotSlots})`;
        
        container.innerHTML = '';
        
        for (let i = 0; i < this.game.stats.plotSlots; i++) {
            const slot = document.createElement('div');
            slot.className = 'plot-slot';
            
            if (i < this.game.stats.equippedRayanIndices.length) {
                const index = this.game.stats.equippedRayanIndices[i];
                if (index >= 0 && index < this.game.stats.inventory.length) {
                    const rayan = this.game.stats.inventory[index];
                    const rarityClass = getRarityClass(rayan.rarity);
                    slot.innerHTML = `
                        <span class="plot-name ${rarityClass}">${rayan.fullName}</span>
                        <span class="plot-value">💰 ${formatBigInt(rayan.totalValue)}/s</span>
                    `;
                } else {
                    slot.classList.add('empty');
                    slot.textContent = 'Leer';
                }
            } else {
                slot.classList.add('empty');
                slot.textContent = 'Leer';
            }
            
            container.appendChild(slot);
        }
    }

    updateInventoryPreview() {
        const container = document.getElementById('inventoryPreview');
        document.getElementById('inventoryCount').textContent = `(${this.game.stats.inventory.length})`;
        
        // Group by name
        const groups = new Map();
        for (const rayan of this.game.stats.inventory) {
            const name = rayan.fullName;
            if (!groups.has(name)) {
                groups.set(name, { rayan: rayan, count: 0 });
            }
            groups.get(name).count++;
        }
        
        // Sort by value and take top 10
        const sorted = Array.from(groups.values())
            .sort((a, b) => Number(b.rayan.totalValue - a.rayan.totalValue))
            .slice(0, 10);
        
        container.innerHTML = '';
        
        for (const item of sorted) {
            const el = document.createElement('div');
            el.className = 'inventory-item';
            const rarityClass = getRarityClass(item.rayan.rarity);
            
            el.innerHTML = `
                <span class="inventory-item-name ${rarityClass}">${item.rayan.fullName}</span>
                <span class="inventory-item-count">x${item.count}</span>
                <span class="inventory-item-value">${formatBigInt(item.rayan.totalValue)}/s</span>
            `;
            
            container.appendChild(el);
        }
        
        if (sorted.length === 0) {
            container.innerHTML = '<div class="text-center text-muted">Noch keine Rayans</div>';
        }
    }

    updateDiceSelector() {
        const select = document.getElementById('diceSelect');
        select.innerHTML = '';
        
        for (let i = 0; i < this.game.stats.ownedDices.length; i++) {
            const dice = this.game.stats.ownedDices[i];
            const option = document.createElement('option');
            option.value = i;
            option.textContent = `${dice.displayName} (${dice.luckMultiplier}x) ${dice.isInfinite ? '' : `[${dice.quantityDisplay}]`}`;
            option.selected = i === this.game.stats.selectedDiceIndex;
            select.appendChild(option);
        }
    }

    updateAdminBadge() {
        const badge = document.getElementById('adminBadge');
        badge.classList.toggle('hidden', !this.game.adminMode);
    }

    updateEvents() {
        const banner = document.getElementById('eventBanner');
        const activeEvents = this.game.currentEvents.filter(e => e.isActive);
        
        if (activeEvents.length > 0) {
            banner.classList.remove('hidden');
            const event = activeEvents[0];
            document.getElementById('eventText').textContent = `🔥 ${event.suffixName} Event! (20x Chance)`;
            const time = event.timeRemaining;
            document.getElementById('eventTimer').textContent = `${time.minutes}:${time.seconds.toString().padStart(2, '0')}`;
        } else {
            banner.classList.add('hidden');
        }
    }

    // ========================================
    // Toast Notifications
    // ========================================
    showToast(message, type = 'info') {
        const toast = document.getElementById('toast');
        const toastMessage = document.getElementById('toastMessage');
        
        toastMessage.textContent = message;
        toast.className = `toast ${type}`;
        toast.classList.remove('hidden');
        
        setTimeout(() => {
            toast.classList.add('hidden');
        }, 3000);
    }
}
