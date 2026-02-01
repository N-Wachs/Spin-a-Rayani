using System.Numerics;
using SpinARayan.Models;

namespace SpinARayan
{
    public partial class FullInventoryForm : Form
    {
        private readonly List<Rayan> _inventory;
        private readonly Action _onInventoryChanged;

        // Modern Theme Colors
        private readonly Color DarkBackground = Color.FromArgb(30, 30, 30);        // BackgroundElevated
        private readonly Color DarkPanel = Color.FromArgb(38, 38, 38);             // BackgroundPanel
        private readonly Color DarkAccent = Color.FromArgb(48, 63, 159);           // PrimaryMedium
        private readonly Color BrightGreen = Color.FromArgb(76, 175, 80);          // Success Green
        private readonly Color BrightBlue = Color.FromArgb(33, 150, 243);          // AccentBlue
        private readonly Color BrightGold = Color.FromArgb(255, 193, 7);           // Warning Amber
        private readonly Color TextColor = Color.FromArgb(255, 255, 255);          // White

        public FullInventoryForm(List<Rayan> inventory, Action onInventoryChanged)
        {
            _inventory = inventory;
            _onInventoryChanged = onInventoryChanged;
            InitializeComponent();
            ApplyDarkMode();
            LoadInventory();
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            panelInventory.BackColor = DarkBackground;
            lblTitle.ForeColor = BrightGold;
            lblTotalRayans.ForeColor = TextColor;
        }

        private void LoadInventory()
        {
            // CRITICAL FIX: Dispose old controls BEFORE clearing to prevent handle leak!
            // When you have 100+ groups, each with 5 controls = 500+ handles!
            // Without disposal, these handles accumulate until Windows runs out (limit: ~10,000)
            foreach (Control control in panelInventory.Controls)
            {
                control.Dispose();
            }
            
            // Performance: Suspend layout w�hrend wir alle Panels erstellen
            panelInventory.SuspendLayout();
            panelInventory.Controls.Clear();

            // Group und sortiere EINMAL statt mehrmals
            var groupedRayans = _inventory
                .GroupBy(r => new { r.Prefix, r.Suffix, r.Rarity, r.BaseValue })
                .Select(g => new
                {
                    Prefix = g.Key.Prefix,
                    Suffix = g.Key.Suffix,
                    FullName = g.First().FullName,
                    Rarity = g.Key.Rarity,
                    BaseValue = g.Key.BaseValue,
                    Count = g.Count(),
                    MaxMultiplier = g.Max(r => r.Multiplier),
                    AvgMultiplier = g.Average(r => r.Multiplier),
                    MaxValue = g.Max(r => (double)r.TotalValue),
                    AvgValue = g.Average(r => (double)r.TotalValue),
                    CanMerge = g.Count() >= 5
                })
                .OrderByDescending(x => x.MaxValue)
                .ToList();

            lblTotalRayans.Text = $"Gesamt Rayans: {_inventory.Count} (Unique: {groupedRayans.Count})";

            // PERFORMANCE: Limit display to prevent handle exhaustion
            // If you have 1000+ unique Rayans, only show top 500
            const int MAX_DISPLAY = 500;
            int displayCount = Math.Min(groupedRayans.Count, MAX_DISPLAY);
            
            if (groupedRayans.Count > MAX_DISPLAY)
            {
                lblTotalRayans.Text += $" (Zeige Top {MAX_DISPLAY})";
            }

            // Erstelle alle Panels in einer Schleife
            int yPosition = 10;
            for (int i = 0; i < displayCount; i++)
            {
                var group = groupedRayans[i];
                var rayanPanel = CreateRayanPanel(group.FullName, group.Rarity, group.Count,
                    group.AvgMultiplier, group.MaxMultiplier, group.AvgValue, group.CanMerge,
                    group.Prefix, group.Suffix, group.BaseValue);
                rayanPanel.Location = new Point(10, yPosition);
                panelInventory.Controls.Add(rayanPanel);
                yPosition += rayanPanel.Height + 10;
            }
            
            // Performance: Resume layout - nur EIN UI-Update!
            panelInventory.ResumeLayout();
        }

        private Panel CreateRayanPanel(string fullName, double rarity, int count, double avgMultiplier,
            double maxMultiplier, double avgValue, bool canMerge,
            string prefix, string suffix, BigInteger baseValue)
        {
            var panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(740, 90),
                BackColor = GetRarityColor(rarity)
            };

            var lblName = new Label
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(400, 25),
                Text = fullName,
                ForeColor = TextColor
            };

            var lblRarity = new Label
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(10, 35),
                Size = new Size(250, 20),
                Text = $"Rarity: 1 in {rarity:N0}",
                ForeColor = TextColor
            };

            var lblCount = new Label
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(10, 55),
                Size = new Size(250, 20),
                Text = $"Anzahl: {count} (� {avgMultiplier:F1}x, max {maxMultiplier:F1}x)",
                ForeColor = TextColor
            };

            // Calculate average value per Rayan (considering all multipliers)
            BigInteger avgValueBigInt = (BigInteger)avgValue;
            
            var lblValue = new Label
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(420, 10),
                Size = new Size(300, 20),
                Text = $"� Wert: {FormatBigInt(avgValueBigInt)}/s (Basis: {FormatBigInt(baseValue)})",
                ForeColor = BrightGreen
            };

            var btnMerge = new Button
            {
                Location = new Point(420, 40),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 9F),
                Text = canMerge ? $"5x mergen ? 1x Merged (5x st�rker)" : $"Ben�tigt 5 St�ck",
                Enabled = canMerge,
                Tag = new { Prefix = prefix, Suffix = suffix, Rarity = rarity, BaseValue = baseValue },
                BackColor = canMerge ? BrightBlue : DarkAccent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnMerge.FlatAppearance.BorderSize = 0;
            btnMerge.Click += BtnMerge_Click;

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblRarity);
            panel.Controls.Add(lblCount);
            panel.Controls.Add(lblValue);
            panel.Controls.Add(btnMerge);

            return panel;
        }

        private Color GetRarityColor(double rarity)
        {
            if (rarity >= 1000000) return Color.FromArgb(127, 107, 0); // Dark Gold (50% dunkler)
            if (rarity >= 100000) return Color.FromArgb(100, 50, 127); // Dark Purple (50% dunkler)
            if (rarity >= 10000) return Color.FromArgb(0, 87, 127); // Dark Blue (50% dunkler)
            if (rarity >= 1000) return Color.FromArgb(0, 127, 63); // Dark Green (50% dunkler)
            return DarkAccent;
        }

        private string FormatBigInt(BigInteger value)
        {
            if (value < 1000) return value.ToString();
            if (value < 1000000) return ((double)value / 1000).ToString("F2") + "K";
            if (value < 1000000000) return ((double)value / 1000000).ToString("F2") + "M";
            if (value < 1000000000000) return ((double)value / 1000000000).ToString("F2") + "B";
            return value.ToString("E2");
        }

        private void BtnMerge_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                dynamic tag = btn.Tag;
                string prefix = tag.Prefix;
                string suffix = tag.Suffix;
                double rarity = tag.Rarity;
                BigInteger baseValue = tag.BaseValue;

                MergeRayans(prefix, suffix, rarity, baseValue);
            }
        }

        private void MergeRayans(string prefix, string suffix, double rarity, BigInteger baseValue)
        {
            // Take ANY 5 Rayans of this type
            var matchingRayans = _inventory
                .Where(r => r.Prefix == prefix && r.Suffix == suffix && r.Rarity == rarity
                    && r.BaseValue == baseValue)
                .Take(5)
                .ToList();

            if (matchingRayans.Count < 5)
            {
                MessageBox.Show("Nicht gen�gend Rayans zum Mergen!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Calculate average multiplier of the 5 Rayans being merged
            double avgMultiplier = matchingRayans.Average(r => r.Multiplier);
            
            // Remove 5 Rayans
            foreach (var rayan in matchingRayans)
            {
                _inventory.Remove(rayan);
            }

            // Create merged Rayan with 5x the average multiplier
            string newSuffix = string.IsNullOrEmpty(suffix) ? "Merged" : $"{suffix} Merged";
            
            var mergedRayan = new Rayan
            {
                Prefix = prefix,
                Suffix = newSuffix,
                Rarity = rarity,
                BaseValue = baseValue,
                Multiplier = avgMultiplier * 5 // 5x stronger than average
            };

            _inventory.Add(mergedRayan);

            // Reload inventory (MessageBox removed to prevent additional handle creation)
            LoadInventory();
            _onInventoryChanged?.Invoke();
        }
        
        private void btnMergeAll_Click(object? sender, EventArgs e)
        {
            // Find all groups that can be merged (5+ Rayans)
            var mergeCandidates = _inventory
                .GroupBy(r => new { r.Prefix, r.Suffix, r.Rarity, r.BaseValue })
                .Where(g => g.Count() >= 5)
                .OrderByDescending(g => g.Count()) // Merge groups with most Rayans first
                .ToList();
            
            if (mergeCandidates.Count == 0)
            {
                MessageBox.Show("Keine Rayans zum Mergen gefunden!\n\nDu brauchst mindestens 5 gleiche Rayans pro Typ.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var result = MessageBox.Show(
                $"M�chtest du {mergeCandidates.Count} Gruppen mergen?\n\n" +
                $"Das erstellt {mergeCandidates.Count} neue Merged Rayans.\n\n" +
                "Diese Aktion kann nicht r�ckg�ngig gemacht werden!",
                "MERGE ALL",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            if (result != DialogResult.Yes)
                return;
            
            int totalMerged = 0;
            int totalRayansUsed = 0;
            
            foreach (var group in mergeCandidates)
            {
                string prefix = group.Key.Prefix;
                string suffix = group.Key.Suffix;
                double rarity = group.Key.Rarity;
                BigInteger baseValue = group.Key.BaseValue;
                
                // Merge as many times as possible (every 5 Rayans)
                int mergeCount = group.Count() / 5;
                
                for (int i = 0; i < mergeCount; i++)
                {
                    // Take 5 Rayans
                    var rayansToMerge = _inventory
                        .Where(r => r.Prefix == prefix && r.Suffix == suffix && 
                                   r.Rarity == rarity && r.BaseValue == baseValue)
                        .Take(5)
                        .ToList();
                    
                    if (rayansToMerge.Count < 5)
                        break;
                    
                    // Calculate average multiplier
                    double avgMultiplier = rayansToMerge.Average(r => r.Multiplier);
                    
                    // Remove 5 Rayans
                    foreach (var rayan in rayansToMerge)
                    {
                        _inventory.Remove(rayan);
                        totalRayansUsed++;
                    }
                    
                    // Create merged Rayan
                    string newSuffix = string.IsNullOrEmpty(suffix) ? "Merged" : $"{suffix} Merged";
                    var mergedRayan = new Rayan
                    {
                        Prefix = prefix,
                        Suffix = newSuffix,
                        Rarity = rarity,
                        BaseValue = baseValue,
                        Multiplier = avgMultiplier * 5
                    };
                    
                    _inventory.Add(mergedRayan);
                    totalMerged++;
                }
            }
            
            MessageBox.Show(
                $"? MERGE ALL abgeschlossen!\n\n" +
                $"Merged: {totalMerged} neue Rayans erstellt\n" +
                $"Verwendet: {totalRayansUsed} Rayans (je 5 pro Merge)\n" +
                $"�brig: {_inventory.Count} Rayans im Inventar",
                "Erfolg",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            
            LoadInventory();
            _onInventoryChanged?.Invoke();
        }
    }
}
