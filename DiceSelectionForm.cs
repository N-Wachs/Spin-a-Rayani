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
    public partial class DiceSelectionForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Action _onDiceSelected;
        private Panel panelDices;
        private Label lblTitle;
        
        // Dark Mode Colors
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

            // Würfelbild hinzufügen
            var pictureBox = new PictureBox
            {
                Location = new Point(10, 15),
                Size = new Size(50, 50),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };
            
            // Bild laden basierend auf Würfelname
            try
            {
                string imageName = dice.Name.ToLower().Replace(" ", "_");
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", $"dice_{imageName}.png");
                
                if (File.Exists(imagePath))
                {
                    pictureBox.Image = Image.FromFile(imagePath);
                }
            }
            catch (Exception ex)
            {
                // Fehler beim Bildladen ignorieren - PictureBox bleibt leer
                Console.WriteLine($"Fehler beim Laden des Würfelbildes für {dice.Name}: {ex.Message}");
            }
            
            panel.Controls.Add(pictureBox);

            // Labels nach rechts verschieben (Platz für Bild)
            var lblName = new Label
            {
                Location = new Point(70, 10),
                Size = new Size(260, 25),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Text = dice.DisplayName,
                ForeColor = dice.IsInfinite ? BrightGreen : BrightGold
            };
            panel.Controls.Add(lblName);

            var lblDescription = new Label
            {
                Location = new Point(70, 40),
                Size = new Size(260, 20),
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
