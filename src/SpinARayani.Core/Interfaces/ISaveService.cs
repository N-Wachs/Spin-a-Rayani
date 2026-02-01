using SpinARayani.Core.Models;

namespace SpinARayani.Core.Interfaces;

public interface ISaveService
{
    void Save(PlayerStats stats);
    PlayerStats Load();
}
