using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;

namespace SpinARayan.Models
{
    public class PlayerStats
    {
        [XmlIgnore]
        public BigInteger Money { get; set; } = 0;

        [XmlElement("Money")]
        public string MoneyString
        {
            get => Money.ToString();
            set => Money = BigInteger.Parse(string.IsNullOrEmpty(value) ? "0" : value);
        }

        public int Gems { get; set; } = 0;
        public int TotalRolls { get; set; } = 0;
        public int Rebirths { get; set; } = 0;
        public int PlotSlots { get; set; } = 3;
        public double RollCooldown { get; set; } = 2.0;
        public int RollCooldownLevel { get; set; } = 0;
        public int LuckBoosterLevel { get; set; } = 0;
        public bool AutoRollUnlocked { get; set; } = false;
        public bool AutoRollActive { get; set; } = false;
        public double PlayTimeMinutes { get; set; } = 0;
        
        // All-Time Stats (NEVER reset by Rebirth)
        [XmlIgnore]
        public BigInteger TotalMoneyEarned { get; set; } = 0;
        
        [XmlElement("TotalMoneyEarned")]
        public string TotalMoneyEarnedString
        {
            get => TotalMoneyEarned.ToString();
            set => TotalMoneyEarned = BigInteger.Parse(string.IsNullOrEmpty(value) ? "0" : value);
        }
        
        public int TotalRollsAllTime { get; set; } = 0;
        public int TotalRebirthsAllTime { get; set; } = 0;
        public double TotalPlayTimeMinutes { get; set; } = 0;
        
        // Best Rayan Ever
        public string BestRayanEverName { get; set; } = "";
        public double BestRayanEverRarity { get; set; } = 0;
        
        [XmlIgnore]
        public BigInteger BestRayanEverValue { get; set; } = 0;
        
        [XmlElement("BestRayanEverValue")]
        public string BestRayanEverValueString
        {
            get => BestRayanEverValue.ToString();
            set => BestRayanEverValue = BigInteger.Parse(string.IsNullOrEmpty(value) ? "0" : value);
        }

        public List<Rayan> Inventory { get; set; } = new List<Rayan>();
        public List<int> EquippedRayanIndices { get; set; } = new List<int>(); // Indices in Inventory
        
        // Dice System
        public List<Dice> OwnedDices { get; set; } = new List<Dice>();
        public int SelectedDiceIndex { get; set; } = 0; // Index in OwnedDices, 0 = Basic Dice
        
        // Quest System - Save quest progress
        public List<Quest> SavedQuests { get; set; } = new List<Quest>();
        
        // Multiplayer System
        public string MultiplayerUsername { get; set; } = "";
        
        // Skip Next Rebirth - Upgrade that grants +2 Rebirths on next rebirth
        public bool SkipNextRebirth { get; set; } = false;


        public double MoneyMultiplier => 1.0 + (Rebirths * 4.0);
        public double LuckMultiplier => 1.0 + (LuckBoosterLevel * 0.25);
        
        // Current Dice Luck Multiplier (from selected dice)
        public double CurrentDiceLuck => SelectedDiceIndex >= 0 && SelectedDiceIndex < OwnedDices.Count 
            ? OwnedDices[SelectedDiceIndex].LuckMultiplier 
            : 1.0;

        // Get selected dice or fallback to Basic
        public Dice GetSelectedDice()
        {
            if (SelectedDiceIndex >= 0 && SelectedDiceIndex < OwnedDices.Count)
            {
                return OwnedDices[SelectedDiceIndex];
            }
            // Fallback: Return Basic Dice if not found
            return OwnedDices.FirstOrDefault(d => d.IsInfinite) ?? new Dice 
            { 
                Name = "Basic Dice", 
                LuckMultiplier = 1.0, 
                IsInfinite = true, 
                Quantity = 0 
            };
        }

        [XmlIgnore]
        public BigInteger NextRebirthCost => BigInteger.Pow(8, Rebirths) * 100000;

        [XmlElement("NextRebirthCost")]
        public string NextRebirthCostString => NextRebirthCost.ToString();
    }
}
