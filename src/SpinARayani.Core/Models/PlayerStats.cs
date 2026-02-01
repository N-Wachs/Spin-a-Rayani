using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpinARayani.Core.Models;

public partial class PlayerStats : ObservableObject
{
    [ObservableProperty]
    private BigInteger _money = 0;

    [ObservableProperty]
    private int _gems = 0;

    [ObservableProperty]
    private int _totalRolls = 0;

    [ObservableProperty]
    private int _rebirths = 0;

    [ObservableProperty]
    private int _plotSlots = 3;

    [ObservableProperty]
    private double _rollCooldown = 2.0;

    [ObservableProperty]
    private int _luckBoosterLevel = 0;

    public List<Rayan> Inventory { get; set; } = new();
    public List<int> EquippedRayanIndices { get; set; } = new();
    public List<Dice> OwnedDices { get; set; } = new();
    public int SelectedDiceIndex { get; set; } = 0;

    public double MoneyMultiplier => 1.0 + (Rebirths * 4.0);
    public double LuckMultiplier => 1.0 + (LuckBoosterLevel * 0.25);
    
    public BigInteger NextRebirthCost => BigInteger.Pow(8, Rebirths) * 100000;
}
