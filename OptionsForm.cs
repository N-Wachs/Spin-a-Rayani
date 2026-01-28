using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using SpinARayan.Models;
using SpinARayan.Services;

namespace SpinARayan
{
    public partial class OptionsForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Action _onOptionsChanged;
        
        // Dark Mode Colors
        private readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
        private readonly Color DarkPanel = Color.FromArgb(45, 45, 48);
        private readonly Color DarkAccent = Color.FromArgb(60, 60, 65);
        private readonly Color BrightGreen = Color.FromArgb(0, 255, 127);
        private readonly Color BrightBlue = Color.FromArgb(0, 174, 255);
        private readonly Color BrightGold = Color.FromArgb(255, 215, 0);
        private readonly Color BrightRed = Color.FromArgb(255, 69, 58);
        private readonly Color TextColor = Color.FromArgb(230, 230, 230);

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
            lblEquipped.Text = $"Ausgerüstet: {stats.EquippedRayanIndices.Count}";
            
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
                "?? WARNUNG ??\n\n" +
                "Möchtest du wirklich deinen KOMPLETTEN Spielstand löschen?\n\n" +
                "Das löscht:\n" +
                "- Alle Rayans\n" +
                "- Alles Geld & Gems\n" +
                "- Alle Rebirths\n" +
                "- Alle Upgrades\n" +
                "- Alle Dices\n" +
                "- Alle Quest-Fortschritte\n\n" +
                "DIESE AKTION KANN NICHT RÜCKGÄNGIG GEMACHT WERDEN!",
                "Spielstand zurücksetzen",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            if (result == DialogResult.Yes)
            {
                // Double confirmation
                var doubleCheck = MessageBox.Show(
                    "Bist du dir ABSOLUT SICHER?\n\nDies ist deine letzte Chance!",
                    "Letzte Bestätigung",
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
                "? Spielstand wurde erfolgreich zurückgesetzt!\n\nDas Spiel wird nun geschlossen.",
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
                Size = new Size(560, 200),
                BackColor = DarkPanel,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var lblMpTitle = new Label
            {
                Text = "?? Multiplayer Einstellungen",
                Location = new Point(10, 10),
                Size = new Size(540, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = BrightBlue
            };
            mpPanel.Controls.Add(lblMpTitle);
            
            // Username
            var lblUsername = new Label
            {
                Text = "Dein Name (für Event-Anzeige):",
                Location = new Point(10, 50),
                Size = new Size(220, 25),
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextColor
            };
            mpPanel.Controls.Add(lblUsername);
            
            var txtUsername = new TextBox
            {
                Location = new Point(240, 48),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F),
                BackColor = DarkAccent,
                ForeColor = TextColor,
                Text = string.IsNullOrEmpty(_gameManager.Stats.MultiplayerUsername) 
                    ? Environment.UserName 
                    : _gameManager.Stats.MultiplayerUsername
            };
            mpPanel.Controls.Add(txtUsername);
            
            var btnSaveUsername = new Button
            {
                Text = "??",
                Location = new Point(450, 48),
                Size = new Size(90, 25),
                Font = new Font("Segoe UI", 10F),
                BackColor = BrightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSaveUsername.FlatAppearance.BorderSize = 0;
            btnSaveUsername.Click += (s, e) =>
            {
                _gameManager.Stats.MultiplayerUsername = txtUsername.Text.Trim();
                _gameManager.Save();
                MessageBox.Show(
                    $"? Username gespeichert: {txtUsername.Text}\n\n" +
                    "Dein Name wird jetzt in Events angezeigt!",
                    "Gespeichert",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };
            mpPanel.Controls.Add(btnSaveUsername);
            
            // Multiplayer Config Button
            var btnConfigMP = new Button
            {
                Text = "?? Multiplayer konfigurieren",
                Location = new Point(10, 90),
                Size = new Size(530, 40),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = BrightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConfigMP.FlatAppearance.BorderSize = 0;
            btnConfigMP.Click += (s, e) =>
            {
                using var setupDialog = new MultiplayerSetupDialog();
                var result = setupDialog.ShowDialog();
                
                if (result == DialogResult.OK && setupDialog.SetupCompleted)
                {
                    MessageBox.Show(
                        "? Multiplayer-Einstellungen gespeichert!\n\n" +
                        "Starte das Spiel neu, damit die Änderungen wirksam werden.",
                        "Neustart erforderlich",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            };
            mpPanel.Controls.Add(btnConfigMP);
            
            // MP Status Display
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "multiplayer.txt");
            bool mpEnabled = File.Exists(configPath);
            
            var lblMpStatus = new Label
            {
                Text = mpEnabled 
                    ? $"? Multiplayer aktiv ({(_gameManager.IsMultiplayerAdmin ? "Admin" : "Client")})" 
                    : "?? Multiplayer inaktiv (Single-Player)",
                Location = new Point(10, 140),
                Size = new Size(530, 25),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = mpEnabled ? BrightGreen : Color.Gray
            };
            mpPanel.Controls.Add(lblMpStatus);
            
            if (mpEnabled && _gameManager.IsMultiplayerConnected)
            {
                var lblMpInfo = new Label
                {
                    Text = "?? Verbunden - Events werden synchronisiert!",
                    Location = new Point(10, 165),
                    Size = new Size(530, 20),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = BrightBlue
                };
                mpPanel.Controls.Add(lblMpInfo);
            }
            
            this.Controls.Add(mpPanel);
            
            // Adjust form size to accommodate multiplayer settings
            this.Height = Math.Max(this.Height, 660);
        }
    }
}
