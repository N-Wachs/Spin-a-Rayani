using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpinARayan.Models
{
    public static class RayanData
    {
        public static readonly List<(string Prefix, double Rarity, BigInteger BaseValue)> Prefixes = new();

        public static readonly List<(string Suffix, double Chance, double Multiplier)> Suffixes = new()
        {
            // Tier 1: Common (niedrige Multiplier, häufig)
            ("Selbstbewusst", 25, 1.5),
            ("GC", 50, 2.0),
            ("Blessed", 75, 2.2),
            ("Shiny", 100, 2.5),
            ("Cursed", 150, 2.8),
            
            // Tier 2: Uncommon (mittlere Multiplier)
            ("SSL", 200, 3.0),
            ("Radiant", 300, 3.5),
            ("Shadow", 400, 4.0),
            ("Golden", 500, 5.0),
            ("Mystic", 600, 6.0),
            
            // Tier 3: Rare (hohe Multiplier)
            ("Cosmic", 800, 8.0),
            ("Void", 1000, 10.0),
            ("Divine", 1500, 12.0),
            ("Infernal", 2000, 15.0),
            
            // Tier 4: Epic (sehr hohe Multiplier)
            ("Primordial", 3000, 18.0),
            ("Ancient", 5000, 20.0),
            ("Transcendent", 7500, 25.0),
            
            // Tier 5: Legendary (extreme Multiplier, sehr selten)
            ("Legendary", 10000, 30.0),
            ("Eternal", 50000, 100.0),
            ("Omega", 100000, 200.0),
            
            // Tier 6: Ultra-Legendary (astronomische Multiplier, extrem selten)
            ("Unstoppable", 200000, 250.0),
            ("Infinite", 500000, 500.0),
            ("Absolute", 1000000, 1000.0)
        };

        static RayanData()
        {
            GenerateRayans();
        }

        private static void GenerateRayans()
        {
            // === TIER 1-2: Common & Uncommon (Rarity 1-50) ===
            AddRayan("Rat", 1, 1);
            AddRayan("Bee", 2, 2);
            AddRayan("Cat", 3, 3);
            AddRayan("Dog", 4, 5);
            AddRayan("Frog", 5, 7);
            AddRayan("Bird", 6, 10);
            AddRayan("Snake", 7, 12);
            AddRayan("Rabbit", 8, 15);
            AddRayan("Turtle", 9, 18);
            AddRayan("Fox", 10, 22);
            
            // === TIER 3: Uncommon+ (Rarity 50-200) ===
            AddRayan("Wolf", 15, 30);
            AddRayan("Bear", 18, 40);
            AddRayan("Eagle", 22, 50);
            AddRayan("Lion", 25, 65);
            AddRayan("Tiger", 30, 80);
            AddRayan("Panther", 35, 100);
            AddRayan("Hawk", 40, 120);
            AddRayan("Owl", 45, 150);
            AddRayan("Raven", 50, 180);
            AddRayan("Falcon", 60, 220);
            
            // === TIER 4: Rare (Rarity 200-1000) ===
            AddRayan("Shark", 80, 300);
            AddRayan("Whale", 100, 400);
            AddRayan("Dolphin", 120, 500);
            AddRayan("Octopus", 150, 650);
            AddRayan("Kraken", 180, 800);
            AddRayan("Hydra", 220, 1000);
            AddRayan("Griffin", 270, 1300);
            AddRayan("Pegasus", 320, 1600);
            AddRayan("Unicorn", 380, 2000);
            AddRayan("Basilisk", 450, 2500);
            
            // === TIER 5: Epic (Rarity 1K-10K) ===
            AddRayan("Phoenix", 550, 3200);
            AddRayan("Dragon", 700, 4000);
            AddRayan("Wyvern", 850, 5000);
            AddRayan("Chimera", 1000, 6500);
            AddRayan("Cerberus", 1200, 8000);
            AddRayan("Leviathan", 1500, 10000);
            AddRayan("Behemoth", 1800, 13000);
            AddRayan("Fenrir", 2200, 16000);
            AddRayan("Jormungandr", 2700, 20000);
            AddRayan("Tiamat", 3300, 25000);
            
            // === TIER 6: Legendary (Rarity 10K-50K) ===
            AddRayan("Zeus", 4000, 32000);
            AddRayan("Odin", 5000, 40000);
            AddRayan("Ra", 6000, 50000);
            AddRayan("Thor", 7500, 65000);
            AddRayan("Anubis", 9000, 80000);
            AddRayan("Hades", 11000, 100000);
            AddRayan("Poseidon", 13500, 130000);
            AddRayan("Athena", 16500, 160000);
            AddRayan("Apollo", 20000, 200000);
            AddRayan("Artemis", 25000, 250000);
            
            // === TIER 7: Mythic (Rarity 50K-200K) ===
            AddRayan("Amaterasu", 30000, 320000);
            AddRayan("Susanoo", 37000, 400000);
            AddRayan("Bahamut", 45000, 500000);
            AddRayan("Quetzalcoatl", 55000, 650000);
            AddRayan("Osiris", 67000, 800000);
            AddRayan("Vishnu", 82000, 1000000);
            AddRayan("Shiva", 100000, 1300000);
            AddRayan("Brahma", 120000, 1600000);
            AddRayan("Izanagi", 145000, 2000000);
            AddRayan("Itzamna", 175000, 2500000);
            
            // === TIER 8-10: Divine & Beyond (Rarity 200K+) ===
            AddRayan("Chronos", 210000, 3200000);
            AddRayan("Gaia", 255000, 4000000);
            AddRayan("Uranus", 310000, 5000000);
            AddRayan("Nyx", 380000, 6500000);
            AddRayan("Erebus", 460000, 8000000);
            AddRayan("Tartarus", 560000, 10000000);
            AddRayan("Aether", 680000, 13000000);
            AddRayan("Chaos", 830000, 16000000);
            AddRayan("Nemesis", 1000000, 20000000);
            AddRayan("Thanatos", 1200000, 25000000);
            
            // === TIER 11: Transcendent (Rarity 1.2M-8M, BaseValue 25M-250M) ===
            AddRayan("Ananke", 1450000, 32000000);
            AddRayan("Hemera", 1750000, 40000000);
            AddRayan("Hypnos", 2100000, 50000000);
            AddRayan("Morpheus", 2550000, 65000000);
            AddRayan("Eros", 3100000, 80000000);
            AddRayan("Persephone", 3750000, 100000000);
            AddRayan("Demeter", 4500000, 130000000);
            AddRayan("Hestia", 5500000, 160000000);
            AddRayan("Hera", 6700000, 200000000);
            AddRayan("Ares", 8200000, 250000000);
            
            // === TIER 12: Primordial (Rarity 8M-60M, BaseValue 250M-2.5B) ===
            AddRayan("Pontus", 10000000, 320000000);
            AddRayan("Oceanus", 12200000, 400000000);
            AddRayan("Hyperion", 14850000, 500000000);
            AddRayan("Theia", 18100000, 650000000);
            AddRayan("Rhea", 22000000, 800000000);
            AddRayan("Themis", 27000000, 1000000000);
            AddRayan("Mnemosyne", 32800000, 1300000000);
            AddRayan("Phoebe", 40000000, 1600000000);
            AddRayan("Tethys", 49000000, 2000000000);
            AddRayan("Coeus", 60000000, 2500000000);
            
            // === TIER 13: Cosmic (Rarity 60M-430M, BaseValue 2.5B-25B) ===
            AddRayan("Crius", 73000000, 3200000000);
            AddRayan("Iapetus", 89000000, 4000000000);
            AddRayan("Cronus", 108000000, 5000000000);
            AddRayan("Prometheus", 132000000, 6500000000);
            AddRayan("Atlas", 160000000, 8000000000);
            AddRayan("Epimetheus", 195000000, 10000000000);
            AddRayan("Menoetius", 237000000, 13000000000);
            AddRayan("Asteria", 290000000, 16000000000);
            AddRayan("Leto", 353000000, 20000000000);
            AddRayan("Hecate", 430000000, 25000000000);
            
            // === TIER 14: Universal (Rarity 430M-3B, BaseValue 25B-250B) ===
            AddRayan("Selene", 523000000, 32000000000);
            AddRayan("Helios", 637000000, 40000000000);
            AddRayan("Eos", 776000000, 50000000000);
            AddRayan("Astraeus", 945000000, 65000000000);
            AddRayan("Pallas", 1150000000, 80000000000);
            AddRayan("Styx", 1400000000, 100000000000);
            AddRayan("Nike", 1700000000, 130000000000);
            AddRayan("Kratos", 2070000000, 160000000000);
            AddRayan("Bia", 2520000000, 200000000000);
            AddRayan("Zelus", 3070000000, 250000000000);
            
            // === TIER 15: Infinite (Rarity 3B-22B, BaseValue 250B-2.5T) ===
            AddRayan("Metis", 3740000000, 320000000000);
            AddRayan("Eileithyia", 4550000000, 400000000000);
            AddRayan("Hebe", 5540000000, 500000000000);
            AddRayan("Hephaestus", 6750000000, 650000000000);
            AddRayan("Hermes", 8220000000, 800000000000);
            AddRayan("Dionysus", 10000000000, 1000000000000);
            AddRayan("Heracles", 12200000000, 1300000000000);
            AddRayan("Perseus", 14850000000, 1600000000000);
            AddRayan("Achilles", 18100000000, 2000000000000);
            AddRayan("Odysseus", 22000000000, 2500000000000);
            
            // === TIER 16: Eternal (Rarity 22B-160B, BaseValue 2.5T-25T) ===
            AddRayan("Theseus", 26800000000, 3200000000000);
            AddRayan("Jason", 32600000000, 4000000000000);
            AddRayan("Bellerophon", 39700000000, 5000000000000);
            AddRayan("Orpheus", 48300000000, 6500000000000);
            AddRayan("Asclepius", 58800000000, 8000000000000);
            AddRayan("Castor", 71600000000, 10000000000000);
            AddRayan("Pollux", 87200000000, 13000000000000);
            AddRayan("Atalanta", 106200000000, 16000000000000);
            AddRayan("Medea", 129300000000, 20000000000000);
            AddRayan("Circe", 157500000000, 25000000000000);
            
            // === TIER 17: Absolute (Rarity 160B-514B, BaseValue 25T-100T) ===
            AddRayan("Calypso", 191800000000, 32000000000000);
            AddRayan("Psyche", 233600000000, 40000000000000);
            AddRayan("Pandora", 284500000000, 50000000000000);
            AddRayan("Arachne", 346400000000, 65000000000000);
            AddRayan("Medusa", 421800000000, 80000000000000);
            AddRayan("Scylla", 513600000000, 100000000000000);
            
            // === Elements (80-100) ===
            AddRayan("Ember", 70, 250);
            AddRayan("Blaze", 180, 750);
            AddRayan("Inferno", 800, 4500);
            AddRayan("Pyre", 3500, 18000);
            AddRayan("Frost", 90, 350);
            AddRayan("Glacier", 240, 950);
            AddRayan("Blizzard", 1100, 5500);
            AddRayan("Tundra", 4500, 22000);
            AddRayan("Spark", 110, 450);
            AddRayan("Bolt", 300, 1200);
            AddRayan("Thunder", 1400, 7000);
            AddRayan("Storm", 5500, 28000);
            
            // === Gems & Minerals (100-120) ===
            AddRayan("Quartz", 160, 600);
            AddRayan("Topaz", 400, 1800);
            AddRayan("Ruby", 1700, 8500);
            AddRayan("Sapphire", 6500, 32000);
            AddRayan("Emerald", 7800, 38000);
            AddRayan("Diamond", 9500, 47000);
            AddRayan("Opal", 11500, 57000);
            AddRayan("Obsidian", 14000, 70000);
            AddRayan("Onyx", 17000, 85000);
            AddRayan("Pearl", 21000, 105000);
            
            // Generate remaining 380 Rayans with combinations
            GenerateCombinedRayans();
        }

        private static void AddRayan(string name, double rarity, long baseValue)
        {
            Prefixes.Add((name, rarity, new BigInteger(baseValue)));
        }

        private static void GenerateCombinedRayans()
        {
            // Prefixes for combinations
            string[] adjectives = new[]
            {
                "Swift", "Mighty", "Ancient", "Eternal", "Cosmic", "Astral", "Celestial",
                "Divine", "Infernal", "Frozen", "Burning", "Storm", "Shadow", "Light",
                "Dark", "Crimson", "Azure", "Golden", "Silver", "Platinum", "Crystal",
                "Phantom", "Spectral", "Ethereal", "Arcane", "Mystic", "Sacred", "Cursed",
                "Blessed", "Radiant", "Void", "Chaos", "Primal", "Elder", "Forgotten",
                "Lost", "Hidden", "Secret", "Royal", "Imperial", "Noble", "Savage",
                "Wild", "Feral", "Tamed", "Broken", "Shattered", "Forged", "Tempered"
            };

            string[] nouns = new[]
            {
                "Warrior", "Sage", "Knight", "Mage", "Priest", "Rogue", "Assassin",
                "Guardian", "Sentinel", "Protector", "Destroyer", "Creator", "Reaper",
                "Slayer", "Hunter", "Seeker", "Wanderer", "Traveler", "Explorer",
                "Champion", "Hero", "Legend", "Titan", "Giant", "Colossus", "Golem",
                "Wraith", "Specter", "Spirit", "Soul", "Essence", "Entity", "Being",
                "Force", "Power", "Energy", "Aura", "Presence", "Manifestation",
                "Avatar", "Vessel", "Herald", "Harbinger", "Omen", "Oracle", "Prophet",
                "Seer", "Shaman", "Druid", "Monk", "Paladin", "Warlock", "Sorcerer"
            };

            int currentIndex = Prefixes.Count;
            int targetCount = 500;
            
            // Start from Scylla's rarity and continue seamlessly
            double baseRarity = 513600000000; // Scylla's rarity (513.6 Billion)
            BigInteger baseValue = new BigInteger(100000000000000); // Scylla's value (100T)
            
            // Generate combinations
            var random = new Random(42); // Fixed seed for consistency
            while (Prefixes.Count < targetCount)
            {
                string adj = adjectives[random.Next(adjectives.Length)];
                string noun = nouns[random.Next(nouns.Length)];
                string combinedName = $"{adj} {noun}";
                
                // Check if name already exists
                if (Prefixes.Any(p => p.Prefix == combinedName))
                    continue;
                
                // Calculate rarity starting from Scylla and increasing with factor 1.2x per tier
                int tier = Prefixes.Count - currentIndex;
                double rarity = baseRarity * Math.Pow(1.2, tier + 1);
                
                // BaseValue increases by factor 10 every 3 tiers
                BigInteger value = baseValue * BigInteger.Pow(new BigInteger(10), (tier + 3) / 3);
                
                Prefixes.Add((combinedName, rarity, value));
            }
        }
    }
}
