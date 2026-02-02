using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using SpinARayan.Config;
using SpinARayan.Services;

namespace SpinARayan.Forms.Dialogs
{
    public partial class SavefileSelectionForm : Form
    {
        public string? SelectedSavefileId { get; private set; }
        public bool CreateNewSavefile { get; private set; }

        private readonly List<SavefileInfo> _savefiles;
        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color BrightGreen = ModernTheme.Success;
        private readonly Color BrightGold = ModernTheme.Warning;
        private readonly Color BrightRed = ModernTheme.Error;
        private readonly Color TextColor = ModernTheme.TextPrimary;

        private Panel panelSavefiles;
        private Button btnNewSavefile;
        private Label lblTitle;

        public SavefileSelectionForm(List<SavefileInfo> savefiles)
        {
            _savefiles = savefiles;
            InitializeComponent();
            ApplyDarkMode();
            LoadSavefiles();
        }

        private void InitializeComponent()
        {
            this.Text = "Savefile auswaehlen";
            this.ClientSize = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = "\U0001F4BE Savefile auswaehlen",
                Location = new Point(20, 20),
                Size = new Size(560, 40),
                Font = new Font("Segoe UI Emoji", 16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Savefiles Panel
            panelSavefiles = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(560, 370),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(panelSavefiles);

            // New Savefile Button
            btnNewSavefile = new Button
            {
                Text = "\U00002795 Neuer Savefile",
                Location = new Point(20, 450),
                Size = new Size(560, 40),
                Font = new Font("Segoe UI Emoji", 12F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnNewSavefile.FlatAppearance.BorderSize = 0;
            btnNewSavefile.Click += BtnNewSavefile_Click;
            this.Controls.Add(btnNewSavefile);
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            lblTitle.ForeColor = BrightGold;
            panelSavefiles.BackColor = DarkBackground;
            btnNewSavefile.BackColor = BrightGreen;
            btnNewSavefile.ForeColor = Color.White;
        }

        private void LoadSavefiles()
        {
            int yPos = 10;
            
            if (_savefiles.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "Keine Savefiles vorhanden.\nErstelle einen neuen Savefile!",
                    Location = new Point(20, 150),
                    Size = new Size(520, 60),
                    Font = new Font("Segoe UI", 12F),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = TextColor
                };
                panelSavefiles.Controls.Add(lblEmpty);
                return;
            }

            foreach (var savefile in _savefiles)
            {
                var panel = CreateSavefilePanel(savefile);
                panel.Location = new Point(10, yPos);
                panelSavefiles.Controls.Add(panel);
                yPos += panel.Height + 10;
            }
        }

        private Panel CreateSavefilePanel(SavefileInfo savefile)
        {
            var panel = new Panel
            {
                Size = new Size(520, 120), // Erhöht von 100 auf 120 für mehr Platz
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DarkPanel
            };

            // Savefile Info
            var lblInfo = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Text = $"Savefile #{savefile.Id}",
                ForeColor = TextColor
            };
            panel.Controls.Add(lblInfo);

            // Stats (mehr Platz für Zeilenumbrüche)
            var lblStats = new Label
            {
                Location = new Point(10, 40),
                Size = new Size(400, 70), // Erhöht von 50 auf 70
                Font = new Font("Segoe UI", 10F),
                Text = $"Money: {FormatMoney(savefile.Money)}\n" +
                       $"Gems: {savefile.Gems}  |  Rebirths: {savefile.Rebirths}\n" +
                       $"Zuletzt gespielt: {FormatDate(savefile.LastPlayed)}",
                ForeColor = TextColor,
                AutoSize = false // Wichtig: Nicht automatisch skalieren
            };
            panel.Controls.Add(lblStats);

            // Admin Badge
            if (savefile.AdminUsed)
            {
                var lblAdmin = new Label
                {
                    Location = new Point(420, 10),
                    Size = new Size(90, 25),
                    Text = "\U000026A0 ADMIN",
                    Font = new Font("Segoe UI Emoji", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = BrightRed,
                    ForeColor = Color.White
                };
                panel.Controls.Add(lblAdmin);
            }

            // Select Button (weiter unten wegen mehr Panel-Höhe)
            var btnSelect = new Button
            {
                Location = new Point(420, 60), // von 50 auf 60
                Size = new Size(90, 40), // von 35 auf 40 (größerer Button)
                Text = "Auswaehlen",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = BrightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = savefile.Id
            };
            btnSelect.FlatAppearance.BorderSize = 0;
            btnSelect.Click += (s, e) =>
            {
                SelectedSavefileId = savefile.Id;
                CreateNewSavefile = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            panel.Controls.Add(btnSelect);

            return panel;
        }

        private void BtnNewSavefile_Click(object? sender, EventArgs e)
        {
            CreateNewSavefile = true;
            SelectedSavefileId = null;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string FormatMoney(string money)
        {
            if (System.Numerics.BigInteger.TryParse(money, out var value))
            {
                if (value < 1000) return value.ToString();
                if (value < 1000000) return ((double)value / 1000).ToString("F1") + "K";
                if (value < 1000000000) return ((double)value / 1000000).ToString("F1") + "M";
                if (value < 1000000000000) return ((double)value / 1000000000).ToString("F1") + "B";
                return value.ToString("E1");
            }
            return money;
        }

        private string FormatDate(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return "Nie";

            if (DateTime.TryParse(dateString, out var date))
            {
                var local = date.ToLocalTime();
                return local.ToString("dd.MM.yyyy HH:mm");
            }

            return dateString;
        }
    }
}
