/**
 * Database Service for Web Version
 * Handles authentication and savefile management via Supabase
 */

class DatabaseService {
    static SUPABASE_URL = 'https://gflohnjhunyukdayaahn.supabase.co/rest/v1';
    static SUPABASE_KEY = 'sb_publishable_dZXMv77hZa3_vZbQTYSKeQ_rZ49Ro9w';
    static ENCRYPTION_KEY = 'SpinARayanSecretKey2025';
    static GAME_VERSION = '3.0.0';

    constructor(username) {
        this.username = username;
        this.currentUserId = null;
        this.currentSavefileId = null;
        this.adminUsedThisSession = false;
    }

    // ========================================
    // Authentication
    // ========================================

    async authenticate(username, password) {
        try {
            const response = await fetch(`${DatabaseService.SUPABASE_URL}/User?username=eq.${username}`, {
                headers: this._getHeaders()
            });

            if (!response.ok) return { success: false };

            const users = await response.json();
            if (!users || users.length === 0) {
                return { success: false, error: 'User nicht gefunden' };
            }

            const user = users[0];
            const storedPassword = this._decryptPassword(user.password);

            if (storedPassword === password) {
                this.currentUserId = user.id.toString();
                console.log(`[DB] User authenticated: ${username}`);
                return { success: true, userId: user.id.toString() };
            } else {
                return { success: false, error: 'Falsches Passwort' };
            }
        } catch (error) {
            console.error('[DB] Auth error:', error);
            return { success: false, error: error.message };
        }
    }

    async register(username, password) {
        try {
            // Check if username exists
            const checkResponse = await fetch(`${DatabaseService.SUPABASE_URL}/User?username=eq.${username}`, {
                headers: this._getHeaders()
            });

            if (checkResponse.ok) {
                const existing = await checkResponse.json();
                if (existing && existing.length > 0) {
                    return { success: false, error: 'Username bereits vergeben!' };
                }
            }

            // Create new user
            const encryptedPassword = this._encryptPassword(password);
            const newUser = {
                username: username,
                password: encryptedPassword,
                created_at: new Date().toISOString(),
                savefile_ids: []
            };

            const response = await fetch(`${DatabaseService.SUPABASE_URL}/User`, {
                method: 'POST',
                headers: this._getHeaders(),
                body: JSON.stringify(newUser)
            });

            if (response.ok) {
                const created = await response.json();
                this.currentUserId = created[0].id.toString();
                console.log(`[DB] User registered: ${username}`);
                return { success: true, userId: created[0].id.toString() };
            }

            return { success: false, error: 'Registration failed' };
        } catch (error) {
            console.error('[DB] Register error:', error);
            return { success: false, error: error.message };
        }
    }

    // ========================================
    // Savefile Management
    // ========================================

    async getAllSavefiles() {
        if (!this.currentUserId) {
            console.error('[DB] No user ID set');
            return [];
        }

        try {
            const response = await fetch(
                `${DatabaseService.SUPABASE_URL}/Savefiles?user_id=eq.${this.currentUserId}&order=last_played.desc`,
                { headers: this._getHeaders() }
            );

            if (!response.ok) return [];

            const savefiles = await response.json();
            return savefiles.map(s => ({
                id: s.id.toString(),
                lastPlayed: s.last_played,
                rebirths: s.rebirths,
                money: s.money || '0',
                gems: s.gems,
                adminUsed: s.admin_used
            }));
        } catch (error) {
            console.error('[DB] Get savefiles error:', error);
            return [];
        }
    }

    async loadSavefile(savefileId) {
        try {
            const response = await fetch(
                `${DatabaseService.SUPABASE_URL}/Savefiles?id=eq.${savefileId}`,
                { headers: this._getHeaders() }
            );

            if (!response.ok) {
                throw new Error('Savefile not found');
            }

            const savefiles = await response.json();
            if (!savefiles || savefiles.length === 0) {
                throw new Error('No data found');
            }

            const saveData = savefiles[0];
            this.currentSavefileId = savefileId;
            
            // Track if admin was used (never downgrade from true to false)
            if (saveData.admin_used && !this.adminUsedThisSession) {
                this.adminUsedThisSession = true;
                console.log('[DB] Admin flag loaded from savefile (was already used)');
            }

            console.log(`[DB] Loaded savefile: ${savefileId}`);
            return this._convertDbToStats(saveData);
        } catch (error) {
            console.error('[DB] Load error:', error);
            throw error;
        }
    }

    async createNewSavefile() {
        if (!this.currentUserId) {
            throw new Error('No user ID set');
        }

        try {
            const newStats = new PlayerStats();
            // Initialize with Basic Dice
            newStats.ownedDices = [
                new Dice("Basic Dice", 1.0, BigInt(0), BigInt(0), true)
            ];

            const saveData = this._convertStatsToDb(newStats);
            saveData.user_id = parseInt(this.currentUserId);
            saveData.created_at = new Date().toISOString();

            const response = await fetch(`${DatabaseService.SUPABASE_URL}/Savefiles`, {
                method: 'POST',
                headers: this._getHeaders(),
                body: JSON.stringify(saveData)
            });

            if (!response.ok) {
                const error = await response.text();
                console.error('[DB] Create savefile failed:', error);
                throw new Error('Failed to create savefile');
            }

            const created = await response.json();
            this.currentSavefileId = created[0].id.toString();

            console.log(`[DB] Created new savefile: ${this.currentSavefileId}`);
            return newStats;
        } catch (error) {
            console.error('[DB] Create savefile error:', error);
            throw error;
        }
    }

    async saveSavefile(stats) {
        if (!this.currentSavefileId || !this.currentUserId) {
            console.error('[DB] No active savefile or user');
            return false;
        }

        try {
            const saveData = this._convertStatsToDb(stats);

            const response = await fetch(
                `${DatabaseService.SUPABASE_URL}/Savefiles?id=eq.${this.currentSavefileId}`,
                {
                    method: 'PATCH',
                    headers: this._getHeaders(),
                    body: JSON.stringify(saveData)
                }
            );

            if (response.ok) {
                console.log(`[DB] Saved savefile: ${this.currentSavefileId}`);
                return true;
            } else {
                const error = await response.text();
                console.error('[DB] Save failed:', error);
                return false;
            }
        } catch (error) {
            console.error('[DB] Save error:', error);
            return false;
        }
    }

    async deleteSavefile(savefileId) {
        try {
            const response = await fetch(
                `${DatabaseService.SUPABASE_URL}/Savefiles?id=eq.${savefileId}`,
                {
                    method: 'DELETE',
                    headers: this._getHeaders()
                }
            );

            if (response.ok) {
                console.log(`[DB] Deleted savefile: ${savefileId}`);
                if (this.currentSavefileId === savefileId) {
                    this.currentSavefileId = null;
                }
                return true;
            }
            return false;
        } catch (error) {
            console.error('[DB] Delete error:', error);
            return false;
        }
    }

    // ========================================
    // Leaderboard
    // ========================================

    async getLeaderboard(category = 0) {
        try {
            const fieldMap = {
                0: 'total_money_earned',
                1: 'best_rayan_rarity',
                2: 'total_rolls_altime',
                3: 'gems',
                4: 'total_playtime_minutes',
                5: 'inventory'
            };

            const field = fieldMap[category] || 'total_money_earned';

            const response = await fetch(
                `${DatabaseService.SUPABASE_URL}/Savefiles?` +
                `select=id,user_id,${field},best_rayan_ever_name,best_rayan_rarity,inventory` +
                `&admin_used=eq.false&order=${field}.desc&limit=50`,
                { headers: this._getHeaders() }
            );

            if (!response.ok) return [];

            const savefiles = await response.json();
            if (!savefiles || savefiles.length === 0) return [];

            // Get usernames
            const userIds = [...new Set(savefiles.map(s => s.user_id))];
            const usernames = await this._getUsernames(userIds);

            // Build leaderboard entries
            const entries = [];
            for (const savefile of savefiles) {
                const username = usernames[savefile.user_id] || 'Unknown';
                const entry = { username, value: '', rawValue: 0 };

                switch (category) {
                    case 0: // Money
                        const money = BigInt(savefile.total_money_earned || '0');
                        entry.value = `?? ${this._formatBigNumber(money)} Money`;
                        entry.rawValue = Number(money > BigInt(Number.MAX_SAFE_INTEGER) ? Number.MAX_SAFE_INTEGER : money);
                        break;
                    case 1: // Rarest Rayan
                        const rarity = parseFloat(savefile.best_rayan_rarity || '0');
                        const name = savefile.best_rayan_ever_name || 'Unknown';
                        entry.value = `? ${name} (1 in ${rarity.toLocaleString()})`;
                        entry.rawValue = rarity;
                        break;
                    case 2: // Rolls
                        const rolls = savefile.total_rolls_altime || 0;
                        entry.value = `?? ${rolls.toLocaleString()} Rolls`;
                        entry.rawValue = rolls;
                        break;
                    case 3: // Gems
                        const gems = savefile.gems || 0;
                        entry.value = `?? ${gems.toLocaleString()} Gems`;
                        entry.rawValue = gems;
                        break;
                    case 4: // PlayTime
                        const minutes = savefile.total_playtime_minutes || 0;
                        entry.value = `?? ${this._formatPlayTime(minutes)}`;
                        entry.rawValue = minutes;
                        break;
                    case 5: // Rayans
                        try {
                            const inventory = JSON.parse(savefile.inventory || '[]');
                            entry.value = `?? ${inventory.length.toLocaleString()} Rayans`;
                            entry.rawValue = inventory.length;
                        } catch {
                            entry.value = '?? 0 Rayans';
                            entry.rawValue = 0;
                        }
                        break;
                }

                entries.push(entry);
            }

            return entries.sort((a, b) => b.rawValue - a.rawValue);
        } catch (error) {
            console.error('[DB] Leaderboard error:', error);
            return [];
        }
    }

    async _getUsernames(userIds) {
        if (userIds.length === 0) return {};

        try {
            const idsParam = userIds.join(',');
            const response = await fetch(
                `${DatabaseService.SUPABASE_URL}/User?select=id,username&id=in.(${idsParam})`,
                { headers: this._getHeaders() }
            );

            if (!response.ok) return {};

            const users = await response.json();
            const result = {};
            users.forEach(u => {
                result[u.id] = u.username;
            });
            return result;
        } catch (error) {
            console.error('[DB] Get usernames error:', error);
            return {};
        }
    }

    // ========================================
    // Helper Methods
    // ========================================

    markAdminUsed() {
        this.adminUsedThisSession = true;
    }

    setCurrentSavefile(savefileId, userId) {
        this.currentSavefileId = savefileId;
        this.currentUserId = userId;
    }

    _getHeaders() {
        return {
            'apikey': DatabaseService.SUPABASE_KEY,
            'Authorization': `Bearer ${DatabaseService.SUPABASE_KEY}`,
            'Content-Type': 'application/json',
            'Prefer': 'return=representation'
        };
    }

    _encryptPassword(password) {
        // Simple XOR encryption (same as C# version)
        const key = DatabaseService.ENCRYPTION_KEY;
        const encrypted = [];
        for (let i = 0; i < password.length; i++) {
            encrypted.push(password.charCodeAt(i) ^ key.charCodeAt(i % key.length));
        }
        return btoa(String.fromCharCode(...encrypted));
    }

    _decryptPassword(encrypted) {
        try {
            const key = DatabaseService.ENCRYPTION_KEY;
            const decoded = atob(encrypted);
            const decrypted = [];
            for (let i = 0; i < decoded.length; i++) {
                decrypted.push(String.fromCharCode(decoded.charCodeAt(i) ^ key.charCodeAt(i % key.length)));
            }
            return decrypted.join('');
        } catch {
            return '';
        }
    }

    _convertStatsToDb(stats) {
        return {
            last_played: new Date().toISOString(),
            admin_used: this.adminUsedThisSession,
            created_in_version: DatabaseService.GAME_VERSION,
            money: stats.money.toString(),
            total_money_earned: stats.totalMoneyEarned.toString(),
            gems: stats.gems,
            rebirths: stats.rebirths,
            plot_slots: stats.plotSlots,
            roll_colldown: stats.rollCooldown.toString(),
            roll_coldown_level: stats.rollCooldownLevel,
            total_rolls_altime: stats.totalRollsAllTime,
            autoroll_unlocked: stats.autoRollUnlocked,
            autoroll_active: stats.autoRollActive,
            playtime_minutes: stats.playTimeMinutes,
            luckbooster_level: stats.luckBoosterLevel,
            total_rebirths: stats.totalRebirthsAllTime,
            total_playtime_minutes: stats.totalPlayTimeMinutes,
            best_rayan_ever_name: stats.bestRayanEverName || '',
            best_rayan_rarity: stats.bestRayanEverRarity.toString(),
            best_rayan_value: stats.bestRayanEverValue.toString(),
            inventory: JSON.stringify(stats.inventory.map(r => r.toObject())),
            equipped_rayan_indices: JSON.stringify(stats.equippedRayanIndices),
            owned_dice: JSON.stringify(stats.ownedDices.map(d => d.toObject())),
            selected_dice_index: stats.selectedDiceIndex,
            saved_quests: JSON.stringify(stats.savedQuests.map(q => q.toObject()))
        };
    }

    _convertDbToStats(data) {
        const stats = new PlayerStats();
        stats.money = BigInt(data.money || '0');
        stats.totalMoneyEarned = BigInt(data.total_money_earned || '0');
        stats.gems = data.gems || 0;
        stats.rebirths = data.rebirths || 0;
        stats.plotSlots = data.plot_slots || 3;
        stats.rollCooldown = parseFloat(data.roll_colldown || '2');
        stats.rollCooldownLevel = data.roll_coldown_level || 0;
        stats.totalRollsAllTime = data.total_rolls_altime || 0;
        stats.autoRollUnlocked = data.autoroll_unlocked || false;
        stats.autoRollActive = data.autoroll_active || false;
        stats.playTimeMinutes = data.playtime_minutes || 0;
        stats.luckBoosterLevel = data.luckbooster_level || 0;
        stats.totalRebirthsAllTime = data.total_rebirths || 0;
        stats.totalPlayTimeMinutes = data.total_playtime_minutes || 0;
        stats.bestRayanEverName = data.best_rayan_ever_name || '';
        stats.bestRayanEverRarity = parseFloat(data.best_rayan_rarity || '0');
        stats.bestRayanEverValue = BigInt(data.best_rayan_value || '0');
        stats.selectedDiceIndex = data.selected_dice_index || 0;

        // Deserialize arrays - Convert C# PascalCase to JS camelCase
        try {
            const rayans = JSON.parse(data.inventory || '[]');
            stats.inventory = rayans.map(obj => {
                // Convert C# property names to JS property names
                return Rayan.fromObject({
                    prefix: obj.Prefix || obj.prefix || '',
                    suffix: obj.Suffix || obj.suffix || '',
                    rarity: obj.Rarity || obj.rarity || 1,
                    baseValue: obj.BaseValue || obj.baseValue || '0',
                    multiplier: obj.Multiplier || obj.multiplier || 1.0
                });
            });
            console.log('[DB] Loaded inventory:', stats.inventory.length, 'rayans');
        } catch (e) {
            console.error('[DB] Error parsing inventory:', e);
            stats.inventory = [];
        }

        try {
            stats.equippedRayanIndices = JSON.parse(data.equipped_rayan_indices || '[]');
        } catch (e) {
            console.error('[DB] Error parsing equipped indices:', e);
            stats.equippedRayanIndices = [];
        }

        try {
            const dices = JSON.parse(data.owned_dice || '[]');
            stats.ownedDices = dices.map(obj => {
                // Convert C# property names to JS property names
                return Dice.fromObject({
                    name: obj.Name || obj.name || 'Basic Dice',
                    luckMultiplier: obj.LuckMultiplier || obj.luckMultiplier || 1.0,
                    cost: obj.Cost || obj.cost || '0',
                    quantity: obj.Quantity || obj.quantity || '0',
                    isInfinite: obj.IsInfinite !== undefined ? obj.IsInfinite : (obj.isInfinite || false)
                });
            });
            console.log('[DB] Loaded dices:', stats.ownedDices.length, 'dices');
        } catch (e) {
            console.error('[DB] Error parsing owned dices:', e);
            stats.ownedDices = [new Dice("Basic Dice", 1.0, BigInt(0), BigInt(0), true)];
        }

        try {
            // Quests need special handling - we need to match with QUEST_DEFINITIONS
            const savedQuests = JSON.parse(data.saved_quests || '[]');
            stats.savedQuests = savedQuests.map(obj => {
                // Find the quest definition by ID (handle both C# and JS property names)
                const questId = obj.Id || obj.id;
                const definition = QUEST_DEFINITIONS.find(q => q.id === questId);
                if (definition) {
                    // Convert C# property names to JS property names
                    return Quest.fromObject({
                        id: questId,
                        currentProgress: obj.CurrentProgress || obj.currentProgress || 0,
                        isCompleted: obj.IsCompleted !== undefined ? obj.IsCompleted : (obj.isCompleted || false),
                        isClaimed: obj.IsClaimed !== undefined ? obj.IsClaimed : (obj.isClaimed || false),
                        timesCompleted: obj.TimesCompleted || obj.timesCompleted || 0,
                        baseProgress: obj.BaseProgress || obj.baseProgress || 0,
                        goal: obj.Goal || obj.goal || definition.goal
                    }, definition);
                } else {
                    console.warn('[DB] Quest definition not found for ID:', questId);
                    return null;
                }
            }).filter(q => q !== null); // Remove null entries
            console.log('[DB] Loaded quests:', stats.savedQuests.length, 'quests');
        } catch (e) {
            console.error('[DB] Error parsing quests:', e);
            stats.savedQuests = [];
        }

        return stats;
    }

    _formatBigNumber(value) {
        if (value < 1000n) return value.toString();
        if (value < 1000000n) return (Number(value) / 1000).toFixed(1) + 'K';
        if (value < 1000000000n) return (Number(value) / 1000000).toFixed(1) + 'M';
        if (value < 1000000000000n) return (Number(value) / 1000000000).toFixed(1) + 'B';
        if (value < 1000000000000000n) return (Number(value) / 1000000000000).toFixed(1) + 'T';
        return value.toString();
    }

    _formatPlayTime(minutes) {
        if (minutes < 60) return `${minutes.toFixed(0)} Minuten`;
        const hours = minutes / 60;
        if (hours < 24) return `${hours.toFixed(1)} Stunden`;
        const days = hours / 24;
        return `${days.toFixed(1)} Tage`;
    }
}
