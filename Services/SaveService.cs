using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SpinARayan.Models;

namespace SpinARayan.Services
{
    public class SaveService
    {
        private readonly string _filePath;

        public SaveService(string fileName = "savegame.xml")
        {
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        public void Save(PlayerStats stats)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PlayerStats));
                using (StreamWriter writer = new StreamWriter(_filePath))
                {
                    serializer.Serialize(writer, stats);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save error: {ex.Message}");
            }
        }

        public PlayerStats Load()
        {
            PlayerStats stats;
            
            if (!File.Exists(_filePath))
            {
                stats = new PlayerStats();
            }
            else
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PlayerStats));
                    using (StreamReader reader = new StreamReader(_filePath))
                    {
                        stats = (PlayerStats)serializer.Deserialize(reader)!;
                    }
                }
                catch (Exception ex)
                {
                    // Show error message to user for corrupted/outdated savefiles
                    System.Windows.Forms.MessageBox.Show(
                        $"Der Spielstand konnte nicht geladen werden!\n\n" +
                        $"Mögliche Ursachen:\n" +
                        $"• Beschädigte Savefile\n" +
                        $"• Veraltete Savefile-Version\n" +
                        $"• Datei wurde manuell bearbeitet\n\n" +
                        $"Ein neuer Spielstand wird erstellt.\n\n" +
                        $"Fehler: {ex.Message}",
                        "Savefile Fehler",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );
                    
                    // Backup corrupted savefile
                    try
                    {
                        string backupPath = _filePath + ".corrupted_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        File.Copy(_filePath, backupPath, true);
                        Console.WriteLine($"Corrupted savefile backed up to: {backupPath}");
                    }
                    catch { }
                    
                    stats = new PlayerStats();
                }
            }
            
            // Ensure Basic Dice always exists
            EnsureBasicDiceExists(stats);
            
            return stats;
        }
        
        private void EnsureBasicDiceExists(PlayerStats stats)
        {
            // Remove ALL variations of Basic Dice
            var dicesToRemove = stats.OwnedDices.Where(d => 
                d.IsInfinite ||
                d.Name.Contains("Basic", StringComparison.OrdinalIgnoreCase) ||
                (d.Cost == 0 && d.LuckMultiplier == 1.0)
            ).ToList();
            
            foreach (var dice in dicesToRemove)
            {
                stats.OwnedDices.Remove(dice);
            }
            
            // Create ONE clean Basic Dice at position 0
            var basicDice = new Dice 
            { 
                Name = "Basic Dice", 
                LuckMultiplier = 1.0, 
                Cost = 0, 
                IsInfinite = true,
                Quantity = 0 
            };
            stats.OwnedDices.Insert(0, basicDice);
            
            // Ensure valid selection
            if (stats.SelectedDiceIndex >= stats.OwnedDices.Count)
            {
                stats.SelectedDiceIndex = 0;
            }
        }
    }
}
