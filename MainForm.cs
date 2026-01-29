using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Numerics;
using SpinARayan.Models;
using SpinARayan.Services;
using System.Reflection;

namespace SpinARayan
{
    public partial class MainForm : Form
    {
        private readonly GameManager _gameManager;
        private System.Windows.Forms.Timer _rollCooldownTimer;
        private System.Windows.Forms.Timer _autoRollTimer;
        private double _rollCooldownRemaining = 0;
        private bool _plotDisplayInitialized = false;
        private List<Panel> _plotPanels = new List<Panel>();
        private Panel? _totalIncomePanel;
        private Label? _lblDiceInfo; // Shows selected dice and quantity
        private PictureBox? _picDiceIcon; // Zeigt Würfelbild an
        private Label? _lblEventDisplay; // Shows current suffix event
        private Label? _lblRebirthBonus; // Shows rebirth bonus
        
        // PERFORMANCE: Dirty flags to avoid unnecessary UI updates
        private bool _plotsDirty = true;
        private bool _inventoryDirty = false;
        private BigInteger _lastMoney = 0;
        private int _lastGems = 0;
        private int _lastRebirths = 0;
        private int _lastLuckBoosterLevel = 0;

        // Dark Mode Colors
        private readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
        private readonly Color DarkPanel = Color.FromArgb(45, 45, 48);
        private readonly Color DarkAccent = Color.FromArgb(60, 60, 65);
        private readonly Color BrightGreen = Color.FromArgb(0, 255, 127);
        private readonly Color BrightBlue = Color.FromArgb(0, 174, 255);
        private readonly Color BrightGold = Color.FromArgb(255, 215, 0);
        private readonly Color BrightRed = Color.FromArgb(255, 69, 58);
        private readonly Color TextColor = Color.FromArgb(230, 230, 230);

        // Cheat Code Detection
        private List<Keys> _cheatSequence = new List<Keys>();
        private DateTime _lastCheatKeyPress = DateTime.MinValue;
        private readonly TimeSpan _cheatKeyTimeout = TimeSpan.FromSeconds(2);

        public MainForm()
        {
            InitializeComponent();

            // Apply Dark Mode
            ApplyDarkMode();

            // Enable double buffering
            SetDoubleBuffered(panelLeft);
            SetDoubleBuffered(panelCenter);
            SetDoubleBuffered(panelRight);

            // MULTIPLAYER CONFIG: Auto-setup or load existing
            string? multiplayerFolder = null;
            bool isMultiplayerAdmin = false;
            
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "multiplayer.txt");
            
            // Check if config exists
            if (!File.Exists(configPath))
            {
                // First run! Show setup dialog
                using var setupDialog = new MultiplayerSetupDialog();
                var result = setupDialog.ShowDialog();
                
                if (result == DialogResult.OK && setupDialog.SetupCompleted)
                {
                    multiplayerFolder = setupDialog.SharedFolder;
                    isMultiplayerAdmin = setupDialog.IsAdmin;
                    Console.WriteLine($"[MainForm] Multiplayer configured via dialog:");
                    Console.WriteLine($"  Folder: {multiplayerFolder}");
                    Console.WriteLine($"  Admin: {isMultiplayerAdmin}");
                }
                else
                {
                    Console.WriteLine("[MainForm] Multiplayer setup skipped - Single-Player mode");
                }
            }
            else
            {
                // Load existing config
                try
                {
                    var lines = File.ReadAllLines(configPath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("FOLDER=", StringComparison.OrdinalIgnoreCase))
                            multiplayerFolder = line.Substring(7).Trim();
                        else if (line.StartsWith("ADMIN=", StringComparison.OrdinalIgnoreCase))
                            isMultiplayerAdmin = bool.Parse(line.Substring(6).Trim());
                    }
                    
                    if (!string.IsNullOrEmpty(multiplayerFolder))
                    {
                        Console.WriteLine($"[MainForm] Multiplayer loaded from config:");
                        Console.WriteLine($"  Folder: {multiplayerFolder}");
                        Console.WriteLine($"  Admin: {isMultiplayerAdmin}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MainForm] Error loading multiplayer config: {ex.Message}");
                }
            }

            _gameManager = new GameManager(multiplayerFolder, isMultiplayerAdmin);
            _gameManager.OnStatsChanged += UpdateUI;
            _gameManager.OnRayanRolled += OnRayanRolled_Handler;
            _gameManager.OnEventChanged += UpdateEventDisplay;
            
            // Initialize cached values to FORCE first update
            _lastMoney = -1; // Force update on first call
            _lastGems = -1; // Force update on first call
            _lastRebirths = -1; // Force update on first call
            _lastLuckBoosterLevel = -1; // Force update on first call

            _rollCooldownTimer = new System.Windows.Forms.Timer();
            _rollCooldownTimer.Interval = 100;
            _rollCooldownTimer.Tick += RollCooldownTimer_Tick;

            _autoRollTimer = new System.Windows.Forms.Timer();
            _autoRollTimer.Interval = 100;
            _autoRollTimer.Tick += AutoRollTimer_Tick;

            CreateDiceInfoLabel();
            CreateEventDisplay();
            
            // Move Rebirth button to right panel (bottom)
            panelCenter.Controls.Remove(btnRebirth);
            btnRebirth.Location = new Point(20, 580);
            btnRebirth.Size = new Size(260, 80);
            btnRebirth.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnRebirth.Text = $"Rebirth\n{FormatBigInt(_gameManager.Stats.NextRebirthCost)}";
            btnRebirth.Enabled = _gameManager.AdminMode || _gameManager.Stats.Money >= _gameManager.Stats.NextRebirthCost;
            panelRight.Controls.Add(btnRebirth);
            
            UpdateUI();

            // Version-Check beim Start
            CheckForUpdates();

            // Enable KeyPreview for cheat code detection
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
        }

        private async void CheckForUpdates()
        {
            try
            {
                var versionService = new VersionService();
                bool updateAvailable = await versionService.CheckForUpdatesAsync();

                if (updateAvailable)
                {
                    var result = MessageBox.Show(
                        versionService.GetUpdateMessage(),
                        "Update verfügbar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                // Fehler beim Version-Check - einfach ignorieren und weiterspielen
                Console.WriteLine($"Version check error: {ex.Message}");
            }
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            // Check if too much time has passed since last key press
            if ((DateTime.Now - _lastCheatKeyPress) > _cheatKeyTimeout)
            {
                _cheatSequence.Clear();
            }

            _lastCheatKeyPress = DateTime.Now;
            _cheatSequence.Add(e.KeyCode);

            // Keep only last 3 keys
            if (_cheatSequence.Count > 3)
            {
                _cheatSequence.RemoveAt(0);
            }

            // Check for cheat code: , - .
            if (_cheatSequence.Count == 3 &&
                _cheatSequence[0] == Keys.Oemcomma &&  // ,
                _cheatSequence[1] == Keys.OemMinus &&  // -
                _cheatSequence[2] == Keys.OemPeriod)   // .
            {
                ActivateAdminMode();
                _cheatSequence.Clear();
            }
            
            // Admin: Press 'E' to force an event
            if (_gameManager.AdminMode && e.KeyCode == Keys.E)
            {
                // Show event selection dialog
                ShowEventSelectionDialog();
                e.Handled = true;
            }
            
            // Multiplayer Admin: Press 'M' to force a multiplayer event
            if (_gameManager.IsMultiplayerAdmin && e.KeyCode == Keys.M)
            {
                ShowEventSelectionDialog();
                e.Handled = true;
            }
        }

        private void ActivateAdminMode()
        {
            _gameManager.AdminMode = !_gameManager.AdminMode;
            UpdateUI();
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            panelLeft.BackColor = DarkPanel;
            panelCenter.BackColor = DarkBackground;
            panelRight.BackColor = DarkPanel;

            // Style all labels
            foreach (Control control in this.Controls)
            {
                ApplyDarkModeToControl(control);
            }
        }

        private void ApplyDarkModeToControl(Control control)
        {
            if (control is Label label)
            {
                label.ForeColor = TextColor;
                label.BackColor = Color.Transparent;
            }
            else if (control is Panel panel)
            {
                panel.BackColor = panel.Name == "panelCenter" ? DarkBackground : DarkPanel;
                foreach (Control child in panel.Controls)
                {
                    ApplyDarkModeToControl(child);
                }
            }
            else if (control is Button button && button.Name != "btnRoll")
            {
                button.BackColor = DarkAccent;
                button.ForeColor = TextColor;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 85);
                button.FlatAppearance.BorderSize = 1;
            }
        }

        private void CreateDiceInfoLabel()
        {
            // Position label centered above roll button
            int labelWidth = 180;
            int labelX = (panelCenter.Width - labelWidth) / 2;
            
            // Würfelbild hinzufügen (links neben dem Label)
            _picDiceIcon = new PictureBox
            {
                Location = new Point(labelX - 45, 150),
                Size = new Size(40, 40),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            _picDiceIcon.Click += (s, e) => OpenDiceSelection();
            panelCenter.Controls.Add(_picDiceIcon);
            
            _lblDiceInfo = new Label
            {
                Location = new Point(labelX, 150),
                Size = new Size(labelWidth, 50),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = BrightBlue,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopCenter,
                Text = "Click to select dice"
            };
            _lblDiceInfo.Click += (s, e) => OpenDiceSelection();
            _lblDiceInfo.Cursor = Cursors.Hand;
            _lblDiceInfo.BringToFront();
            panelCenter.Controls.Add(_lblDiceInfo);
        }

        private void OpenDiceSelection()
        {
            var diceSelectionForm = new DiceSelectionForm(_gameManager, () =>
            {
                UpdateDiceInfo();
                UpdateUI();
            });
            diceSelectionForm.ShowDialog();
        }
        
        private void CreateEventDisplay()
        {
            _lblEventDisplay = new Label
            {
                Location = new Point(0, 0),
                Size = new Size(1200, 40),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "",
                Visible = false
            };
            this.Controls.Add(_lblEventDisplay);
            _lblEventDisplay.BringToFront();
        }
        
        private void UpdateEventDisplay(SuffixEvent? suffixEvent)
        {
            if (_lblEventDisplay == null) return;
            
            if (suffixEvent != null && suffixEvent.IsActive)
            {
                int minutes = (int)suffixEvent.TimeRemaining.TotalMinutes;
                int seconds = suffixEvent.TimeRemaining.Seconds;
                
                string adminPrefix = _gameManager.AdminMode ? "[ADMIN] " : "";
                _lblEventDisplay.Text = $"{adminPrefix}🔥 {suffixEvent.EventName} - {suffixEvent.SuffixName} 20x häufiger! ({minutes}:{seconds:D2} verbleibend)";
                _lblEventDisplay.BackColor = GetEventColor(suffixEvent.SuffixName);
                _lblEventDisplay.Visible = true;
            }
            else
            {
                _lblEventDisplay.Visible = false;
            }
        }
        
        private Color GetEventColor(string suffixName)
        {
            return suffixName switch
            {
                // Tier 1: Common (helle/mittlere Farben)
                "Selbstbewusst" => Color.FromArgb(0, 100, 180),  // Dark Blue
                "GC" => Color.FromArgb(180, 100, 0),             // Dark Orange
                "Blessed" => Color.FromArgb(0, 150, 150),        // Dark Cyan
                "Shiny" => Color.FromArgb(180, 180, 0),          // Dark Yellow
                "Cursed" => Color.FromArgb(80, 0, 80),           // Dark Purple
                
                // Tier 2: Uncommon
                "SSL" => Color.FromArgb(0, 150, 100),            // Dark Teal
                "Radiant" => Color.FromArgb(200, 150, 0),        // Bright Gold
                "Shadow" => Color.FromArgb(50, 50, 50),          // Very Dark Gray
                "Golden" => Color.FromArgb(150, 120, 0),         // Dark Gold
                "Mystic" => Color.FromArgb(120, 0, 150),         // Dark Magenta
                
                // Tier 3: Rare (intensive Farben)
                "Cosmic" => Color.FromArgb(0, 50, 150),          // Deep Blue
                "Void" => Color.FromArgb(100, 0, 150),           // Dark Purple
                "Divine" => Color.FromArgb(180, 180, 180),       // Silver
                "Infernal" => Color.FromArgb(180, 0, 0),         // Dark Red
                
                // Tier 4: Epic (sehr intensive Farben)
                "Primordial" => Color.FromArgb(0, 100, 0),       // Dark Green
                "Ancient" => Color.FromArgb(150, 0, 0),          // Dark Red
                "Transcendent" => Color.FromArgb(150, 100, 200), // Light Purple
                
                // Tier 5: Legendary (spektakuläre Farben)
                "Legendary" => Color.FromArgb(200, 100, 0),      // Orange-Gold
                "Eternal" => Color.FromArgb(100, 0, 100),        // Dark Magenta
                "Omega" => Color.FromArgb(200, 0, 200),          // Bright Magenta
                
                // Tier 6: Ultra-Legendary (astronomische Farben)
                "Unstoppable" => Color.FromArgb(255, 50, 0),     // Bright Red-Orange
                "Infinite" => Color.FromArgb(0, 255, 255),       // Bright Cyan
                "Absolute" => Color.FromArgb(255, 255, 255),     // Pure White
                
                _ => Color.FromArgb(80, 80, 80)                  // Dark Gray (fallback)
            };
        }

        private void UpdateRollButtonText()
        {
            if (_rollCooldownRemaining > 0)
            {
                btnRoll.Text = $"ROLL\n{_rollCooldownRemaining:F1}s";
                btnRoll.Enabled = false;
            }
            else
            {
                var selectedDice = _gameManager.Stats.GetSelectedDice();
                btnRoll.Text = $"🎲 ROLL\n{selectedDice.Name}";
                // Only enable if AutoRoll is NOT active
                btnRoll.Enabled = !_gameManager.Stats.AutoRollActive;
            }
        }

        private void UpdateDiceInfo()
        {
            if (_lblDiceInfo == null) return;
            
            var selectedDice = _gameManager.Stats.GetSelectedDice();
            string quantityText = selectedDice.QuantityDisplay;
            _lblDiceInfo.Text = $"🎲 {selectedDice.Name}\nQuantity: {quantityText}\n(Click to change)";
            
            // Würfelbild aktualisieren
            if (_picDiceIcon != null)
            {
                try
                {
                    string imageName = selectedDice.Name.ToLower().Replace(" ", "_");
                    string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", $"dice_{imageName}.png");
                    
                    if (File.Exists(imagePath))
                    {
                        // Altes Bild freigeben falls vorhanden
                        if (_picDiceIcon.Image != null)
                        {
                            var oldImage = _picDiceIcon.Image;
                            _picDiceIcon.Image = null;
                            oldImage.Dispose();
                        }
                        
                        _picDiceIcon.Image = Image.FromFile(imagePath);
                    }
                }
                catch (Exception ex)
                {
                    // Fehler beim Bildladen ignorieren
                    Console.WriteLine($"Fehler beim Laden des Würfelbildes für {selectedDice.Name}: {ex.Message}");
                }
            }
        }

        private void AutoRollTimer_Tick(object? sender, EventArgs e)
        {
            if (_gameManager.Stats.AutoRollActive && _rollCooldownRemaining <= 0)
            {
                _gameManager.Roll();
                _rollCooldownRemaining = _gameManager.Stats.RollCooldown;
                _rollCooldownTimer.Start();
            }
        }

        private void RollCooldownTimer_Tick(object? sender, EventArgs e)
        {
            _rollCooldownRemaining -= 0.1;
            if (_rollCooldownRemaining <= 0)
            {
                _rollCooldownTimer.Stop();
                // Only enable button if AutoRoll is NOT active
                if (!_gameManager.Stats.AutoRollActive)
                {
                    btnRoll.Enabled = true;
                }
                UpdateRollButtonText();
            }
            else
            {
                btnRoll.Text = $"ROLL\n{_rollCooldownRemaining:F1}s";
            }
        }

        private void UpdateUI()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateUI));
                return;
            }

            // PERFORMANCE: Only update labels that changed
            if (_gameManager.Stats.Money != _lastMoney)
            {
                lblMoney.Text = $"💰 {FormatBigInt(_gameManager.Stats.Money)}";
                _lastMoney = _gameManager.Stats.Money;
                
                // Update rebirth button enabled state when money changes
                btnRebirth.Enabled = _gameManager.AdminMode || _gameManager.Stats.Money >= _gameManager.Stats.NextRebirthCost;
            }
            
            if (_gameManager.Stats.Gems != _lastGems)
            {
                lblGems.Text = $"💎 {_gameManager.Stats.Gems}";
                _lastGems = _gameManager.Stats.Gems;
            }
            
            if (_gameManager.Stats.Rebirths != _lastRebirths || _gameManager.Stats.LuckBoosterLevel != _lastLuckBoosterLevel)
            {
                // Update labels if either Rebirths OR LuckBoosterLevel changed
                if (_gameManager.Stats.Rebirths != _lastRebirths)
                {
                    lblRebirths.Text = $"Rebirths: {_gameManager.Stats.Rebirths}";
                    
                    // Update Rebirth Bonus display
                    double rebirthBonus = _gameManager.Stats.Rebirths * 50.0; // 50% per rebirth
                    lblRebirthBonus.Text = $"🔄 Rebirth: +{rebirthBonus:F0}%";
                    
                    // Update rebirth button text with NEW cost
                    btnRebirth.Text = $"Rebirth\n{FormatBigInt(_gameManager.Stats.NextRebirthCost)}";
                    btnRebirth.Enabled = _gameManager.AdminMode || _gameManager.Stats.Money >= _gameManager.Stats.NextRebirthCost;
                    
                    _lastRebirths = _gameManager.Stats.Rebirths;
                    _plotsDirty = true; // Rebirths change plot slots
                }
                
                // Always update Luck display if either changed
                double luckBonus = _gameManager.GetTotalLuckBonus();
                lblLuck.Text = $"🍀 Luck: +{luckBonus:F0}%";
                
                _lastLuckBoosterLevel = _gameManager.Stats.LuckBoosterLevel;
            }

            // PERFORMANCE: Only update plots when inventory actually changed
            if (_plotsDirty)
            {
                UpdatePlotDisplay();
                _plotsDirty = false;
            }

            // Update Dice Info (lightweight)
            UpdateDiceInfo();

            // Update AutoRoll indicator and Roll button state
            if (_gameManager.Stats.AutoRollActive)
            {
                _autoRollTimer.Start();
                lblAutoRollStatus.Text = "AutoRoll: AN";
                // Disable manual roll button during AutoRoll
                btnRoll.Enabled = false;
            }
            else
            {
                _autoRollTimer.Stop();
                lblAutoRollStatus.Text = _gameManager.Stats.AutoRollUnlocked ? "AutoRoll: AUS" : "";
                // Enable manual roll button only if no cooldown
                btnRoll.Enabled = _rollCooldownRemaining <= 0;
            }
        }

        private void UpdatePlotDisplay()
        {
            if (!_plotDisplayInitialized)
            {
                InitializePlotDisplay();
                _plotDisplayInitialized = true;
            }

            BigInteger totalIncome = 0;

            int visiblePlots = Math.Min(_gameManager.Stats.PlotSlots, _plotPanels.Count);
            for (int i = 0; i < visiblePlots; i++)
            {
                Panel plotPanel = _plotPanels[i];
                plotPanel.Visible = true;

                if (i < _gameManager.Stats.EquippedRayanIndices.Count)
                {
                    int rayanIndex = _gameManager.Stats.EquippedRayanIndices[i];
                    if (rayanIndex >= 0 && rayanIndex < _gameManager.Stats.Inventory.Count)
                    {
                        var rayan = _gameManager.Stats.Inventory[rayanIndex];
                        totalIncome += rayan.TotalValue;

                        Color newColor = GetRarityColor(rayan.Rarity);
                        if (plotPanel.BackColor != newColor)
                            plotPanel.BackColor = newColor;

                        Label lblName = (Label)plotPanel.Controls[1];
                        if (lblName.Text != rayan.FullName)
                        {
                            lblName.Text = rayan.FullName;
                            lblName.ForeColor = TextColor;
                        }

                        Label lblValue = (Label)plotPanel.Controls[2];
                        string valueText = $"{FormatBigInt(rayan.TotalValue)}/s";
                        if (lblValue.Text != valueText)
                            lblValue.Text = valueText;
                        lblValue.Visible = true;
                    }
                    else
                    {
                        plotPanel.BackColor = DarkAccent;
                        ((Label)plotPanel.Controls[1]).Text = "[Leer]";
                        ((Label)plotPanel.Controls[1]).ForeColor = Color.Gray;
                        plotPanel.Controls[2].Visible = false;
                    }
                }
                else
                {
                    plotPanel.BackColor = DarkAccent;
                    ((Label)plotPanel.Controls[1]).Text = "[Leer]";
                    ((Label)plotPanel.Controls[1]).ForeColor = Color.Gray;
                    plotPanel.Controls[2].Visible = false;
                }
            }

            for (int i = visiblePlots; i < _plotPanels.Count; i++)
            {
                _plotPanels[i].Visible = false;
            }

            if (_totalIncomePanel != null)
            {
                // Show actual income with multiplier
                BigInteger totalIncomeWithMultiplier = new BigInteger((double)totalIncome * _gameManager.Stats.MoneyMultiplier);
                string totalText = $"{FormatBigInt(totalIncomeWithMultiplier)}/s";
                
                // Also show multiplier if > 1
                if (_gameManager.Stats.MoneyMultiplier > 1.0)
                {
                    totalText += $" (x{_gameManager.Stats.MoneyMultiplier:F1})";
                }
                
                Label lblTotalIncome = (Label)_totalIncomePanel.Controls[1];
                if (lblTotalIncome.Text != totalText)
                    lblTotalIncome.Text = totalText;
            }
        }

        private void InitializePlotDisplay()
        {
            panelLeft.SuspendLayout();
            panelLeft.Controls.Clear();
            _plotPanels.Clear();

            var titleLabel = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(280, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "📊 PLOTS",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = BrightGold
            };
            panelLeft.Controls.Add(titleLabel);

            int yPosition = 45;
            for (int i = 0; i < 10; i++)
            {
                var plotPanel = CreateCompactPlotPanel(i, yPosition);
                _plotPanels.Add(plotPanel);
                panelLeft.Controls.Add(plotPanel);
                yPosition += 50;
            }

            _totalIncomePanel = new Panel
            {
                Location = new Point(10, yPosition),
                Size = new Size(280, 45),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(0, 80, 0)
            };

            Label lblTotal = new Label
            {
                Location = new Point(5, 5),
                Size = new Size(270, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = "💰 Total Income:",
                ForeColor = BrightGreen
            };
            _totalIncomePanel.Controls.Add(lblTotal);

            Label lblTotalValue = new Label
            {
                Location = new Point(5, 23),
                Size = new Size(270, 18),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "0/s",
                ForeColor = BrightGreen
            };
            _totalIncomePanel.Controls.Add(lblTotalValue);

            panelLeft.Controls.Add(_totalIncomePanel);
            
            // Move AutoEquip button below Total Income Panel
            yPosition += 55; // Total Income height + gap
            panelLeft.Controls.Remove(btnAutoEquip);
            panelCenter.Controls.Remove(btnAutoEquip);
            btnAutoEquip.Location = new Point(10, yPosition);
            btnAutoEquip.Size = new Size(280, 50);
            btnAutoEquip.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAutoEquip.Text = "⬆️ AUTO EQUIP";
            panelLeft.Controls.Add(btnAutoEquip);
            
            panelLeft.ResumeLayout();
        }

        private Panel CreateCompactPlotPanel(int index, int yPosition)
        {
            Panel plotPanel = new Panel
            {
                Location = new Point(10, yPosition),
                Size = new Size(280, 45),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DarkAccent,
                Visible = false
            };

            Label lblPlotNumber = new Label
            {
                Location = new Point(5, 3),
                Size = new Size(50, 15),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Text = $"Plot {index + 1}",
                ForeColor = Color.Gray
            };
            plotPanel.Controls.Add(lblPlotNumber);

            Label lblRayanName = new Label
            {
                Location = new Point(5, 18),
                Size = new Size(270, 16),
                Font = new Font("Segoe UI", 8F),
                Text = "[Leer]",
                ForeColor = Color.Gray
            };
            plotPanel.Controls.Add(lblRayanName);

            Label lblRayanValue = new Label
            {
                Location = new Point(200, 3),
                Size = new Size(75, 15),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Text = "0/s",
                ForeColor = BrightGreen,
                TextAlign = ContentAlignment.TopRight,
                Visible = false
            };
            plotPanel.Controls.Add(lblRayanValue);

            return plotPanel;
        }

        private Color GetRarityColor(double rarity)
        {
            if (rarity >= 1000000) return Color.FromArgb(127, 107, 0); // Dark Gold (50% dunkler)
            if (rarity >= 100000) return Color.FromArgb(100, 50, 127); // Dark Purple (50% dunkler)
            if (rarity >= 10000) return Color.FromArgb(0, 87, 127); // Dark Blue (50% dunkler)
            if (rarity >= 1000) return Color.FromArgb(0, 127, 63); // Dark Green (50% dunkler)
            return DarkAccent;
        }

        private void OnRayanRolled_Handler(Rayan rayan)
        {
            // PERFORMANCE: Mark plots as dirty when new Rayan added
            _plotsDirty = true;
            ShowNewRayan(rayan);
        }

        private void ShowNewRayan(Rayan rayan)
        {
            lblLastRoll.Text = $"🎲 {rayan.FullName}\n1 in {rayan.Rarity:N0}";
        }
        
        private void ShowEventSelectionDialog()
        {
            // Create dark-themed selection dialog
            var dialog = new Form
            {
                Text = _gameManager.IsMultiplayerConnected ? "Event starten (Multiplayer)" : "Event starten (Lokal)",
                Size = new Size(400, 550),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = DarkBackground
            };
            
            var titleLabel = new Label
            {
                Text = "Wähle Event-Suffix:",
                Location = new Point(20, 20),
                Size = new Size(350, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = BrightGold
            };
            dialog.Controls.Add(titleLabel);
            
            if (_gameManager.IsMultiplayerConnected)
            {
                var mpLabel = new Label
                {
                    Text = "🌐 Multiplayer Modus - Alle Spieler erhalten dieses Event!",
                    Location = new Point(20, 50),
                    Size = new Size(350, 40),
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = BrightBlue
                };
                dialog.Controls.Add(mpLabel);
            }
            
            // Create ListBox with all suffixes
            var listBox = new ListBox
            {
                Location = new Point(20, _gameManager.IsMultiplayerConnected ? 100 : 60),
                Size = new Size(350, 350),
                Font = new Font("Segoe UI", 10F),
                BackColor = DarkPanel,
                ForeColor = TextColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Add all suffixes from RayanData
            foreach (var suffix in RayanData.Suffixes)
            {
                listBox.Items.Add($"{suffix.Suffix} (1:{suffix.Chance:N0}, {suffix.Multiplier}x)");
            }
            
            listBox.SelectedIndex = 0;
            dialog.Controls.Add(listBox);
            
            // Start Event button
            var btnStart = new Button
            {
                Text = "Event starten",
                Location = new Point(20, 470),
                Size = new Size(170, 40),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = BrightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += (s, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    var selectedSuffix = RayanData.Suffixes[listBox.SelectedIndex];
                    _gameManager.ForceSpecificEvent(selectedSuffix.Suffix);
                    dialog.Close();
                }
            };
            dialog.Controls.Add(btnStart);
            
            // Cancel button
            var btnCancel = new Button
            {
                Text = "Abbrechen",
                Location = new Point(200, 470),
                Size = new Size(170, 40),
                Font = new Font("Segoe UI", 11F),
                BackColor = DarkAccent,
                ForeColor = TextColor,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 85);
            btnCancel.Click += (s, e) => dialog.Close();
            dialog.Controls.Add(btnCancel);
            
            dialog.ShowDialog(this);
        }

        private string FormatBigInt(BigInteger value)
        {
            if (value < 1000) return value.ToString();
            if (value < 1000000) return ((double)value / 1000).ToString("F1") + "K";
            if (value < 1000000000) return ((double)value / 1000000).ToString("F1") + "M";
            if (value < 1000000000000) return ((double)value / 1000000000).ToString("F1") + "B";
            return value.ToString("E1");
        }

        private void btnRoll_Click(object sender, EventArgs e)
        {
            _gameManager.Roll();
            btnRoll.Enabled = false;
            _rollCooldownRemaining = _gameManager.Stats.RollCooldown;
            _rollCooldownTimer.Start();
        }

        private void btnAutoEquip_Click(object sender, EventArgs e)
        {
            _gameManager.AutoEquipBest();
            // PERFORMANCE: Mark plots as dirty when equipment changes
            _plotsDirty = true;
        }

        private void btnRebirth_Click(object sender, EventArgs e)
        {
            _gameManager.Rebirth();
            // PERFORMANCE: Mark plots as dirty after rebirth
            _plotsDirty = true;
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            var fullInventoryForm = new FullInventoryForm(_gameManager.Stats.Inventory, () =>
            {
                _gameManager.Save();
                UpdateUI();
            });
            fullInventoryForm.ShowDialog();
        }

        private void btnUpgrades_Click(object sender, EventArgs e)
        {
            var upgradeForm = new UpgradeForm(_gameManager, () =>
            {
                UpdateUI();
            });
            upgradeForm.ShowDialog();
        }

        private void btnQuests_Click(object sender, EventArgs e)
        {
            var questForm = new QuestForm(_gameManager, () =>
            {
                UpdateUI();
            });
            questForm.ShowDialog();
        }

        private void btnShop_Click(object sender, EventArgs e)
        {
            var diceShopForm = new DiceShopForm(_gameManager, () =>
            {
                UpdateUI();
            });
            diceShopForm.ShowDialog();
        }
        
        private void btnOptions_Click(object sender, EventArgs e)
        {
            var optionsForm = new OptionsForm(_gameManager, () =>
            {
                UpdateUI();
            });
            optionsForm.ShowDialog();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _gameManager.Save();
            base.OnFormClosing(e);
        }

        private static void SetDoubleBuffered(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }
    }
}
