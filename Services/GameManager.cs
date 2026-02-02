using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using SpinARayan.Models;

namespace SpinARayan.Services
{
    public class GameManager
    {
        public PlayerStats Stats { get; private set; }
        private readonly SaveService _saveService;
        private readonly DatabaseService? _databaseService;
        private readonly RollService _rollService;
        private readonly QuestService _questService;
        private EventSyncService? _eventSync;
        private System.Windows.Forms.Timer _gameTimer;
        private System.Windows.Forms.Timer _autoSaveTimer;
        private DateTime _lastUpdate;
        private DateTime _nextEventTime;
        private DateTime _lastDbSync = DateTime.MinValue;
        private List<SuffixEvent> _currentEvents = new List<SuffixEvent>();
        private readonly System.Threading.SynchronizationContext? _uiContext;
        private bool _isLoadingFromDb = false;

        public event Action? OnStatsChanged;
        public event Action<Rayan>? OnRayanRolled;
        public event Action<List<SuffixEvent>>? OnEventsChanged;

        // Admin Mode (Cheat Code)
        public bool AdminMode { get; set; } = false;
        
        // Multiplayer Mode
        public bool IsMultiplayerEnabled => _eventSync != null;
        public bool IsMultiplayerConnected => _eventSync?.IsConnected ?? false;
        public string? MultiplayerUsername => _eventSync?.Username;
        
        public List<SuffixEvent> CurrentEvents => _currentEvents.Where(e => e.IsActive).ToList();

        public GameManager(string? multiplayerUsername = null, DatabaseService? databaseService = null)
        {
            // Capture UI synchronization context for cross-thread safety
            _uiContext = System.Threading.SynchronizationContext.Current;
            
            _saveService = new SaveService();
            _rollService = new RollService();
            _questService = new QuestService();
            
            // Use provided database service (from login system) or create new one
            if (databaseService != null)
            {
                _databaseService = databaseService;
                Console.WriteLine($"[GameManager] Using provided DatabaseService");
                
                // Try to load from database with already selected savefile
                try
                {
                    Console.WriteLine($"[GameManager] Loading from database (savefile already selected)...");
                    // FIXED: Don't block UI thread - use Task.Run to execute on background thread
                    var loadTask = Task.Run(async () => await LoadFromDatabaseAsync());
                    loadTask.Wait(); // This is safer than GetAwaiter().GetResult()
                }
                catch (Exception loadEx)
                {
                    Console.WriteLine($"[GameManager] Database load failed: {loadEx.Message}");
                }
            }
            else if (!string.IsNullOrEmpty(multiplayerUsername))
            {
                // Old system: Initialize DatabaseService if multiplayer enabled
                try
                {
                    _databaseService = new DatabaseService(multiplayerUsername);
                    Console.WriteLine($"[GameManager] DatabaseService initialized for: {multiplayerUsername}");
                    
                    // Try to load from database first (don't block UI thread!)
                    try
                    {
                        // FIXED: Don't block UI thread - use Task.Run to execute on background thread
                        var loadTask = Task.Run(async () => await LoadFromDatabaseAsync());
                        loadTask.Wait(); // This is safer than GetAwaiter().GetResult()
                    }
                    catch (Exception loadEx)
                    {
                        Console.WriteLine($"[GameManager] Database load failed: {loadEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GameManager] DatabaseService initialization failed: {ex.Message}");
                    _databaseService = null;
                }
            }
            
            // Fallback to local load if no database or loading failed
            if (Stats == null)
            {
                Console.WriteLine($"[GameManager] Loading from local file (fallback)");
                Stats = _saveService.Load();
            }
            else
            {
                Console.WriteLine($"[GameManager] Stats loaded from database successfully");
            }
            
            // Load saved quest progress
            _questService.LoadQuestsFromStats(Stats);
            
            // Initialize multiplayer if username provided
            if (!string.IsNullOrEmpty(multiplayerUsername))
            {
                try
                {
                    _eventSync = new EventSyncService(this, multiplayerUsername);
                    Console.WriteLine($"[GameManager] Multiplayer enabled - Username: {multiplayerUsername}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GameManager] Multiplayer initialization failed: {ex.Message}");
                    _eventSync = null;
                }
            }
            
            _lastUpdate = DateTime.Now;
            _nextEventTime = DateTime.Now.AddMinutes(5); // Erstes Event nach 5 Minuten

            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 1000; // 1 second
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();
            
            // Setup auto-save timer for cloud sync (every 20 seconds)
            if (_databaseService != null)
            {
                _autoSaveTimer = new System.Windows.Forms.Timer();
                _autoSaveTimer.Interval = 20000; // 20 seconds
                _autoSaveTimer.Tick += AutoSaveTimer_Tick;
                _autoSaveTimer.Start();
                Console.WriteLine("[GameManager] Auto-save to database enabled (every 20s)");
            }
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            UpdateFarming();
            Stats.PlayTimeMinutes += 1.0 / 60.0;
            Stats.TotalPlayTimeMinutes += 1.0 / 60.0; // Track all-time
            _questService.UpdateProgress(Stats);
            
            // Check for event updates
            UpdateEvents();
            
            // Remove expired events and update display
            _currentEvents.RemoveAll(evt => !evt.IsActive);
            
            // Update event display every second if we have active events
            if (_currentEvents.Any(e => e.IsActive))
            {
                // THREAD-SAFE: Always invoke on UI thread
                var activeEventsList = _currentEvents.Where(e => e.IsActive).ToList();
                InvokeOnUIThread(() =>
                {
                    OnEventsChanged?.Invoke(activeEventsList);
                });
            }

            // NO LOCAL AUTO-SAVE - Database only via AutoSaveTimer (every 20s)

            // PERFORMANCE: Only trigger OnStatsChanged, don't force full UI update
            // The UI will decide what to update based on dirty flags
            OnStatsChanged?.Invoke();
        }

        public void UpdateFarming()
        {
            BigInteger incomePerSecond = 0;
            foreach (int index in Stats.EquippedRayanIndices)
            {
                if (index >= 0 && index < Stats.Inventory.Count)
                {
                    incomePerSecond += Stats.Inventory[index].TotalValue;
                }
            }

            // Apply base money multiplier + event money multipliers
            double totalMoneyMultiplier = Stats.MoneyMultiplier;
            foreach (var evt in _currentEvents.Where(e => e.IsActive))
            {
                totalMoneyMultiplier *= evt.MoneyMultiplier;
            }

            BigInteger earnedThisSecond = new BigInteger((double)incomePerSecond * totalMoneyMultiplier);
            Stats.Money += earnedThisSecond;
            Stats.TotalMoneyEarned += earnedThisSecond; // Track all-time
        }

        public void Roll()
        {
            // Get selected dice
            var selectedDice = Stats.GetSelectedDice();
            
            // Check if dice is available (for non-infinite dices)
            if (!selectedDice.IsInfinite && selectedDice.Quantity <= 0)
            {
                // No rolls left, fallback to Basic Dice
                var basicDice = Stats.OwnedDices.FirstOrDefault(d => d.IsInfinite);
                if (basicDice != null)
                {
                    Stats.SelectedDiceIndex = Stats.OwnedDices.IndexOf(basicDice);
                    selectedDice = basicDice;
                }
            }

            // Combine luck from booster, current dice, and rebirths
            double luckFromBooster = Stats.LuckMultiplier;
            double luckFromDice = selectedDice.LuckMultiplier;
            double luckFromRebirths = 1.0 + (Stats.Rebirths * 0.5); // +50% per Rebirth
            double totalLuck = luckFromBooster * luckFromDice * luckFromRebirths;
            
            // Debug output
            Console.WriteLine($"Roll - Booster Luck: {luckFromBooster:F2}, Dice Luck: {luckFromDice:F2}, Rebirth Luck: {luckFromRebirths:F2}, Total: {totalLuck:F2}");
            
            // Pass all active events to roll service
            var activeEvents = _currentEvents.Where(e => e.IsActive).ToList();
            var rayan = _rollService.Roll(totalLuck, activeEvents);
            Stats.Inventory.Add(rayan);
            Stats.TotalRolls++;
            Stats.TotalRollsAllTime++; // Track all-time
            
            // Track best rayan ever
            if (rayan.Rarity > Stats.BestRayanEverRarity)
            {
                Stats.BestRayanEverName = rayan.FullName;
                Stats.BestRayanEverRarity = rayan.Rarity;
                Stats.BestRayanEverValue = rayan.TotalValue;
            }
            
            // Reduce quantity for non-infinite dices
            if (!selectedDice.IsInfinite)
            {
                selectedDice.Quantity--;
                // Automatically switch to Basic Dice when current dice is empty
                if (selectedDice.Quantity <= 0)
                {
                    Stats.OwnedDices.Remove(selectedDice);
                    // Select Basic Dice automatically
                    var basicDice = Stats.OwnedDices.FirstOrDefault(d => d.IsInfinite);
                    if (basicDice != null)
                    {
                        Stats.SelectedDiceIndex = Stats.OwnedDices.IndexOf(basicDice);
                    }
                    else
                    {
                        Stats.SelectedDiceIndex = 0;
                    }
                }
            }
            
            OnRayanRolled?.Invoke(rayan);
            OnStatsChanged?.Invoke();
        }

        public void AutoEquipBest()
        {
            var sortedInventory = Stats.Inventory
                .Select((r, i) => new { Rayan = r, Index = i })
                .OrderByDescending(x => x.Rayan.TotalValue)
                .Take(Stats.PlotSlots)
                .ToList();

            Stats.EquippedRayanIndices = sortedInventory.Select(x => x.Index).ToList();
            OnStatsChanged?.Invoke();
        }

        public void Rebirth()
        {
            if (AdminMode || Stats.Money >= Stats.NextRebirthCost)
            {
                Stats.Money = 0;
                Stats.Inventory.Clear();
                Stats.EquippedRayanIndices.Clear();
                Stats.Rebirths++;
                Stats.TotalRebirthsAllTime++; // Track all-time
                
                // Plot Slots nur erhöhen wenn unter 10 (Maximum)
                if (Stats.PlotSlots < 10)
                {
                    Stats.PlotSlots++;
                }
                
                // Keep only Basic Dice and reset quantities
                var basicDice = Stats.OwnedDices.FirstOrDefault(d => d.IsInfinite);
                Stats.OwnedDices.Clear();
                if (basicDice != null)
                {
                    Stats.OwnedDices.Add(basicDice);
                }
                Stats.SelectedDiceIndex = 0;
                
                // Use standard Save() method which respects database-only mode
                Save();
                OnStatsChanged?.Invoke();
            }
        }

        public QuestService GetQuestService() => _questService;

        public void ToggleAutoRoll()
        {
            if (Stats.AutoRollUnlocked)
            {
                Stats.AutoRollActive = !Stats.AutoRollActive;
                OnStatsChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// Mark that admin mode was used (tracked in database)
        /// </summary>
        public void MarkAdminUsed()
        {
            _databaseService?.MarkAdminUsed();
        }

        public void Save()
        {
            // Save quest progress before saving stats
            _questService.SaveQuestsToStats();
            
            // Priority: Database-only mode when database service is available
            if (_databaseService != null)
            {
                // Async save to DB (fire and forget for performance)
                _ = SaveToDbAsync();
                // NOTE: Local save is NOT performed here - database is the primary storage
                // Local XML is only used as emergency fallback when database is unavailable
            }
            else
            {
                // Fallback to local save only if no database service
                // This should only happen in offline mode
                _saveService.Save(Stats);
                Console.WriteLine("[GameManager] WARNING: Saving to local file (offline mode)");
            }
        }
        
        /// <summary>
        /// Save to database asynchronously
        /// Database-only mode: No local fallback during normal saves
        /// </summary>
        private async Task SaveToDbAsync()
        {
            try
            {
                var success = await _databaseService!.SavePlayerDataAsync(Stats);
                if (!success)
                {
                    Console.WriteLine($"[GameManager] Database save failed (will retry on next auto-save)");
                    // Do NOT fallback to local save - database is the primary storage
                    // Local saves are only created as emergency fallback during app close (SaveSync)
                }
                else
                {
                    Console.WriteLine($"[GameManager] Database save successful");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameManager] Database save error: {ex.Message}");
                // Do NOT fallback to local save - will retry on next auto-save
            }
        }
        
        /// <summary>
        /// Save to database synchronously (for app close)
        /// Database-only mode: Local saves are only created as emergency fallback
        /// </summary>
        public void SaveSync()
        {
            Console.WriteLine("[GameManager] SaveSync started");
            
            // Save quest progress
            _questService.SaveQuestsToStats();
            
            bool databaseSaveSuccessful = false;
            bool isOfflineMode = _databaseService == null;
            
            if (_databaseService != null)
            {
                try
                {
                    Console.WriteLine("[GameManager] Starting database save...");
                    var task = Task.Run(async () => await _databaseService.SavePlayerDataAsync(Stats));
                    
                    // Wait max 5 seconds for save to complete
                    if (task.Wait(TimeSpan.FromSeconds(5)))
                    {
                        if (task.Result)
                        {
                            Console.WriteLine("[GameManager] Database save successful");
                            databaseSaveSuccessful = true;
                        }
                        else
                        {
                            Console.WriteLine("[GameManager] Database save failed (returned false)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[GameManager] Database save timeout (5s exceeded)");
                    }
                }
                catch (AggregateException ae)
                {
                    Console.WriteLine($"[GameManager] Database save error: {ae.InnerException?.Message ?? ae.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GameManager] Database save error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("[GameManager] No database service available (offline mode)");
            }
            
            // Only save to local file in offline mode OR if database save failed
            if (isOfflineMode || !databaseSaveSuccessful)
            {
                try
                {
                    _saveService.Save(Stats);
                    if (isOfflineMode)
                    {
                        Console.WriteLine("[GameManager] Local save created (offline mode)");
                    }
                    else
                    {
                        Console.WriteLine("[GameManager] WARNING: Local emergency save created (database save failed)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GameManager] Emergency local save error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("[GameManager] Local save skipped (database save successful)");
            }
            
            Console.WriteLine("[GameManager] SaveSync completed");
        }
        
        public double GetTotalLuckBonus()
        {
            // Calculate total luck bonus as percentage (excluding base 100%)
            double luckFromBooster = Stats.LuckBoosterLevel * 0.25; // 25% per level
            var selectedDice = Stats.GetSelectedDice();
            double luckFromDice = selectedDice.LuckMultiplier - 1.0; // Subtract base 1.0
            double luckFromRebirths = Stats.Rebirths * 0.5; // 50% per Rebirth
            
            // Apply event luck multipliers
            double luckFromEvents = 0.0;
            foreach (var evt in _currentEvents.Where(e => e.IsActive))
            {
                luckFromEvents += (evt.LuckMultiplier - 1.0);
            }
            
            return (luckFromBooster + luckFromDice + luckFromRebirths + luckFromEvents) * 100; // Return as percentage
        }
        
        /// <summary>
        /// Get effective roll cooldown with event modifiers applied
        /// </summary>
        public double GetEffectiveRollCooldown()
        {
            double baseCooldown = Stats.RollCooldown;
            
            // Apply event roll time modifiers
            foreach (var evt in _currentEvents.Where(e => e.IsActive))
            {
                baseCooldown *= evt.RollTimeModifier;
            }
            
            return baseCooldown;
        }
        
        public void ForceEvent()
        {
            // Admin can force an event immediately
            StartRandomEvent();
        }
        
        /// <summary>
        /// Force a SPECIFIC event and publish to multiplayer if enabled
        /// </summary>
        public async void ForceSpecificEvent(string suffixName)
        {
            // Get username
            string username = _eventSync?.Username ?? Environment.UserName;
            
            // Generate local event ID
            long eventId = DateTime.Now.Ticks; // Use timestamp as unique ID
            
            // Publish to multiplayer if enabled
            if (_eventSync != null)
            {
                await _eventSync.PublishEventAsync(suffixName, 2.5);
                Console.WriteLine($"[GameManager] Published multiplayer event: {suffixName} from {username}");
                // Note: Real DB ID will come from polling
                return; // Let polling handle the event
            }
            
            // Apply locally if no multiplayer - create basic event
            var localEvent = new SharedEventData
            {
                Id = eventId,
                SuffixName = suffixName,
                CreatedFrom = username,
                StartsAt = DateTime.UtcNow,
                EndsAt = DateTime.UtcNow.AddMinutes(2.5),
                EventName = $"{suffixName} Event!"
            };
            ApplyRemoteEvent(localEvent);
        }
        
        /// <summary>
        /// Apply remote event from multiplayer sync - Adds to active events list with full parameters!
        /// THREAD-SAFE: Can be called from background threads
        /// </summary>
        public void ApplyRemoteEvent(SharedEventData eventData)
        {
            Console.WriteLine($"[GameManager] ApplyRemoteEvent called:");
            Console.WriteLine($"[GameManager]   ID: {eventData.Id}");
            Console.WriteLine($"[GameManager]   Suffix: {eventData.SuffixName}");
            Console.WriteLine($"[GameManager]   Created by: {eventData.CreatedFrom}");
            Console.WriteLine($"[GameManager]   Luck Multiplier: {eventData.LuckMultiplier}");
            Console.WriteLine($"[GameManager]   Money Multiplier: {eventData.MoneyMultiplier}");
            Console.WriteLine($"[GameManager]   Roll Time: {eventData.RollTime}");
            Console.WriteLine($"[GameManager]   Suffix Boost: {eventData.SuffixBoostMultiplier}");
            Console.WriteLine($"[GameManager]   Multiplayer enabled: {IsMultiplayerEnabled}");
            
            // Check if this event is already in the list
            if (eventData.Id.HasValue && _currentEvents.Any(e => e.EventId == eventData.Id.Value))
            {
                Console.WriteLine($"[GameManager]   Event already exists in active list");
                return;
            }
            
            bool isOwnEvent = _eventSync != null && eventData.CreatedFrom == _eventSync.Username;
            
            // Calculate duration from timestamps
            double durationMinutes = (eventData.EndsAt - eventData.StartsAt).TotalMinutes;
            
            // Use custom suffix boost or default 20x
            double suffixBoost = eventData.SuffixBoostMultiplier ?? 20.0;
            
            var newEvent = new SuffixEvent
            {
                EventId = eventData.Id ?? DateTime.Now.Ticks,
                SuffixName = eventData.SuffixName ?? "Unknown",
                EventName = isOwnEvent ? $"{eventData.EventName}" : $"{eventData.EventName} (von {eventData.CreatedFrom})",
                CreatedBy = eventData.CreatedFrom,
                StartTime = eventData.StartsAt.ToLocalTime(),
                EndTime = eventData.EndsAt.ToLocalTime(),
                BoostMultiplier = suffixBoost,
                // Apply multipliers from event
                LuckMultiplier = eventData.LuckMultiplier ?? 1.0f,
                MoneyMultiplier = eventData.MoneyMultiplier ?? 1.0f,
                RollTimeModifier = eventData.RollTime ?? 1.0f
            };
            
            _currentEvents.Add(newEvent);
            _nextEventTime = DateTime.Now.AddMinutes(5); // Reset local event timer
            
            Console.WriteLine($"[GameManager] ? Event added to active list!");
            Console.WriteLine($"[GameManager]   Event Name: {newEvent.EventName}");
            Console.WriteLine($"[GameManager]   Duration: {durationMinutes:F1} minutes");
            Console.WriteLine($"[GameManager]   Suffix Boost: {newEvent.BoostMultiplier}x");
            Console.WriteLine($"[GameManager]   Luck Boost: {newEvent.LuckMultiplier}x");
            Console.WriteLine($"[GameManager]   Money Boost: {newEvent.MoneyMultiplier}x");
            Console.WriteLine($"[GameManager]   Roll Speed: {newEvent.RollTimeModifier}x");
            Console.WriteLine($"[GameManager]   Active events: {_currentEvents.Count(e => e.IsActive)}");
            
            // THREAD-SAFE: Invoke event on UI thread to prevent crashes
            InvokeOnUIThread(() =>
            {
                OnEventsChanged?.Invoke(_currentEvents.Where(e => e.IsActive).ToList());
            });
        }
        
        /// <summary>
        /// Helper method to safely invoke actions on UI thread
        /// </summary>
        private void InvokeOnUIThread(Action action)
        {
            if (_uiContext != null)
            {
                _uiContext.Post(_ => action(), null);
            }
            else
            {
                // Fallback: run directly if no UI context captured
                action();
            }
        }
        
        private void UpdateEvents()
        {
            // Remove expired events
            _currentEvents.RemoveAll(evt => !evt.IsActive);
            
            // Start new event every 5 minutes
            if (DateTime.Now >= _nextEventTime)
            {
                StartRandomEvent();
                _nextEventTime = DateTime.Now.AddMinutes(5);
            }
        }
        
        private void StartRandomEvent()
        {
            var random = new Random();
            var suffixes = RayanData.Suffixes;
            
            // Equal probability for all suffixes
            // Each suffix has the same chance to get an event
            int selectedIndex = random.Next(suffixes.Count);
            
            var selectedSuffix = suffixes[selectedIndex];
            
            var newEvent = new SuffixEvent
            {
                EventId = -1, // Local events get negative IDs
                SuffixName = selectedSuffix.Suffix,
                EventName = $"{selectedSuffix.Suffix} Event!",
                CreatedBy = "Local",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(2.5), // 2.5 Minuten Dauer
                BoostMultiplier = 20.0
            };
            
            _currentEvents.Add(newEvent);
            
            // THREAD-SAFE: Invoke on UI thread
            var activeEventsList = _currentEvents.Where(e => e.IsActive).ToList();
            InvokeOnUIThread(() =>
            {
                OnEventsChanged?.Invoke(activeEventsList);
            });
        }
        
        /// <summary>
        /// Auto-save timer tick - Saves to database every 20 seconds
        /// </summary>
        private async void AutoSaveTimer_Tick(object? sender, EventArgs e)
        {
            if (_databaseService == null || _isLoadingFromDb)
                return;
                
            try
            {
                // Save quest progress before syncing
                _questService.SaveQuestsToStats();
                
                // Save to database (no conflict check needed - DB is single source of truth)
                bool success = await _databaseService.SavePlayerDataAsync(Stats);
                
                if (success)
                {
                    _lastDbSync = DateTime.Now;
                    Console.WriteLine($"[GameManager] Cloud sync successful at {DateTime.Now:HH:mm:ss}");
                }
                else
                {
                    Console.WriteLine($"[GameManager] Cloud sync failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameManager] Auto-save error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load player data from database
        /// </summary>
        private async Task LoadFromDatabaseAsync()
        {
            if (_databaseService == null)
                return;
                
            try
            {
                _isLoadingFromDb = true;
                var dbStats = await _databaseService.LoadOrCreateSavefileAsync();
                
                if (dbStats != null)
                {
                    Stats = dbStats;
                    Console.WriteLine($"[GameManager] Loaded player data from database");
                }
                else
                {
                    // Loading failed, use local
                    Stats = _saveService.Load();
                    Console.WriteLine($"[GameManager] Database load failed, using local save");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameManager] Database load error: {ex.Message}");
                Stats = _saveService.Load(); // Fallback to local
            }
            finally
            {
                _isLoadingFromDb = false;
            }
        }
        
        /// <summary>
        /// Check for conflicts between local and cloud data, resolve by taking cloud version
        /// </summary>
        private async Task CheckAndResolveConflictsAsync()
        {
            if (_databaseService == null)
                return;
                
            try
            {
                var cloudStats = await _databaseService.LoadOrCreateSavefileAsync();
                
                if (cloudStats == null)
                    return;
                    
                // Compare key metrics to detect conflicts
                bool hasConflict = 
                    cloudStats.Money != Stats.Money ||
                    cloudStats.Gems != Stats.Gems ||
                    cloudStats.Rebirths != Stats.Rebirths ||
                    cloudStats.TotalRollsAllTime != Stats.TotalRollsAllTime;
                
                if (hasConflict)
                {
                    Console.WriteLine($"[GameManager] WARNING CONFLICT DETECTED!");
                    Console.WriteLine($"[GameManager]   Local  - Money: {Stats.Money}, Gems: {Stats.Gems}, Rebirths: {Stats.Rebirths}");
                    Console.WriteLine($"[GameManager]   Cloud  - Money: {cloudStats.Money}, Gems: {cloudStats.Gems}, Rebirths: {cloudStats.Rebirths}");
                    Console.WriteLine($"[GameManager]   Resolution: Using CLOUD version");
                    
                    // Show warning to user (thread-safe)
                    InvokeOnUIThread(() =>
                    {
                        System.Windows.Forms.MessageBox.Show(
                            $"Konflikt erkannt!\n\n" +
                            $"Lokale Daten weichen von Cloud-Daten ab.\n" +
                            $"Cloud-Version wird uebernommen.\n\n" +
                            $"Cloud: {FormatMoney(cloudStats.Money)} Money, {cloudStats.Gems} Gems\n" +
                            $"Lokal: {FormatMoney(Stats.Money)} Money, {Stats.Gems} Gems",
                            "Daten-Synchronisation",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Warning
                        );
                    });
                    
                    // Overwrite local with cloud data
                    Stats = cloudStats;
                    // Use standard Save() method which respects database-only mode
                    Save();
                    OnStatsChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameManager] Conflict check error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Save using standard method (respects database-only mode)
        /// </summary>
        private void SaveLocal()
        {
            _questService.SaveQuestsToStats();
            Save(); // Use standard Save() which prioritizes database
        }
        
        /// <summary>
        /// Format BigInteger for display
        /// </summary>
        private string FormatMoney(BigInteger value)
        {
            if (value < 1000) return value.ToString();
            if (value < 1000000) return ((double)value / 1000).ToString("F1") + "K";
            if (value < 1000000000) return ((double)value / 1000000).ToString("F1") + "M";
            if (value < 1000000000000) return ((double)value / 1000000000).ToString("F1") + "B";
            return value.ToString("E1");
        }
        
        /// <summary>
        /// End a specific event by ID (used for early termination from cloud sync)
        /// </summary>
        public void EndEventById(long eventId)
        {
            var eventToEnd = _currentEvents.FirstOrDefault(e => e.EventId == eventId);
            
            if (eventToEnd != null)
            {
                Console.WriteLine($"[GameManager] Ending event {eventId}: {eventToEnd.EventName}");
                
                // Set end time to now to expire it
                eventToEnd.EndTime = DateTime.Now.AddSeconds(-1);
                
                // Remove from active list
                _currentEvents.Remove(eventToEnd);
                
                // Notify UI
                InvokeOnUIThread(() =>
                {
                    OnEventsChanged?.Invoke(_currentEvents.Where(e => e.IsActive).ToList());
                });
            }
        }

    }
}
