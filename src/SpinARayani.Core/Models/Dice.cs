using System.Numerics;

namespace SpinARayani.Core.Models;

public class Dice
{
    public string Name { get; set; } = string.Empty;
    public double LuckMultiplier { get; set; } = 1.0;
    public BigInteger Cost { get; set; }
    public BigInteger Quantity { get; set; } = 0;
    public bool IsInfinite { get; set; } = false;

    public string Description => IsInfinite ? "Always available" : $"+{(LuckMultiplier - 1.0) * 100:F0}% Luck";
    public string DisplayName => IsInfinite ? Name : $"?? {Name}";
}
