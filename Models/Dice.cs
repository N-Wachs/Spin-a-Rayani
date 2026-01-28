using System.Numerics;

namespace SpinARayan.Models
{
    public class Dice
    {
        public string Name { get; set; } = "";
        public double LuckMultiplier { get; set; } = 1.0; // 1.0 = kein Bonus, 1.5 = +50%, 2.0 = +100%
        public int Cost { get; set; } // In Money (nicht Gems!)
        public int Quantity { get; set; } = 0; // Wie viele Rolls mit diesem Dice verfügbar sind
        public bool IsInfinite { get; set; } = false; // True für Basic Dice (unendlich verfügbar)
        
        public string Description => IsInfinite ? "Always available" : $"+{(LuckMultiplier - 1.0) * 100:F0}% Luck";
        public string DisplayName => IsInfinite ? Name : $"? {Name}";
        public string CostDisplay => IsInfinite ? "Free" : $"{Cost:N0} $";
        public string QuantityDisplay => IsInfinite ? "?" : Quantity.ToString();
    }
}

