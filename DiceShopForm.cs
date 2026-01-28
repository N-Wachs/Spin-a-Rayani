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
    public partial class DiceShopForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Action _onDicePurchased;
        
        // Dark Mode Colors
        private readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
        private readonly Color DarkPanel = Color.FromArgb(45, 45, 48);
        private readonly Color DarkAccent = Color.FromArgb(60, 60, 65);
        private readonly Color BrightGreen = Color.FromArgb(0, 255, 127);
        private readonly Color BrightBlue = Color.FromArgb(0, 174, 255);
        private readonly Color BrightGold = Color.FromArgb(255, 215, 0);
        private readonly Color TextColor = Color.FromArgb(230, 230, 230);

        // Available Dices (templates for shop)
        private List<Dice> _availableDices = new List<Dice>
        {
            // Tier 1: Starter
            new Dice { Name = "Silver Dice", LuckMultiplier = 1.5, Cost = 2500 },
            new Dice { Name = "Bronze Dice", LuckMultiplier = 1.8, Cost = 5000 },
            
            // Tier 2: Common
            new Dice { Name = "Golden Dice", LuckMultiplier = 2.0, Cost = 20000 },
            new Dice { Name = "Copper Dice", LuckMultiplier = 2.3, Cost = 40000 },
            new Dice { Name = "Iron Dice", LuckMultiplier = 2.6, Cost = 75000 },
            
            // Tier 3: Uncommon
            new Dice { Name = "Diamond Dice", LuckMultiplier = 3.0, Cost = 100000 },
            new Dice { Name = "Ruby Dice", LuckMultiplier = 3.5, Cost = 200000 },
            new Dice { Name = "Sapphire Dice", LuckMultiplier = 4.0, Cost = 350000 },
            
            // Tier 4: Rare
            new Dice { Name = "Platinum Dice", LuckMultiplier = 5.0, Cost = 500000 },
            new Dice { Name = "Obsidian Dice", LuckMultiplier = 6.0, Cost = 800000 },
            new Dice { Name = "Jade Dice", LuckMultiplier = 7.0, Cost = 1200000 },
            
            // Tier 5: Epic
            new Dice { Name = "Emerald Dice", LuckMultiplier = 10.0, Cost = 2000000 },
            new Dice { Name = "Amethyst Dice", LuckMultiplier = 12.0, Cost = 3500000 },
            new Dice { Name = "Crystal Dice", LuckMultiplier = 15.0, Cost = 6000000 },
            
            // Tier 6: Legendary
            new Dice { Name = "Celestial Dice", LuckMultiplier = 20.0, Cost = 10000000 },
            new Dice { Name = "Divine Dice", LuckMultiplier = 30.0, Cost = 25000000 },
            new Dice { Name = "Cosmic Dice", LuckMultiplier = 50.0, Cost = 100000000 }
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
            lblMoney.Text = $"?? Money: {FormatBigInt(_gameManager.Stats.Money)}";
            panelDices.Controls.Clear();

            int yPosition = 10;
            foreach (var dice in _availableDices)
            {
                var dicePanel = CreateDicePanel(dice);
                dicePanel.Location = new Point(10, yPosition);
                panelDices.Controls.Add(dicePanel);
                yPosition += dicePanel.Height + 10;
            }
        }

        private Panel CreateDicePanel(Dice dice)
        {
            var panel = new Panel
            {
                Size = new Size(770, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DarkPanel
            };

            var lblName = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Text = dice.DisplayName,
                ForeColor = BrightGold
            };
            panel.Controls.Add(lblName);

            var lblDescription = new Label
            {
                Location = new Point(10, 40),
                Size = new Size(300, 20),
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
                Location = new Point(570, 20),
                Size = new Size(90, 40),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
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
                Location = new Point(670, 20),
                Size = new Size(80, 40),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
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
                LoadDiceShop();
                _onDicePurchased?.Invoke();
            }
        }
        
        private void BtnBuyMax_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Dice templateDice)
            {
                int quantityToBuy;
                
                if (_gameManager.AdminMode)
                {
                    // In Admin Mode: Buy 100 at once
                    quantityToBuy = 100;
                }
                else
                {
                    // Calculate how many we can afford
                    if (templateDice.Cost <= 0 || _gameManager.Stats.Money < templateDice.Cost)
                    {
                        return;
                    }
                    
                    quantityToBuy = (int)(_gameManager.Stats.Money / templateDice.Cost);
                    
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
                LoadDiceShop();
                _onDicePurchased?.Invoke();
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
