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
using SpinARayan.Config;

namespace SpinARayan
{
    public partial class MainForm : Form
    {
        private readonly GameManager _gameManager;
        private System.Windows.Forms.Timer _rollCooldownTimer;
        private System.Windows.Forms.Timer _autoRollTimer;
        private System.Windows.Forms.Timer _autoSaveTimer; // Auto-save every 20 seconds
        private double _rollCooldownRemaining = 0;
        private bool _plotDisplayInitialized = false;
        private List<Panel> _plotPanels = new List<Panel>();
        private Panel? _totalIncomePanel;
        private Label? _lblDiceInfo; // Shows selected dice quantity
        private PictureBox? _picDiceSelector; // Shows dice image, clickable to open inventory
        private Panel? _eventDisplayPanel; // Main event display panel
        private Label? _lblEventCount; // Shows event count
        private ComboBox? _comboEventSelector; // Dropdown for event selection
        private Label? _lblEventDetails; // Shows selected event details
        private Label? _lblRebirthBonus; // Shows rebirth bonus
        
        // PERFORMANCE: Dirty flags to avoid unnecessary UI updates
        private bool _plotsDirty = true;
        private bool _inventoryDirty = false;
        private BigInteger _lastMoney = 0;
        private int _lastGems = 0;
        private int _lastRebirths = 0;
        private int _lastLuckBoosterLevel = 0;
        
        
        // PERFORMANCE: Image cache for dice images (keep in RAM)
        private Dictionary<string, Image> _diceImageCache = new Dictionary<string, Image>();

        // Modern Theme Colors (from ModernTheme.cs)
        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color DarkAccent = ModernTheme.PrimaryMedium;
        private readonly Color BrightGreen = ModernTheme.Success;
        private readonly Color BrightBlue = ModernTheme.AccentBlue;
        private readonly Color BrightGold = ModernTheme.Warning;
        private readonly Color BrightRed = ModernTheme.Error;
        private readonly Color TextColor = ModernTheme.TextPrimary;
        private readonly Color RebirthColor = ModernTheme.Rebirth;
        
        // Database and Savefile tracking
        private DatabaseService? _databaseService;
        private string? _currentSavefileId;

        public MainForm() : this(null, null, null)
        {
        }
        
        public MainForm(string? username, DatabaseService? dbService, string? savefileId)
        {
            _databaseService = dbService;
            _currentSavefileId = savefileId;
            
            InitializeComponent();
            
            // Set application icon from embedded resource
            try
            {
                string resourceName = "Spin_a_Rayan.Assets.app_icon.ico";
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        // Load .ico file directly - supports multiple resolutions
                        this.Icon = new Icon(stream);
                    }
                    else
                    {
                        Console.WriteLine($"[MainForm] Icon resource not found: {resourceName}");
                        Console.WriteLine("[MainForm] Available resources:");
                        foreach (var res in assembly.GetManifestResourceNames())
                        {
                            Console.WriteLine($"  - {res}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainForm] Could not load icon: {ex.Message}");
            }

            // Apply Dark Mode
            ApplyDarkMode();

            // Enable double buffering
            SetDoubleBuffered(panelLeft);
            SetDoubleBuffered(panelCenter);
            SetDoubleBuffered(panelRight);

            // MULTIPLAYER CONFIG: Use provided username or ask
            string? multiplayerUsername = username; // Use provided username from login
            
            if (string.IsNullOrEmpty(multiplayerUsername))
            {
                // Fallback: Old system - ask for username
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "multiplayer_username.txt");
                
                // Check if config exists
                if (!File.Exists(configPath))
                {
                // First run! Ask for username
                using var inputDialog = new Form
                {
                    Text = "🌐 Multiplayer Setup",
                    Size = new Size(450, 270),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    StartPosition = FormStartPosition.CenterScreen,
                    BackColor = DarkBackground
                };
                
                var lblInfo = new Label
                {
                    Text = "Möchtest du Multiplayer aktivieren?\n\n" +
                           "Mit Multiplayer können Events mit anderen Spielern\n" +
                           "automatisch synchronisiert werden.\n\n" +
                           "Du kannst diese Einstellung später jederzeit ändern.",
                    Location = new Point(20, 20),
                    Size = new Size(410, 90),
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = TextColor
                };
                inputDialog.Controls.Add(lblInfo);
                
                var lblUsername = new Label
                {
                    Text = "Dein Username:",
                    Location = new Point(20, 125),
                    Size = new Size(120, 25),
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = TextColor
                };
                inputDialog.Controls.Add(lblUsername);
                
                var txtUsername = new TextBox
                {
                    Location = new Point(140, 123),
                    Size = new Size(270, 25),
                    Font = new Font("Segoe UI", 10F),
                    Text = Environment.UserName,
                    BackColor = DarkAccent,
                    ForeColor = TextColor
                };
                inputDialog.Controls.Add(txtUsername);
                
                var btnOk = new Button
                {
                    Text = "✅ Aktivieren",
                    Location = new Point(20, 170),
                    Size = new Size(190, 40),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    BackColor = BrightGreen,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.OK
                };
                btnOk.FlatAppearance.BorderSize = 0;
                inputDialog.Controls.Add(btnOk);
                
                var btnSkip = new Button
                {
                    Text = "⏭️ Single-Player",
                    Location = new Point(220, 170),
                    Size = new Size(190, 40),
                    Font = new Font("Segoe UI", 10F),
                    BackColor = Color.Gray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel
                };
                btnSkip.FlatAppearance.BorderSize = 0;
                inputDialog.Controls.Add(btnSkip);
                
                inputDialog.AcceptButton = btnOk;
                inputDialog.CancelButton = btnSkip;
                
                var result = inputDialog.ShowDialog();
                
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    multiplayerUsername = txtUsername.Text.Trim();
                    
                    // Save config
                    try
                    {
                        File.WriteAllText(configPath, multiplayerUsername);
                        Console.WriteLine($"[MainForm] Multiplayer enabled - Username: {multiplayerUsername}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MainForm] Error saving multiplayer config: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("[MainForm] Multiplayer skipped - Single-Player mode");
                }
            }
            else
            {
                // Load existing config
                try
                {
                    multiplayerUsername = File.ReadAllText(configPath).Trim();
                    
                    if (!string.IsNullOrEmpty(multiplayerUsername))
                    {
                        Console.WriteLine($"[MainForm] Multiplayer loaded - Username: {multiplayerUsername}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MainForm] Error loading multiplayer config: {ex.Message}");
                }
            }
            }

            _gameManager = new GameManager(multiplayerUsername, _databaseService);
            _gameManager.OnStatsChanged += UpdateUI;
            _gameManager.OnRayanRolled += OnRayanRolled_Handler;
            _gameManager.OnEventsChanged += UpdateEventDisplay;
            
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

            // Auto-save timer: Save every 20 seconds for data safety
            _autoSaveTimer = new System.Windows.Forms.Timer();
            _autoSaveTimer.Interval = 20000; // 20 seconds
            _autoSaveTimer.Tick += AutoSaveTimer_Tick;
            _autoSaveTimer.Start();
            Console.WriteLine("[MainForm] Auto-save timer started (every 20 seconds)");

            CreateDiceInfoLabel();
            CreateEventDisplay();
            
            
            
            
            // Move Rebirth button to right panel (bottom)
            panelCenter.Controls.Remove(btnRebirth);
            btnRebirth.Location = new Point(20, 720);
            btnRebirth.Size = new Size(310, 90);
            btnRebirth.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            btnRebirth.Text = $"Rebirth\n{FormatBigInt(_gameManager.Stats.NextRebirthCost)}";
            btnRebirth.Enabled = _gameManager.AdminMode || _gameManager.Stats.Money >= _gameManager.Stats.NextRebirthCost;
            panelRight.Controls.Add(btnRebirth);
            
            // Add Rebirth counter label below the button
            var lblRebirthCounter = new Label
            {
                Location = new Point(20, 815),
                Size = new Size(310, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = $"Rebirth #{_gameManager.Stats.Rebirths}",
                ForeColor = RebirthColor,
                TextAlign = ContentAlignment.TopCenter,
                Name = "lblRebirthCounter"
            };
            panelRight.Controls.Add(lblRebirthCounter);
            
            // Add debug button for manual polling (Admin/Multiplayer only)
            CreateDebugPollButton();
            
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
            // Admin: Press 'E' to force an event
            if (_gameManager.AdminMode && e.KeyCode == Keys.E)
            {
                // Show event selection dialog
                ShowEventSelectionDialog();
                e.Handled = true;
            }
            
            // Multiplayer: Press 'M' to publish a multiplayer event
            if (_gameManager.IsMultiplayerEnabled && e.KeyCode == Keys.M)
            {
                ShowEventSelectionDialog();
                e.Handled = true;
            }
        }

        private void ActivateAdminMode()
        {
            _gameManager.AdminMode = !_gameManager.AdminMode;
            
            // Track admin usage in database
            if (_gameManager.AdminMode)
            {
                _gameManager.MarkAdminUsed();
                Console.WriteLine($"[MainForm] Admin mode activated - tracking in database");
            }
            else
            {
                Console.WriteLine($"[MainForm] Admin mode deactivated");
            }
            
            UpdateUI();
        }

#if DEBUG
        /// <summary>
        /// Public method for console to toggle admin mode (DEBUG only)
        /// </summary>
        public void ToggleAdminModeFromConsole()
        {
            ActivateAdminMode();
            Console.WriteLine($"[Console] Admin mode toggled via console command: {(_gameManager.AdminMode ? "ENABLED" : "DISABLED")}");
        }
        
        /// <summary>
        /// Add money from console command (DEBUG only, triggers admin flag)
        /// </summary>
        public void AddMoneyFromConsole(BigInteger amount)
        {
            _gameManager.Stats.Money += amount;
            _gameManager.MarkAdminUsed();
            Console.WriteLine($"[Console] Added {amount} money. New total: {_gameManager.Stats.Money}");
            Console.WriteLine($"[Console] Admin flag set!");
            UpdateUI();
        }
        
        /// <summary>
        /// Add gems from console command (DEBUG only, triggers admin flag)
        /// </summary>
        public void AddGemsFromConsole(int amount)
        {
            _gameManager.Stats.Gems += amount;
            _gameManager.MarkAdminUsed();
            Console.WriteLine($"[Console] Added {amount} gems. New total: {_gameManager.Stats.Gems}");
            Console.WriteLine($"[Console] Admin flag set!");
            UpdateUI();
        }
        
        /// <summary>
        /// Set roll cooldown speed from console command (DEBUG only, triggers admin flag)
        /// </summary>
        public void SetRollSpeedFromConsole(double seconds)
        {
            _gameManager.Stats.RollCooldown = seconds;
            _gameManager.MarkAdminUsed();
            Console.WriteLine($"[Console] Roll cooldown set to {seconds:F1} seconds");
            Console.WriteLine($"[Console] Admin flag set!");
            
            // If roll is currently on cooldown, update it
            if (_rollCooldownRemaining > seconds)
            {
                _rollCooldownRemaining = seconds;
            }
            
            UpdateUI();
        }
        
        /// <summary>
        /// Roll multiple times from console command (DEBUG only, triggers admin flag)
        /// For testing probabilities without timer
        /// </summary>
        public void RollMultipleFromConsole(int count)
        {
            Console.WriteLine($"[Console] Starting {count} rolls for testing...");
            _gameManager.MarkAdminUsed();
            
            var rarityStats = new Dictionary<string, int>();
            var suffixStats = new Dictionary<string, int>();
            int totalRolls = 0;
            
            var startTime = DateTime.Now;
            
            for (int i = 0; i < count; i++)
            {
                _gameManager.Roll();
                totalRolls++;
                
                // Get last rolled rayan
                var lastRayan = _gameManager.Stats.Inventory.LastOrDefault();
                if (lastRayan != null)
                {
                    // Track prefix stats
                    if (!rarityStats.ContainsKey(lastRayan.Prefix))
                        rarityStats[lastRayan.Prefix] = 0;
                    rarityStats[lastRayan.Prefix]++;
                    
                    // Track suffix stats
                    string suffixKey = string.IsNullOrEmpty(lastRayan.Suffix) ? "(No Suffix)" : lastRayan.Suffix;
                    if (!suffixStats.ContainsKey(suffixKey))
                        suffixStats[suffixKey] = 0;
                    suffixStats[suffixKey]++;
                }
            }
            
            var endTime = DateTime.Now;
            var duration = (endTime - startTime).TotalSeconds;
            
            Console.WriteLine($"[Console] Completed {totalRolls} rolls in {duration:F2} seconds");
            Console.WriteLine($"[Console] Admin flag set!");
            Console.WriteLine($"");
            Console.WriteLine($"[Console] === PREFIX STATISTICS ===");
            foreach (var kvp in rarityStats.OrderByDescending(x => x.Value))
            {
                double percentage = (kvp.Value / (double)totalRolls) * 100.0;
                Console.WriteLine($"[Console]   {kvp.Key,-20}: {kvp.Value,6} ({percentage,6:F2}%)");
            }
            Console.WriteLine($"");
            Console.WriteLine($"[Console] === SUFFIX STATISTICS ===");
            foreach (var kvp in suffixStats.OrderByDescending(x => x.Value))
            {
                double percentage = (kvp.Value / (double)totalRolls) * 100.0;
                Console.WriteLine($"[Console]   {kvp.Key,-20}: {kvp.Value,6} ({percentage,6:F2}%)");
            }
            
            UpdateUI();
        }
#endif

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
            // Position centered above roll button with more spacing
            int width = 200;
            int centerX = panelCenter.Width / 2;
            
            // DICE IMAGE - clickable, leads to inventory (larger size for better visibility)
            // Centered DIRECTLY above roll button
            _picDiceSelector = new PictureBox
            {
                Location = new Point(centerX - 100, 350), // Centered: 700/2 - 200/2 = 250
                Size = new Size(200, 200), // Large visible image
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            _picDiceSelector.Click += (s, e) => OpenDiceSelection();
            panelCenter.Controls.Add(_picDiceSelector);
            _picDiceSelector.BringToFront(); // Keep in front
            
            // QUANTITY LABEL - below image, centered
            _lblDiceInfo = new Label
            {
                Location = new Point(centerX - 100, 560), // Below image
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = BrightBlue,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopCenter,
                Text = "Quantity: ∞"
            };
            panelCenter.Controls.Add(_lblDiceInfo);
            _lblDiceInfo.BringToFront(); // Keep label in front
        }
        
        private void CreateDebugPollButton()
        {
            var btnDebugPoll = new Button
            {
                Text = "🔍 Debug: Poll Events",
                Location = new Point(20, 640),
                Size = new Size(310, 60),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(100, 50, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                Name = "btnDebugPoll"
            };
            btnDebugPoll.FlatAppearance.BorderSize = 0;
            btnDebugPoll.Click += async (s, e) => await DebugPollEvents();
            panelRight.Controls.Add(btnDebugPoll);
        }
        
        private async System.Threading.Tasks.Task DebugPollEvents()
        {
            try
            {
                var eventSync = typeof(GameManager).GetField("_eventSync", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(_gameManager) as EventSyncService;
                
                if (eventSync == null)
                {
                    MessageBox.Show(
                        "❌ EventSync nicht initialisiert!\n\n" +
                        "Multiplayer ist nicht aktiv.",
                        "Debug: Poll Events",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }
                
                // Call PollForEventsAsync directly
                await eventSync.PollForEventsAsync();
                
                // Show results
                var activeEvents = _gameManager.CurrentEvents;
                
                string message = $"🔍 Debug: Event Poll Results\n\n" +
                    $"Aktueller Zeitpunkt:\n" +
                    $"  Lokal: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                    $"  UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n\n" +
                    $"Aktive Events: {activeEvents.Count}\n";
                
                if (activeEvents.Any())
                {
                    message += "\nGefundene Events:\n";
                    foreach (var evt in activeEvents)
                    {
                        message += $"\n• {evt.SuffixName} (ID: {evt.EventId})\n";
                        message += $"  Von: {evt.CreatedBy}\n";
                        message += $"  Verbleibend: {evt.TimeRemaining.TotalMinutes:F1} min\n";
                        message += $"  Suffix Boost: {evt.BoostMultiplier}x\n";
                        message += $"  Luck: {evt.LuckMultiplier}x\n";
                        message += $"  Money: {evt.MoneyMultiplier}x\n";
                        message += $"  Roll Speed: {evt.RollTimeModifier}x\n";
                    }
                }
                else
                {
                    message += "\n⚠️ Keine aktiven Events gefunden.\n\n";
                    message += "Mögliche Gründe:\n";
                    message += "• Events sind bereits abgelaufen\n";
                    message += "• Zeitzone-Problem (UTC vs Lokal)\n";
                    message += "• Keine Events in DB vorhanden";
                }
                
                MessageBox.Show(
                    message,
                    "Debug: Poll Events",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Debug Poll Error:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Debug: Poll Events",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
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
            // Main event panel with dark background
            _eventDisplayPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1200, 50),
                BackColor = Color.FromArgb(40, 40, 40),
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(_eventDisplayPanel);
            _eventDisplayPanel.BringToFront();
            
            // Event count label on left
            _lblEventCount = new Label
            {
                Location = new Point(10, 5),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = BrightGold,
                BackColor = Color.Transparent,
                Text = "🔥 0 Events aktiv"
            };
            _eventDisplayPanel.Controls.Add(_lblEventCount);
            
            // Dropdown selector for events (darker, subtle styling)
            _comboEventSelector = new ComboBox
            {
                Location = new Point(220, 10),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.FromArgb(200, 200, 200),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                DisplayMember = "DisplayText"
            };
            _comboEventSelector.SelectedIndexChanged += ComboEventSelector_SelectedIndexChanged;
            _eventDisplayPanel.Controls.Add(_comboEventSelector);
            
            // Event details label on right (smaller font, less screaming)
            _lblEventDetails = new Label
            {
                Location = new Point(530, 0),
                Size = new Size(650, 50),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(220, 220, 220),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = ""
            };
            _eventDisplayPanel.Controls.Add(_lblEventDetails);
        }
        
        private void ComboEventSelector_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_comboEventSelector == null || _lblEventDetails == null) return;
            
            var selectedEvent = _comboEventSelector.SelectedItem as SuffixEvent;
            if (selectedEvent != null)
            {
                UpdateEventDetailsLabel(selectedEvent);
            }
        }
        
        private void UpdateEventDetailsLabel(SuffixEvent evt)
        {
            if (_lblEventDetails == null) return;
            
            int minutes = (int)evt.TimeRemaining.TotalMinutes;
            int seconds = evt.TimeRemaining.Seconds;
            
            string creatorInfo = string.IsNullOrEmpty(evt.CreatedBy) || evt.CreatedBy == "Local" 
                ? "" 
                : $" (von {evt.CreatedBy})";
            
            // Build detailed event info with all multipliers
            string eventInfo = $"{evt.EventName}{creatorInfo} | {minutes}:{seconds:D2} verbleibend\n";
            
            // Add multiplier info
            var multipliers = new List<string>();
            
            if (!string.IsNullOrEmpty(evt.SuffixName))
                multipliers.Add($"Suffix: {evt.SuffixName} {evt.BoostMultiplier}x");
            
            if (evt.LuckMultiplier != 1.0f)
                multipliers.Add($"Luck: {evt.LuckMultiplier}x");
            
            if (evt.MoneyMultiplier != 1.0f)
                multipliers.Add($"Money: {evt.MoneyMultiplier}x");
            
            if (evt.RollTimeModifier != 1.0f)
                multipliers.Add($"Roll Speed: {evt.RollTimeModifier}x");
            
            if (multipliers.Any())
                eventInfo += string.Join(" | ", multipliers);
            
            _lblEventDetails.Text = eventInfo;
            
            // Subtle color based on suffix (muted versions)
            var baseColor = GetEventColor(evt.SuffixName);
            _lblEventDetails.ForeColor = Color.FromArgb(
                Math.Max(baseColor.R - 30, 150), 
                Math.Max(baseColor.G - 30, 150), 
                Math.Max(baseColor.B - 30, 150)
            );
        }
        
        private void UpdateEventDisplay(List<SuffixEvent> activeEvents)
        {
            if (_eventDisplayPanel == null || _lblEventCount == null || _comboEventSelector == null) return;
            
            // Hide panel if no active events (check for null, empty, or all inactive)
            if (activeEvents == null || activeEvents.Count == 0 || !activeEvents.Any(e => e.IsActive))
            {
                _eventDisplayPanel.Visible = false;
                return;
            }
            
            _eventDisplayPanel.Visible = true;
            
            // Update count
            string adminPrefix = _gameManager.AdminMode ? "[ADMIN] " : "";
            _lblEventCount.Text = $"{adminPrefix}🔥 {activeEvents.Count} Event{(activeEvents.Count > 1 ? "s" : "")} aktiv";
            
            // Update dropdown (oldest first)
            var sortedEvents = activeEvents.OrderBy(e => e.StartTime).ToList();
            
            // Save current selection
            var currentSelection = _comboEventSelector.SelectedItem as SuffixEvent;
            
            _comboEventSelector.Items.Clear();
            foreach (var evt in sortedEvents)
            {
                _comboEventSelector.Items.Add(evt);
            }
            
            // Try to restore selection, or select first (oldest)
            if (currentSelection != null && sortedEvents.Any(e => e.EventId == currentSelection.EventId))
            {
                _comboEventSelector.SelectedItem = sortedEvents.First(e => e.EventId == currentSelection.EventId);
            }
            else if (sortedEvents.Any())
            {
                _comboEventSelector.SelectedIndex = 0;
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
            if (_lblDiceInfo == null || _picDiceSelector == null) return;
            
            var selectedDice = _gameManager.Stats.GetSelectedDice();
            string quantityText = selectedDice.QuantityDisplay;
            
            // Update label - only quantity
            _lblDiceInfo.Text = $"Quantity: {quantityText}";
            
            // Update dice image - with caching!
            var diceImage = LoadDiceImage(selectedDice.Name);
            if (diceImage != null)
            {
                _picDiceSelector.Image = diceImage; // Don't dispose - it's cached!
            }
        }
        
        private Image? LoadDiceImage(string diceName)
        {
            // Check cache first
            if (_diceImageCache.TryGetValue(diceName, out Image? cachedImage))
            {
                return cachedImage;
            }
            
            try
            {
                // New naming system: "Absolute Dice.jpg"
                string imageName = diceName + ".jpg";
                string resourceName = $"Spin_a_Rayan.Assets.{imageName}";
                
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var image = Image.FromStream(stream);
                        
                        // Cache the image for future use
                        _diceImageCache[diceName] = image;
                        
                        return image;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainForm] Could not load dice image for {diceName}: {ex.Message}");
            }
            return null;
        }

        private void AutoRollTimer_Tick(object? sender, EventArgs e)
        {
            if (_gameManager.Stats.AutoRollActive && _rollCooldownRemaining <= 0)
            {
                _gameManager.Roll();
                _rollCooldownRemaining = _gameManager.GetEffectiveRollCooldown();
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

        private void AutoSaveTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                _gameManager.Save();
                Console.WriteLine($"[MainForm] Auto-saved at {DateTime.Now:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainForm] Auto-save failed: {ex.Message}");
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
                    
                    // Update rebirth counter label below button
                    var lblRebirthCounter = panelRight.Controls.Find("lblRebirthCounter", false).FirstOrDefault() as Label;
                    if (lblRebirthCounter != null)
                    {
                        lblRebirthCounter.Text = $"Rebirth #{_gameManager.Stats.Rebirths}";
                    }
                    
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
            
            // Show/hide debug poll button based on Admin mode and Multiplayer
            var btnDebugPoll = panelRight.Controls.Find("btnDebugPoll", false).FirstOrDefault();
            if (btnDebugPoll != null)
            {
                btnDebugPoll.Visible = _gameManager.AdminMode && _gameManager.IsMultiplayerEnabled;
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
                Size = new Size(330, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Text = "📊 PLOTS",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = BrightGold
            };
            panelLeft.Controls.Add(titleLabel);

            int yPosition = 50;
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
                Size = new Size(330, 55),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(0, 80, 0)
            };

            Label lblTotal = new Label
            {
                Location = new Point(5, 5),
                Size = new Size(320, 22),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "💰 Total Income:",
                ForeColor = BrightGreen
            };
            _totalIncomePanel.Controls.Add(lblTotal);

            Label lblTotalValue = new Label
            {
                Location = new Point(5, 28),
                Size = new Size(320, 22),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Text = "0/s",
                ForeColor = BrightGreen
            };
            _totalIncomePanel.Controls.Add(lblTotalValue);

            panelLeft.Controls.Add(_totalIncomePanel);
            
            // Move AutoEquip button below Total Income Panel
            yPosition += 65; // Total Income height + gap
            panelLeft.Controls.Remove(btnAutoEquip);
            panelCenter.Controls.Remove(btnAutoEquip);
            btnAutoEquip.Location = new Point(10, yPosition);
            btnAutoEquip.Size = new Size(330, 60);
            btnAutoEquip.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnAutoEquip.Text = "⬆️ AUTO EQUIP";
            panelLeft.Controls.Add(btnAutoEquip);
            
            panelLeft.ResumeLayout();
        }

        private Panel CreateCompactPlotPanel(int index, int yPosition)
        {
            Panel plotPanel = new Panel
            {
                Location = new Point(10, yPosition),
                Size = new Size(330, 50),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DarkAccent,
                Visible = false
            };

            Label lblPlotNumber = new Label
            {
                Location = new Point(5, 3),
                Size = new Size(60, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = $"Plot {index + 1}",
                ForeColor = Color.Gray
            };
            plotPanel.Controls.Add(lblPlotNumber);

            Label lblRayanName = new Label
            {
                Location = new Point(5, 22),
                Size = new Size(320, 20),
                Font = new Font("Segoe UI", 9F),
                Text = "[Leer]",
                ForeColor = Color.Gray
            };
            plotPanel.Controls.Add(lblRayanName);

            Label lblRayanValue = new Label
            {
                Location = new Point(240, 3),
                Size = new Size(85, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
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
            // Ultra-Extreme Tiers (1 Quintillion+) - Gedämpfte spektakuläre Farben
            if (rarity >= 1000000000000000000) return Color.FromArgb(180, 0, 180);    // Dark Magenta - Pure Energy
            if (rarity >= 100000000000000000) return Color.FromArgb(0, 180, 180);     // Dark Cyan - Infinite Light
            if (rarity >= 10000000000000000) return Color.FromArgb(180, 180, 0);      // Dark Yellow - Radiant Sun
            
            // Hyper-Legendary Tiers (1 Trillion - 1 Quadrillion) - Gedämpfte kosmische Farben
            if (rarity >= 1000000000000000) return Color.FromArgb(200, 100, 200);     // Medium Magenta - Cosmic Aura
            if (rarity >= 100000000000000) return Color.FromArgb(100, 200, 200);      // Medium Cyan - Astral Glow
            if (rarity >= 10000000000000) return Color.FromArgb(200, 200, 100);       // Medium Yellow - Celestial Shine
            
            // Ultra-Legendary Tiers (100B - 1T) - Gedämpfte intensive Farben
            if (rarity >= 1000000000000) return Color.FromArgb(200, 80, 150);         // Medium Pink - Transcendent
            if (rarity >= 500000000000) return Color.FromArgb(80, 150, 200);          // Medium Blue - Absolute
            if (rarity >= 200000000000) return Color.FromArgb(150, 200, 80);          // Medium Lime - Eternal
            if (rarity >= 100000000000) return Color.FromArgb(200, 150, 80);          // Medium Peach - Infinite
            
            // Legendary+ Tiers (10B - 100B) - Gedämpfte helle Farben
            if (rarity >= 50000000000) return Color.FromArgb(200, 120, 0);            // Medium Orange - Ultra
            if (rarity >= 20000000000) return Color.FromArgb(160, 80, 200);           // Medium Purple-Pink - Mythic
            if (rarity >= 10000000000) return Color.FromArgb(80, 200, 120);           // Medium Mint - Divine
            
            // Epic+ Tiers (1B - 10B) - Gedämpfte mittlere Farben
            if (rarity >= 5000000000) return Color.FromArgb(200, 80, 80);             // Medium Salmon - Epic+
            if (rarity >= 2000000000) return Color.FromArgb(80, 120, 200);            // Medium Sky Blue - Cosmic
            if (rarity >= 1000000000) return Color.FromArgb(120, 200, 80);            // Medium Light Green - Universal
            
            // Rare+ Tiers (100M - 1B) - Kräftige gedämpfte Farben
            if (rarity >= 500000000) return Color.FromArgb(160, 120, 0);              // Darker Gold - Ancient
            if (rarity >= 200000000) return Color.FromArgb(120, 80, 160);             // Darker Violet - Primordial
            if (rarity >= 100000000) return Color.FromArgb(0, 120, 160);              // Darker Teal - Legendary
            
            // Uncommon+ Tiers (10M - 100M) - Gedämpfte kräftige Farben
            if (rarity >= 50000000) return Color.FromArgb(150, 100, 0);               // Dark Gold
            if (rarity >= 20000000) return Color.FromArgb(100, 70, 140);              // Dark Violet
            if (rarity >= 10000000) return Color.FromArgb(0, 100, 140);               // Dark Teal
            
            // Standard Tiers (1M - 10M) - Original Farben
            if (rarity >= 5000000) return Color.FromArgb(127, 107, 0);                // Very Dark Gold
            if (rarity >= 1000000) return Color.FromArgb(100, 50, 127);               // Dark Purple
            if (rarity >= 100000) return Color.FromArgb(0, 87, 127);                  // Dark Blue
            if (rarity >= 10000) return Color.FromArgb(0, 127, 63);                   // Dark Green
            
            return DarkAccent; // Common
        }

        private void OnRayanRolled_Handler(Rayan rayan)
        {
            // PERFORMANCE: Mark plots as dirty when new Rayan added
            _plotsDirty = true;
            ShowNewRayan(rayan);
        }
        
        private int CalculateFlashCount(double adjustedRarity)
        {
            // Flash thresholds based on adjusted rarity (rarity / luck)
            // NEW SYSTEM: Real probability of 1 in 125 (0.8%) = 3 flashes
            // adjustedRarity represents the "felt" rarity after luck is applied
            
            if (adjustedRarity >= 1000000000000000) return 15; // Ultra-Legendary
            if (adjustedRarity >= 100000000000000) return 14;
            if (adjustedRarity >= 10000000000000) return 13;
            if (adjustedRarity >= 1000000000000) return 12;
            if (adjustedRarity >= 100000000000) return 11;
            if (adjustedRarity >= 10000000000) return 10;
            if (adjustedRarity >= 1000000000) return 9;
            if (adjustedRarity >= 100000000) return 8;
            if (adjustedRarity >= 10000000) return 7;
            if (adjustedRarity >= 1000000) return 6;
            if (adjustedRarity >= 100000) return 5;
            if (adjustedRarity >= 10000) return 4;
            if (adjustedRarity >= 125) return 3; // ← NEW: 1 in 125 = 3 flashes (0.8%)
            if (adjustedRarity >= 50) return 2;  // 1 in 50 = 2 flashes (2%)
            if (adjustedRarity >= 25) return 1;  // 1 in 25 = 1 flash (4%)
            
            return 0; // No flash for common items (> 4% chance)
        }
        
        private async void TriggerFlashEffect(Color flashColor, int flashCount)
        {
            Console.WriteLine($"[Flash] Starting flash effect: {flashCount} flashes");
            
            // Store original color
            Color originalColor = this.BackColor;
            
            // Flash effect with 100ms duration as requested
            for (int i = 0; i < flashCount; i++)
            {
                Console.WriteLine($"[Flash] Flash #{i + 1}");
                
                // Flash to rarity color
                this.BackColor = flashColor;
                await Task.Delay(100); // Hold for 100ms (as requested)
                
                // Back to original
                this.BackColor = originalColor;
                await Task.Delay(100); // Wait 100ms before next flash
            }
            
            Console.WriteLine($"[Flash] Flash effect completed");
        }

        private void ShowNewRayan(Rayan rayan)
        {
            // Get tier color (Bronze, Silver, Gold) based on adjusted rarity
            Color tierColor = GetTierColor(rayan.AdjustedRarity);
            
            lblLastRoll.Text = $"🎲 {rayan.FullName}\n1 in {rayan.Rarity:N0}";
            lblLastRoll.ForeColor = tierColor;
        }
        
        /// <summary>
        /// Get tier color based on adjusted rarity (Bronze/Silver/Gold system)
        /// </summary>
        private Color GetTierColor(double adjustedRarity)
        {
            // GOLD Tier (Top 0.8% - 1 in 125 or rarer)
            if (adjustedRarity >= 125)
            {
                return Color.FromArgb(255, 215, 0); // Gold
            }
            
            // SILVER Tier (Top 2-4% - 1 in 25 to 1 in 125)
            if (adjustedRarity >= 25)
            {
                return Color.FromArgb(192, 192, 192); // Silver
            }
            
            // BRONZE Tier (Top 4-10% - 1 in 10 to 1 in 25)
            if (adjustedRarity >= 10)
            {
                return Color.FromArgb(205, 127, 50); // Bronze
            }
            
            // WHITE (Common - more than 10% chance)
            return Color.White;
        }
        
        private void ShowEventSelectionDialog()
        {
            // Admin mode: Show advanced event creation dialog
            if (_gameManager.AdminMode)
            {
                ShowAdminEventCreationDialog();
                return;
            }
            
            // Regular mode: Simple suffix selection
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
        
        private void ShowAdminEventCreationDialog()
        {
            var dialog = new Form
            {
                Text = "[ADMIN] Event erstellen - Erweiterte Optionen",
                Size = new Size(500, 700),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = DarkBackground
            };
            
            var titleLabel = new Label
            {
                Text = "⚙️ Admin Event Erstellung",
                Location = new Point(20, 20),
                Size = new Size(450, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = BrightRed
            };
            dialog.Controls.Add(titleLabel);
            
            var infoLabel = new Label
            {
                Text = "Gib die exakten Werte für die SupaBase DB ein (leer = null):",
                Location = new Point(20, 55),
                Size = new Size(450, 25),
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextColor
            };
            dialog.Controls.Add(infoLabel);
            
            int yPos = 90;
            int labelWidth = 170;
            int inputWidth = 280;
            int spacing = 45;
            
            // Event Name
            dialog.Controls.Add(CreateLabel("Event Name:", 20, yPos, labelWidth));
            var txtEventName = CreateTextBox("Suffix Event", 195, yPos, inputWidth);
            dialog.Controls.Add(txtEventName);
            yPos += spacing;
            
            // Suffix Name (Dropdown mit null-Option)
            dialog.Controls.Add(CreateLabel("Suffix Name:", 20, yPos, labelWidth));
            var comboSuffix = new ComboBox
            {
                Location = new Point(195, yPos),
                Size = new Size(inputWidth, 25),
                Font = new Font("Segoe UI", 10F),
                BackColor = DarkAccent,
                ForeColor = TextColor,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            
            // Add "(kein Suffix)" as null option
            comboSuffix.Items.Add("(kein Suffix)");
            
            // Add all suffixes from RayanData
            foreach (var suffix in RayanData.Suffixes)
            {
                comboSuffix.Items.Add(suffix.Suffix);
            }
            
            comboSuffix.SelectedIndex = 0; // Default to "(kein Suffix)"
            dialog.Controls.Add(comboSuffix);
            yPos += spacing;
            
            // Suffix Boost Multiplier (only visible if suffix is selected)
            var lblSuffixBoost = CreateLabel("Suffix Boost (wenn gesetzt):", 20, yPos, labelWidth);
            lblSuffixBoost.Visible = false;
            dialog.Controls.Add(lblSuffixBoost);
            
            var numSuffixBoost = CreateNumericUpDown(20.0M, 195, yPos, inputWidth);
            numSuffixBoost.Visible = false;
            numSuffixBoost.Minimum = 1M;
            numSuffixBoost.Maximum = 1000M;
            numSuffixBoost.DecimalPlaces = 1;
            dialog.Controls.Add(numSuffixBoost);
            
            // Show/hide suffix boost based on suffix selection
            comboSuffix.SelectedIndexChanged += (s, e) =>
            {
                bool hasSuffix = comboSuffix.SelectedIndex > 0; // Index 0 is "(kein Suffix)"
                lblSuffixBoost.Visible = hasSuffix;
                numSuffixBoost.Visible = hasSuffix;
            };
            yPos += spacing;
            
            // Created From
            dialog.Controls.Add(CreateLabel("Created From:", 20, yPos, labelWidth));
            var txtCreatedFrom = CreateTextBox(_gameManager.MultiplayerUsername ?? Environment.UserName, 195, yPos, inputWidth);
            dialog.Controls.Add(txtCreatedFrom);
            yPos += spacing;
            
            // Starts At (DateTime) - Shows local time, will be converted to UTC on save
            dialog.Controls.Add(CreateLabel("Starts At (Lokal):", 20, yPos, labelWidth));
            var dtpStartsAt = new DateTimePicker
            {
                Location = new Point(195, yPos),
                Size = new Size(inputWidth, 25),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss",
                Value = DateTime.Now // Current LOCAL time
            };
            dialog.Controls.Add(dtpStartsAt);
            yPos += spacing;
            
            // Duration (Minutes)
            dialog.Controls.Add(CreateLabel("Duration (Minutes):", 20, yPos, labelWidth));
            var numDuration = new NumericUpDown
            {
                Location = new Point(195, yPos),
                Size = new Size(inputWidth, 25),
                Font = new Font("Segoe UI", 10F),
                BackColor = DarkAccent,
                ForeColor = TextColor,
                DecimalPlaces = 1,
                Increment = 0.5M,
                Minimum = 0.5M,
                Maximum = 1440M,
                Value = 2.5M
            };
            dialog.Controls.Add(numDuration);
            yPos += spacing;
            
            // Luck Multiplier (nullable)
            dialog.Controls.Add(CreateLabel("Luck Multiplier (leer = null):", 20, yPos, labelWidth));
            var txtLuckMult = CreateTextBox("", 195, yPos, inputWidth);
            txtLuckMult.PlaceholderText = "z.B. 1.5 (leer = kein Effekt)";
            dialog.Controls.Add(txtLuckMult);
            yPos += spacing;
            
            // Money Multiplier (nullable)
            dialog.Controls.Add(CreateLabel("Money Multiplier (leer = null):", 20, yPos, labelWidth));
            var txtMoneyMult = CreateTextBox("", 195, yPos, inputWidth);
            txtMoneyMult.PlaceholderText = "z.B. 2.0 (leer = kein Effekt)";
            dialog.Controls.Add(txtMoneyMult);
            yPos += spacing;
            
            // Roll Time (nullable) - Speed modifier
            dialog.Controls.Add(CreateLabel("Roll Time (leer = null):", 20, yPos, labelWidth));
            var txtRollTime = CreateTextBox("", 195, yPos, inputWidth);
            txtRollTime.PlaceholderText = "z.B. 0.5 = doppelt so schnell";
            dialog.Controls.Add(txtRollTime);
            
            var lblRollTimeHint = new Label
            {
                Text = "⚡ 0.5 = doppelt so schnell, 2.0 = halb so schnell",
                Location = new Point(195, yPos + 28),
                Size = new Size(inputWidth, 20),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                ForeColor = Color.Gray
            };
            dialog.Controls.Add(lblRollTimeHint);
            yPos += spacing + 25;
            
            // Create Event button
            var btnCreate = new Button
            {
                Text = "🚀 Event erstellen",
                Location = new Point(20, yPos),
                Size = new Size(220, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = BrightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Click += async (s, e) =>
            {
                try
                {
                    // Parse nullable fields
                    float? luckMult = null;
                    if (!string.IsNullOrWhiteSpace(txtLuckMult.Text) && 
                        float.TryParse(txtLuckMult.Text, out float luckVal) && 
                        luckVal != 1.0f)
                    {
                        luckMult = luckVal;
                    }
                    
                    float? moneyMult = null;
                    if (!string.IsNullOrWhiteSpace(txtMoneyMult.Text) && 
                        float.TryParse(txtMoneyMult.Text, out float moneyVal) && 
                        moneyVal != 1.0f)
                    {
                        moneyMult = moneyVal;
                    }
                    
                    float? rollTime = null;
                    if (!string.IsNullOrWhiteSpace(txtRollTime.Text) && 
                        float.TryParse(txtRollTime.Text, out float rollVal) && 
                        rollVal != 1.0f)
                    {
                        rollTime = rollVal;
                    }
                    
                    string? suffixName = comboSuffix.SelectedIndex == 0 
                        ? null 
                        : comboSuffix.SelectedItem?.ToString();
                    
                    // Build SharedEventData object
                    var eventData = new SharedEventData
                    {
                        EventName = txtEventName.Text,
                        SuffixName = suffixName,
                        SuffixBoostMultiplier = suffixName != null ? (double)numSuffixBoost.Value : null,
                        CreatedFrom = txtCreatedFrom.Text,
                        StartsAt = dtpStartsAt.Value.ToUniversalTime(),
                        EndsAt = dtpStartsAt.Value.ToUniversalTime().AddMinutes((double)numDuration.Value),
                        LuckMultiplier = luckMult,
                        MoneyMultiplier = moneyMult,
                        RollTime = rollTime
                    };
                    
                    // Publish via EventSyncService
                    if (_gameManager.IsMultiplayerEnabled)
                    {
                        var eventSync = typeof(GameManager).GetField("_eventSync", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.GetValue(_gameManager) as EventSyncService;
                        
                        if (eventSync != null)
                        {
                            // Use PublishCustomEventAsync for full control over all fields
                            // This will also apply the event immediately to this client
                            await eventSync.PublishCustomEventAsync(eventData);
                            
                            dialog.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            "❌ Multiplayer nicht aktiv!\n\n" +
                            "Admin-Events können nur im Multiplayer-Modus erstellt werden.",
                            "Fehler",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"❌ Fehler beim Erstellen des Events:\n\n{ex.Message}",
                        "Fehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            };
            dialog.Controls.Add(btnCreate);
            
            // Cancel button
            var btnCancel = new Button
            {
                Text = "Abbrechen",
                Location = new Point(255, yPos),
                Size = new Size(220, 45),
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
        
        private Label CreateLabel(string text, int x, int y, int width)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = TextColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }
        
        private TextBox CreateTextBox(string text, int x, int y, int width)
        {
            return new TextBox
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10F),
                BackColor = DarkAccent,
                ForeColor = TextColor
            };
        }
        
        private NumericUpDown CreateNumericUpDown(decimal value, int x, int y, int width)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10F),
                BackColor = DarkAccent,
                ForeColor = TextColor,
                DecimalPlaces = 2,
                Increment = 0.1M,
                Minimum = 0.1M,
                Maximum = 1000M,
                Value = value
            };
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
            _rollCooldownRemaining = _gameManager.GetEffectiveRollCooldown();
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
            }, _databaseService, _currentSavefileId);
            optionsForm.ShowDialog();
        }
        
        private void btnLeaderboard_Click(object sender, EventArgs e)
        {
            if (_databaseService == null)
            {
                MessageBox.Show(
                    "Leaderboard ist nur im Online-Modus verfügbar!",
                    "Offline-Modus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }
            
            var leaderboardForm = new Forms.Dialogs.LeaderboardForm(_databaseService);
            leaderboardForm.ShowDialog();
        }
        
        private void btnFeedback_Click(object sender, EventArgs e)
        {
            if (_databaseService == null)
            {
                MessageBox.Show(
                    "Feedback ist nur im Online-Modus verfügbar!",
                    "Offline-Modus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }
            
            var feedbackForm = new Forms.Dialogs.FeedbackForm(_databaseService, _gameManager.Stats.MultiplayerUsername);
            feedbackForm.ShowDialog();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // Stop all timers
                _rollCooldownTimer?.Stop();
                _autoRollTimer?.Stop();
                _autoSaveTimer?.Stop();
                
                // Final synchronous save before closing (waits for DB)
                _gameManager.SaveSync();
                Console.WriteLine("[MainForm] Final save completed on form closing");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainForm] Error during form closing: {ex.Message}");
            }
            
            // Clean up cached images
            foreach (var image in _diceImageCache.Values)
            {
                image?.Dispose();
            }
            _diceImageCache.Clear();
            
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
