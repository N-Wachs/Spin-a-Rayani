using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpinARayani.Core.Interfaces;
using SpinARayani.Core.Models;

namespace SpinARayani.UI.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IGameService _gameService;

    [ObservableProperty]
    private PlayerStats _stats;

    [ObservableProperty]
    private string _lastRoll = "???";

    public MainViewModel(IGameService gameService)
    {
        _gameService = gameService;
        _stats = _gameService.Stats;
    }

    [RelayCommand]
    private async Task Roll()
    {
        await _gameService.RollAsync();
        
        // Update LastRoll display with the most recent roll
        if (Stats.Inventory.Count > 0)
        {
            var lastRayan = Stats.Inventory.Last();
            LastRoll = lastRayan.FullName;
        }
        else
        {
            LastRoll = "No Rayans yet!";
        }
        
        OnPropertyChanged(nameof(Stats));
    }

    [RelayCommand]
    private async Task Rebirth()
    {
        await _gameService.RebirthAsync();
        OnPropertyChanged(nameof(Stats));
    }
}
