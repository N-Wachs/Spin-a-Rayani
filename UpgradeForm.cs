using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SpinARayan.Models;
using SpinARayan.Services;
using System.Numerics;

namespace SpinARayan
{
    public partial class UpgradeForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Action _onUpgradeChanged;

        // Dark Mode Colors
        private readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
        private readonly Color DarkPanel = Color.FromArgb(45, 45, 48);
        private readonly Color DarkAccent = Color.FromArgb(60, 60, 65);
        private readonly Color BrightGreen = Color.FromArgb(0, 255, 127);
        private readonly Color BrightBlue = Color.FromArgb(0, 174, 255);
        private readonly Color BrightGold = Color.FromArgb(255, 215, 0);
        private readonly Color TextColor = Color.FromArgb(230, 230, 230);

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
            UpdatePlotSlotInfo();
            UpdateLuckBoosterInfo();
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

        private void UpdatePlotSlotInfo()
        {
            int currentSlots = _gameManager.Stats.PlotSlots;
            const int maxSlots = 10;
            
            lblPlotSlots.Text = $"Plot Slots: {currentSlots} / {maxSlots} (Maximum)";
            btnBuyPlotSlot.Text = "Nur durch Rebirth erhöhbar!";
            btnBuyPlotSlot.Enabled = false;
            btnBuyPlotSlot.BackColor = DarkAccent;
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

        private void btnBuyPlotSlot_Click(object sender, EventArgs e)
        {
            // Plot Slots can only be increased through Rebirths
            // This button is disabled
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUpgrades();
        }
    }
}