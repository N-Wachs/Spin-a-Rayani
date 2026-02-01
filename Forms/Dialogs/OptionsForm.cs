using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using SpinARayan.Models;
using SpinARayan.Services;
using SpinARayan.Config;

namespace SpinARayan
{
    public partial class OptionsForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Action _onOptionsChanged;
        
        // Modern Theme Colors (from ModernTheme.cs)
        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color DarkAccent = ModernTheme.PrimaryMedium;
        private readonly Color BrightGreen = ModernTheme.Success;
        private readonly Color BrightBlue = ModernTheme.AccentBlue;
        private readonly Color BrightGold = ModernTheme.Warning;
        private readonly Color BrightRed = ModernTheme.Error;
        private readonly Color TextColor = ModernTheme.TextPrimary;

        public OptionsForm(GameManager gameManager, Action onOptionsChanged)
        {
            _gameManager = gameManager;
            _onOptionsChanged = onOptionsChanged;
            InitializeComponent();
            ApplyDarkMode();
            AddMultiplayerSettings();
            LoadStatistics();
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            
            foreach (Control control in this.Controls)
            {
                if (control is Panel panel)
                {
                    panel.BackColor = DarkPanel;
                    foreach (Control child in panel.Controls)
                    {
                        if (child is Label label)
                        {
                            label.ForeColor = TextColor;
                        }
                    }
                }
                else if (control is Label label)
                {
                    label.ForeColor = label.Name == "lblTitle" ? BrightGold : TextColor;
                }
            }
        }

        private void LoadStatistics()
        {
            var stats = _gameManager.Stats;
            
            // All-Time Stats (NEVER reset by Rebirth)
            lblTotalRolls.Text = $"Total Rolls: {stats.TotalRollsAllTime:N0}";
            lblPlayTime.Text = $"Spielzeit: {FormatPlayTime(stats.TotalPlayTimeMinutes)}";
            lblRebirths.Text = $"Total Rebirths: {stats.TotalRebirthsAllTime}";
            
            // Currency (All-Time)
            lblMoney.Text = $"Verdient (gesamt): {FormatBigInt(stats.TotalMoneyEarned)}";
            lblGems.Text = $"Gems (aktuell): {stats.Gems}";
            
            // Inventory Stats (Current)
            lblInventoryCount.Text = $"Rayans im Inventar: {stats.Inventory.Count:N0}";
            var uniqueRayans = stats.Inventory
                .GroupBy(r => new { r.Prefix, r.Suffix })
                .Count();
            lblUniqueRayans.Text = $"Einzigartige Typen: {uniqueRayans}";
            
            // Best Rayan Ever (All-Time)
            if (!string.IsNullOrEmpty(stats.BestRayanEverName))
            {
                lblBestRayan.Text = $"Bester Rayan (jemals): {stats.BestRayanEverName}";
                lblBestRarity.Text = $"Rarity: 1 in {stats.BestRayanEverRarity:N0}";
                lblBestValue.Text = $"Wert: {FormatBigInt(stats.BestRayanEverValue)}/s";
            }
            else
            {
                lblBestRayan.Text = "Bester Rayan (jemals): -";
                lblBestRarity.Text = "Rarity: -";
                lblBestValue.Text = "Wert: -";
            }
            
            // Plot Stats (Current)
            lblPlotSlots.Text = $"Plot Slots: {stats.PlotSlots}/10";
            lblEquipped.Text = $"Ausger�stet: {stats.EquippedRayanIndices.Count}";
            
            // Income Stats (Current)
            BigInteger totalIncome = 0;
            foreach (int index in stats.EquippedRayanIndices)
            {
                if (index >= 0 && index < stats.Inventory.Count)
                {
                    totalIncome += stats.Inventory[index].TotalValue;
                }
            }
            BigInteger actualIncome = new BigInteger((double)totalIncome * stats.MoneyMultiplier);
            lblTotalIncome.Text = $"Total Income: {FormatBigInt(actualIncome)}/s";
            lblMoneyMultiplier.Text = $"Money Multiplier: {stats.MoneyMultiplier:F1}x";
            
            // Luck Stats (Current)
            lblLuckMultiplier.Text = $"Luck Multiplier: {stats.LuckMultiplier:F2}x";
            lblLuckBoosterLevel.Text = $"Luck Booster Level: {stats.LuckBoosterLevel}";
            
            // Dice Stats (Current)
            lblDicesOwned.Text = $"Dices besessen: {stats.OwnedDices.Count}";
            var selectedDice = stats.GetSelectedDice();
            lblCurrentDice.Text = $"Aktueller Dice: {selectedDice.Name}";
        }

        private string FormatPlayTime(double minutes)
        {
            if (minutes < 60)
                return $"{minutes:F0} Minuten";
            
            double hours = minutes / 60;
            if (hours < 24)
                return $"{hours:F1} Stunden";
            
            double days = hours / 24;
            return $"{days:F1} Tage";
        }

        private string FormatBigInt(BigInteger value)
        {
            if (value < 1000) return value.ToString();
            if (value < 1000000) return ((double)value / 1000).ToString("F1") + "K";
            if (value < 1000000000) return ((double)value / 1000000).ToString("F1") + "M";
            if (value < 1000000000000) return ((double)value / 1000000000).ToString("F1") + "B";
            return value.ToString("E1");
        }

        private void btnResetGame_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "\U000026A0 WARNUNG \U000026A0\n\n" +
                "Moechtest du wirklich deinen KOMPLETTEN Spielstand loeschen?\n\n" +
                "Das loescht:\n" +
                "- Alle Rayans\n" +
                "- Alles Geld & Gems\n" +
                "- Alle Rebirths\n" +
                "- Alle Upgrades\n" +
                "- Alle Dices\n" +
                "- Alle Quest-Fortschritte\n\n" +
                "DIESE AKTION KANN NICHT RUECKGAENGIG GEMACHT WERDEN!",
                "Spielstand zuruecksetzen",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            if (result == DialogResult.Yes)
            {
                // Double confirmation
                var doubleCheck = MessageBox.Show(
                    "Bist du dir ABSOLUT SICHER?\n\nDies ist deine letzte Chance!",
                    "Letzte Best�tigung",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2
                );

                if (doubleCheck == DialogResult.Yes)
                {
                    ResetGame();
                }
            }
        }

        private void ResetGame()
        {
            // Create fresh stats
            _gameManager.Stats.Money = 0;
            _gameManager.Stats.Gems = 0;
            _gameManager.Stats.Rebirths = 0;
            _gameManager.Stats.PlotSlots = 3;
            _gameManager.Stats.TotalRolls = 0;
            _gameManager.Stats.PlayTimeMinutes = 0;
            
            // Reset All-Time Stats too
            _gameManager.Stats.TotalMoneyEarned = 0;
            _gameManager.Stats.TotalRollsAllTime = 0;
            _gameManager.Stats.TotalRebirthsAllTime = 0;
            _gameManager.Stats.TotalPlayTimeMinutes = 0;
            _gameManager.Stats.BestRayanEverName = "";
            _gameManager.Stats.BestRayanEverRarity = 0;
            _gameManager.Stats.BestRayanEverValue = 0;
            
            _gameManager.Stats.Inventory.Clear();
            _gameManager.Stats.EquippedRayanIndices.Clear();
            _gameManager.Stats.OwnedDices.Clear();
            
            _gameManager.Stats.LuckBoosterLevel = 0;
            _gameManager.Stats.RollCooldownLevel = 0;
            _gameManager.Stats.AutoRollUnlocked = false;
            _gameManager.Stats.AutoRollActive = false;
            
            // Reset quests
            var questService = _gameManager.GetQuestService();
            foreach (var quest in questService.Quests)
            {
                quest.BaseProgress = 0;
                quest.CurrentProgress = 0;
                quest.IsCompleted = false;
                quest.IsClaimed = false;
            }
            
            // Save and notify
            _gameManager.Save();
            _onOptionsChanged?.Invoke();
            
            MessageBox.Show(
                "? Spielstand wurde erfolgreich zur�ckgesetzt!\n\nDas Spiel wird nun geschlossen.",
                "Reset erfolgreich",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            
            // Close options form and restart game
            this.Close();
            Application.Restart();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void AddMultiplayerSettings()
        {
            // Create Multiplayer Settings Panel
            var mpPanel = new Panel
            {
                Location = new Point(20, 420),
                Size = new Size(560, 160),
                BackColor = DarkPanel,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var lblMpTitle = new Label
            {
                Text = "\U00002699\U0000FE0F Multiplayer Einstellungen",
                Location = new Point(10, 10),
                Size = new Size(540, 30),
                Font = new Font("Segoe UI Emoji", 12F, FontStyle.Bold),
                ForeColor = BrightBlue
            };
            mpPanel.Controls.Add(lblMpTitle);
            
            // Multiplayer Status
            bool mpEnabled = _gameManager.IsMultiplayerEnabled;
            
            var lblMpStatus = new Label
            {
                Text = mpEnabled 
                    ? $"\U00002705 Multiplayer aktiv - Username: {_gameManager.MultiplayerUsername}" 
                    : "\U0000274C Multiplayer inaktiv",
                Location = new Point(10, 50),
                Size = new Size(530, 25),
                Font = new Font("Segoe UI Emoji", 9F, FontStyle.Bold),
                ForeColor = mpEnabled ? BrightGreen : Color.Gray
            };
            mpPanel.Controls.Add(lblMpStatus);
            
            if (mpEnabled)
            {
                var lblMpInfo = new Label
                {
                    Text = "\U00002139\U0000FE0F Events werden automatisch mit allen Spielern synchronisiert!",
                    Location = new Point(10, 80),
                    Size = new Size(530, 20),
                    Font = new Font("Segoe UI Emoji", 8F),
                    ForeColor = BrightBlue
                };
                mpPanel.Controls.Add(lblMpInfo);
                
                var lblMpHint = new Label
                {
                    Text = "\U00002139\U0000FE0F Hinweis: Multiplayer-Einstellung wird beim Start festgelegt",
                    Location = new Point(10, 105),
                    Size = new Size(530, 20),
                    Font = new Font("Segoe UI Emoji", 8F, FontStyle.Italic),
                    ForeColor = TextColor
                };
                mpPanel.Controls.Add(lblMpHint);
            }
            else
            {
                var lblMpHint = new Label
                {
                    Text = "\U0001F4A1 Tipp: Starte das Spiel neu, um Multiplayer zu aktivieren.\n    Du wirst nach deinem Username gefragt.",
                    Location = new Point(10, 80),
                    Size = new Size(530, 40),
                    Font = new Font("Segoe UI Emoji", 8F),
                    ForeColor = TextColor
                };
                mpPanel.Controls.Add(lblMpHint);
            }
            
            this.Controls.Add(mpPanel);
            
            // Adjust form size to accommodate multiplayer settings
            this.Height = Math.Max(this.Height, 660);
        }
    }
}
