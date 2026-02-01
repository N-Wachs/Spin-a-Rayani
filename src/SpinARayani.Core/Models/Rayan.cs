using System.Numerics;

namespace SpinARayani.Core.Models;

public class Rayan
{
    public string Prefix { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public double Rarity { get; set; }
    public BigInteger BaseValue { get; set; }
    public double Multiplier { get; set; } = 1.0;

    public string FullName => $"{(string.IsNullOrEmpty(Prefix) ? "" : Prefix + " ")}Rayan{(string.IsNullOrEmpty(Suffix) ? "" : " " + Suffix)}";
    
    public BigInteger TotalValue => new BigInteger((double)BaseValue * Multiplier);

    public override string ToString() => FullName;
}
