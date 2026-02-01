using SpinARayani.Core.Models;

namespace SpinARayani.Core.Interfaces;

public interface IGameService
{
    PlayerStats Stats { get; }
    Task InitializeAsync();
    Task RollAsync();
    Task RebirthAsync();
    void SaveGame();
    void LoadGame();
}
