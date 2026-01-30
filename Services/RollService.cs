using System;
using System.Linq;
using SpinARayan.Models;

namespace SpinARayan.Services
{
    public class RollService
    {
        private readonly Random _random = new Random();
        
        // PERFORMANCE: Cache sorted lists - only sort ONCE!
        private readonly List<(string Prefix, double Rarity, System.Numerics.BigInteger BaseValue)> _sortedPrefixes;
        private readonly List<(string Suffix, double Chance, double Multiplier)> _sortedSuffixes;

        public RollService()
        {
            // Sort ONCE in constructor instead of every roll
            _sortedPrefixes = RayanData.Prefixes.OrderByDescending(x => x.Rarity).ToList();
            _sortedSuffixes = RayanData.Suffixes.OrderByDescending(x => x.Chance).ToList();
        }

        public Rayan Roll(double luckMultiplier, List<SuffixEvent>? activeEvents = null)
        {
            // Roll for Prefix (affected by luck)
            var prefixData = SelectFromList(luckMultiplier);

            // Roll for Suffix (NOT affected by luck, only by events)
            var suffixData = SelectSuffix(activeEvents);

            return new Rayan
            {
                Prefix = prefixData.Prefix,
                Rarity = prefixData.Rarity,
                BaseValue = prefixData.BaseValue,
                Suffix = suffixData?.Suffix ?? "",
                Multiplier = suffixData?.Multiplier ?? 1.0
            };
        }

        private (string Prefix, double Rarity, System.Numerics.BigInteger BaseValue) SelectFromList(double luck)
        {
            // PERFORMANCE: Use cached sorted list
            foreach (var item in _sortedPrefixes)
            {
                double chance = 1.0 / (item.Rarity / luck);
                if (_random.NextDouble() < chance)
                {
                    return item;
                }
            }

            return _sortedPrefixes[^1]; // Last item (least rare)
        }

        private (string Suffix, double Chance, double Multiplier)? SelectSuffix(List<SuffixEvent>? activeEvents)
        {
            // PERFORMANCE: Use cached sorted list
            foreach (var item in _sortedSuffixes)
            {
                double baseChance = item.Chance;
                
                // Check if ANY active event boosts this suffix
                if (activeEvents != null && activeEvents.Any())
                {
                    var matchingEvent = activeEvents.FirstOrDefault(e => e.IsActive && e.SuffixName == item.Suffix);
                    if (matchingEvent != null)
                    {
                        baseChance /= matchingEvent.BoostMultiplier; // Lower chance value = higher probability
                        Console.WriteLine($"[RollService] Boosting {item.Suffix} by {matchingEvent.BoostMultiplier}x (Event: {matchingEvent.EventName})");
                    }
                }
                
                double chance = 1.0 / baseChance;
                if (_random.NextDouble() < chance)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
