using System.Numerics;
using SpinARayani.Core.Interfaces;
using SpinARayani.Core.Models;

namespace SpinARayani.Core.Services;

public class RollService : IRollService
{
    private readonly Random _random = new();

    public Rayan Roll(double luck)
    {
        var rayan = new Rayan();
        
        // Roll for prefix
        var prefix = RollPrefix(luck);
        rayan.Prefix = prefix.Prefix;
        rayan.Rarity = prefix.Rarity;
        rayan.BaseValue = prefix.BaseValue;
        
        // Roll for suffix
        var suffix = RollSuffix(luck);
        if (suffix != null)
        {
            rayan.Suffix = suffix.Value.Suffix;
            rayan.Multiplier = suffix.Value.Multiplier;
        }
        
        return rayan;
    }

    private (string Prefix, double Rarity, BigInteger BaseValue) RollPrefix(double luck)
    {
        // Weighted random selection based on rarity
        double totalWeight = RayanData.Prefixes.Sum(p => 1.0 / p.Rarity);
        double roll = _random.NextDouble() * totalWeight * (1.0 / luck);
        
        double cumulative = 0;
        foreach (var prefix in RayanData.Prefixes)
        {
            cumulative += 1.0 / prefix.Rarity;
            if (roll <= cumulative)
            {
                return prefix;
            }
        }
        
        return RayanData.Prefixes.Last();
    }

    private (string Suffix, double Chance, double Multiplier)? RollSuffix(double luck)
    {
        // Determine if we get a suffix based on luck
        foreach (var suffix in RayanData.Suffixes.OrderByDescending(s => s.Chance))
        {
            double chance = suffix.Chance / 100.0 * luck;
            if (_random.NextDouble() < chance / 1000.0)
            {
                return suffix;
            }
        }
        
        return null;
    }
}
