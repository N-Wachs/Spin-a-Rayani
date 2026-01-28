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
        private SuffixEvent? _currentEvent;

        public event Action? OnStatsChanged;
        public event Action<Rayan>? OnRayanRolled;
        public event Action<SuffixEvent>? OnEventChanged;

        // Admin Mode (Cheat Code)
        public bool AdminMode { get; set; } = false;
        
        // Multiplayer Mode
        public bool IsMultiplayerAdmin { get; private set; } = false;
        public bool IsMultiplayerConnected => _eventSync?.IsConnected ?? false;
        
        public SuffixEvent? CurrentEvent => _currentEvent;

        public GameManager(string? sharedFolderPath = null, bool isMultiplayerAdmin = false)
        {
            _saveService = new SaveService();
            _rollService = new RollService();
            _questService = new QuestService();
            Stats = _saveService.Load();
            
            // Load saved quest progress
            _questService.LoadQuestsFromStats(Stats);
            
            IsMultiplayerAdmin = isMultiplayerAdmin;
            
            // Initialize multiplayer if enabled
            if (!string.IsNullOrEmpty(sharedFolderPath))
            {
                try
                {
                    _eventSync = new EventSyncService(this, sharedFolderPath, isMultiplayerAdmin);
                    Console.WriteLine($"[GameManager] Multiplayer enabled - Role: {(isMultiplayerAdmin ? "ADMIN" : "CLIENT")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GameManager] Multiplayer disabled: {ex.Message}");
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
            
            // Update event display every second (even if event didn't change)
            if (_currentEvent != null && _currentEvent.IsActive)
            {
                OnEventChanged?.Invoke(_currentEvent);
            }

            // Autosave every 60 seconds (1 minute)
            if ((int)(Stats.PlayTimeMinutes * 60) % 60 == 0 && (int)(Stats.PlayTimeMinutes * 60) > 0)
            {
                Save();
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

            BigInteger earnedThisSecond = new BigInteger((double)incomePerSecond * Stats.MoneyMultiplier);
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
            
            var rayan = _rollService.Roll(totalLuck, _currentEvent);
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
            
            return (luckFromBooster + luckFromDice + luckFromRebirths) * 100; // Return as percentage
        }
        
        public void ForceEvent()
        {
            // Admin can force an event immediately
            StartRandomEvent();
        }
        
        /// <summary>
        /// Force a SPECIFIC event (Admin only) and publish to multiplayer if enabled
        /// </summary>
        public void ForceSpecificEvent(string suffixName)
        {
            if (!AdminMode && !IsMultiplayerAdmin)
            {
                Console.WriteLine("[GameManager] ERROR: Only admin can force specific events!");
                return;
            }
            
            // Get custom username or fallback to Windows username
            string username = string.IsNullOrEmpty(Stats.MultiplayerUsername) 
                ? Environment.UserName 
                : Stats.MultiplayerUsername;
            
            // Publish to multiplayer if enabled
            if (_eventSync != null && IsMultiplayerAdmin)
            {
                _eventSync.PublishEvent(suffixName, username);
                Console.WriteLine($"[GameManager] Published multiplayer event: {suffixName} from {username}");
            }
            
            // Apply locally
            ApplyRemoteEvent(suffixName, username);
        }
        
        /// <summary>
        /// Apply remote event from multiplayer sync - ALWAYS OVERRIDES local events!
        /// </summary>
        public void ApplyRemoteEvent(string suffixName, string adminName)
        {
            Console.WriteLine($"[GameManager] ApplyRemoteEvent called:");
            Console.WriteLine($"[GameManager]   Suffix: {suffixName}");
            Console.WriteLine($"[GameManager]   Admin: {adminName}");
            Console.WriteLine($"[GameManager]   IsMultiplayerAdmin: {IsMultiplayerAdmin}");
            
            // Check if we have an active local event that will be overridden
            if (_currentEvent != null && _currentEvent.IsActive)
            {
                Console.WriteLine($"[GameManager]   Overriding active local event: {_currentEvent.SuffixName}");
            }
            
            _currentEvent = new SuffixEvent
            {
                SuffixName = suffixName,
                EventName = IsMultiplayerAdmin ? $"{suffixName} Event!" : $"{suffixName} Event! (von {adminName})",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(2.5),
                BoostMultiplier = 20.0
            };
            
            _nextEventTime = DateTime.Now.AddMinutes(5); // Reset local event timer
            OnEventChanged?.Invoke(_currentEvent);
            
            Console.WriteLine($"[GameManager] ? Event applied successfully!");
            Console.WriteLine($"[GameManager]   Event Name: {_currentEvent.EventName}");
            Console.WriteLine($"[GameManager]   Duration: 2.5 minutes");
            Console.WriteLine($"[GameManager]   Boost: {_currentEvent.BoostMultiplier}x");
            Console.WriteLine($"[GameManager]   Ends at: {_currentEvent.EndTime:HH:mm:ss}");
        }
        
        private void UpdateEvents()
        {
            // Check if current event expired
            if (_currentEvent != null && !_currentEvent.IsActive)
            {
                _currentEvent = null;
                OnEventChanged?.Invoke(null);
            }
            
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
            
            _currentEvent = new SuffixEvent
            {
                SuffixName = selectedSuffix.Suffix,
                EventName = $"{selectedSuffix.Suffix} Event!",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(2.5), // 2.5 Minuten Dauer
                BoostMultiplier = 20.0
            };
            
            OnEventChanged?.Invoke(_currentEvent);
        }

    }
}
