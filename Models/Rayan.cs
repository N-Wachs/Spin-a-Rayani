using System.Numerics;
using System.Xml.Serialization;

namespace SpinARayan.Models
{
    public class Rayan
    {
        public string Prefix { get; set; } = "";
        public string Suffix { get; set; } = "";
        public double Rarity { get; set; } // 1 in X
        
        [XmlIgnore]
        public BigInteger BaseValue { get; set; }
        
        // XML serialization workaround for BigInteger
        [XmlElement("BaseValue")]
        public string BaseValueString
        {
            get => BaseValue.ToString();
            set => BaseValue = string.IsNullOrEmpty(value) ? BigInteger.Zero : BigInteger.Parse(value);
        }
        
        public double Multiplier { get; set; } = 1.0;
        
        // Adjusted Rarity (Rarity / LuckMultiplier) - used for flash effects
        // Not serialized, only used for UI effects
        [XmlIgnore]
        public double AdjustedRarity { get; set; } = 0;

        public string FullName => $"{(string.IsNullOrEmpty(Prefix) ? "" : Prefix + " ")}Rayan{(string.IsNullOrEmpty(Suffix) ? "" : " " + Suffix)}";

        public BigInteger TotalValue
        {
            get
            {
                // BigInteger multiplication with double
                return new BigInteger((double)BaseValue * Multiplier);
            }
        }

        public override string ToString() => FullName;
    }
}

