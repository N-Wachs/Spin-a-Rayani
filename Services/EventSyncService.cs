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
        private readonly System.Timers.Timer? _pollTimer;
        private readonly string _sharedFolder;
        private readonly string _eventFile;
        private readonly bool _isAdmin;
        private DateTime _lastEventPublish = DateTime.MinValue;
        private long _lastProcessedTimestamp = 0; // Track last processed event to avoid duplicates
        private DateTime _lastFileCheck = DateTime.MinValue;

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
                Console.WriteLine($"[EventSync] Folder: {_sharedFolder}");
                Console.WriteLine($"[EventSync] Event File: {_eventFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] ERROR creating folder: {ex.Message}");
                throw;
            }

            // Setup POLLING Timer (better for OneDrive than FileWatcher!)
            if (!_isAdmin)
            {
                try
                {
                    _pollTimer = new System.Timers.Timer(2000); // Check every 2 seconds
                    _pollTimer.Elapsed += (s, e) => PollForEvents();
                    _pollTimer.AutoReset = true;
                    _pollTimer.Start();
                    Console.WriteLine($"[EventSync] Polling active (every 2s) - OneDrive compatible mode");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EventSync] ERROR setting up polling: {ex.Message}");
                    throw;
                }
            }
            else
            {
                Console.WriteLine($"[EventSync] Admin mode - publishing only");
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
                _lastProcessedTimestamp = eventData.Timestamp; // Admin skips own event in polling
                
                Console.WriteLine($"[EventSync] ? Published Event: {suffixName} from {eventData.AdminName} at {DateTime.Now:HH:mm:ss}");
                Console.WriteLine($"[EventSync] File written to: {_eventFile}");
                Console.WriteLine($"[EventSync] Event Timestamp: {eventData.Timestamp}");
                Console.WriteLine($"[EventSync] Admin saved own timestamp to skip re-processing");
                
                if (File.Exists(_eventFile))
                {
                    var fileInfo = new FileInfo(_eventFile);
                    Console.WriteLine($"[EventSync] File size: {fileInfo.Length} bytes");
                    Console.WriteLine($"[EventSync] File modified: {fileInfo.LastWriteTime:HH:mm:ss.fff}");
                    Console.WriteLine($"[EventSync] OneDrive should sync this to clients now...");
                }
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

        /// <summary>
        /// Polling method for OneDrive compatibility (better than FileWatcher)
        /// Clients poll every 2 seconds for new events and OVERRIDE local events!
        /// </summary>
        private void PollForEvents()
        {
            try
            {
                // Throttle logging to once per 10 seconds
                bool shouldLog = (DateTime.Now - _lastFileCheck).TotalSeconds >= 10;
                
                if (!File.Exists(_eventFile))
                {
                    if (shouldLog)
                    {
                        Console.WriteLine($"[EventSync] Poll: No events.json found yet ({DateTime.Now:HH:mm:ss})");
                        Console.WriteLine($"[EventSync] Waiting for admin to start first event...");
                    }
                    _lastFileCheck = DateTime.Now;
                    return;
                }

                var fileInfo = new FileInfo(_eventFile);
                
                if (shouldLog)
                {
                    Console.WriteLine($"[EventSync] Poll: events.json found!");
                    Console.WriteLine($"[EventSync]   Size: {fileInfo.Length} bytes");
                    Console.WriteLine($"[EventSync]   Modified: {fileInfo.LastWriteTime:HH:mm:ss.fff}");
                    Console.WriteLine($"[EventSync]   Checking for new events...");
                }
                
                _lastFileCheck = DateTime.Now;
                
                // Read and parse event file
                string json = File.ReadAllText(_eventFile);
                var eventData = JsonSerializer.Deserialize<SharedEventData>(json);

                if (eventData == null)
                {
                    Console.WriteLine("[EventSync] ERROR: Could not deserialize event data!");
                    Console.WriteLine($"[EventSync] JSON: {json}");
                    return;
                }

                // Skip if we already processed this exact event
                if (eventData.Timestamp <= _lastProcessedTimestamp)
                {
                    if (shouldLog)
                    {
                        Console.WriteLine($"[EventSync] Event already processed (Timestamp: {eventData.Timestamp})");
                    }
                    return; // Already processed, skip
                }

                // Check if event is still active (within duration)
                var elapsed = DateTime.UtcNow - eventData.StartTime;
                if (elapsed.TotalMinutes > eventData.DurationMinutes)
                {
                    if (shouldLog)
                    {
                        Console.WriteLine($"[EventSync] Event expired ({elapsed.TotalMinutes:F1} min old, max {eventData.DurationMinutes} min)");
                    }
                    // Update timestamp anyway to not spam expired messages
                    _lastProcessedTimestamp = eventData.Timestamp;
                    return;
                }

                // ? NEW AND ACTIVE EVENT! Apply it NOW and OVERRIDE any local event!
                Console.WriteLine($"[EventSync] ================================================");
                Console.WriteLine($"[EventSync] ? NEW EVENT DETECTED!");
                Console.WriteLine($"[EventSync]   Suffix: {eventData.SuffixName}");
                Console.WriteLine($"[EventSync]   Admin: {eventData.AdminName}");
                Console.WriteLine($"[EventSync]   Started: {eventData.StartTime:HH:mm:ss} UTC");
                Console.WriteLine($"[EventSync]   Age: {elapsed.TotalSeconds:F1}s");
                Console.WriteLine($"[EventSync]   Timestamp: {eventData.Timestamp}");
                Console.WriteLine($"[EventSync]   Applying event and OVERRIDING any local event...");
                
                _lastProcessedTimestamp = eventData.Timestamp;
                _gameManager.ApplyRemoteEvent(eventData.SuffixName, eventData.AdminName);
                
                Console.WriteLine($"[EventSync] ? Event successfully applied!");
                Console.WriteLine($"[EventSync] ================================================");
            }
            catch (IOException ioEx)
            {
                // File locked by OneDrive - will retry on next poll (2s)
                Console.WriteLine($"[EventSync] File locked by OneDrive, retrying in 2s... ({ioEx.Message})");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"[EventSync] JSON Parse ERROR: {jsonEx.Message}");
                Console.WriteLine($"[EventSync] File might be corrupted or partially written by OneDrive");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] Poll ERROR: {ex.Message}");
                Console.WriteLine($"[EventSync] Stack: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Manual sync check for debugging
        /// </summary>
        public void ForceSyncCheck()
        {
            Console.WriteLine($"[EventSync] === MANUAL SYNC CHECK ===");
            Console.WriteLine($"[EventSync] Mode: {(_isAdmin ? "ADMIN" : "CLIENT")}");
            Console.WriteLine($"[EventSync] Folder: {_sharedFolder}");
            Console.WriteLine($"[EventSync] Folder exists: {Directory.Exists(_sharedFolder)}");
            Console.WriteLine($"[EventSync] Event file: {_eventFile}");
            Console.WriteLine($"[EventSync] Event file exists: {File.Exists(_eventFile)}");
            
            if (File.Exists(_eventFile))
            {
                var fileInfo = new FileInfo(_eventFile);
                Console.WriteLine($"[EventSync] File size: {fileInfo.Length} bytes");
                Console.WriteLine($"[EventSync] Last modified: {fileInfo.LastWriteTime:HH:mm:ss.fff}");
                Console.WriteLine($"[EventSync] Last accessed: {fileInfo.LastAccessTime:HH:mm:ss.fff}");
                
                try
                {
                    string json = File.ReadAllText(_eventFile);
                    Console.WriteLine($"[EventSync] JSON Content:\n{json}");
                    
                    var eventData = JsonSerializer.Deserialize<SharedEventData>(json);
                    if (eventData != null)
                    {
                        Console.WriteLine($"[EventSync] Parsed Event: {eventData.SuffixName} from {eventData.AdminName}");
                        Console.WriteLine($"[EventSync] Event Timestamp: {eventData.Timestamp}");
                        Console.WriteLine($"[EventSync] Last Processed: {_lastProcessedTimestamp}");
                        Console.WriteLine($"[EventSync] Already processed: {eventData.Timestamp <= _lastProcessedTimestamp}");
                        
                        var elapsed = DateTime.UtcNow - eventData.StartTime;
                        Console.WriteLine($"[EventSync] Event age: {elapsed.TotalSeconds:F1}s");
                        Console.WriteLine($"[EventSync] Event active: {elapsed.TotalMinutes <= eventData.DurationMinutes}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EventSync] Error reading file: {ex.Message}");
                }
            }
            
            Console.WriteLine($"[EventSync] === END SYNC CHECK ===");
        }

        /// <summary>
        /// Check if multiplayer is connected
        /// </summary>
        public bool IsConnected => Directory.Exists(_sharedFolder);

        public void Dispose()
        {
            if (_pollTimer != null)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
                Console.WriteLine("[EventSync] Disposed");
            }
        }
    }
}
