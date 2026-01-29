using System.Numerics;
using System.Xml.Serialization;

namespace SpinARayan.Models
{
    public class Dice
    {
        public string Name { get; set; } = "";
        public double LuckMultiplier { get; set; } = 1.0; // 1.0 = kein Bonus, 1.5 = +50%, 2.0 = +100%
        
        [XmlIgnore]
        public BigInteger Cost { get; set; } // In Money (nicht Gems!)
        
        [XmlElement("Cost")]
        public string CostString
        {
            get => Cost.ToString();
            set => Cost = BigInteger.Parse(string.IsNullOrEmpty(value) ? "0" : value);
        }
        
        [XmlIgnore]
        public BigInteger Quantity { get; set; } = 0; // Wie viele Rolls mit diesem Dice verfügbar sind
        
        [XmlElement("Quantity")]
        public string QuantityString
        {
            get => Quantity.ToString();
            set => Quantity = BigInteger.Parse(string.IsNullOrEmpty(value) ? "0" : value);
        }
        
        public bool IsInfinite { get; set; } = false; // True für Basic Dice (unendlich verfügbar)
        public string ImagePath => $"/Assets/dice_{Name.ToLower().Replace(" ", "_")}.png";
        
        public string Description => IsInfinite ? "Always available" : $"+{(LuckMultiplier - 1.0) * 100:F0}% Luck";
        public string DisplayName => IsInfinite ? Name : $"?? {Name}";
        
        public string CostDisplay
        {
            get
            {
                if (IsInfinite) return "Free";
                if (Cost < 1000) return Cost.ToString();
                if (Cost < 1000000) return ((double)Cost / 1000).ToString("F1") + "K";
                if (Cost < 1000000000) return ((double)Cost / 1000000).ToString("F1") + "M";
                if (Cost < 1000000000000) return ((double)Cost / 1000000000).ToString("F1") + "B";
                if (Cost < 1000000000000000) return ((double)Cost / 1000000000000).ToString("F1") + "T";
                return Cost.ToString("E1");
            }
        }
        
        public string QuantityDisplay
        {
            get
            {
                if (IsInfinite) return "?";
                if (Quantity < 1000) return Quantity.ToString();
                if (Quantity < 1000000) return ((double)Quantity / 1000).ToString("F1") + "K";
                if (Quantity < 1000000000) return ((double)Quantity / 1000000).ToString("F1") + "M";
                if (Quantity < 1000000000000) return ((double)Quantity / 1000000000).ToString("F1") + "B";
                if (Quantity < 1000000000000000) return ((double)Quantity / 1000000000000).ToString("F1") + "T";
                return Quantity.ToString("E1");
            }
        }
    }
}


