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
            ("Omega", 100000, 200.0)
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
                
                // Calculate rarity exponentially
                int tier = Prefixes.Count - currentIndex;
                double rarity = Math.Pow(1.15, tier + 100) * 10;
                BigInteger baseValue = BigInteger.Pow(10, (tier + 100) / 8) * 100;
                
                Prefixes.Add((combinedName, rarity, baseValue));
            }
        }
    }
}
