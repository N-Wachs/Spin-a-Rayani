using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SpinARayan.Models;

namespace SpinARayan.Services
{
    /// <summary>
    /// Synchronizes events between multiple game instances using SupaBase database.
    /// All users can publish and receive events in real-time.
    /// </summary>
    public class EventSyncService : IDisposable
    {
        private readonly GameManager _gameManager;
        private readonly System.Timers.Timer _pollTimer;
        private readonly HttpClient _httpClient;
        private readonly string _username;
        
        private const string SUPABASE_URL = "https://gflohnjhunyukdayaahn.supabase.co/rest/v1/Game%20Events";
        private const string SUPABASE_KEY = "sb_publishable_dZXMv77hZa3_vZbQTYSKeQ_rZ49Ro9w";
        private const int MAX_EVENTS_TO_FETCH = 50; // Limit DB query size
        
        private HashSet<long> _processedEventIds = new HashSet<long>();
        private DateTime _lastPollTime = DateTime.MinValue;

        public EventSyncService(GameManager gameManager, string username)
        {
            _gameManager = gameManager;
            _username = username;

            // Setup HTTP client with SupaBase authentication
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("apikey", SUPABASE_KEY);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {SUPABASE_KEY}");

            Console.WriteLine($"[EventSync] Initialized - Username: {_username}");
            Console.WriteLine($"[EventSync] Connected to SupaBase");

            // Setup polling timer - check every 10 seconds for new events
            _pollTimer = new System.Timers.Timer(10000);
            _pollTimer.Elapsed += async (s, e) => await PollForEventsAsync();
            _pollTimer.AutoReset = true;
            _pollTimer.Start();
            
            Console.WriteLine($"[EventSync] Polling active (every 10s)");
            
            // Initial poll on startup
            Task.Run(async () => await PollForEventsAsync());
        }

        /// <summary>
        /// Publish a custom event with all parameters (Admin mode)
        /// </summary>
        public async Task PublishCustomEventAsync(SharedEventData eventData)
        {
            try
            {
                var json = JsonSerializer.Serialize(eventData, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"[EventSync] Publishing custom event: {eventData.EventName} by {eventData.CreatedFrom}");
                Console.WriteLine($"[EventSync] Suffix: {eventData.SuffixName ?? "(none)"}");
                Console.WriteLine($"[EventSync] JSON: {json}");
                
                // Create request with Prefer header to get the created data back
                var request = new HttpRequestMessage(HttpMethod.Post, SUPABASE_URL)
                {
                    Content = content
                };
                request.Headers.Add("Prefer", "return=representation");
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    
                    // Check if we got a response body
                    if (!string.IsNullOrWhiteSpace(responseJson))
                    {
                        var createdEvent = JsonSerializer.Deserialize<SharedEventData[]>(responseJson);
                        
                        if (createdEvent != null && createdEvent.Length > 0 && createdEvent[0].Id.HasValue)
                        {
                            var createdData = createdEvent[0];
                            _processedEventIds.Add(createdData.Id.Value);
                            Console.WriteLine($"[EventSync] ? Custom event published successfully! ID: {createdData.Id.Value}");
                            
                            // ? Apply the event IMMEDIATELY to this client (don't wait for poll)
                            _gameManager.ApplyRemoteEvent(createdData);
                            Console.WriteLine($"[EventSync] ? Event applied locally!");
                            
                            // Build detailed summary
                            var summary = $"? Event erfolgreich veröffentlicht!\n\n" +
                                $"Event-ID: {createdData.Id.Value}\n" +
                                $"Event: {eventData.EventName}\n" +
                                $"Suffix: {eventData.SuffixName ?? "(kein Suffix)"}\n";
                            
                            if (eventData.LuckMultiplier.HasValue)
                                summary += $"Luck: {eventData.LuckMultiplier.Value}x\n";
                            if (eventData.MoneyMultiplier.HasValue)
                                summary += $"Money: {eventData.MoneyMultiplier.Value}x\n";
                            if (eventData.RollTime.HasValue)
                                summary += $"Roll Speed: {eventData.RollTime.Value}x\n";
                            if (eventData.SuffixBoostMultiplier.HasValue)
                                summary += $"Suffix Boost: {eventData.SuffixBoostMultiplier.Value}x\n";
                            
                            var duration = (eventData.EndsAt - eventData.StartsAt).TotalMinutes;
                            summary += $"Dauer: {duration:F1} Minuten\n\n";
                            summary += "Das Event ist jetzt bei allen Spielern aktiv!";
                            
                            System.Windows.Forms.MessageBox.Show(
                                summary,
                                "Event veröffentlicht",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information
                            );
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[EventSync] ? Custom event published successfully (no response body)");
                        
                        System.Windows.Forms.MessageBox.Show(
                            $"? Event erfolgreich veröffentlicht!\n\n" +
                            $"Event: {eventData.EventName}\n" +
                            $"Suffix: {eventData.SuffixName ?? "(kein Suffix)"}\n\n" +
                            $"Das Event wird in wenigen Sekunden bei allen Spielern aktiv!",
                            "Event veröffentlicht",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Information
                        );
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[EventSync] ? Publish failed: {response.StatusCode}");
                    Console.WriteLine($"[EventSync] Error: {error}");
                    
                    System.Windows.Forms.MessageBox.Show(
                        $"Event konnte nicht veröffentlicht werden!\n\n" +
                        $"Fehler: {response.StatusCode}\n" +
                        $"Details: {error}",
                        "Event-Sync Fehler",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] Publish ERROR: {ex.Message}");
                System.Windows.Forms.MessageBox.Show(
                    $"Event konnte nicht veröffentlicht werden!\n\n" +
                    $"Stelle sicher dass:\n" +
                    $"• Du eine Internet-Verbindung hast\n" +
                    $"• SupaBase erreichbar ist\n\n" +
                    $"Fehler: {ex.Message}",
                    "Event-Sync Fehler",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Publish a new event to SupaBase (available for all users)
        /// </summary>
        public async Task PublishEventAsync(string suffixName, double durationMinutes = 2.5)
        {
            try
            {
                var eventData = new SharedEventData
                {
                    EventName = "Suffix Event",
                    SuffixName = suffixName,
                    CreatedFrom = _username,
                    StartsAt = DateTime.UtcNow,
                    EndsAt = DateTime.UtcNow.AddMinutes(durationMinutes)
                };

                var json = JsonSerializer.Serialize(eventData, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"[EventSync] Publishing event: {suffixName} by {_username}");
                Console.WriteLine($"[EventSync] Duration: {durationMinutes} minutes");
                
                // Create request with Prefer header to get the created data back
                var request = new HttpRequestMessage(HttpMethod.Post, SUPABASE_URL)
                {
                    Content = content
                };
                request.Headers.Add("Prefer", "return=representation");
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    
                    // Check if we got a response body
                    if (!string.IsNullOrWhiteSpace(responseJson))
                    {
                        var createdEvent = JsonSerializer.Deserialize<SharedEventData[]>(responseJson);
                        
                        if (createdEvent != null && createdEvent.Length > 0 && createdEvent[0].Id.HasValue)
                        {
                            var createdData = createdEvent[0];
                            _processedEventIds.Add(createdData.Id.Value);
                            Console.WriteLine($"[EventSync] ? Event published successfully! ID: {createdData.Id.Value}");
                            
                            // ? Apply the event IMMEDIATELY to this client (don't wait for poll)
                            _gameManager.ApplyRemoteEvent(createdData);
                            Console.WriteLine($"[EventSync] ? Event applied locally!");
                            
                            // Show success message to user
                            System.Windows.Forms.MessageBox.Show(
                                $"? Event erfolgreich veröffentlicht!\n\n" +
                                $"Event-ID: {createdData.Id.Value}\n" +
                                $"Suffix: {suffixName}\n" +
                                $"Dauer: {durationMinutes} Minuten\n\n" +
                                $"Das Event ist jetzt bei allen Spielern aktiv!",
                                "Event veröffentlicht",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information
                            );
                        }
                    }
                    else
                    {
                        // Success but no response body (shouldn't happen with Prefer header, but just in case)
                        Console.WriteLine($"[EventSync] ? Event published successfully (no response body)");
                        
                        System.Windows.Forms.MessageBox.Show(
                            $"? Event erfolgreich veröffentlicht!\n\n" +
                            $"Suffix: {suffixName}\n" +
                            $"Dauer: {durationMinutes} Minuten\n\n" +
                            $"Das Event wird in wenigen Sekunden bei allen Spielern aktiv!",
                            "Event veröffentlicht",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Information
                        );
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[EventSync] ? Publish failed: {response.StatusCode}");
                    Console.WriteLine($"[EventSync] Error: {error}");
                    
                    System.Windows.Forms.MessageBox.Show(
                        $"Event konnte nicht veröffentlicht werden!\n\n" +
                        $"Fehler: {response.StatusCode}\n" +
                        $"Details: {error}",
                        "Event-Sync Fehler",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] Publish ERROR: {ex.Message}");
                System.Windows.Forms.MessageBox.Show(
                    $"Event konnte nicht veröffentlicht werden!\n\n" +
                    $"Stelle sicher dass:\n" +
                    $"• Du eine Internet-Verbindung hast\n" +
                    $"• SupaBase erreichbar ist\n\n" +
                    $"Fehler: {ex.Message}",
                    "Event-Sync Fehler",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Poll SupaBase for active events and apply all new ones (public for debugging)
        /// </summary>
        public async Task PollForEventsAsync()
        {
            try
            {
                bool shouldLog = (DateTime.Now - _lastPollTime).TotalSeconds >= 30;
                
                // Query for active events: starts_at <= now() AND ends_at > now()
                // Format: ?starts_at=lte.{now}&ends_at=gt.{now}
                // Ordered by starts_at ascending (oldest first), limit to MAX_EVENTS_TO_FETCH
                var now = DateTime.UtcNow.ToString("O"); // ISO 8601 format
                var url = $"{SUPABASE_URL}?starts_at=lte.{now}&ends_at=gt.{now}&order=starts_at.asc&limit={MAX_EVENTS_TO_FETCH}";
                
                // ALWAYS log for debugging
                Console.WriteLine($"[EventSync] ================================================");
                Console.WriteLine($"[EventSync] Polling for active events... ({DateTime.Now:HH:mm:ss})");
                Console.WriteLine($"[EventSync] Current UTC time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"[EventSync] Query URL: {url}");
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[EventSync] ? Poll failed: {response.StatusCode}");
                    Console.WriteLine($"[EventSync] ================================================");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[EventSync] Response JSON: {json}");
                var events = JsonSerializer.Deserialize<List<SharedEventData>>(json);

                if (events == null || events.Count == 0)
                {
                    Console.WriteLine($"[EventSync] No active events found");
                    Console.WriteLine($"[EventSync] ================================================");
                    _lastPollTime = DateTime.Now;
                    return;
                }

                Console.WriteLine($"[EventSync] ? Found {events.Count} active event(s)!");

                // Process all new events
                int newEventCount = 0;
                foreach (var eventData in events)
                {
                    if (!eventData.Id.HasValue) continue;
                    
                    // Skip if we already processed this event
                    if (_processedEventIds.Contains(eventData.Id.Value))
                    {
                        continue;
                    }

                    // ? NEW EVENT! Apply it now
                    Console.WriteLine($"[EventSync] ================================================");
                    Console.WriteLine($"[EventSync] ? NEW EVENT DETECTED!");
                    Console.WriteLine($"[EventSync]   ID: {eventData.Id.Value}");
                    Console.WriteLine($"[EventSync]   Suffix: {eventData.SuffixName}");
                    Console.WriteLine($"[EventSync]   Created by: {eventData.CreatedFrom}");
                    Console.WriteLine($"[EventSync]   Started: {eventData.StartsAt:HH:mm:ss} UTC");
                    Console.WriteLine($"[EventSync]   Ends: {eventData.EndsAt:HH:mm:ss} UTC");
                    Console.WriteLine($"[EventSync]   Remaining: {eventData.RemainingTime.TotalMinutes:F1} min");
                    
                    _processedEventIds.Add(eventData.Id.Value);
                    
                    // Apply the event with ALL parameters
                    _gameManager.ApplyRemoteEvent(eventData);
                    newEventCount++;
                    
                    Console.WriteLine($"[EventSync] ? Event applied!");
                    Console.WriteLine($"[EventSync] ================================================");
                }
                
                if (newEventCount > 0 && shouldLog)
                {
                    Console.WriteLine($"[EventSync] Applied {newEventCount} new event(s)");
                }
                
                // Check for ended events in database that are still active locally
                await CheckForEndedEventsAsync();
                
                _lastPollTime = DateTime.Now;
            }
            catch (HttpRequestException httpEx)
            {
                // Network error - retry on next poll
                Console.WriteLine($"[EventSync] Network error, retrying in 3s... ({httpEx.Message})");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"[EventSync] JSON Parse ERROR: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] Poll ERROR: {ex.Message}");
                Console.WriteLine($"[EventSync] Stack: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Get all currently active events (for UI display)
        /// </summary>
        public async Task<List<SharedEventData>> GetActiveEventsAsync()
        {
            try
            {
                var url = $"{SUPABASE_URL}?ends_at=gte.{DateTime.UtcNow:O}&order=id.desc";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    return new List<SharedEventData>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SharedEventData>>(json) ?? new List<SharedEventData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] GetActiveEvents ERROR: {ex.Message}");
                return new List<SharedEventData>();
            }
        }

        /// <summary>
        /// Manual sync check for debugging
        /// </summary>
        public async Task ForceSyncCheckAsync()
        {
            Console.WriteLine($"[EventSync] === MANUAL SYNC CHECK ===");
            Console.WriteLine($"[EventSync] Username: {_username}");
            Console.WriteLine($"[EventSync] SupaBase URL: {SUPABASE_URL}");
            Console.WriteLine($"[EventSync] Processed events: {_processedEventIds.Count}");
            
            
            try
            {
                var events = await GetActiveEventsAsync();
                Console.WriteLine($"[EventSync] Active events found: {events.Count}");
                
                foreach (var evt in events)
                {
                    Console.WriteLine($"[EventSync]   - ID {evt.Id}: {evt.SuffixName} by {evt.CreatedFrom}");
                    Console.WriteLine($"[EventSync]     Ends: {evt.EndsAt:HH:mm:ss}, Remaining: {evt.RemainingTime.TotalMinutes:F1} min");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] Sync check failed: {ex.Message}");
            }
            
            Console.WriteLine($"[EventSync] === END SYNC CHECK ===");
        }

        /// <summary>
        /// Check if multiplayer is connected (always true if initialized)
        /// </summary>
        public bool IsConnected => true;
        
        /// <summary>
        /// Check if any events ended early in the database and end them locally too
        /// </summary>
        private async Task CheckForEndedEventsAsync()
        {
            try
            {
                // Get list of currently active local event IDs (only multiplayer events, not local ones)
                var activeMultiplayerEventIds = _gameManager.CurrentEvents
                    .Where(e => e.EventId > 0) // Only multiplayer events (positive IDs)
                    .Select(e => e.EventId)
                    .ToList();
                
                if (activeMultiplayerEventIds.Count == 0)
                    return;
                    
                // Query database for these specific events
                foreach (var eventId in activeMultiplayerEventIds)
                {
                    var url = $"{SUPABASE_URL}?id=eq.{eventId}";
                    var response = await _httpClient.GetAsync(url);
                    
                    if (!response.IsSuccessStatusCode)
                        continue;
                        
                    var json = await response.Content.ReadAsStringAsync();
                    var events = JsonSerializer.Deserialize<List<SharedEventData>>(json);
                    
                    if (events == null || events.Count == 0)
                    {
                        // Event not found in DB anymore - end it locally
                        Console.WriteLine($"[EventSync] WARNING Event {eventId} not found in DB - ending locally");
                        _gameManager.EndEventById(eventId);
                        continue;
                    }
                    
                    var dbEvent = events[0];
                    
                    // Check if event ended early (EndsAt is in the past but we still have it active)
                    if (dbEvent.EndsAt <= DateTime.UtcNow)
                    {
                        Console.WriteLine($"[EventSync] WARNING Event {eventId} ended early in DB - ending locally");
                        Console.WriteLine($"[EventSync]   DB EndTime: {dbEvent.EndsAt:HH:mm:ss} UTC");
                        Console.WriteLine($"[EventSync]   Current Time: {DateTime.UtcNow:HH:mm:ss} UTC");
                        _gameManager.EndEventById(eventId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventSync] CheckForEndedEvents ERROR: {ex.Message}");
            }
        }



        public string Username => _username;

        public void Dispose()
        {
            _pollTimer?.Stop();
            _pollTimer?.Dispose();
            _httpClient?.Dispose();
            Console.WriteLine("[EventSync] Disposed");
        }
    }
}

