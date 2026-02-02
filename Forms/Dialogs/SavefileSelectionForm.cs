using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpinARayan.Config;
using SpinARayan.Services;

namespace SpinARayan.Forms.Dialogs
{
    public partial class SavefileSelectionForm : Form
    {
        public string? SelectedSavefileId { get; private set; }
        public bool CreateNewSavefile { get; private set; }

        private readonly List<SavefileInfo> _savefiles;
        private readonly DatabaseService? _databaseService; // For deleting savefiles
        
        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color BrightGreen = ModernTheme.Success;
        private readonly Color BrightGold = ModernTheme.Warning;
        private readonly Color BrightRed = ModernTheme.Error;
        private readonly Color TextColor = ModernTheme.TextPrimary;

        private Panel panelSavefiles;
        private Button btnNewSavefile;
        private Label lblTitle;
        private Label lblSavefileCount;

        // Old constructor for backwards compatibility
        public SavefileSelectionForm(List<SavefileInfo> savefiles) : this(null, savefiles)
        {
        }

        // New constructor with DatabaseService (for deleting savefiles)
        public SavefileSelectionForm(DatabaseService? databaseService, List<SavefileInfo> savefiles)
        {
            _databaseService = databaseService;
            _savefiles = savefiles;
            InitializeComponent();
            ApplyDarkMode();
            LoadSavefiles();
        }

        private void InitializeComponent()
        {
            this.Text = "Savefile auswaehlen";
            this.ClientSize = new Size(600, 540);
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

            // Savefile Count Label
            lblSavefileCount = new Label
            {
                Text = $"{_savefiles.Count} / 10 Savefiles",
                Location = new Point(20, 65),
                Size = new Size(560, 20),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblSavefileCount);

            // Savefiles Panel
            panelSavefiles = new Panel
            {
                Location = new Point(20, 90),
                Size = new Size(560, 390),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(panelSavefiles);

            // New Savefile Button
            btnNewSavefile = new Button
            {
                Text = "\U00002795 Neuer Savefile",
                Location = new Point(20, 490),
                Size = new Size(560, 40),
                Font = new Font("Segoe UI Emoji", 12F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = _savefiles.Count < 10 // Disable if at limit
            };
            btnNewSavefile.FlatAppearance.BorderSize = 0;
            btnNewSavefile.Click += BtnNewSavefile_Click;
            
            if (_savefiles.Count >= 10)
            {
                btnNewSavefile.Text = "\U000026D4 Limit erreicht (10/10)";
            }
            
            this.Controls.Add(btnNewSavefile);
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            lblTitle.ForeColor = BrightGold;
            lblSavefileCount.ForeColor = TextColor;
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
                Location = new Point(320, 80), // Verschoben nach links
                Size = new Size(90, 30),
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

            // Delete Button (only if DatabaseService is available)
            if (_databaseService != null)
            {
                var btnDelete = new Button
                {
                    Location = new Point(420, 80),
                    Size = new Size(90, 30),
                    Text = "\U0001F5D1 Loeschen",
                    Font = new Font("Segoe UI Emoji", 8F, FontStyle.Bold),
                    BackColor = BrightRed,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Tag = savefile.Id
                };
                btnDelete.FlatAppearance.BorderSize = 0;
                btnDelete.Click += async (s, e) => await BtnDelete_ClickAsync(savefile.Id, panel);
                panel.Controls.Add(btnDelete);
            }

            return panel;
        }

        private async Task BtnDelete_ClickAsync(string savefileId, Panel panel)
        {
            var result = MessageBox.Show(
                $"Möchtest du Savefile #{savefileId} wirklich löschen?\n\n" +
                "Diese Aktion kann nicht rückgängig gemacht werden!",
                "Savefile löschen",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            if (result == DialogResult.Yes)
            {
                // Delete from database
                if (_databaseService != null)
                {
                    var success = await _databaseService.DeleteSavefileAsync(savefileId);

                    if (success)
                    {
                        // Remove from local list
                        _savefiles.RemoveAll(s => s.Id == savefileId);
                        
                        // Remove panel from UI
                        panelSavefiles.Controls.Remove(panel);
                        panel.Dispose();

                        // Update savefile count label
                        lblSavefileCount.Text = $"{_savefiles.Count} / 10 Savefiles";
                        
                        // Enable/disable "New Savefile" button
                        btnNewSavefile.Enabled = _savefiles.Count < 10;
                        btnNewSavefile.Text = _savefiles.Count >= 10 
                            ? "\U000026D4 Limit erreicht (10/10)"
                            : "\U00002795 Neuer Savefile";

                        // Reorganize remaining panels
                        int yPos = 10;
                        foreach (Control control in panelSavefiles.Controls)
                        {
                            if (control is Panel savefilePanel)
                            {
                                savefilePanel.Location = new Point(10, yPos);
                                yPos += savefilePanel.Height + 10;
                            }
                        }

                        MessageBox.Show(
                            $"Savefile #{savefileId} wurde erfolgreich gelöscht!",
                            "Erfolg",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                        
                        // If no savefiles left, show empty message
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
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            "Fehler beim Löschen des Savefiles!",
                            "Fehler",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
        }

        private void BtnNewSavefile_Click(object? sender, EventArgs e)
        {
            // Check limit
            if (_savefiles.Count >= 10)
            {
                MessageBox.Show(
                    "Du hast bereits 10 Savefiles!\n\n" +
                    "Bitte lösche einen alten Savefile, bevor du einen neuen erstellst.",
                    "Limit erreicht",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            
            CreateNewSavefile = true;
            SelectedSavefileId = null;
            this.DialogResult = DialogResult.Retry; // Changed from OK to Retry
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
