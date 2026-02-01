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
    public partial class DiceShopForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Action _onDicePurchased;
        
        // Modern Theme Colors (from ModernTheme.cs)
        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color DarkAccent = ModernTheme.PrimaryMedium;
        private readonly Color BrightGreen = ModernTheme.Success;
        private readonly Color BrightBlue = ModernTheme.AccentBlue;
        private readonly Color BrightGold = ModernTheme.Warning;
        private readonly Color BrightRed = ModernTheme.Error;
        private readonly Color TextColor = ModernTheme.TextPrimary;

        // Available Dices (templates for shop)
        private List<Dice> _availableDices = new List<Dice>
        {
            // Tier 1: Starter (x3)
            new Dice { Name = "Silver Dice", LuckMultiplier = 1.5, Cost = 7500 },
            new Dice { Name = "Bronze Dice", LuckMultiplier = 1.8, Cost = 15000 },
            
            // Tier 2: Common (x3)
            new Dice { Name = "Golden Dice", LuckMultiplier = 2.0, Cost = 60000 },
            new Dice { Name = "Copper Dice", LuckMultiplier = 2.3, Cost = 120000 },
            new Dice { Name = "Iron Dice", LuckMultiplier = 2.6, Cost = 225000 },
            
            // Tier 3: Uncommon (x3)
            new Dice { Name = "Diamond Dice", LuckMultiplier = 3.0, Cost = 300000 },
            new Dice { Name = "Ruby Dice", LuckMultiplier = 3.5, Cost = 600000 },
            new Dice { Name = "Sapphire Dice", LuckMultiplier = 4.0, Cost = 1050000 },
            
            // Tier 4: Rare (x3)
            new Dice { Name = "Platinum Dice", LuckMultiplier = 5.0, Cost = 1500000 },
            new Dice { Name = "Obsidian Dice", LuckMultiplier = 6.0, Cost = 2400000 },
            new Dice { Name = "Jade Dice", LuckMultiplier = 7.0, Cost = 3600000 },
            
            // Tier 5: Epic (x3)
            new Dice { Name = "Emerald Dice", LuckMultiplier = 10.0, Cost = 6000000 },
            new Dice { Name = "Amethyst Dice", LuckMultiplier = 12.0, Cost = 10500000 },
            new Dice { Name = "Crystal Dice", LuckMultiplier = 15.0, Cost = 18000000 },
            
            // Tier 6: Legendary (x3)
            new Dice { Name = "Celestial Dice", LuckMultiplier = 20.0, Cost = 30000000 },
            new Dice { Name = "Divine Dice", LuckMultiplier = 30.0, Cost = 75000000 },
            new Dice { Name = "Cosmic Dice", LuckMultiplier = 50.0, Cost = 300000000 },
            
            // Tier 7: Mythic (x3)
            new Dice { Name = "Quantum Dice", LuckMultiplier = 75.0, Cost = 750000000 },
            new Dice { Name = "Void Dice", LuckMultiplier = 100.0, Cost = 1500000000 },
            new Dice { Name = "Ethereal Dice", LuckMultiplier = 150.0, Cost = 3000000000 },
            new Dice { Name = "Astral Dice", LuckMultiplier = 200.0, Cost = 7500000000 },
            new Dice { Name = "Primordial Dice", LuckMultiplier = 300.0, Cost = 15000000000 },
            
            // Tier 8: Transcendent (x3)
            new Dice { Name = "Eternal Dice", LuckMultiplier = 500.0, Cost = 30000000000 },
            new Dice { Name = "Infinite Dice", LuckMultiplier = 750.0, Cost = 75000000000 },
            new Dice { Name = "Absolute Dice", LuckMultiplier = 1000.0, Cost = 150000000000 },
            new Dice { Name = "Omnipotent Dice", LuckMultiplier = 1500.0, Cost = 300000000000 },
            new Dice { Name = "Godlike Dice", LuckMultiplier = 2500.0, Cost = 750000000000 },
            
            // Tier 9: Ultimate (x3)
            new Dice { Name = "Alpha Dice", LuckMultiplier = 5000.0, Cost = 3000000000000 },
            new Dice { Name = "Omega Dice", LuckMultiplier = 10000.0, Cost = 15000000000000 },
            new Dice { Name = "Zenith Dice", LuckMultiplier = 25000.0, Cost = 75000000000000 },
            new Dice { Name = "Apex Dice", LuckMultiplier = 50000.0, Cost = 300000000000000 },
            new Dice { Name = "Supreme Dice", LuckMultiplier = 100000.0, Cost = 3000000000000000 }
        };

        public DiceShopForm(GameManager gameManager, Action onDicePurchased)
        {
            _gameManager = gameManager;
            _onDicePurchased = onDicePurchased;
            InitializeComponent();
            ApplyDarkMode();
            InitializeOwnedDices();
            LoadDiceShop();
        }

        private void InitializeOwnedDices()
        {
            // Basic Dice initialization is now handled by SaveService.Load()
            // This method is now empty but kept for future use
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            panelDices.BackColor = DarkBackground;
            lblTitle.ForeColor = BrightGold;
            lblMoney.ForeColor = BrightGreen;
        }

        private void LoadDiceShop()
        {
            lblMoney.Text = $"\U0001F4B0 Money: {FormatBigInt(_gameManager.Stats.Money)}";
            
            panelDices.SuspendLayout(); // Verhindert Flackern
            panelDices.Controls.Clear();

            int yPosition = 10;
            foreach (var dice in _availableDices)
            {
                var dicePanel = CreateDicePanel(dice);
                dicePanel.Location = new Point(10, yPosition);
                panelDices.Controls.Add(dicePanel);
                yPosition += dicePanel.Height + 10;
            }
            
            panelDices.ResumeLayout(); // Aktiviert Layout wieder
        }
        
        private void UpdateShopUI()
        {
            // Nur Geld und Button-States updaten, KEINE Panels neu erstellen
            lblMoney.Text = $"\U0001F4B0 Money: {FormatBigInt(_gameManager.Stats.Money)}";
            
            // Update button states in existing panels
            foreach (Control control in panelDices.Controls)
            {
                if (control is Panel dicePanel && dicePanel.Tag is Dice dice)
                {
                    // Find and update buttons
                    foreach (Control panelControl in dicePanel.Controls)
                    {
                        if (panelControl is Button btn)
                        {
                            bool canAfford = _gameManager.AdminMode || _gameManager.Stats.Money >= dice.Cost;
                            btn.Enabled = canAfford;
                        }
                    }
                }
            }
        }

        private Panel CreateDicePanel(Dice dice)
        {
            var panel = new Panel
            {
                Size = new Size(770, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DarkPanel,
                Tag = dice // Speichere dice reference fuer Updates
            };

            // Try to load dice image from embedded resources
            var picBox = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(80, 80), // Square image, almost full height (100-20 margins)
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };
            
            try
            {
                // New naming system: "Absolute Dice.jpg"
                string imageName = dice.Name + ".jpg";
                string resourceName = $"Spin_a_Rayan.Assets.{imageName}";
                
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        picBox.Image = Image.FromStream(stream);
                    }
                    else
                    {
                        Console.WriteLine($"[DiceShop] Resource not found: {resourceName}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Silently fail if image not found
                Console.WriteLine($"[DiceShop] Could not load image for {dice.Name}: {ex.Message}");
            }
            
            panel.Controls.Add(picBox);

            var lblName = new Label
            {
                Location = new Point(100, 10), // Moved right for image
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Text = dice.DisplayName,
                ForeColor = BrightGold
            };
            panel.Controls.Add(lblName);

            var lblDescription = new Label
            {
                Location = new Point(100, 40), // Moved right for image
                Size = new Size(250, 20),
                Font = new Font("Segoe UI", 10F),
                Text = dice.Description,
                ForeColor = BrightBlue
            };
            panel.Controls.Add(lblDescription);

            var lblCost = new Label
            {
                Location = new Point(350, 10),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Text = $"Cost: {dice.CostDisplay}",
                ForeColor = TextColor
            };
            panel.Controls.Add(lblCost);

            var btnBuy = new Button
            {
                Location = new Point(570, 10),
                Size = new Size(90, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = _gameManager.AdminMode ? "FREE" : "BUY",
                Enabled = _gameManager.AdminMode || _gameManager.Stats.Money >= dice.Cost,
                BackColor = BrightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = dice
            };
            btnBuy.FlatAppearance.BorderSize = 0;
            btnBuy.Click += BtnBuy_Click;
            panel.Controls.Add(btnBuy);
            
            var btnBuyMax = new Button
            {
                Location = new Point(670, 10),
                Size = new Size(80, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "MAX",
                Enabled = _gameManager.AdminMode || _gameManager.Stats.Money >= dice.Cost,
                BackColor = BrightGold,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Tag = dice
            };
            btnBuyMax.FlatAppearance.BorderSize = 0;
            btnBuyMax.Click += BtnBuyMax_Click;
            panel.Controls.Add(btnBuyMax);
            
            var btnCustom = new Button
            {
                Location = new Point(570, 55),
                Size = new Size(180, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "CUSTOM",
                Enabled = _gameManager.AdminMode || _gameManager.Stats.Money >= dice.Cost,
                BackColor = BrightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = dice
            };
            btnCustom.FlatAppearance.BorderSize = 0;
            btnCustom.Click += BtnCustom_Click;
            panel.Controls.Add(btnCustom);

            return panel;
        }

        private void BtnBuy_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Dice templateDice)
            {
                if (!_gameManager.AdminMode && _gameManager.Stats.Money < templateDice.Cost)
                {
                    return;
                }

                if (!_gameManager.AdminMode)
                {
                    _gameManager.Stats.Money -= templateDice.Cost;
                }

                // Find existing dice in owned list or create new
                var existingDice = _gameManager.Stats.OwnedDices.FirstOrDefault(d => d.Name == templateDice.Name);
                if (existingDice != null)
                {
                    // Increase quantity
                    existingDice.Quantity++;
                }
                else
                {
                    // Add new dice
                    _gameManager.Stats.OwnedDices.Add(new Dice
                    {
                        Name = templateDice.Name,
                        LuckMultiplier = templateDice.LuckMultiplier,
                        Cost = templateDice.Cost,
                        Quantity = 1,
                        IsInfinite = false
                    });
                }

                _gameManager.Save();
                UpdateShopUI(); // Nur UI updaten, nicht neu laden!
                _onDicePurchased?.Invoke();
            }
        }
        
        private void BtnBuyMax_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Dice templateDice)
            {
                BigInteger quantityToBuy;
                
                if (_gameManager.AdminMode)
                {
                    // In Admin Mode: Buy 1000 at once
                    quantityToBuy = 1000;
                }
                else
                {
                    // Calculate how many we can afford
                    if (templateDice.Cost <= 0 || _gameManager.Stats.Money < templateDice.Cost)
                    {
                        return;
                    }
                    
                    quantityToBuy = _gameManager.Stats.Money / templateDice.Cost;
                    
                    if (quantityToBuy <= 0)
                    {
                        return;
                    }
                    
                    // Deduct money
                    _gameManager.Stats.Money -= templateDice.Cost * quantityToBuy;
                }

                // Find existing dice in owned list or create new
                var existingDice = _gameManager.Stats.OwnedDices.FirstOrDefault(d => d.Name == templateDice.Name);
                if (existingDice != null)
                {
                    // Increase quantity
                    existingDice.Quantity += quantityToBuy;
                }
                else
                {
                    // Add new dice
                    _gameManager.Stats.OwnedDices.Add(new Dice
                    {
                        Name = templateDice.Name,
                        LuckMultiplier = templateDice.LuckMultiplier,
                        Cost = templateDice.Cost,
                        Quantity = quantityToBuy,
                        IsInfinite = false
                    });
                }

                _gameManager.Save();
                UpdateShopUI(); // Nur UI updaten!
                _onDicePurchased?.Invoke();
            }
        }
        
        private void BtnCustom_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Dice templateDice)
            {
                // Ask user for custom amount
                using (var inputForm = new Form())
                {
                    inputForm.Text = "Custom Amount";
                    inputForm.Size = new Size(400, 180);
                    inputForm.StartPosition = FormStartPosition.CenterParent;
                    inputForm.BackColor = DarkBackground;
                    inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    inputForm.MaximizeBox = false;
                    inputForm.MinimizeBox = false;

                    var lblPrompt = new Label
                    {
                        Location = new Point(20, 20),
                        Size = new Size(360, 30),
                        Text = $"Wie viele {templateDice.Name} moechtest du kaufen?",
                        Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                        ForeColor = TextColor
                    };
                    
                    var txtAmount = new TextBox
                    {
                        Location = new Point(20, 60),
                        Size = new Size(360, 30),
                        Font = new Font("Segoe UI", 12F),
                        Text = "1"
                    };
                    
                    var lblCost = new Label
                    {
                        Location = new Point(20, 100),
                        Size = new Size(360, 25),
                        Font = new Font("Segoe UI", 10F),
                        ForeColor = BrightBlue,
                        Text = $"Kosten: {FormatBigInt(templateDice.Cost)}"
                    };
                    
                    var btnOk = new Button
                    {
                        Location = new Point(120, 140),
                        Size = new Size(80, 30),
                        Text = "OK",
                        DialogResult = DialogResult.OK,
                        BackColor = BrightGreen,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnOk.FlatAppearance.BorderSize = 0;
                    
                    var btnCancel = new Button
                    {
                        Location = new Point(210, 140),
                        Size = new Size(80, 30),
                        Text = "Cancel",
                        DialogResult = DialogResult.Cancel,
                        BackColor = DarkAccent,
                        ForeColor = TextColor,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnCancel.FlatAppearance.BorderSize = 0;
                    
                    // Update cost label when amount changes
                    txtAmount.TextChanged += (s, args) =>
                    {
                        if (BigInteger.TryParse(txtAmount.Text, out BigInteger amount) && amount > 0)
                        {
                            BigInteger totalCost = templateDice.Cost * amount;
                            lblCost.Text = $"Kosten: {FormatBigInt(totalCost)}";
                            lblCost.ForeColor = _gameManager.Stats.Money >= totalCost ? BrightGreen : BrightRed;
                        }
                        else
                        {
                            lblCost.Text = "Ungueltige Eingabe";
                            lblCost.ForeColor = BrightRed;
                        }
                    };

                    inputForm.Controls.Add(lblPrompt);
                    inputForm.Controls.Add(txtAmount);
                    inputForm.Controls.Add(lblCost);
                    inputForm.Controls.Add(btnOk);
                    inputForm.Controls.Add(btnCancel);
                    
                    inputForm.AcceptButton = btnOk;
                    inputForm.CancelButton = btnCancel;

                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        if (BigInteger.TryParse(txtAmount.Text, out BigInteger quantityToBuy) && quantityToBuy > 0)
                        {
                            BigInteger totalCost = templateDice.Cost * quantityToBuy;
                            
                            if (!_gameManager.AdminMode)
                            {
                                if (_gameManager.Stats.Money < totalCost)
                                {
                                    MessageBox.Show("Nicht genug Geld!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                                
                                _gameManager.Stats.Money -= totalCost;
                            }
                            
                            // Find existing dice or create new
                            var existingDice = _gameManager.Stats.OwnedDices.FirstOrDefault(d => d.Name == templateDice.Name);
                            if (existingDice != null)
                            {
                                existingDice.Quantity += quantityToBuy;
                            }
                            else
                            {
                                _gameManager.Stats.OwnedDices.Add(new Dice
                                {
                                    Name = templateDice.Name,
                                    LuckMultiplier = templateDice.LuckMultiplier,
                                    Cost = templateDice.Cost,
                                    Quantity = quantityToBuy,
                                    IsInfinite = false
                                });
                            }
                            
                            _gameManager.Save();
                            UpdateShopUI(); // Nur UI updaten!
                            _onDicePurchased?.Invoke();
                        }
                        else
                        {
                            MessageBox.Show("Ungueltige Anzahl!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private string FormatBigInt(BigInteger value)
        {
            if (value < 1000) return value.ToString();
            if (value < 1000000) return ((double)value / 1000).ToString("F1") + "K";
            if (value < 1000000000) return ((double)value / 1000000).ToString("F1") + "M";
            if (value < 1000000000000) return ((double)value / 1000000000).ToString("F1") + "B";
            return value.ToString("E1");
        }
    }
}
