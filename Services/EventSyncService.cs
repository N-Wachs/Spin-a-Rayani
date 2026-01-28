using System;
using System.IO;
using System.Text.Json;
using SpinARayan.Models;

namespace SpinARayan.Services
{
    /// <summary>
    /// Synchronizes events between multiple game instances using a shared cloud folder (OneDrive/Dropbox).
    /// Admin can publish events, Clients receive and apply them automatically.
    /// </summary>
    public class EventSyncService : IDisposable
    {
        private readonly GameManager _gameManager;
        private readonly FileSystemWatcher? _watcher;
        private readonly string _sharedFolder;
        private readonly string _eventFile;
        private readonly bool _isAdmin;
        private DateTime _lastEventPublish = DateTime.MinValue;

        public EventSyncService(GameManager gameManager, string sharedFolderPath, bool isAdmin = false)
        {
            _gameManager = gameManager;
            _isAdmin = isAdmin;
            _sharedFolder = sharedFolderPath;
            _eventFile = Path.Combine(_sharedFolder, "events.json");

            // Create folder if not exists
            try
            {
                Directory.CreateDirectory(_sharedFolder);
                Console.WriteLine($"[EventSync] Initialized - Mode: {(isAdmin ? "ADMIN" : "CLIENT")}");
                Console.WriteLine($"[EventSync] Watching: {_sharedFolder}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] ERROR creating folder: {ex.Message}");
                throw;
            }

            // Setup FileWatcher (nur für Clients, Admin nutzt eigenes Event)
            if (!_isAdmin)
            {
                try
                {
                    _watcher = new FileSystemWatcher(_sharedFolder, "events.json");
                    _watcher.Changed += OnEventFileChanged;
                    _watcher.Created += OnEventFileChanged;
                    _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime;
                    _watcher.EnableRaisingEvents = true;
                    Console.WriteLine($"[EventSync] FileWatcher active - waiting for admin events...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EventSync] ERROR setting up watcher: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Publish event to shared folder (Admin only!)
        /// </summary>
        public void PublishEvent(string suffixName, string? customUsername = null)
        {
            if (!_isAdmin)
            {
                Console.WriteLine("[EventSync] ERROR: Only admin can publish events!");
                return;
            }

            try
            {
                var eventData = new SharedEventData
                {
                    SuffixName = suffixName,
                    StartTime = DateTime.UtcNow,
                    DurationMinutes = 2.5,
                    AdminName = customUsername ?? Environment.UserName,
                    Timestamp = DateTime.UtcNow.Ticks // Für Duplikat-Erkennung
                };

                string json = JsonSerializer.Serialize(eventData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                File.WriteAllText(_eventFile, json);
                _lastEventPublish = DateTime.UtcNow;
                
                Console.WriteLine($"[EventSync] ? Published Event: {suffixName} from {eventData.AdminName} at {DateTime.Now:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] Publish ERROR: {ex.Message}");
                System.Windows.Forms.MessageBox.Show(
                    $"Event konnte nicht veröffentlicht werden!\n\n" +
                    $"Stelle sicher dass:\n" +
                    $"• OneDrive läuft\n" +
                    $"• Du Schreibzugriff hast\n" +
                    $"• Der Ordner existiert\n\n" +
                    $"Fehler: {ex.Message}",
                    "Event-Sync Fehler",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );
            }
        }

        private void OnEventFileChanged(object sender, FileSystemEventArgs e)
        {
            // File changed/created ? Load and apply event!
            try
            {
                // Kleine Verzögerung, damit File fertig geschrieben ist (OneDrive braucht manchmal etwas)
                System.Threading.Thread.Sleep(200);

                if (!File.Exists(_eventFile))
                    return;

                string json = File.ReadAllText(_eventFile);
                var eventData = JsonSerializer.Deserialize<SharedEventData>(json);

                if (eventData == null)
                {
                    Console.WriteLine("[EventSync] ERROR: Could not deserialize event data");
                    return;
                }

                // Check if event is still active
                var elapsed = DateTime.UtcNow - eventData.StartTime;
                if (elapsed.TotalMinutes > eventData.DurationMinutes)
                {
                    Console.WriteLine($"[EventSync] Event expired ({elapsed.TotalMinutes:F1} min old) - ignoring");
                    return;
                }

                // Apply event to game!
                _gameManager.ApplyRemoteEvent(eventData.SuffixName, eventData.AdminName);
                Console.WriteLine($"[EventSync] ? Applied Event: {eventData.SuffixName} from {eventData.AdminName}");
            }
            catch (IOException ioEx)
            {
                // File locked by OneDrive - retry once
                Console.WriteLine($"[EventSync] File locked, retrying... ({ioEx.Message})");
                System.Threading.Thread.Sleep(500);
                OnEventFileChanged(sender, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] Load ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if multiplayer is connected
        /// </summary>
        public bool IsConnected => Directory.Exists(_sharedFolder);

        public void Dispose()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                Console.WriteLine("[EventSync] Disposed");
            }
        }
    }
}
