using System.Text.Json;
using SpinARayani.Core.Interfaces;
using SpinARayani.Core.Models;

namespace SpinARayani.Core.Services;

public class SaveService : ISaveService
{
    private readonly string _saveFilePath;

    public SaveService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var saveFolderPath = Path.Combine(appDataPath, "SpinARayani");
        Directory.CreateDirectory(saveFolderPath);
        _saveFilePath = Path.Combine(saveFolderPath, "save.json");
    }

    public void Save(PlayerStats stats)
    {
        try
        {
            var json = JsonSerializer.Serialize(stats, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_saveFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving game: {ex.Message}");
        }
    }

    public PlayerStats Load()
    {
        try
        {
            if (File.Exists(_saveFilePath))
            {
                var json = File.ReadAllText(_saveFilePath);
                var stats = JsonSerializer.Deserialize<PlayerStats>(json);
                if (stats != null)
                {
                    return stats;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading game: {ex.Message}");
        }
        
        // Return default stats if no save file exists or loading failed
        return new PlayerStats();
    }
}
