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
        private System.Windows.Forms.Timer _gameTimer;
        private DateTime _lastUpdate;
        private DateTime _nextEventTime;
        private SuffixEvent? _currentEvent;

        public event Action? OnStatsChanged;
        public event Action<Rayan>? OnRayanRolled;
        public event Action<SuffixEvent>? OnEventChanged;

        // Admin Mode (Cheat Code)
        public bool AdminMode { get; set; } = false;
        
        public SuffixEvent? CurrentEvent => _currentEvent;

        public GameManager()
        {
            _saveService = new SaveService();
            _rollService = new RollService();
            _questService = new QuestService();
            Stats = _saveService.Load();
            _lastUpdate = DateTime.Now;
            _nextEventTime = DateTime.Now.AddMinutes(10);

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

            // Autosave every 10 minutes
            if ((int)(Stats.PlayTimeMinutes * 60) % 600 == 0)
            {
                _saveService.Save(Stats);
            }

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

        public void Save() => _saveService.Save(Stats);
        
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
        
        private void UpdateEvents()
        {
            // Check if current event expired
            if (_currentEvent != null && !_currentEvent.IsActive)
            {
                _currentEvent = null;
                OnEventChanged?.Invoke(null);
            }
            
            // Start new event every 10 minutes
            if (DateTime.Now >= _nextEventTime)
            {
                StartRandomEvent();
                _nextEventTime = DateTime.Now.AddMinutes(10);
            }
        }
        
        private void StartRandomEvent()
        {
            var random = new Random();
            var suffixes = RayanData.Suffixes;
            
            // Weighted selection: rarer suffixes have rarer events
            // Weight = 1 / sqrt(Chance) - so rarer suffixes have lower weights
            var weights = suffixes.Select(s => 1.0 / Math.Sqrt(s.Chance)).ToList();
            double totalWeight = weights.Sum();
            
            // Generate random number between 0 and totalWeight
            double randomValue = random.NextDouble() * totalWeight;
            
            // Select suffix based on weighted probability
            double cumulativeWeight = 0;
            int selectedIndex = 0;
            for (int i = 0; i < suffixes.Count; i++)
            {
                cumulativeWeight += weights[i];
                if (randomValue <= cumulativeWeight)
                {
                    selectedIndex = i;
                    break;
                }
            }
            
            var selectedSuffix = suffixes[selectedIndex];
            
            _currentEvent = new SuffixEvent
            {
                SuffixName = selectedSuffix.Suffix,
                EventName = $"{selectedSuffix.Suffix} Event!",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(5),
                BoostMultiplier = 5.0
            };
            
            OnEventChanged?.Invoke(_currentEvent);
        }

    }
}
