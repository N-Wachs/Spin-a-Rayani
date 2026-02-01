/**
 * Spin a Rayan - Web Version
 * Game Data (Prefixes, Suffixes, Dices)
 */

// ========================================
// Suffixes with their chances and multipliers
// ========================================
const SUFFIXES = [
    // Tier 1: Common
    { suffix: "Selbstbewusst", chance: 25, multiplier: 1.5 },
    { suffix: "GC", chance: 50, multiplier: 2.0 },
    { suffix: "Blessed", chance: 75, multiplier: 2.2 },
    { suffix: "Shiny", chance: 100, multiplier: 2.5 },
    { suffix: "Cursed", chance: 150, multiplier: 2.8 },

    // Tier 2: Uncommon
    { suffix: "SSL", chance: 200, multiplier: 3.0 },
    { suffix: "Radiant", chance: 300, multiplier: 3.5 },
    { suffix: "Shadow", chance: 400, multiplier: 4.0 },
    { suffix: "Golden", chance: 500, multiplier: 5.0 },
    { suffix: "Mystic", chance: 600, multiplier: 6.0 },

    // Tier 3: Rare
    { suffix: "Cosmic", chance: 800, multiplier: 8.0 },
    { suffix: "Void", chance: 1000, multiplier: 10.0 },
    { suffix: "Divine", chance: 1500, multiplier: 12.0 },
    { suffix: "Infernal", chance: 2000, multiplier: 15.0 },

    // Tier 4: Epic
    { suffix: "Primordial", chance: 3000, multiplier: 18.0 },
    { suffix: "Ancient", chance: 5000, multiplier: 20.0 },
    { suffix: "Transcendent", chance: 7500, multiplier: 25.0 },

    // Tier 5: Legendary
    { suffix: "Legendary", chance: 10000, multiplier: 30.0 },
    { suffix: "Eternal", chance: 50000, multiplier: 100.0 },
    { suffix: "Omega", chance: 100000, multiplier: 200.0 },

    // Tier 6: Ultra-Legendary
    { suffix: "Unstoppable", chance: 200000, multiplier: 250.0 },
    { suffix: "Infinite", chance: 500000, multiplier: 500.0 },
    { suffix: "Absolute", chance: 1000000, multiplier: 1000.0 }
];

// ========================================
// Prefixes (Rayan types) with their rarities and base values
// ========================================
const PREFIXES = [];

function addRayan(name, rarity, baseValue) {
    PREFIXES.push({ prefix: name, rarity: rarity, baseValue: BigInt(baseValue) });
}

function generateRayans() {
    // === TIER 1-2: Common & Uncommon (Rarity 1-50) ===
    addRayan("Rat", 1, 1);
    addRayan("Bee", 2, 2);
    addRayan("Cat", 3, 3);
    addRayan("Dog", 4, 5);
    addRayan("Frog", 5, 7);
    addRayan("Bird", 6, 10);
    addRayan("Snake", 7, 12);
    addRayan("Rabbit", 8, 15);
    addRayan("Turtle", 9, 18);
    addRayan("Fox", 10, 22);

    // === TIER 3: Uncommon+ (Rarity 50-200) ===
    addRayan("Wolf", 15, 30);
    addRayan("Bear", 18, 40);
    addRayan("Eagle", 22, 50);
    addRayan("Lion", 25, 65);
    addRayan("Tiger", 30, 80);
    addRayan("Panther", 35, 100);
    addRayan("Hawk", 40, 120);
    addRayan("Owl", 45, 150);
    addRayan("Raven", 50, 180);
    addRayan("Falcon", 60, 220);

    // === TIER 4: Rare (Rarity 200-1000) ===
    addRayan("Shark", 80, 300);
    addRayan("Whale", 100, 400);
    addRayan("Dolphin", 120, 500);
    addRayan("Octopus", 150, 650);
    addRayan("Kraken", 180, 800);
    addRayan("Hydra", 220, 1000);
    addRayan("Griffin", 270, 1300);
    addRayan("Pegasus", 320, 1600);
    addRayan("Unicorn", 380, 2000);
    addRayan("Basilisk", 450, 2500);

    // === TIER 5: Epic (Rarity 1K-10K) ===
    addRayan("Phoenix", 550, 3200);
    addRayan("Dragon", 700, 4000);
    addRayan("Wyvern", 850, 5000);
    addRayan("Chimera", 1000, 6500);
    addRayan("Cerberus", 1200, 8000);
    addRayan("Leviathan", 1500, 10000);
    addRayan("Behemoth", 1800, 13000);
    addRayan("Fenrir", 2200, 16000);
    addRayan("Jormungandr", 2700, 20000);
    addRayan("Tiamat", 3300, 25000);

    // === TIER 6: Legendary (Rarity 10K-50K) ===
    addRayan("Zeus", 4000, 32000);
    addRayan("Odin", 5000, 40000);
    addRayan("Ra", 6000, 50000);
    addRayan("Thor", 7500, 65000);
    addRayan("Anubis", 9000, 80000);
    addRayan("Hades", 11000, 100000);
    addRayan("Poseidon", 13500, 130000);
    addRayan("Athena", 16500, 160000);
    addRayan("Apollo", 20000, 200000);
    addRayan("Artemis", 25000, 250000);

    // === TIER 7: Mythic (Rarity 50K-200K) ===
    addRayan("Amaterasu", 30000, 320000);
    addRayan("Susanoo", 37000, 400000);
    addRayan("Bahamut", 45000, 500000);
    addRayan("Quetzalcoatl", 55000, 650000);
    addRayan("Osiris", 67000, 800000);
    addRayan("Vishnu", 82000, 1000000);
    addRayan("Shiva", 100000, 1300000);
    addRayan("Brahma", 120000, 1600000);
    addRayan("Izanagi", 145000, 2000000);
    addRayan("Itzamna", 175000, 2500000);

    // === TIER 8-10: Divine & Beyond (Rarity 200K+) ===
    addRayan("Chronos", 210000, 3200000);
    addRayan("Gaia", 255000, 4000000);
    addRayan("Uranus", 310000, 5000000);
    addRayan("Nyx", 380000, 6500000);
    addRayan("Erebus", 460000, 8000000);
    addRayan("Tartarus", 560000, 10000000);
    addRayan("Aether", 680000, 13000000);
    addRayan("Chaos", 830000, 16000000);
    addRayan("Nemesis", 1000000, 20000000);
    addRayan("Thanatos", 1200000, 25000000);

    // === TIER 11: Transcendent (Rarity 1.2M-8M, BaseValue 25M-250M) ===
    addRayan("Ananke", 1450000, 32000000);
    addRayan("Hemera", 1750000, 40000000);
    addRayan("Hypnos", 2100000, 50000000);
    addRayan("Morpheus", 2550000, 65000000);
    addRayan("Eros", 3100000, 80000000);
    addRayan("Persephone", 3750000, 100000000);
    addRayan("Demeter", 4500000, 130000000);
    addRayan("Hestia", 5500000, 160000000);
    addRayan("Hera", 6700000, 200000000);
    addRayan("Ares", 8200000, 250000000);

    // === TIER 12: Primordial (Rarity 8M-60M, BaseValue 250M-2.5B) ===
    addRayan("Pontus", 10000000, 320000000);
    addRayan("Oceanus", 12200000, 400000000);
    addRayan("Hyperion", 14850000, 500000000);
    addRayan("Theia", 18100000, 650000000);
    addRayan("Rhea", 22000000, 800000000);
    addRayan("Themis", 27000000, 1000000000);
    addRayan("Mnemosyne", 32800000, 1300000000);
    addRayan("Phoebe", 40000000, 1600000000);
    addRayan("Tethys", 49000000, 2000000000);
    addRayan("Coeus", 60000000, 2500000000);

    // === TIER 13: Cosmic (Rarity 60M-430M, BaseValue 2.5B-25B) ===
    addRayan("Crius", 73000000, 3200000000);
    addRayan("Iapetus", 89000000, 4000000000);
    addRayan("Cronus", 108000000, 5000000000);
    addRayan("Prometheus", 132000000, 6500000000);
    addRayan("Atlas", 160000000, 8000000000);
    addRayan("Epimetheus", 195000000, 10000000000);
    addRayan("Menoetius", 237000000, 13000000000);
    addRayan("Asteria", 290000000, 16000000000);
    addRayan("Leto", 353000000, 20000000000);
    addRayan("Hecate", 430000000, 25000000000);

    // === TIER 14: Universal (Rarity 430M-3B, BaseValue 25B-250B) ===
    addRayan("Selene", 523000000, 32000000000);
    addRayan("Helios", 637000000, 40000000000);
    addRayan("Eos", 776000000, 50000000000);
    addRayan("Astraeus", 945000000, 65000000000);
    addRayan("Pallas", 1150000000, 80000000000);
    addRayan("Styx", 1400000000, 100000000000);
    addRayan("Nike", 1700000000, 130000000000);
    addRayan("Kratos", 2070000000, 160000000000);
    addRayan("Bia", 2520000000, 200000000000);
    addRayan("Zelus", 3070000000, 250000000000);

    // === TIER 15: Infinite (Rarity 3B-22B, BaseValue 250B-2.5T) ===
    addRayan("Metis", 3740000000, 320000000000);
    addRayan("Eileithyia", 4550000000, 400000000000);
    addRayan("Hebe", 5540000000, 500000000000);
    addRayan("Hephaestus", 6750000000, 650000000000);
    addRayan("Hermes", 8220000000, 800000000000);
    addRayan("Dionysus", 10000000000, 1000000000000);
    addRayan("Heracles", 12200000000, 1300000000000);
    addRayan("Perseus", 14850000000, 1600000000000);
    addRayan("Achilles", 18100000000, 2000000000000);
    addRayan("Odysseus", 22000000000, 2500000000000);

    // === TIER 16: Eternal (Rarity 22B-160B, BaseValue 2.5T-25T) ===
    addRayan("Theseus", 26800000000, 3200000000000);
    addRayan("Jason", 32600000000, 4000000000000);
    addRayan("Bellerophon", 39700000000, 5000000000000);
    addRayan("Orpheus", 48300000000, 6500000000000);
    addRayan("Asclepius", 58800000000, 8000000000000);
    addRayan("Castor", 71600000000, 10000000000000);
    addRayan("Pollux", 87200000000, 13000000000000);
    addRayan("Atalanta", 106200000000, 16000000000000);
    addRayan("Medea", 129300000000, 20000000000000);
    addRayan("Circe", 157500000000, 25000000000000);

    // === TIER 17: Absolute (Rarity 160B-514B, BaseValue 25T-100T) ===
    addRayan("Calypso", 191800000000, 32000000000000);
    addRayan("Psyche", 233600000000, 40000000000000);
    addRayan("Pandora", 284500000000, 50000000000000);
    addRayan("Arachne", 346400000000, 65000000000000);
    addRayan("Medusa", 421800000000, 80000000000000);
    addRayan("Scylla", 513600000000, 100000000000000);

    // === Elements (80-100) ===
    addRayan("Ember", 70, 250);
    addRayan("Blaze", 180, 750);
    addRayan("Inferno", 800, 4500);
    addRayan("Pyre", 3500, 18000);
    addRayan("Frost", 90, 350);
    addRayan("Glacier", 240, 950);
    addRayan("Blizzard", 1100, 5500);
    addRayan("Tundra", 4500, 22000);
    addRayan("Spark", 110, 450);
    addRayan("Bolt", 300, 1200);
    addRayan("Thunder", 1400, 7000);
    addRayan("Storm", 5500, 28000);

    // === Gems & Minerals (100-120) ===
    addRayan("Quartz", 160, 600);
    addRayan("Topaz", 400, 1800);
    addRayan("Ruby", 1700, 8500);
    addRayan("Sapphire", 6500, 32000);
    addRayan("Emerald", 7800, 38000);
    addRayan("Diamond", 9500, 47000);
    addRayan("Opal", 11500, 57000);
    addRayan("Obsidian", 14000, 70000);
    addRayan("Onyx", 17000, 85000);
    addRayan("Pearl", 21000, 105000);

    // Generate remaining combined Rayans
    generateCombinedRayans();
}

function generateCombinedRayans() {
    const adjectives = [
        "Swift", "Mighty", "Ancient", "Eternal", "Cosmic", "Astral", "Celestial",
        "Divine", "Infernal", "Frozen", "Burning", "Storm", "Shadow", "Light",
        "Dark", "Crimson", "Azure", "Golden", "Silver", "Platinum", "Crystal",
        "Phantom", "Spectral", "Ethereal", "Arcane", "Mystic", "Sacred", "Cursed",
        "Blessed", "Radiant", "Void", "Chaos", "Primal", "Elder", "Forgotten",
        "Lost", "Hidden", "Secret", "Royal", "Imperial", "Noble", "Savage",
        "Wild", "Feral", "Tamed", "Broken", "Shattered", "Forged", "Tempered"
    ];

    const nouns = [
        "Warrior", "Sage", "Knight", "Mage", "Priest", "Rogue", "Assassin",
        "Guardian", "Sentinel", "Protector", "Destroyer", "Creator", "Reaper",
        "Slayer", "Hunter", "Seeker", "Wanderer", "Traveler", "Explorer",
        "Champion", "Hero", "Legend", "Titan", "Giant", "Colossus", "Golem",
        "Wraith", "Specter", "Spirit", "Soul", "Essence", "Entity", "Being",
        "Force", "Power", "Energy", "Aura", "Presence", "Manifestation",
        "Avatar", "Vessel", "Herald", "Harbinger", "Omen", "Oracle", "Prophet",
        "Seer", "Shaman", "Druid", "Monk", "Paladin", "Warlock", "Sorcerer"
    ];

    const currentIndex = PREFIXES.length;
    const targetCount = 500;

    // Start from Scylla's rarity
    let baseRarity = 513600000000;
    let baseValue = BigInt("100000000000000");

    // Seeded random for consistency
    let seed = 42;
    const seededRandom = () => {
        seed = (seed * 1103515245 + 12345) & 0x7fffffff;
        return seed / 0x7fffffff;
    };

    const existingNames = new Set(PREFIXES.map(p => p.prefix));

    while (PREFIXES.length < targetCount) {
        const adj = adjectives[Math.floor(seededRandom() * adjectives.length)];
        const noun = nouns[Math.floor(seededRandom() * nouns.length)];
        const combinedName = `${adj} ${noun}`;

        if (existingNames.has(combinedName)) continue;

        existingNames.add(combinedName);

        const tier = PREFIXES.length - currentIndex;
        const rarity = baseRarity * Math.pow(1.2, tier + 1);
        const value = baseValue * BigInt(Math.pow(10, Math.floor((tier + 3) / 3)));

        PREFIXES.push({ prefix: combinedName, rarity: rarity, baseValue: value });
    }
}

// ========================================
// Dice Templates for Shop
// ========================================
const DICE_TEMPLATES = [
    // Tier 1: Starter
    { name: "Silver Dice", luckMultiplier: 1.5, cost: BigInt(7500) },
    { name: "Bronze Dice", luckMultiplier: 1.8, cost: BigInt(15000) },

    // Tier 2: Common
    { name: "Golden Dice", luckMultiplier: 2.0, cost: BigInt(60000) },
    { name: "Copper Dice", luckMultiplier: 2.3, cost: BigInt(120000) },
    { name: "Iron Dice", luckMultiplier: 2.6, cost: BigInt(225000) },

    // Tier 3: Uncommon
    { name: "Diamond Dice", luckMultiplier: 3.0, cost: BigInt(300000) },
    { name: "Ruby Dice", luckMultiplier: 3.5, cost: BigInt(600000) },
    { name: "Sapphire Dice", luckMultiplier: 4.0, cost: BigInt(1050000) },

    // Tier 4: Rare
    { name: "Platinum Dice", luckMultiplier: 5.0, cost: BigInt(1500000) },
    { name: "Obsidian Dice", luckMultiplier: 6.0, cost: BigInt(2400000) },
    { name: "Jade Dice", luckMultiplier: 7.0, cost: BigInt(3600000) },

    // Tier 5: Epic
    { name: "Emerald Dice", luckMultiplier: 10.0, cost: BigInt(6000000) },
    { name: "Amethyst Dice", luckMultiplier: 12.0, cost: BigInt(10500000) },
    { name: "Crystal Dice", luckMultiplier: 15.0, cost: BigInt(18000000) },

    // Tier 6: Legendary
    { name: "Celestial Dice", luckMultiplier: 20.0, cost: BigInt(30000000) },
    { name: "Divine Dice", luckMultiplier: 30.0, cost: BigInt(75000000) },
    { name: "Cosmic Dice", luckMultiplier: 50.0, cost: BigInt(300000000) },

    // Tier 7: Mythic
    { name: "Quantum Dice", luckMultiplier: 75.0, cost: BigInt(750000000) },
    { name: "Void Dice", luckMultiplier: 100.0, cost: BigInt(1500000000) },
    { name: "Ethereal Dice", luckMultiplier: 150.0, cost: BigInt(3000000000) },
    { name: "Astral Dice", luckMultiplier: 200.0, cost: BigInt(7500000000) },
    { name: "Primordial Dice", luckMultiplier: 300.0, cost: BigInt(15000000000) },

    // Tier 8: Transcendent
    { name: "Eternal Dice", luckMultiplier: 500.0, cost: BigInt(30000000000) },
    { name: "Infinite Dice", luckMultiplier: 750.0, cost: BigInt(75000000000) },
    { name: "Absolute Dice", luckMultiplier: 1000.0, cost: BigInt(150000000000) },
    { name: "Omnipotent Dice", luckMultiplier: 1500.0, cost: BigInt(300000000000) },
    { name: "Godlike Dice", luckMultiplier: 2500.0, cost: BigInt(750000000000) },

    // Tier 9: Ultimate
    { name: "Alpha Dice", luckMultiplier: 5000.0, cost: BigInt("3000000000000") },
    { name: "Omega Dice", luckMultiplier: 10000.0, cost: BigInt("15000000000000") },
    { name: "Zenith Dice", luckMultiplier: 25000.0, cost: BigInt("75000000000000") },
    { name: "Apex Dice", luckMultiplier: 50000.0, cost: BigInt("300000000000000") },
    { name: "Supreme Dice", luckMultiplier: 100000.0, cost: BigInt("3000000000000000") }
];

// ========================================
// Quest Definitions
// ========================================
const QUEST_DEFINITIONS = [
    // Rolling Quests (repeatable)
    {
        id: "roll_100",
        description: "Rolle 100 mal",
        goal: 100,
        initialGoal: 100,
        goalIncrement: 50,
        rewardGems: 100,
        isRepeatable: true,
        type: "rolls"
    },
    {
        id: "roll_1000",
        description: "Rolle 1000 mal",
        goal: 1000,
        initialGoal: 1000,
        goalIncrement: 500,
        rewardGems: 1200,
        isRepeatable: true,
        type: "rolls"
    },
    {
        id: "roll_10000",
        description: "Rolle 10.000 mal",
        goal: 10000,
        initialGoal: 10000,
        goalIncrement: 5000,
        rewardGems: 5000,
        isRepeatable: true,
        type: "rolls"
    },
    // Time-based Quests (not repeatable)
    {
        id: "play_30min",
        description: "Spiele 30 Minuten",
        goal: 30,
        initialGoal: 30,
        goalIncrement: 0,
        rewardGems: 2000,
        isRepeatable: false,
        type: "playtime"
    },
    {
        id: "play_120min",
        description: "Spiele 2 Stunden",
        goal: 120,
        initialGoal: 120,
        goalIncrement: 0,
        rewardGems: 3500,
        isRepeatable: false,
        type: "playtime"
    },
    // Rebirth Quests (not repeatable)
    {
        id: "rebirth_5",
        description: "Rebirthe 5 mal",
        goal: 5,
        initialGoal: 5,
        goalIncrement: 0,
        rewardGems: 500,
        isRepeatable: false,
        type: "rebirths"
    },
    {
        id: "rebirth_25",
        description: "Rebirthe 25 mal",
        goal: 25,
        initialGoal: 25,
        goalIncrement: 0,
        rewardGems: 2500,
        isRepeatable: false,
        type: "rebirths"
    }
];

// Initialize Rayans on load
generateRayans();

// Sort prefixes by rarity (descending) for roll optimization
const SORTED_PREFIXES = [...PREFIXES].sort((a, b) => b.rarity - a.rarity);
const SORTED_SUFFIXES = [...SUFFIXES].sort((a, b) => b.chance - a.chance);
