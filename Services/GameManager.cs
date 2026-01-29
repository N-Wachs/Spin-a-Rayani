using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SpinARayan.Models;

namespace SpinARayan.Services
{
    public class GameManager
    {
        public PlayerStats Stats { get; private set; }
        public bool AdminMode { get; set; } = false;

        private readonly SaveService _saveService;
        private readonly RollService _rollService;
        private readonly ShopService _shopService;
        private readonly QuestService _questService;
        private System.Windows.Forms.Timer _gameTimer;
        
        private DateTime _nextEventTime;
        private SuffixEvent? _currentEvent;
        private bool _eventDrop1Triggered = false;
        private bool _eventDrop2Triggered = false;

        public event Action? OnStatsChanged;
        public event Action<Rayan>? OnRayanRolled;
        public event Action<SuffixEvent?>? OnEventChanged;

        public SuffixEvent? CurrentEvent => _currentEvent;

        public GameManager()
        {
            _saveService = new SaveService();
            _rollService = new RollService();
            _shopService = new ShopService();
            _questService = new QuestService();
            
            Stats = _saveService.Load();
            
            _nextEventTime = DateTime.Now.AddMinutes(5);

            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 1000;
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            UpdateFarming();
            Stats.PlayTimeMinutes += 1.0 / 60.0;
            _questService.UpdateProgress(Stats);
            UpdateEvents();

            if (_currentEvent != null && _currentEvent.IsActive)
            {
                OnEventChanged?.Invoke(_currentEvent);
                
                double elapsedMinutes = (DateTime.Now - _currentEvent.StartTime).TotalMinutes;
                if (elapsedMinutes >= 1.25 && !_eventDrop1Triggered)
                {
                    TriggerGuaranteedDrop();
                    _eventDrop1Triggered = true;
                }
                if (elapsedMinutes >= 2.5 && !_eventDrop2Triggered)
                {
                    TriggerGuaranteedDrop();
                    _eventDrop2Triggered = true;
                }
            }

            if ((int)(Stats.PlayTimeMinutes * 60) % 600 == 0)
            {
                _saveService.Save(Stats);
            }

            OnStatsChanged?.Invoke();
        }

        private void TriggerGuaranteedDrop()
        {
            if (_currentEvent == null) return;
            var suffix = RayanData.Suffixes.FirstOrDefault(s => s.Suffix == _currentEvent.SuffixName);
            var prefix = RayanData.Prefixes[new Random().Next(RayanData.Prefixes.Count)];
            var guaranteedRayan = new Rayan 
            { 
                Prefix = prefix.Prefix, 
                Rarity = prefix.Rarity, 
                BaseValue = prefix.BaseValue, 
                Suffix = suffix.Suffix, 
                Multiplier = suffix.Multiplier 
            };
            Stats.Inventory.Add(guaranteedRayan);
            OnRayanRolled?.Invoke(guaranteedRayan);
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
            Stats.Money += new BigInteger((double)incomePerSecond * Stats.MoneyMultiplier);
        }

        public void Roll()
        {
            double totalLuck = Stats.LuckMultiplier * Stats.CurrentDiceLuck;
            var rayan = _rollService.Roll(totalLuck, _currentEvent);
            Stats.Inventory.Add(rayan);
            Stats.TotalRolls++;
            OnRayanRolled?.Invoke(rayan);
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
                if (Stats.PlotSlots < 10) Stats.PlotSlots++;
                _saveService.Save(Stats);
                OnStatsChanged?.Invoke();
            }
        }

        private void UpdateEvents()
        {
            if (_currentEvent != null && !_currentEvent.IsActive)
            {
                _currentEvent = null;
                OnEventChanged?.Invoke(null);
            }
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
            int selectedIndex = random.Next(suffixes.Count);
            var selectedSuffix = suffixes[selectedIndex];
            
            _currentEvent = new SuffixEvent
            {
                SuffixName = selectedSuffix.Suffix,
                EventName = $"{selectedSuffix.Suffix} Event!",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(2.5),
                BoostMultiplier = 20.0
            };
            _eventDrop1Triggered = false;
            _eventDrop2Triggered = false;
            OnEventChanged?.Invoke(_currentEvent);
        }

        public void Save() => _saveService.Save(Stats);
        public ShopService GetShopService() => _shopService;
        public QuestService GetQuestService() => _questService;
        public void ToggleAutoRoll() => Stats.AutoRollActive = !Stats.AutoRollActive;
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
    }
}
