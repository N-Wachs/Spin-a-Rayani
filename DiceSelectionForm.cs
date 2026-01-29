using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SpinARayan.Models;
using SpinARayan.Services;

namespace SpinARayan
{
    public partial class DiceSelectionForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Action _onDiceSelected;
        
        private readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
        private readonly Color DarkPanel = Color.FromArgb(45, 45, 48);
        private readonly Color DarkAccent = Color.FromArgb(60, 60, 65);
        private readonly Color BrightGreen = Color.FromArgb(0, 255, 127);
        private readonly Color BrightBlue = Color.FromArgb(0, 174, 255);
        private readonly Color BrightGold = Color.FromArgb(255, 215, 0);
        private readonly Color TextColor = Color.FromArgb(230, 230, 230);

        public DiceSelectionForm(GameManager gameManager, Action onDiceSelected)
        {
            _gameManager = gameManager;
            _onDiceSelected = onDiceSelected;
            InitializeComponent();
            ApplyDarkMode();
            LoadDices();
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            panelDices.BackColor = DarkBackground;
        }

        private void LoadDices()
        {
            panelDices.Controls.Clear();
            var sortedDices = _gameManager.Stats.OwnedDices
                .Select((dice, index) => new { Dice = dice, OriginalIndex = index })
                .Where(x => x.Dice.IsInfinite || x.Dice.Quantity > 0)
                .OrderByDescending(x => x.Dice.LuckMultiplier)
                .ToList();

            int yPosition = 10;
            foreach (var item in sortedDices)
            {
                var panel = CreateDicePanel(item.Dice, item.OriginalIndex);
                panel.Location = new Point(10, yPosition);
                panelDices.Controls.Add(panel);
                yPosition += panel.Height + 10;
            }
        }

        private Panel CreateDicePanel(Dice dice, int index)
        {
            bool isSelected = _gameManager.Stats.SelectedDiceIndex == index;
            
            var panel = new Panel
            {
                Size = new Size(750, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = isSelected ? Color.FromArgb(0, 80, 120) : DarkPanel
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
                Text = dice.DisplayName,
                ForeColor = dice.IsInfinite ? BrightGreen : BrightGold
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

            var lblQuantity = new Label
            {
                Location = new Point(400, 10),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Text = $"Quantity: {dice.QuantityDisplay}",
                ForeColor = dice.IsInfinite ? BrightGreen : TextColor
            };
            panel.Controls.Add(lblQuantity);

            var btnSelect = new Button
            {
                Location = new Point(600, 25),
                Size = new Size(130, 50),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Text = isSelected ? "SELECTED" : "SELECT",
                Enabled = !isSelected,
                BackColor = isSelected ? DarkAccent : BrightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = index
            };
            btnSelect.FlatAppearance.BorderSize = 0;
            btnSelect.Click += (s, e) => {
                _gameManager.Stats.SelectedDiceIndex = index;
                _gameManager.Save();
                LoadDices();
                _onDiceSelected?.Invoke();
            };
            panel.Controls.Add(btnSelect);

            return panel;
        }
    }
}
