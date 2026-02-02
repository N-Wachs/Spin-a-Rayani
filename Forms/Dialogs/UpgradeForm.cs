using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SpinARayan.Models;
using SpinARayan.Services;
using System.Numerics;
using SpinARayan.Config;

namespace SpinARayan
{
    public partial class UpgradeForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Action _onUpgradeChanged;

        // Modern Theme Colors (from ModernTheme.cs)
        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color DarkAccent = ModernTheme.PrimaryMedium;
        private readonly Color BrightGreen = ModernTheme.Success;
        private readonly Color BrightBlue = ModernTheme.AccentBlue;
        private readonly Color BrightGold = ModernTheme.Warning;
        private readonly Color TextColor = ModernTheme.TextPrimary;

        public UpgradeForm(GameManager gameManager, Action onUpgradeChanged)
        {
            _gameManager = gameManager;
            _onUpgradeChanged = onUpgradeChanged;
            InitializeComponent();
            ApplyDarkMode();
            LoadUpgrades();
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            lblTitle.ForeColor = BrightGold;
            lblGems.ForeColor = BrightBlue;
            lblMoney.ForeColor = BrightGreen;
            tabControl.BackColor = DarkBackground;
            tabControl.ForeColor = TextColor;
            tabGemsUpgrades.BackColor = DarkBackground;
            tabMoneyUpgrades.BackColor = DarkBackground;

            ApplyDarkModeToContainer(tabGemsUpgrades);
            ApplyDarkModeToContainer(tabMoneyUpgrades);

            btnRefresh.BackColor = DarkAccent;
            btnRefresh.ForeColor = TextColor;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 85);
        }

        private void ApplyDarkModeToContainer(Control container)
        {
            foreach (Control control in container.Controls)
            {
                if (control is Panel panel)
                {
                    panel.BackColor = DarkPanel;
                    foreach (Control child in panel.Controls)
                    {
                        if (child is Label label)
                        {
                            label.ForeColor = TextColor;
                            label.BackColor = Color.Transparent;
                        }
                        else if (child is Button button)
                        {
                            // Button-Farbe wird in LoadUpgrades() gesetzt
                            button.ForeColor = Color.White;
                            button.FlatStyle = FlatStyle.Flat;
                            button.FlatAppearance.BorderSize = 0;
                        }
                    }
                }
            }
        }

        private void LoadUpgrades()
        {
            lblGems.Text = $"💎 Gems: {_gameManager.Stats.Gems}";
            lblMoney.Text = $"💰 Geld: {FormatBigInt(_gameManager.Stats.Money)}";
            LoadGemsUpgrades();
            LoadMoneyUpgrades();
        }

        private void LoadGemsUpgrades()
        {
            if (_gameManager.Stats.AutoRollUnlocked)
            {
                btnAutoRollUnlock.Enabled = false;
                btnAutoRollUnlock.Text = "AutoRoll freigeschaltet ✓";
                btnAutoRollUnlock.BackColor = DarkAccent;
                btnAutoRollToggle.Enabled = true;
                btnAutoRollToggle.Text = _gameManager.Stats.AutoRollActive ? "AutoRoll: AN" : "AutoRoll: AUS";
                btnAutoRollToggle.BackColor = BrightBlue;
            }
            else
            {
                btnAutoRollUnlock.Enabled = _gameManager.AdminMode || _gameManager.Stats.Gems >= 100;
                btnAutoRollUnlock.Text = _gameManager.AdminMode ? 
                    "AutoRoll freischalten (GRATIS)" : 
                    $"AutoRoll freischalten (100 Gems)";
                btnAutoRollUnlock.BackColor = btnAutoRollUnlock.Enabled ? BrightBlue : DarkAccent;
                btnAutoRollToggle.Enabled = false;
                btnAutoRollToggle.Text = "AutoRoll: Gesperrt";
                btnAutoRollToggle.BackColor = DarkAccent;
            }
            UpdateRollCooldownInfo();
        }

        private void LoadMoneyUpgrades()
        {
            UpdateLuckBoosterInfo();
            UpdateSkipNextRebirthInfo();
        }

        private void UpdateLuckBoosterInfo()
        {
            int currentLevel = _gameManager.Stats.LuckBoosterLevel;
            double currentLuck = _gameManager.Stats.LuckMultiplier;
            BigInteger nextLevelCost = CalculateLuckBoosterCost(currentLevel);
            btnBuyLuckBooster.Text = _gameManager.AdminMode ?
                "Luck +25% kaufen (GRATIS)" :
                $"Luck +25% kaufen ({FormatBigInt(nextLevelCost)} Money)";
            btnBuyLuckBooster.Enabled = _gameManager.AdminMode || _gameManager.Stats.Money >= nextLevelCost;
            btnBuyLuckBooster.BackColor = btnBuyLuckBooster.Enabled ? BrightGreen : DarkAccent;
            lblLuckBooster.Text = $"Luck Multiplier: {currentLuck:F2}x (Level {currentLevel})";
        }

        private void UpdateRollCooldownInfo()
        {
            double currentCooldown = _gameManager.Stats.RollCooldown;
            int currentLevel = _gameManager.Stats.RollCooldownLevel;
            int nextLevelCost = CalculateRollCooldownCost(currentLevel);
            
            // Calculate next cooldown (-0.2s per upgrade, minimum 0.5s)
            double nextCooldown = Math.Max(0.5, currentCooldown - 0.2);
            
            btnReduceCooldown.Text = _gameManager.AdminMode ?
                $"Cooldown → {nextCooldown:F1}s (GRATIS)" :
                $"Cooldown → {nextCooldown:F1}s ({nextLevelCost} Gems)";
            btnReduceCooldown.Enabled = (_gameManager.AdminMode || _gameManager.Stats.Gems >= nextLevelCost) && currentCooldown > 0.5;
            btnReduceCooldown.BackColor = btnReduceCooldown.Enabled ? BrightBlue : DarkAccent;
            lblRollCooldown.Text = $"Roll Cooldown: {currentCooldown:F1}s (Level {currentLevel})";
        }

        private BigInteger CalculatePlotSlotCost(int currentSlots)
        {
            return BigInteger.Pow(10, currentSlots - 2) * 1000;
        }

        private BigInteger CalculateLuckBoosterCost(int level)
        {
            if (level == 0) return 5000;
            return new BigInteger(5000 * Math.Pow(1.5, level));
        }

        private int CalculateRollCooldownCost(int level)
        {
            // Start: 200 Gems, then *1.5 per level
            if (level == 0) return 200;
            return (int)(200 * Math.Pow(1.5, level));
        }

        private string FormatBigInt(BigInteger value)
        {
            if (value < 1000) return value.ToString();
            if (value < 1000000) return ((double)value / 1000).ToString("F1") + "K";
            if (value < 1000000000) return ((double)value / 1000000).ToString("F1") + "M";
            if (value < 1000000000000) return ((double)value / 1000000000).ToString("F1") + "B";
            return value.ToString("E1");
        }

        private void btnAutoRollUnlock_Click(object sender, EventArgs e)
        {
            if (_gameManager.AdminMode || _gameManager.Stats.Gems >= 100)
            {
                if (!_gameManager.AdminMode)
                {
                    _gameManager.Stats.Gems -= 100;
                }
                _gameManager.Stats.AutoRollUnlocked = true;
                _gameManager.Save();
                LoadUpgrades();
                _onUpgradeChanged?.Invoke();
            }
        }

        private void btnAutoRollToggle_Click(object sender, EventArgs e)
        {
            if (_gameManager.Stats.AutoRollUnlocked)
            {
                _gameManager.ToggleAutoRoll();
                LoadUpgrades();
                _onUpgradeChanged?.Invoke();
            }
        }

        private void btnBuyLuckBooster_Click(object sender, EventArgs e)
        {
            BigInteger cost = CalculateLuckBoosterCost(_gameManager.Stats.LuckBoosterLevel);
            if (_gameManager.AdminMode || _gameManager.Stats.Money >= cost)
            {
                if (!_gameManager.AdminMode)
                {
                    _gameManager.Stats.Money -= cost;
                }
                _gameManager.Stats.LuckBoosterLevel++;
                _gameManager.Save();
                LoadUpgrades();
                _onUpgradeChanged?.Invoke();
            }
        }

        private void btnReduceCooldown_Click(object sender, EventArgs e)
        {
            int cost = CalculateRollCooldownCost(_gameManager.Stats.RollCooldownLevel);
            if ((_gameManager.AdminMode || _gameManager.Stats.Gems >= cost) && _gameManager.Stats.RollCooldown > 0.5)
            {
                if (!_gameManager.AdminMode)
                {
                    _gameManager.Stats.Gems -= cost;
                }
                _gameManager.Stats.RollCooldownLevel++;
                
                // Reduce cooldown by 0.2s, minimum 0.5s
                _gameManager.Stats.RollCooldown = Math.Max(0.5, _gameManager.Stats.RollCooldown - 0.2);
                
                _gameManager.Save();
                LoadUpgrades();
                _onUpgradeChanged?.Invoke();
            }
        }

        private void btnBuySkipNextRebirth_Click(object sender, EventArgs e)
        {
            BigInteger cost = CalculateSkipNextRebirthCost();
            
            // Confirmation dialog
            var result = MessageBox.Show(
                $"⚡ Skip Next Rebirth kaufen?\n\n" +
                $"Kosten: {FormatBigInt(cost)} Money\n\n" +
                $"Effekte:\n" +
                $"✅ Beim nächsten Rebirth: +2 Rebirths statt +1\n" +
                $"❌ SOFORT: Verliert alle Dices (außer Basic)\n" +
                $"❌ SOFORT: Verliert alle Rayans\n" +
                $"💰 Zieht nur {FormatBigInt(cost)} ab, nicht den Rebirth-Preis\n\n" +
                $"Möchtest du fortfahren?",
                "Skip Next Rebirth",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            
            if (result == DialogResult.Yes)
            {
                if (_gameManager.AdminMode || _gameManager.Stats.Money >= cost)
                {
                    // Deduct money
                    if (!_gameManager.AdminMode)
                    {
                        _gameManager.Stats.Money -= cost;
                    }
                    
                    // Clear inventory and dices immediately
                    _gameManager.Stats.Inventory.Clear();
                    _gameManager.Stats.EquippedRayanIndices.Clear();
                    
                    var basicDice = _gameManager.Stats.OwnedDices.FirstOrDefault(d => d.IsInfinite);
                    _gameManager.Stats.OwnedDices.Clear();
                    if (basicDice != null)
                    {
                        _gameManager.Stats.OwnedDices.Add(basicDice);
                    }
                    _gameManager.Stats.SelectedDiceIndex = 0;
                    
                    // Set flag for next rebirth
                    _gameManager.Stats.SkipNextRebirth = true;
                    
                    _gameManager.Save();
                    LoadUpgrades();
                    _onUpgradeChanged?.Invoke();
                    
                    MessageBox.Show(
                        "✅ Skip Next Rebirth aktiviert!\n\n" +
                        "Beim nächsten Rebirth erhältst du +2 Rebirths!",
                        "Erfolg",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUpgrades();
        }

        private void UpdateSkipNextRebirthInfo()
        {
            BigInteger cost = CalculateSkipNextRebirthCost();
            
            if (_gameManager.Stats.SkipNextRebirth)
            {
                // Already purchased, show active state
                btnBuySkipNextRebirth.Text = "✅ AKTIV - Nächster Rebirth: +2";
                btnBuySkipNextRebirth.Enabled = false;
                btnBuySkipNextRebirth.BackColor = BrightGreen;
                lblSkipNextRebirth.Text = "✅ Upgrade aktiv! Beim nächsten Rebirth erhältst du +2 Rebirths.\n" +
                                          "Das Upgrade wurde bereits bezahlt.\n" +
                                          "Nach dem Rebirth wird dieser Effekt zurückgesetzt.";
            }
            else
            {
                btnBuySkipNextRebirth.Text = _gameManager.AdminMode ?
                    $"Upgrade kaufen (GRATIS)" :
                    $"Upgrade kaufen ({FormatBigInt(cost)} Money)";
                btnBuySkipNextRebirth.Enabled = _gameManager.AdminMode || _gameManager.Stats.Money >= cost;
                btnBuySkipNextRebirth.BackColor = btnBuySkipNextRebirth.Enabled ? BrightGold : DarkAccent;
                lblSkipNextRebirth.Text = $"Kosten: 60% Aufpreis vom aktuellen Rebirth-Preis.\n" +
                                          $"Beim nächsten Rebirth erhältst du +2 Rebirths statt +1.\n" +
                                          $"⚠️ Verliert alle Dices (außer Basic) und Rayans SOFORT!\n" +
                                          $"💰 Zieht nur {FormatBigInt(cost)} ab, nicht den Rebirth-Preis.";
            }
        }

        private BigInteger CalculateSkipNextRebirthCost()
        {
            // 60% Aufpreis vom aktuellen Rebirth-Preis
            return _gameManager.Stats.NextRebirthCost * 16 / 10; // 1.6x = 160%
        }
    }
}