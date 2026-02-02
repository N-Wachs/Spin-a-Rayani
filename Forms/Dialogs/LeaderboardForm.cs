using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SpinARayan.Config;
using SpinARayan.Services;

namespace SpinARayan.Forms.Dialogs
{
    public partial class LeaderboardForm : Form
    {
        private readonly DatabaseService _databaseService;
        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color BrightGold = ModernTheme.Warning;
        private readonly Color BrightGreen = ModernTheme.Success;
        private readonly Color BrightBlue = ModernTheme.AccentBlue;
        private readonly Color TextColor = ModernTheme.TextPrimary;
        private readonly Color TextSecondary = ModernTheme.TextSecondary;
        
        private ComboBox? _comboCategory;
        private Panel? _leaderboardPanel;
        private Label? _lblLoading;
        
        public LeaderboardForm(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            InitializeComponent();
            ApplyDarkMode();
            LoadLeaderboard();
        }
        
        private void InitializeComponent()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi; // DPI-Aware: Skalierung an DPI anpassen
            this.Text = "🏆 Leaderboard";
            this.ClientSize = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimumSize = new Size(600, 500); // DPI-Aware: Minimale Größe
            this.BackColor = DarkBackground;
            
            // Title
            var lblTitle = new Label
            {
                Text = "🏆 LEADERBOARD",
                Location = new Point(20, 20),
                Size = new Size(760, 40),
                Font = new Font("Segoe UI Emoji", 18F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = BrightGold,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);
            
            // Category selection
            var lblCategory = new Label
            {
                Text = "Kategorie:",
                Location = new Point(20, 75),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = TextColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblCategory);
            
            _comboCategory = new ComboBox
            {
                Location = new Point(130, 75),
                Size = new Size(640, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = DarkPanel,
                ForeColor = TextColor,
                FlatStyle = FlatStyle.Flat
            };
            _comboCategory.Items.AddRange(new object[]
            {
                "💰 Meistes Geld",
                "🎲 Seltenster Rayan",
                "?? Meiste Rolls",
                "?? Meiste Gems",
                "?? Meiste Spielzeit",
                "?? Meiste Rayans"
            });
            _comboCategory.SelectedIndex = 0;
            _comboCategory.SelectedIndexChanged += ComboCategory_SelectedIndexChanged;
            this.Controls.Add(_comboCategory);
            
            // Leaderboard panel
            _leaderboardPanel = new Panel
            {
                Location = new Point(20, 120),
                Size = new Size(760, 520),
                BackColor = DarkPanel,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            this.Controls.Add(_leaderboardPanel);
            
            // Loading label
            _lblLoading = new Label
            {
                Text = "Lade Daten...",
                Location = new Point(300, 250),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 12F),
                ForeColor = TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _leaderboardPanel.Controls.Add(_lblLoading);
            
            // Close button
            var btnClose = new Button
            {
                Text = "Schlie�en",
                Location = new Point(650, 650),
                Size = new Size(130, 40),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = ModernTheme.PrimaryMedium,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
            
            // Info label
            var lblInfo = new Label
            {
                Text = "?? Nur legitime Spieler (ohne Admin-Modus) werden angezeigt",
                Location = new Point(20, 655),
                Size = new Size(600, 20),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = TextSecondary
            };
            this.Controls.Add(lblInfo);
        }
        
        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
        }
        
        private async void LoadLeaderboard()
        {
            if (_lblLoading != null)
                _lblLoading.Visible = true;
                
            try
            {
                var category = _comboCategory?.SelectedIndex ?? 0;
                var entries = await _databaseService.GetLeaderboardAsync(category);
                
                DisplayLeaderboard(entries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LeaderboardForm] Error loading leaderboard: {ex.Message}");
                MessageBox.Show(
                    "Fehler beim Laden des Leaderboards!\n\n" + ex.Message,
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                if (_lblLoading != null)
                    _lblLoading.Visible = false;
            }
        }
        
        private void DisplayLeaderboard(List<LeaderboardEntry> entries)
        {
            if (_leaderboardPanel == null)
                return;
                
            _leaderboardPanel.Controls.Clear();
            
            if (entries.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "Keine Eintr�ge gefunden",
                    Location = new Point(280, 250),
                    Size = new Size(200, 30),
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = TextSecondary,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                _leaderboardPanel.Controls.Add(lblEmpty);
                return;
            }
            
            int yPos = 10;
            int rank = 1;
            
            foreach (var entry in entries.Take(50)) // Top 50
            {
                var entryPanel = CreateLeaderboardEntry(entry, rank);
                entryPanel.Location = new Point(10, yPos);
                _leaderboardPanel.Controls.Add(entryPanel);
                
                yPos += entryPanel.Height + 5;
                rank++;
            }
        }
        
        private Panel CreateLeaderboardEntry(LeaderboardEntry entry, int rank)
        {
            var panel = new Panel
            {
                Size = new Size(720, 60),
                BackColor = rank <= 3 ? GetRankColor(rank) : Color.FromArgb(48, 48, 48),
                BorderStyle = BorderStyle.None
            };
            
            // Rank
            var lblRank = new Label
            {
                Text = GetRankEmoji(rank) + rank.ToString(),
                Location = new Point(10, 15),
                Size = new Size(60, 30),
                Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panel.Controls.Add(lblRank);
            
            // Username
            var lblUsername = new Label
            {
                Text = entry.Username,
                Location = new Point(80, 10),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.Controls.Add(lblUsername);
            
            // Value
            var lblValue = new Label
            {
                Text = entry.ValueFormatted,
                Location = new Point(80, 32),
                Size = new Size(600, 20),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(189, 189, 189),
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.Controls.Add(lblValue);
            
            return panel;
        }
        
        private Color GetRankColor(int rank)
        {
            return rank switch
            {
                1 => Color.FromArgb(255, 215, 0),   // Gold
                2 => Color.FromArgb(192, 192, 192), // Silver
                3 => Color.FromArgb(205, 127, 50),  // Bronze
                _ => Color.FromArgb(48, 48, 48)
            };
        }
        
        private string GetRankEmoji(int rank)
        {
            return rank switch
            {
                1 => "?? ",
                2 => "?? ",
                3 => "?? ",
                _ => ""
            };
        }
        
        private void ComboCategory_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadLeaderboard();
        }
    }
    
    public class LeaderboardEntry
    {
        public string Username { get; set; } = "";
        public string ValueFormatted { get; set; } = "";
        public long RawValue { get; set; }
    }
}
