using System.Numerics;

namespace SpinARayani.Core.Models;

public static class RayanData
{
    public static readonly List<(string Prefix, double Rarity, BigInteger BaseValue)> Prefixes = new()
    {
        ("Rat", 1, 1),
        ("Bee", 2, 2),
        ("Cat", 3, 3),
        ("Dog", 4, 5),
        ("Frog", 5, 7),
        // ... weitere aus dem Original
    };

    public static readonly List<(string Suffix, double Chance, double Multiplier)> Suffixes = new()
    {
        ("Selbstbewusst", 25, 1.5),
        ("GC", 50, 2.0),
        ("Blessed", 75, 2.2),
        ("Shiny", 100, 2.5),
        ("Cursed", 150, 2.8),
        // ... weitere aus dem Original
    };
}
