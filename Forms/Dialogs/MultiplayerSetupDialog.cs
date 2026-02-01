using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SpinARayan.Config;

namespace SpinARayan
{
    public class MultiplayerSetupDialog : Form
    {
        private TextBox txtFolder;
        private RadioButton rbAdmin;
        private RadioButton rbClient;
        private Button btnSave;
        private Button btnSkip;
        private Button btnBrowse;
        
        // Modern Theme Colors (from ModernTheme.cs)
        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color DarkAccent = ModernTheme.PrimaryMedium;
        private readonly Color BrightBlue = ModernTheme.AccentBlue;
        private readonly Color BrightGold = ModernTheme.Warning;
        private readonly Color TextColor = ModernTheme.TextPrimary;

        public string? SharedFolder { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool SetupCompleted { get; private set; }

        public MultiplayerSetupDialog()
        {
            InitializeComponent();
            ApplyDarkMode();
        }

        private void InitializeComponent()
        {
            this.Text = "Multiplayer Setup - Spin a Rayan";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title
            var lblTitle = new Label
            {
                Text = "🌐 Multiplayer Konfiguration",
                Location = new Point(20, 20),
                Size = new Size(550, 40),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = BrightGold
            };
            this.Controls.Add(lblTitle);

            // Description
            var lblDesc = new Label
            {
                Text = "M�chtest du Multiplayer aktivieren?\n" +
                       "Admin kann Events f�r alle Spieler starten, Clients empfangen Events.",
                Location = new Point(20, 70),
                Size = new Size(550, 50),
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextColor
            };
            this.Controls.Add(lblDesc);

            // Role Selection
            var lblRole = new Label
            {
                Text = "W�hle deine Rolle:",
                Location = new Point(20, 140),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextColor
            };
            this.Controls.Add(lblRole);

            rbAdmin = new RadioButton
            {
                Text = "Admin (Ich starte Events)",
                Location = new Point(40, 170),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextColor,
                Checked = true
            };
            this.Controls.Add(rbAdmin);

            rbClient = new RadioButton
            {
                Text = "Client (Ich empfange Events)",
                Location = new Point(40, 200),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextColor
            };
            this.Controls.Add(rbClient);

            // Folder Path
            var lblFolder = new Label
            {
                Text = "OneDrive/Dropbox Ordner:",
                Location = new Point(20, 240),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextColor
            };
            this.Controls.Add(lblFolder);

            txtFolder = new TextBox
            {
                Location = new Point(20, 270),
                Size = new Size(460, 30),
                Font = new Font("Segoe UI", 10F),
                BackColor = DarkPanel,
                ForeColor = TextColor,
                Text = GetDefaultOneDrivePath()
            };
            this.Controls.Add(txtFolder);

            btnBrowse = new Button
            {
                Text = "📁",
                Location = new Point(490, 270),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 12F),
                BackColor = DarkAccent,
                ForeColor = TextColor,
                FlatStyle = FlatStyle.Flat
            };
            btnBrowse.FlatAppearance.BorderSize = 1;
            btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(92, 107, 192); // PrimaryLight
            btnBrowse.Click += BtnBrowse_Click;
            this.Controls.Add(btnBrowse);

            // Helper text
            var lblHelper = new Label
            {
                Text = "Beispiel: C:\\Users\\[DeinName]\\OneDrive\\Anwendungen\\Spin a Rayan",
                Location = new Point(20, 305),
                Size = new Size(550, 20),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblHelper);

            // Save Button
            btnSave = new Button
            {
                Text = "✓ Multiplayer aktivieren",
                Location = new Point(20, 350),
                Size = new Size(270, 50),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = BrightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 1;
            btnSave.FlatAppearance.BorderColor = Color.FromArgb(66, 165, 245); // AccentBlueLight
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Skip Button
            btnSkip = new Button
            {
                Text = "Später / Single-Player",
                Location = new Point(300, 350),
                Size = new Size(270, 50),
                Font = new Font("Segoe UI", 11F),
                BackColor = DarkAccent,
                ForeColor = TextColor,
                FlatStyle = FlatStyle.Flat
            };
            btnSkip.FlatAppearance.BorderSize = 1;
            btnSkip.FlatAppearance.BorderColor = Color.FromArgb(92, 107, 192); // PrimaryLight
            btnSkip.Click += BtnSkip_Click;
            this.Controls.Add(btnSkip);
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
        }

        private string GetDefaultOneDrivePath()
        {
            // Try to detect OneDrive path automatically
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string oneDrivePath = Path.Combine(userProfile, "OneDrive", "Anwendungen", "Spin a Rayan");
            
            if (Directory.Exists(Path.Combine(userProfile, "OneDrive")))
            {
                return oneDrivePath;
            }
            
            // Try Dropbox
            string dropboxPath = Path.Combine(userProfile, "Dropbox", "Anwendungen", "Spin a Rayan");
            if (Directory.Exists(Path.Combine(userProfile, "Dropbox")))
            {
                return dropboxPath;
            }
            
            // Default fallback
            return oneDrivePath;
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "W�hle deinen OneDrive/Dropbox Ordner f�r Multiplayer:",
                ShowNewFolderButton = true
            };

            if (!string.IsNullOrEmpty(txtFolder.Text))
            {
                folderDialog.SelectedPath = txtFolder.Text;
            }

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = folderDialog.SelectedPath;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string folder = txtFolder.Text.Trim();

            if (string.IsNullOrEmpty(folder))
            {
                MessageBox.Show(
                    "Bitte gib einen Ordner-Pfad an!",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Try to create folder if it doesn't exist
            try
            {
                if (!Directory.Exists(folder))
                {
                    var result = MessageBox.Show(
                        $"Der Ordner existiert nicht:\n{folder}\n\nSoll er erstellt werden?",
                        "Ordner erstellen?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        Directory.CreateDirectory(folder);
                        MessageBox.Show(
                            "? Ordner erfolgreich erstellt!",
                            "Erfolg",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Erstellen des Ordners:\n{ex.Message}\n\n" +
                    "Bitte erstelle den Ordner manuell oder w�hle einen anderen Pfad.",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Save settings
            SharedFolder = folder;
            IsAdmin = rbAdmin.Checked;
            SetupCompleted = true;

            // Create multiplayer.txt
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "multiplayer.txt");
                string configContent = $"# Multiplayer Config - Auto-Generated\n" +
                                      $"# Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                                      $"FOLDER={folder}\n" +
                                      $"ADMIN={IsAdmin.ToString().ToLower()}\n\n" +
                                      $"# Role: {(IsAdmin ? "ADMIN" : "CLIENT")}\n" +
                                      $"# {(IsAdmin ? "Du kannst Events starten mit 'E' oder 'M'" : "Du empf�ngst Events vom Admin")}\n";

                File.WriteAllText(configPath, configContent);

                MessageBox.Show(
                    $"? Multiplayer erfolgreich konfiguriert!\n\n" +
                    $"Rolle: {(IsAdmin ? "Admin (Events starten)" : "Client (Events empfangen)")}\n" +
                    $"Ordner: {folder}\n\n" +
                    $"Config gespeichert unter:\n{configPath}\n\n" +
                    (IsAdmin ? "Dr�cke 'E' oder 'M' im Spiel um Events zu starten!" : "Warte auf Events vom Admin!"),
                    "Setup abgeschlossen",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Speichern der Config:\n{ex.Message}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnSkip_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Multiplayer-Setup �berspringen?\n\n" +
                "Du kannst es sp�ter aktivieren indem du\n" +
                "eine 'multiplayer.txt' Datei erstellst.",
                "Setup �berspringen",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                SetupCompleted = false;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
    }
}
