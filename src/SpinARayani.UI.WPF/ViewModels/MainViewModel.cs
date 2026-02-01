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

    public MainViewModel(IGameService gameService)
    {
        _gameService = gameService;
        _stats = _gameService.Stats;
    }

    [RelayCommand]
    private async Task Roll()
    {
        await _gameService.RollAsync();
        OnPropertyChanged(nameof(Stats));
    }

    [RelayCommand]
    private async Task Rebirth()
    {
        await _gameService.RebirthAsync();
        OnPropertyChanged(nameof(Stats));
    }
}
