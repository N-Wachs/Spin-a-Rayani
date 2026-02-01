/**
 * Spin a Rayan - Web Version
 * Game Services (RollService, SaveService, QuestService, GameManager)
 */

// ========================================
// Save Service (localStorage)
// ========================================
class SaveService {
    static SAVE_KEY = 'spinARayanSave';

    static save(stats) {
        try {
            const data = stats.toObject();
            localStorage.setItem(this.SAVE_KEY, JSON.stringify(data));
            console.log('[SaveService] Game saved successfully');
        } catch (error) {
            console.error('[SaveService] Save failed:', error);
        }
    }

    static load() {
        try {
            const data = localStorage.getItem(this.SAVE_KEY);
            if (data) {
                const parsed = JSON.parse(data);
                const stats = PlayerStats.fromObject(parsed);
                console.log('[SaveService] Game loaded successfully');
                return stats;
            }
        } catch (error) {
            console.error('[SaveService] Load failed:', error);
        }

        // Return new stats with Basic Dice initialized
        const stats = new PlayerStats();
        stats.ownedDices = [
            new Dice("Basic Dice", 1.0, BigInt(0), BigInt(0), true)
        ];
        return stats;
    }

    static reset() {
        localStorage.removeItem(this.SAVE_KEY);
        console.log('[SaveService] Save deleted');
    }
}

// ========================================
// Roll Service
// ========================================
class RollService {
    constructor() {
        // Use pre-sorted lists from data.js
        this.sortedPrefixes = SORTED_PREFIXES;
        this.sortedSuffixes = SORTED_SUFFIXES;
    }

    roll(luckMultiplier, activeEvents = []) {
        // Roll for Prefix (affected by luck)
        const prefixData = this.selectPrefix(luckMultiplier);

        // Roll for Suffix (NOT affected by luck, only by events)
        const suffixData = this.selectSuffix(activeEvents);

        return new Rayan(
            prefixData.prefix,
            suffixData ? suffixData.suffix : "",
            prefixData.rarity,
            prefixData.baseValue,
            suffixData ? suffixData.multiplier : 1.0
        );
    }

    selectPrefix(luck) {
        for (const item of this.sortedPrefixes) {
            const chance = 1.0 / (item.rarity / luck);
            if (Math.random() < chance) {
                return item;
            }
        }
        // Return least rare if nothing else rolled
        return this.sortedPrefixes[this.sortedPrefixes.length - 1];
    }

    selectSuffix(activeEvents = []) {
        for (const item of this.sortedSuffixes) {
            let baseChance = item.chance;

            // Check if any active event boosts this suffix
            const matchingEvent = activeEvents.find(e => e.isActive && e.suffixName === item.suffix);
            if (matchingEvent) {
                baseChance /= matchingEvent.boostMultiplier;
            }

            const chance = 1.0 / baseChance;
            if (Math.random() < chance) {
                return item;
            }
        }
        return null;
    }
}

// ========================================
// Quest Service
// ========================================
class QuestService {
    constructor() {
        this.quests = [];
        this.initializeQuests();
    }

    initializeQuests() {
        this.quests = QUEST_DEFINITIONS.map(def => new Quest(def));
    }

    loadQuestsFromStats(stats) {
        if (stats.savedQuests && stats.savedQuests.length > 0) {
            for (const savedQuest of stats.savedQuests) {
                const quest = this.quests.find(q => q.id === savedQuest.id);
                if (quest) {
                    quest.currentProgress = savedQuest.currentProgress || 0;
                    quest.isCompleted = savedQuest.isCompleted || false;
                    quest.isClaimed = savedQuest.isClaimed || false;
                    quest.baseProgress = savedQuest.baseProgress || 0;
                    quest.timesCompleted = savedQuest.timesCompleted || 0;
                    quest.goal = savedQuest.goal || quest.initialGoal;
                }
            }
        }
    }

    saveQuestsToStats(stats) {
        stats.savedQuests = this.quests.map(q => q.toObject());
    }

    updateProgress(stats) {
        for (const quest of this.quests) {
            if (quest.isCompleted) continue;

            let actualProgress = 0;

            switch (quest.type) {
                case 'rolls':
                    actualProgress = stats.totalRolls;
                    break;
                case 'playtime':
                    actualProgress = Math.floor(stats.playTimeMinutes);
                    break;
                case 'rebirths':
                    actualProgress = stats.rebirths;
                    break;
            }

            quest.currentProgress = actualProgress - quest.baseProgress;

            if (quest.currentProgress >= quest.goal && !quest.isCompleted) {
                quest.isCompleted = true;
            }
        }
    }

    claimReward(questId, stats) {
        const quest = this.quests.find(q => q.id === questId);
        if (quest && quest.isCompleted && !quest.isClaimed) {
            stats.gems += quest.rewardGems;
            quest.isClaimed = true;

            if (quest.isRepeatable) {
                quest.reset();
            }

            this.saveQuestsToStats(stats);
            return quest.rewardGems;
        }
        return 0;
    }
}

// ========================================
// Game Manager
// ========================================
class GameManager {
    constructor() {
        this.stats = SaveService.load();
        this.rollService = new RollService();
        this.questService = new QuestService();
        
        this.currentEvents = [];
        this.nextEventTime = Date.now() + 5 * 60 * 1000; // First event in 5 minutes
        
        this.adminMode = false;
        this.lastUpdate = Date.now();
        
        // Event callbacks
        this.onStatsChanged = null;
        this.onRayanRolled = null;
        this.onEventsChanged = null;

        // Load quest progress
        this.questService.loadQuestsFromStats(this.stats);

        // Ensure Basic Dice exists
        if (this.stats.ownedDices.length === 0) {
            this.stats.ownedDices.push(new Dice("Basic Dice", 1.0, BigInt(0), BigInt(0), true));
        }

        // Start game timer
        this.startGameTimer();
    }

    startGameTimer() {
        setInterval(() => this.gameTick(), 1000);
    }

    gameTick() {
        this.updateFarming();
        this.stats.playTimeMinutes += 1.0 / 60.0;
        this.stats.totalPlayTimeMinutes += 1.0 / 60.0;
        this.questService.updateProgress(this.stats);

        // Check for event updates
        this.updateEvents();

        // Remove expired events
        this.currentEvents = this.currentEvents.filter(evt => evt.isActive);

        // Update event display
        if (this.currentEvents.length > 0 && this.onEventsChanged) {
            this.onEventsChanged(this.currentEvents.filter(e => e.isActive));
        }

        // Autosave every 60 seconds
        if (Math.floor(this.stats.playTimeMinutes * 60) % 60 === 0 && this.stats.playTimeMinutes > 0) {
            this.save();
        }

        if (this.onStatsChanged) {
            this.onStatsChanged();
        }
    }

    updateFarming() {
        let incomePerSecond = BigInt(0);
        
        for (const index of this.stats.equippedRayanIndices) {
            if (index >= 0 && index < this.stats.inventory.length) {
                incomePerSecond += this.stats.inventory[index].totalValue;
            }
        }

        // Apply money multiplier
        let totalMoneyMultiplier = this.stats.moneyMultiplier;
        for (const evt of this.currentEvents.filter(e => e.isActive)) {
            totalMoneyMultiplier *= evt.moneyMultiplier;
        }

        const earnedThisSecond = BigInt(Math.floor(Number(incomePerSecond) * totalMoneyMultiplier));
        this.stats.money += earnedThisSecond;
        this.stats.totalMoneyEarned += earnedThisSecond;
    }

    updateEvents() {
        // Remove expired events
        this.currentEvents = this.currentEvents.filter(evt => evt.isActive);

        // Start new event every 5 minutes
        if (Date.now() >= this.nextEventTime) {
            this.startRandomEvent();
            this.nextEventTime = Date.now() + 5 * 60 * 1000;
        }
    }

    startRandomEvent() {
        const suffixes = SUFFIXES;
        const selectedIndex = Math.floor(Math.random() * suffixes.length);
        const selectedSuffix = suffixes[selectedIndex];

        const newEvent = new SuffixEvent(
            selectedSuffix.suffix,
            `${selectedSuffix.suffix} Event!`,
            2.5, // 2.5 minutes duration
            20.0 // 20x boost
        );

        this.currentEvents.push(newEvent);
        
        if (this.onEventsChanged) {
            this.onEventsChanged(this.currentEvents.filter(e => e.isActive));
        }
    }

    forceEvent(suffixName = null) {
        let suffix;
        if (suffixName) {
            suffix = SUFFIXES.find(s => s.suffix === suffixName);
        } else {
            const index = Math.floor(Math.random() * SUFFIXES.length);
            suffix = SUFFIXES[index];
        }

        if (suffix) {
            const newEvent = new SuffixEvent(
                suffix.suffix,
                `${suffix.suffix} Event!`,
                2.5,
                20.0
            );
            this.currentEvents.push(newEvent);
            
            if (this.onEventsChanged) {
                this.onEventsChanged(this.currentEvents.filter(e => e.isActive));
            }
        }
    }

    roll() {
        // Get selected dice
        let selectedDice = this.stats.getSelectedDice();

        // Check if dice is available
        if (!selectedDice.isInfinite && selectedDice.quantity <= 0) {
            const basicDice = this.stats.ownedDices.find(d => d.isInfinite);
            if (basicDice) {
                this.stats.selectedDiceIndex = this.stats.ownedDices.indexOf(basicDice);
                selectedDice = basicDice;
            }
        }

        // Calculate total luck
        const luckFromBooster = this.stats.luckMultiplier;
        const luckFromDice = selectedDice.luckMultiplier;
        const luckFromRebirths = 1.0 + (this.stats.rebirths * 0.5);
        const totalLuck = luckFromBooster * luckFromDice * luckFromRebirths;

        // Roll with active events
        const activeEvents = this.currentEvents.filter(e => e.isActive);
        const rayan = this.rollService.roll(totalLuck, activeEvents);

        this.stats.inventory.push(rayan);
        this.stats.totalRolls++;
        this.stats.totalRollsAllTime++;

        // Track best rayan ever
        if (rayan.rarity > this.stats.bestRayanEverRarity) {
            this.stats.bestRayanEverName = rayan.fullName;
            this.stats.bestRayanEverRarity = rayan.rarity;
            this.stats.bestRayanEverValue = rayan.totalValue;
        }

        // Reduce quantity for non-infinite dices
        if (!selectedDice.isInfinite) {
            selectedDice.quantity--;
            if (selectedDice.quantity <= 0) {
                // Remove dice and switch to Basic
                const index = this.stats.ownedDices.indexOf(selectedDice);
                if (index > -1) {
                    this.stats.ownedDices.splice(index, 1);
                }
                const basicDice = this.stats.ownedDices.find(d => d.isInfinite);
                if (basicDice) {
                    this.stats.selectedDiceIndex = this.stats.ownedDices.indexOf(basicDice);
                } else {
                    this.stats.selectedDiceIndex = 0;
                }
            }
        }

        if (this.onRayanRolled) {
            this.onRayanRolled(rayan);
        }
        if (this.onStatsChanged) {
            this.onStatsChanged();
        }

        return rayan;
    }

    autoEquipBest() {
        const sortedInventory = this.stats.inventory
            .map((r, i) => ({ rayan: r, index: i }))
            .sort((a, b) => Number(b.rayan.totalValue - a.rayan.totalValue))
            .slice(0, this.stats.plotSlots);

        this.stats.equippedRayanIndices = sortedInventory.map(x => x.index);
        
        if (this.onStatsChanged) {
            this.onStatsChanged();
        }
    }

    getIncomePerSecond() {
        let income = BigInt(0);
        for (const index of this.stats.equippedRayanIndices) {
            if (index >= 0 && index < this.stats.inventory.length) {
                income += this.stats.inventory[index].totalValue;
            }
        }
        return BigInt(Math.floor(Number(income) * this.stats.moneyMultiplier));
    }

    rebirth() {
        if (this.adminMode || this.stats.money >= this.stats.nextRebirthCost) {
            this.stats.money = BigInt(0);
            this.stats.inventory = [];
            this.stats.equippedRayanIndices = [];
            this.stats.rebirths++;
            this.stats.totalRebirthsAllTime++;

            if (this.stats.plotSlots < 10) {
                this.stats.plotSlots++;
            }

            // Keep only Basic Dice
            const basicDice = this.stats.ownedDices.find(d => d.isInfinite);
            this.stats.ownedDices = [];
            if (basicDice) {
                this.stats.ownedDices.push(basicDice);
            } else {
                this.stats.ownedDices.push(new Dice("Basic Dice", 1.0, BigInt(0), BigInt(0), true));
            }
            this.stats.selectedDiceIndex = 0;

            this.save();
            
            if (this.onStatsChanged) {
                this.onStatsChanged();
            }

            return true;
        }
        return false;
    }

    mergeRayans(rayanName) {
        // Find all Rayans with this name
        const indices = [];
        for (let i = 0; i < this.stats.inventory.length; i++) {
            if (this.stats.inventory[i].fullName === rayanName) {
                indices.push(i);
            }
        }

        // Need at least 5 to merge
        if (indices.length < 5) return null;

        // Get the first 5
        const toMerge = indices.slice(0, 5);
        const baseRayan = this.stats.inventory[toMerge[0]];

        // Create merged rayan (5x value)
        const mergedRayan = new Rayan(
            baseRayan.prefix,
            baseRayan.suffix,
            baseRayan.rarity,
            baseRayan.baseValue * BigInt(5),
            baseRayan.multiplier
        );

        // Remove merged rayans (in reverse order to maintain indices)
        for (const index of toMerge.reverse()) {
            this.stats.inventory.splice(index, 1);
        }

        // Update equipped indices
        this.stats.equippedRayanIndices = this.stats.equippedRayanIndices
            .map(idx => {
                let newIdx = idx;
                for (const removed of toMerge) {
                    if (idx > removed) newIdx--;
                }
                return newIdx;
            })
            .filter(idx => idx >= 0);

        // Add merged rayan
        this.stats.inventory.push(mergedRayan);

        if (this.onStatsChanged) {
            this.onStatsChanged();
        }

        return mergedRayan;
    }

    mergeAll() {
        let mergedCount = 0;
        let canMerge = true;

        while (canMerge) {
            canMerge = false;
            
            // Group rayans by name
            const groups = new Map();
            for (const rayan of this.stats.inventory) {
                const name = rayan.fullName;
                if (!groups.has(name)) {
                    groups.set(name, 0);
                }
                groups.set(name, groups.get(name) + 1);
            }

            // Find first group with 5+ rayans
            for (const [name, count] of groups) {
                if (count >= 5) {
                    this.mergeRayans(name);
                    mergedCount++;
                    canMerge = true;
                    break;
                }
            }
        }

        return mergedCount;
    }

    buyDice(template, quantity = 1) {
        const totalCost = template.cost * BigInt(quantity);
        
        if (!this.adminMode && this.stats.money < totalCost) {
            return false;
        }

        if (!this.adminMode) {
            this.stats.money -= totalCost;
        }

        // Find existing dice or create new
        let existingDice = this.stats.ownedDices.find(d => d.name === template.name);
        if (existingDice) {
            existingDice.quantity += BigInt(quantity);
        } else {
            const newDice = new Dice(
                template.name,
                template.luckMultiplier,
                template.cost,
                BigInt(quantity),
                false
            );
            this.stats.ownedDices.push(newDice);
        }

        this.save();
        
        if (this.onStatsChanged) {
            this.onStatsChanged();
        }

        return true;
    }

    buyMaxDice(template) {
        if (this.adminMode) {
            return this.buyDice(template, 1000);
        }

        if (template.cost <= 0 || this.stats.money < template.cost) {
            return false;
        }

        const quantity = Number(this.stats.money / template.cost);
        if (quantity <= 0) return false;

        return this.buyDice(template, quantity);
    }

    selectDice(index) {
        if (index >= 0 && index < this.stats.ownedDices.length) {
            this.stats.selectedDiceIndex = index;
            if (this.onStatsChanged) {
                this.onStatsChanged();
            }
        }
    }

    unlockAutoRoll() {
        const cost = 100;
        if (this.adminMode || this.stats.gems >= cost) {
            if (!this.adminMode) {
                this.stats.gems -= cost;
            }
            this.stats.autoRollUnlocked = true;
            this.save();
            if (this.onStatsChanged) {
                this.onStatsChanged();
            }
            return true;
        }
        return false;
    }

    toggleAutoRoll() {
        if (this.stats.autoRollUnlocked) {
            this.stats.autoRollActive = !this.stats.autoRollActive;
            if (this.onStatsChanged) {
                this.onStatsChanged();
            }
        }
    }

    buyRollCooldownUpgrade() {
        const cost = this.getRollCooldownCost();
        if ((this.adminMode || this.stats.gems >= cost) && this.stats.rollCooldown > 0.5) {
            if (!this.adminMode) {
                this.stats.gems -= cost;
            }
            this.stats.rollCooldownLevel++;
            this.stats.rollCooldown = Math.max(0.5, this.stats.rollCooldown - 0.2);
            this.save();
            if (this.onStatsChanged) {
                this.onStatsChanged();
            }
            return true;
        }
        return false;
    }

    getRollCooldownCost() {
        if (this.stats.rollCooldownLevel === 0) return 200;
        return Math.floor(200 * Math.pow(1.5, this.stats.rollCooldownLevel));
    }

    buyLuckBooster() {
        const cost = this.getLuckBoosterCost();
        if (this.adminMode || this.stats.money >= cost) {
            if (!this.adminMode) {
                this.stats.money -= cost;
            }
            this.stats.luckBoosterLevel++;
            this.save();
            if (this.onStatsChanged) {
                this.onStatsChanged();
            }
            return true;
        }
        return false;
    }

    getLuckBoosterCost() {
        if (this.stats.luckBoosterLevel === 0) return BigInt(5000);
        return BigInt(Math.floor(5000 * Math.pow(1.5, this.stats.luckBoosterLevel)));
    }

    getTotalLuckBonus() {
        const luckFromBooster = this.stats.luckBoosterLevel * 0.25;
        const selectedDice = this.stats.getSelectedDice();
        const luckFromDice = selectedDice.luckMultiplier - 1.0;
        const luckFromRebirths = this.stats.rebirths * 0.5;

        let luckFromEvents = 0;
        for (const evt of this.currentEvents.filter(e => e.isActive)) {
            luckFromEvents += (evt.luckMultiplier - 1.0);
        }

        return (luckFromBooster + luckFromDice + luckFromRebirths + luckFromEvents) * 100;
    }

    getEffectiveRollCooldown() {
        let baseCooldown = this.stats.rollCooldown;
        for (const evt of this.currentEvents.filter(e => e.isActive)) {
            baseCooldown *= evt.rollTimeModifier;
        }
        return baseCooldown;
    }

    save() {
        this.questService.saveQuestsToStats(this.stats);
        SaveService.save(this.stats);
    }

    resetGame() {
        SaveService.reset();
        this.stats = SaveService.load();
        this.questService.initializeQuests();
        if (this.onStatsChanged) {
            this.onStatsChanged();
        }
    }

    toggleAdminMode() {
        this.adminMode = !this.adminMode;
        if (this.onStatsChanged) {
            this.onStatsChanged();
        }
    }
}
