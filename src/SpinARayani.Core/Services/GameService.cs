using SpinARayani.Core.Interfaces;
using SpinARayani.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpinARayani.Core.Services;

public partial class GameService : ObservableObject, IGameService
{
    [ObservableProperty]
    private PlayerStats _stats = new();

    private readonly IRollService _rollService;
    private readonly ISaveService _saveService;

    public GameService(IRollService rollService, ISaveService saveService)
    {
        _rollService = rollService;
        _saveService = saveService;
    }

    public async Task InitializeAsync()
    {
        LoadGame();
        await Task.CompletedTask;
    }

    public async Task RollAsync()
    {
        var luck = Stats.LuckMultiplier * Stats.LuckMultiplier; // Simplified for example
        var rayan = _rollService.Roll(luck);
        Stats.Inventory.Add(rayan);
        Stats.TotalRolls++;
        await Task.CompletedTask;
    }

    public async Task RebirthAsync()
    {
        if (Stats.Money >= Stats.NextRebirthCost)
        {
            Stats.Money -= Stats.NextRebirthCost;
            Stats.Rebirths++;
            Stats.PlotSlots++;
            // Reset some stats if needed
        }
        await Task.CompletedTask;
    }

    public void SaveGame() => _saveService.Save(Stats);
    public void LoadGame() => Stats = _saveService.Load();
}

public interface IRollService { Rayan Roll(double luck); }
public interface ISaveService { void Save(PlayerStats stats); PlayerStats Load(); }
