using System.Numerics;

namespace SpinARayani.Core.Models;

public static class RayanData
{
    // Prefixes with rarity (1 in X) and base value
    public static readonly List<(string Prefix, double Rarity, BigInteger BaseValue)> Prefixes = new()
    {
        // Common tier
        ("Rat", 1, 1),
        ("Bee", 1, 1),
        ("Cat", 1, 1),
        ("Dog", 1, 1),
        ("Frog", 1, 1),
        
        // Uncommon tier
        ("Wolf", 5, 10),
        ("Bear", 5, 10),
        ("Eagle", 5, 10),
        ("Lion", 5, 10),
        
        // Rare tier
        ("Dragon", 20, 100),
        ("Phoenix", 20, 100),
        ("Griffin", 20, 100),
        
        // Epic tier
        ("Titan", 100, 1000),
        ("Demon", 100, 1000),
        ("Angel", 100, 1000),
        
        // Legendary tier
        ("God", 1000, 10000),
        ("Ancient", 1000, 10000),
        
        // Mythic tier
        ("Cosmic", 10000, 100000),
        ("Void", 10000, 100000),
        
        // Divine tier
        ("Celestial", 100000, 1000000),
        ("Divine", 100000, 1000000),
        
        // Celestial tier
        ("Eternal", 1000000, 10000000),
        
        // Galactic tier
        ("Galactic", 10000000, BigInteger.Parse("100000000")),
        
        // Universal tier
        ("Universal", 100000000, BigInteger.Parse("1000000000")),
    };

    // Suffixes with chance (1 in X) and multiplier
    public static readonly List<(string Suffix, double Chance, double Multiplier)> Suffixes = new()
    {
        ("Selbstbewusst", 25, 1.5),
        ("GC", 50, 2.0),
        ("Shiny", 100, 2.5),
        ("SSL", 200, 3.0),
        ("Golden", 500, 5.0),
        ("Void", 1000, 10.0),
        ("Ancient", 5000, 20.0),
        ("Eternal", 50000, 100.0),
    };
}
