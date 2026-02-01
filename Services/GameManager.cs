using System;
using System.Linq;
using System.Numerics;
using SpinARayan.Models;

namespace SpinARayan.Services
{
    public class GameManager
    {
        public PlayerStats Stats { get; private set; }
        private readonly SaveService _saveService;
        private readonly RollService _rollService;
        private readonly QuestService _questService;
        private EventSyncService? _eventSync;
        private System.Windows.Forms.Timer _gameTimer;
        private DateTime _lastUpdate;
        private DateTime _nextEventTime;
        private List<SuffixEvent> _currentEvents = new List<SuffixEvent>();
        private readonly System.Threading.SynchronizationContext? _uiContext;

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

        public GameManager(string? multiplayerUsername = null)
        {
            // Capture UI synchronization context for cross-thread safety
            _uiContext = System.Threading.SynchronizationContext.Current;
            
            _saveService = new SaveService();
            _rollService = new RollService();
            _questService = new QuestService();
            Stats = _saveService.Load();
            
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

            // Autosave every 20 seconds for data safety
            if ((int)(Stats.PlayTimeMinutes * 60) % 20 == 0 && (int)(Stats.PlayTimeMinutes * 60) > 0)
            {
                Save();
                Console.WriteLine($"[GameManager] Auto-saved at {DateTime.Now:HH:mm:ss}");
            }

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
                
                _saveService.Save(Stats);
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

        public void Save()
        {
            // Save quest progress before saving stats
            _questService.SaveQuestsToStats();
            _saveService.Save(Stats);
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

    }
}
