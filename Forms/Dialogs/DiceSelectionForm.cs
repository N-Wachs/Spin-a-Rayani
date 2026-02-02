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

        // DPI-Aware: Hilfsmethode zum Skalieren von Werten basierend auf aktueller DPI
        private int ScaleForDpi(int baseValue)
        {
            return (int)(baseValue * this.DeviceDpi / 96.0f);
        }

        private void InitializeComponents()
        {
            this.Text = "Select Dice";
            this.Size = new Size(ScaleForDpi(880), ScaleForDpi(750)); // DPI-Aware: Skalierte Form-Größe
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AutoScaleMode = AutoScaleMode.Dpi; // DPI-Aware: AutoScaleMode setzen

            lblTitle = new Label
            {
                Location = new Point(ScaleForDpi(20), ScaleForDpi(20)),
                Size = new Size(ScaleForDpi(400), ScaleForDpi(40)),
                Font = new Font("Segoe UI Emoji", 18F, FontStyle.Bold, GraphicsUnit.Point),
                Text = "\U0001F3B2 SELECT DICE"
            };
            this.Controls.Add(lblTitle);

            panelDices = new Panel
            {
                Location = new Point(ScaleForDpi(20), ScaleForDpi(70)),
                Size = new Size(ScaleForDpi(820), ScaleForDpi(650)),
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

            // Sort dices: Basic Dice first, then by LuckMultiplier descending
            var sortedDices = _gameManager.Stats.OwnedDices
                .Select((dice, index) => new { Dice = dice, OriginalIndex = index })
                .Where(x => x.Dice.IsInfinite || x.Dice.Quantity > 0)
                .OrderBy(x => x.Dice.IsInfinite ? 0 : 1) // Infinite (Basic) first
                .ThenByDescending(x => x.Dice.LuckMultiplier) // Then by luck
                .ToList();

            int yPosition = ScaleForDpi(10); // DPI-Aware: Skalierte Y-Position
            foreach (var item in sortedDices)
            {
                var dicePanel = CreateDicePanel(item.Dice, item.OriginalIndex);
                dicePanel.Location = new Point(ScaleForDpi(10), yPosition);
                panelDices.Controls.Add(dicePanel);
                yPosition += dicePanel.Height + ScaleForDpi(10); // DPI-Aware: Skalierter Abstand
            }
        }

        private Panel CreateDicePanel(Dice dice, int index)
        {
            bool isSelected = _gameManager.Stats.SelectedDiceIndex == index;
            
            // DPI-Aware: Skalierte Panel-Größe
            var panel = new Panel
            {
                Size = new Size(ScaleForDpi(750), ScaleForDpi(80)),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = isSelected ? Color.FromArgb(0, 80, 120) : DarkPanel
            };

            // Try to load dice image from embedded resources
            // DPI-Aware: Skalierte Positionen und Größen
            var picBox = new PictureBox
            {
                Location = new Point(ScaleForDpi(10), ScaleForDpi(5)),
                Size = new Size(ScaleForDpi(70), ScaleForDpi(70)),
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

            // DPI-Aware: Skalierte Label-Positionen und -Größen
            var lblName = new Label
            {
                Location = new Point(ScaleForDpi(90), ScaleForDpi(10)),
                Size = new Size(ScaleForDpi(250), ScaleForDpi(25)),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point),
                Text = dice.DisplayName,
                ForeColor = dice.IsInfinite ? BrightGreen : BrightGold
            };
            panel.Controls.Add(lblName);

            var lblDescription = new Label
            {
                Location = new Point(ScaleForDpi(90), ScaleForDpi(40)),
                Size = new Size(ScaleForDpi(250), ScaleForDpi(20)),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Text = dice.Description,
                ForeColor = BrightBlue
            };
            panel.Controls.Add(lblDescription);

            var lblQuantity = new Label
            {
                Location = new Point(ScaleForDpi(350), ScaleForDpi(10)),
                Size = new Size(ScaleForDpi(200), ScaleForDpi(25)),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point),
                Text = $"Quantity: {dice.QuantityDisplay}",
                ForeColor = dice.IsInfinite ? BrightGreen : dice.Quantity > 0 ? TextColor : Color.Red
            };
            panel.Controls.Add(lblQuantity);

            var lblLuck = new Label
            {
                Location = new Point(ScaleForDpi(350), ScaleForDpi(40)),
                Size = new Size(ScaleForDpi(200), ScaleForDpi(20)),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Text = $"Luck: {dice.LuckMultiplier:F1}x",
                ForeColor = BrightGold
            };
            panel.Controls.Add(lblLuck);

            // DPI-Aware: Skalierte Button-Position und -Größe
            var btnSelect = new Button
            {
                Location = new Point(ScaleForDpi(600), ScaleForDpi(20)),
                Size = new Size(ScaleForDpi(130), ScaleForDpi(40)),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
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
