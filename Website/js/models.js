/**
 * Spin a Rayan - Web Version
 * Game Models
 */

// ========================================
// Rayan Model
// ========================================
class Rayan {
    constructor(prefix = "", suffix = "", rarity = 1, baseValue = BigInt(0), multiplier = 1.0) {
        this.prefix = prefix;
        this.suffix = suffix;
        this.rarity = rarity;
        this.baseValue = baseValue;
        this.multiplier = multiplier;
    }

    get fullName() {
        const prefixPart = this.prefix ? `${this.prefix} ` : "";
        const suffixPart = this.suffix ? ` ${this.suffix}` : "";
        return `${prefixPart}Rayan${suffixPart}`;
    }

    get totalValue() {
        return BigInt(Math.floor(Number(this.baseValue) * this.multiplier));
    }

    // Create from plain object (for loading from localStorage)
    static fromObject(obj) {
        const rayan = new Rayan();
        rayan.prefix = obj.prefix || "";
        rayan.suffix = obj.suffix || "";
        rayan.rarity = obj.rarity || 1;
        rayan.baseValue = BigInt(obj.baseValue || "0");
        rayan.multiplier = obj.multiplier || 1.0;
        return rayan;
    }

    // Convert to plain object for saving
    toObject() {
        return {
            prefix: this.prefix,
            suffix: this.suffix,
            rarity: this.rarity,
            baseValue: this.baseValue.toString(),
            multiplier: this.multiplier
        };
    }
}

// ========================================
// Dice Model
// ========================================
class Dice {
    constructor(name = "Basic Dice", luckMultiplier = 1.0, cost = BigInt(0), quantity = BigInt(0), isInfinite = false) {
        this.name = name;
        this.luckMultiplier = luckMultiplier;
        this.cost = cost;
        this.quantity = quantity;
        this.isInfinite = isInfinite;
    }

    get description() {
        if (this.isInfinite) return "Immer verfügbar";
        return `+${((this.luckMultiplier - 1.0) * 100).toFixed(0)}% Luck`;
    }

    get displayName() {
        return this.isInfinite ? this.name : `🎲 ${this.name}`;
    }

    get quantityDisplay() {
        if (this.isInfinite) return "∞";
        return formatBigInt(this.quantity);
    }

    static fromObject(obj) {
        const dice = new Dice();
        dice.name = obj.name || "Basic Dice";
        dice.luckMultiplier = obj.luckMultiplier || 1.0;
        dice.cost = BigInt(obj.cost || "0");
        dice.quantity = BigInt(obj.quantity || "0");
        dice.isInfinite = obj.isInfinite || false;
        return dice;
    }

    toObject() {
        return {
            name: this.name,
            luckMultiplier: this.luckMultiplier,
            cost: this.cost.toString(),
            quantity: this.quantity.toString(),
            isInfinite: this.isInfinite
        };
    }
}

// ========================================
// Quest Model
// ========================================
class Quest {
    constructor(definition) {
        this.id = definition.id;
        this.description = definition.description;
        this.goal = definition.goal;
        this.initialGoal = definition.initialGoal;
        this.goalIncrement = definition.goalIncrement;
        this.rewardGems = definition.rewardGems;
        this.isRepeatable = definition.isRepeatable;
        this.type = definition.type;
        
        this.currentProgress = 0;
        this.isCompleted = false;
        this.isClaimed = false;
        this.timesCompleted = 0;
        this.baseProgress = 0;
    }

    get progressPercentage() {
        return Math.min(100, (this.currentProgress / this.goal) * 100);
    }

    reset() {
        this.baseProgress += this.goal;
        this.currentProgress = 0;
        this.isCompleted = false;
        this.isClaimed = false;
        this.timesCompleted++;

        if (this.goalIncrement > 0) {
            this.goal += this.goalIncrement;
        }
    }

    static fromObject(obj, definition) {
        const quest = new Quest(definition);
        quest.currentProgress = obj.currentProgress || 0;
        quest.isCompleted = obj.isCompleted || false;
        quest.isClaimed = obj.isClaimed || false;
        quest.timesCompleted = obj.timesCompleted || 0;
        quest.baseProgress = obj.baseProgress || 0;
        quest.goal = obj.goal || definition.goal;
        return quest;
    }

    toObject() {
        return {
            id: this.id,
            currentProgress: this.currentProgress,
            isCompleted: this.isCompleted,
            isClaimed: this.isClaimed,
            timesCompleted: this.timesCompleted,
            baseProgress: this.baseProgress,
            goal: this.goal
        };
    }
}

// ========================================
// SuffixEvent Model
// ========================================
class SuffixEvent {
    constructor(suffixName, eventName, durationMinutes = 2.5, boostMultiplier = 20.0) {
        this.eventId = Date.now();
        this.suffixName = suffixName;
        this.eventName = eventName;
        this.startTime = new Date();
        this.endTime = new Date(this.startTime.getTime() + durationMinutes * 60 * 1000);
        this.boostMultiplier = boostMultiplier;
        this.luckMultiplier = 1.0;
        this.moneyMultiplier = 1.0;
        this.rollTimeModifier = 1.0;
    }

    get isActive() {
        const now = new Date();
        return now >= this.startTime && now < this.endTime;
    }

    get timeRemaining() {
        if (!this.isActive) return { minutes: 0, seconds: 0 };
        const remaining = this.endTime.getTime() - Date.now();
        return {
            minutes: Math.floor(remaining / 60000),
            seconds: Math.floor((remaining % 60000) / 1000)
        };
    }

    get displayText() {
        const time = this.timeRemaining;
        return `${this.suffixName} Event! - ${time.minutes}:${time.seconds.toString().padStart(2, '0')}`;
    }
}

// ========================================
// PlayerStats Model
// ========================================
class PlayerStats {
    constructor() {
        this.money = BigInt(0);
        this.gems = 0;
        this.totalRolls = 0;
        this.rebirths = 0;
        this.plotSlots = 3;
        this.rollCooldown = 2.0;
        this.rollCooldownLevel = 0;
        this.luckBoosterLevel = 0;
        this.autoRollUnlocked = false;
        this.autoRollActive = false;
        this.playTimeMinutes = 0;

        // All-Time Stats
        this.totalMoneyEarned = BigInt(0);
        this.totalRollsAllTime = 0;
        this.totalRebirthsAllTime = 0;
        this.totalPlayTimeMinutes = 0;

        // Best Rayan Ever
        this.bestRayanEverName = "";
        this.bestRayanEverRarity = 0;
        this.bestRayanEverValue = BigInt(0);

        // Game data
        this.inventory = [];
        this.equippedRayanIndices = [];
        this.ownedDices = [];
        this.selectedDiceIndex = 0;
        this.savedQuests = [];
    }

    get moneyMultiplier() {
        return 1.0 + (this.rebirths * 4.0);
    }

    get luckMultiplier() {
        return 1.0 + (this.luckBoosterLevel * 0.25);
    }

    get currentDiceLuck() {
        if (this.selectedDiceIndex >= 0 && this.selectedDiceIndex < this.ownedDices.length) {
            return this.ownedDices[this.selectedDiceIndex].luckMultiplier;
        }
        return 1.0;
    }

    get nextRebirthCost() {
        return BigInt(8) ** BigInt(this.rebirths) * BigInt(100000);
    }

    getSelectedDice() {
        if (this.selectedDiceIndex >= 0 && this.selectedDiceIndex < this.ownedDices.length) {
            return this.ownedDices[this.selectedDiceIndex];
        }
        // Fallback to Basic Dice
        const basicDice = this.ownedDices.find(d => d.isInfinite);
        if (basicDice) return basicDice;
        return new Dice("Basic Dice", 1.0, BigInt(0), BigInt(0), true);
    }

    static fromObject(obj) {
        const stats = new PlayerStats();
        
        stats.money = BigInt(obj.money || "0");
        stats.gems = obj.gems || 0;
        stats.totalRolls = obj.totalRolls || 0;
        stats.rebirths = obj.rebirths || 0;
        stats.plotSlots = obj.plotSlots || 3;
        stats.rollCooldown = obj.rollCooldown || 2.0;
        stats.rollCooldownLevel = obj.rollCooldownLevel || 0;
        stats.luckBoosterLevel = obj.luckBoosterLevel || 0;
        stats.autoRollUnlocked = obj.autoRollUnlocked || false;
        stats.autoRollActive = obj.autoRollActive || false;
        stats.playTimeMinutes = obj.playTimeMinutes || 0;

        stats.totalMoneyEarned = BigInt(obj.totalMoneyEarned || "0");
        stats.totalRollsAllTime = obj.totalRollsAllTime || 0;
        stats.totalRebirthsAllTime = obj.totalRebirthsAllTime || 0;
        stats.totalPlayTimeMinutes = obj.totalPlayTimeMinutes || 0;

        stats.bestRayanEverName = obj.bestRayanEverName || "";
        stats.bestRayanEverRarity = obj.bestRayanEverRarity || 0;
        stats.bestRayanEverValue = BigInt(obj.bestRayanEverValue || "0");

        stats.inventory = (obj.inventory || []).map(r => Rayan.fromObject(r));
        stats.equippedRayanIndices = obj.equippedRayanIndices || [];
        stats.ownedDices = (obj.ownedDices || []).map(d => Dice.fromObject(d));
        stats.selectedDiceIndex = obj.selectedDiceIndex || 0;
        stats.savedQuests = obj.savedQuests || [];

        return stats;
    }

    toObject() {
        return {
            money: this.money.toString(),
            gems: this.gems,
            totalRolls: this.totalRolls,
            rebirths: this.rebirths,
            plotSlots: this.plotSlots,
            rollCooldown: this.rollCooldown,
            rollCooldownLevel: this.rollCooldownLevel,
            luckBoosterLevel: this.luckBoosterLevel,
            autoRollUnlocked: this.autoRollUnlocked,
            autoRollActive: this.autoRollActive,
            playTimeMinutes: this.playTimeMinutes,

            totalMoneyEarned: this.totalMoneyEarned.toString(),
            totalRollsAllTime: this.totalRollsAllTime,
            totalRebirthsAllTime: this.totalRebirthsAllTime,
            totalPlayTimeMinutes: this.totalPlayTimeMinutes,

            bestRayanEverName: this.bestRayanEverName,
            bestRayanEverRarity: this.bestRayanEverRarity,
            bestRayanEverValue: this.bestRayanEverValue.toString(),

            inventory: this.inventory.map(r => r.toObject()),
            equippedRayanIndices: this.equippedRayanIndices,
            ownedDices: this.ownedDices.map(d => d.toObject()),
            selectedDiceIndex: this.selectedDiceIndex,
            savedQuests: this.savedQuests
        };
    }
}

// ========================================
// Utility Functions
// ========================================
function formatBigInt(value) {
    const num = typeof value === 'bigint' ? value : BigInt(value || 0);
    
    if (num < 1000n) return num.toString();
    if (num < 1000000n) return (Number(num) / 1000).toFixed(1) + "K";
    if (num < 1000000000n) return (Number(num) / 1000000).toFixed(1) + "M";
    if (num < 1000000000000n) return (Number(num) / 1000000000).toFixed(1) + "B";
    if (num < 1000000000000000n) return (Number(num) / 1000000000000).toFixed(1) + "T";
    if (num < 1000000000000000000n) return (Number(num) / 1000000000000000).toFixed(1) + "Q";
    
    // For extremely large numbers
    const str = num.toString();
    const exp = str.length - 1;
    const mantissa = str[0] + "." + str.slice(1, 3);
    return `${mantissa}e${exp}`;
}

function getRarityClass(rarity) {
    if (rarity < 10) return 'rarity-common';
    if (rarity < 100) return 'rarity-uncommon';
    if (rarity < 1000) return 'rarity-rare';
    if (rarity < 10000) return 'rarity-epic';
    if (rarity < 100000) return 'rarity-legendary';
    if (rarity < 1000000) return 'rarity-mythic';
    if (rarity < 10000000) return 'rarity-divine';
    if (rarity < 100000000) return 'rarity-transcendent';
    if (rarity < 1000000000) return 'rarity-cosmic';
    return 'rarity-ultimate';
}

function getRarityName(rarity) {
    if (rarity < 10) return 'Common';
    if (rarity < 100) return 'Uncommon';
    if (rarity < 1000) return 'Rare';
    if (rarity < 10000) return 'Epic';
    if (rarity < 100000) return 'Legendary';
    if (rarity < 1000000) return 'Mythic';
    if (rarity < 10000000) return 'Divine';
    if (rarity < 100000000) return 'Transcendent';
    if (rarity < 1000000000) return 'Cosmic';
    return 'Ultimate';
}

function formatRarity(rarity) {
    if (rarity < 1000) return `1:${rarity.toFixed(0)}`;
    if (rarity < 1000000) return `1:${(rarity / 1000).toFixed(1)}K`;
    if (rarity < 1000000000) return `1:${(rarity / 1000000).toFixed(1)}M`;
    if (rarity < 1000000000000) return `1:${(rarity / 1000000000).toFixed(1)}B`;
    return `1:${(rarity / 1000000000000).toFixed(1)}T`;
}

function formatTime(minutes) {
    const hours = Math.floor(minutes / 60);
    const mins = Math.floor(minutes % 60);
    if (hours > 0) {
        return `${hours}h ${mins}m`;
    }
    return `${mins}m`;
}
