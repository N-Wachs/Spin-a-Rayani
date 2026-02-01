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
    public partial class DiceSelectionForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Action _onDiceSelected;
        private Panel panelDices;
        private Label lblTitle;
        
        // Modern Theme Colors (from ModernTheme.cs)
        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color DarkAccent = ModernTheme.PrimaryMedium;
        private readonly Color BrightGreen = ModernTheme.Success;
        private readonly Color BrightBlue = ModernTheme.AccentBlue;
        private readonly Color BrightGold = ModernTheme.Warning;
        private readonly Color TextColor = ModernTheme.TextPrimary;

        public DiceSelectionForm(GameManager gameManager, Action onDiceSelected)
        {
            _gameManager = gameManager;
            _onDiceSelected = onDiceSelected;
            InitializeComponents();
            ApplyDarkMode();
            LoadDices();
        }

        private void InitializeComponents()
        {
            this.Text = "Select Dice";
            this.Size = new Size(880, 750);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            lblTitle = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(400, 40),
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                Text = "?? SELECT DICE"
            };
            this.Controls.Add(lblTitle);

            panelDices = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(820, 650),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(panelDices);
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            lblTitle.ForeColor = BrightGold;
            panelDices.BackColor = DarkBackground;
        }

        private void LoadDices()
        {
            panelDices.Controls.Clear();

            // Sort dices by LuckMultiplier descending (highest first)
            var sortedDices = _gameManager.Stats.OwnedDices
                .Select((dice, index) => new { Dice = dice, OriginalIndex = index })
                .Where(x => x.Dice.IsInfinite || x.Dice.Quantity > 0)
                .OrderByDescending(x => x.Dice.LuckMultiplier)
                .ToList();

            int yPosition = 10;
            foreach (var item in sortedDices)
            {
                var dicePanel = CreateDicePanel(item.Dice, item.OriginalIndex);
                dicePanel.Location = new Point(10, yPosition);
                panelDices.Controls.Add(dicePanel);
                yPosition += dicePanel.Height + 10;
            }
        }

        private Panel CreateDicePanel(Dice dice, int index)
        {
            bool isSelected = _gameManager.Stats.SelectedDiceIndex == index;
            
            var panel = new Panel
            {
                Size = new Size(750, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = isSelected ? Color.FromArgb(0, 80, 120) : DarkPanel
            };

            // Try to load dice image from embedded resources
            var picBox = new PictureBox
            {
                Location = new Point(10, 5),
                Size = new Size(70, 70), // Square image, almost full height
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiceSelection] Could not load image for {dice.Name}: {ex.Message}");
            }
            
            panel.Controls.Add(picBox);

            var lblName = new Label
            {
                Location = new Point(90, 10), // Moved right for image
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Text = dice.DisplayName,
                ForeColor = dice.IsInfinite ? BrightGreen : BrightGold
            };
            panel.Controls.Add(lblName);

            var lblDescription = new Label
            {
                Location = new Point(90, 40), // Moved right for image
                Size = new Size(250, 20),
                Font = new Font("Segoe UI", 10F),
                Text = dice.Description,
                ForeColor = BrightBlue
            };
            panel.Controls.Add(lblDescription);

            var lblQuantity = new Label
            {
                Location = new Point(350, 10),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Text = $"Quantity: {dice.QuantityDisplay}",
                ForeColor = dice.IsInfinite ? BrightGreen : dice.Quantity > 0 ? TextColor : Color.Red
            };
            panel.Controls.Add(lblQuantity);

            var lblLuck = new Label
            {
                Location = new Point(350, 40),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 10F),
                Text = $"Luck: {dice.LuckMultiplier:F1}x",
                ForeColor = BrightGold
            };
            panel.Controls.Add(lblLuck);

            var btnSelect = new Button
            {
                Location = new Point(600, 20),
                Size = new Size(130, 40),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = isSelected ? "SELECTED" : "SELECT",
                Enabled = !isSelected && (dice.IsInfinite || dice.Quantity > 0),
                BackColor = isSelected ? DarkAccent : BrightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = index
            };
            btnSelect.FlatAppearance.BorderSize = 0;
            btnSelect.Click += BtnSelect_Click;
            panel.Controls.Add(btnSelect);

            return panel;
        }

        private void BtnSelect_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is int index)
            {
                _gameManager.Stats.SelectedDiceIndex = index;
                _gameManager.Save();
                LoadDices();
                _onDiceSelected?.Invoke();
            }
        }
    }
}
