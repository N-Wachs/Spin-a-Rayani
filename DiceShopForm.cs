using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        
        private readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
        private readonly Color DarkPanel = Color.FromArgb(45, 45, 48);
        private readonly Color DarkAccent = Color.FromArgb(60, 60, 65);
        private readonly Color BrightGreen = Color.FromArgb(0, 255, 127);
        private readonly Color BrightBlue = Color.FromArgb(0, 174, 255);
        private readonly Color BrightGold = Color.FromArgb(255, 215, 0);
        private readonly Color BrightRed = Color.FromArgb(255, 69, 58);
        private readonly Color TextColor = Color.FromArgb(230, 230, 230);

        private List<Dice> _availableDices = new List<Dice>
        {
            new Dice { Name = "Silver Dice", LuckMultiplier = 1.5, Cost = 7500 },
            new Dice { Name = "Bronze Dice", LuckMultiplier = 1.8, Cost = 15000 },
            new Dice { Name = "Golden Dice", LuckMultiplier = 2.0, Cost = 60000 },
            new Dice { Name = "Copper Dice", LuckMultiplier = 2.3, Cost = 120000 },
            new Dice { Name = "Iron Dice", LuckMultiplier = 2.6, Cost = 225000 },
            new Dice { Name = "Diamond Dice", LuckMultiplier = 3.0, Cost = 300000 },
            new Dice { Name = "Ruby Dice", LuckMultiplier = 3.5, Cost = 600000 },
            new Dice { Name = "Sapphire Dice", LuckMultiplier = 4.0, Cost = 1050000 },
            new Dice { Name = "Platinum Dice", LuckMultiplier = 5.0, Cost = 1500000 },
            new Dice { Name = "Obsidian Dice", LuckMultiplier = 6.0, Cost = 2400000 },
            new Dice { Name = "Jade Dice", LuckMultiplier = 7.0, Cost = 3600000 },
            new Dice { Name = "Emerald Dice", LuckMultiplier = 10.0, Cost = 6000000 },
            new Dice { Name = "Amethyst Dice", LuckMultiplier = 12.0, Cost = 10500000 },
            new Dice { Name = "Crystal Dice", LuckMultiplier = 15.0, Cost = 18000000 },
            new Dice { Name = "Celestial Dice", LuckMultiplier = 20.0, Cost = 30000000 },
            new Dice { Name = "Divine Dice", LuckMultiplier = 30.0, Cost = 75000000 },
            new Dice { Name = "Cosmic Dice", LuckMultiplier = 50.0, Cost = 300000000 },
            new Dice { Name = "Quantum Dice", LuckMultiplier = 75.0, Cost = 750000000 },
            new Dice { Name = "Void Dice", LuckMultiplier = 100.0, Cost = 1500000000 },
            new Dice { Name = "Ethereal Dice", LuckMultiplier = 150.0, Cost = 3000000000 },
            new Dice { Name = "Astral Dice", LuckMultiplier = 200.0, Cost = 7500000000 },
            new Dice { Name = "Primordial Dice", LuckMultiplier = 300.0, Cost = 15000000000 },
            new Dice { Name = "Eternal Dice", LuckMultiplier = 500.0, Cost = 30000000000 },
            new Dice { Name = "Infinite Dice", LuckMultiplier = 750.0, Cost = 75000000000 },
            new Dice { Name = "Absolute Dice", LuckMultiplier = 1000.0, Cost = 150000000000 },
            new Dice { Name = "Omnipotent Dice", LuckMultiplier = 1500.0, Cost = 300000000000 },
            new Dice { Name = "Godlike Dice", LuckMultiplier = 2500.0, Cost = 750000000000 },
            new Dice { Name = "Alpha Dice", LuckMultiplier = 5000.0, Cost = 3000000000000 },
            new Dice { Name = "Omega Dice", LuckMultiplier = 10000.0, Cost = 15000000000000 },
            new Dice { Name = "Zenith Dice", LuckMultiplier = 25000.0, Cost = 75000000000000 },
            new Dice { Name = "Apex Dice", LuckMultiplier = 5000.0, Cost = 300000000000000 },
            new Dice { Name = "Supreme Dice", LuckMultiplier = 100000.0, Cost = 3000000000000000 }
        };

        public DiceShopForm(GameManager gameManager, Action onDicePurchased)
        {
            _gameManager = gameManager;
            _onDicePurchased = onDicePurchased;
            InitializeComponent();
            ApplyDarkMode();
            LoadDiceShop();
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            panelDices.BackColor = DarkBackground;
        }

        private void LoadDiceShop()
        {
            panelDices.Controls.Clear();
            int yPosition = 10;
            foreach (var dice in _availableDices)
            {
                var panel = CreateDiceShopPanel(dice);
                panel.Location = new Point(10, yPosition);
                panelDices.Controls.Add(panel);
                yPosition += panel.Height + 10;
            }
        }

        private Panel CreateDiceShopPanel(Dice dice)
        {
            var panel = new Panel
            {
                Size = new Size(750, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DarkPanel
            };

            string fullPath = Path.Combine(Application.StartupPath, dice.ImagePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                var pbDice = new PictureBox
                {
                    Location = new Point(10, 10),
                    Size = new Size(80, 80),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(fullPath)
                };
                panel.Controls.Add(pbDice);
            }

            var lblName = new Label
            {
                Location = new Point(100, 10),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Text = dice.Name,
                ForeColor = BrightGold
            };
            panel.Controls.Add(lblName);

            var lblDescription = new Label
            {
                Location = new Point(100, 40),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 10F),
                Text = dice.Description,
                ForeColor = BrightBlue
            };
            panel.Controls.Add(lblDescription);

            var lblCost = new Label
            {
                Location = new Point(400, 10),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Text = $"Cost: {dice.CostDisplay}",
                ForeColor = _gameManager.Stats.Money >= dice.Cost ? BrightGreen : BrightRed
            };
            panel.Controls.Add(lblCost);

            var btnBuy = new Button
            {
                Location = new Point(600, 25),
                Size = new Size(130, 50),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Text = "BUY",
                BackColor = BrightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = dice
            };
            btnBuy.FlatAppearance.BorderSize = 0;
            btnBuy.Click += BtnBuy_Click;
            panel.Controls.Add(btnBuy);

            return panel;
        }

        private void BtnBuy_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Dice templateDice)
            {
                if (_gameManager.Stats.Money >= templateDice.Cost || _gameManager.AdminMode)
                {
                    if (!_gameManager.AdminMode) _gameManager.Stats.Money -= templateDice.Cost;
                    
                    var existingDice = _gameManager.Stats.OwnedDices.FirstOrDefault(d => d.Name == templateDice.Name);
                    if (existingDice != null) existingDice.Quantity += 100;
                    else _gameManager.Stats.OwnedDices.Add(new Dice { Name = templateDice.Name, LuckMultiplier = templateDice.LuckMultiplier, Cost = templateDice.Cost, Quantity = 100 });
                    
                    _gameManager.Save();
                    _onDicePurchased?.Invoke();
                    LoadDiceShop();
                }
            }
        }
    }
}
