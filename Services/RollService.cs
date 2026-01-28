using System;
using System.Linq;
using SpinARayan.Models;

namespace SpinARayan.Services
{
    public class RollService
    {
        private readonly Random _random = new Random();

        public Rayan Roll(double luckMultiplier, SuffixEvent? currentEvent = null)
        {
            // Roll for Prefix (affected by luck)
            var prefixData = SelectFromList(RayanData.Prefixes, luckMultiplier);

            // Roll for Suffix (NOT affected by luck, only by events)
            var suffixData = SelectSuffix(currentEvent);

            return new Rayan
            {
                Prefix = prefixData.Prefix,
                Rarity = prefixData.Rarity,
                BaseValue = prefixData.BaseValue,
                Suffix = suffixData?.Suffix ?? "",
                Multiplier = suffixData?.Multiplier ?? 1.0
            };
        }

        private (string Prefix, double Rarity, System.Numerics.BigInteger BaseValue) SelectFromList(List<(string Prefix, double Rarity, System.Numerics.BigInteger BaseValue)> list, double luck)
        {
            // Simple rarity logic: higher luck shifts the random value
            // We sort by rarity descending to check rarest first
            var sorted = list.OrderByDescending(x => x.Rarity).ToList();

            foreach (var item in sorted)
            {
                double chance = 1.0 / (item.Rarity / luck);
                if (_random.NextDouble() < chance)
                {
                    return item;
                }
            }

            return list[0]; // Default to Common
        }

        private (string Suffix, double Chance, double Multiplier)? SelectSuffix(SuffixEvent? currentEvent)
        {
            var sorted = RayanData.Suffixes.OrderByDescending(x => x.Chance).ToList();
            
            foreach (var item in sorted)
            {
                double baseChance = item.Chance;
                
                // If there's an active event for this suffix, make it 5x more likely
                if (currentEvent != null && currentEvent.IsActive && currentEvent.SuffixName == item.Suffix)
                {
                    baseChance /= currentEvent.BoostMultiplier; // Lower chance value = higher probability
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
