using SpinARayan.Models;
using SpinARayan.Services;

namespace SpinARayan
{
    public partial class QuestForm : Form
    {
        private readonly GameManager _gameManager;
        private readonly Action _onQuestClaimed;

        // Dark Mode Colors
        private readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
        private readonly Color DarkPanel = Color.FromArgb(45, 45, 48);
        private readonly Color DarkAccent = Color.FromArgb(60, 60, 65);
        private readonly Color BrightGreen = Color.FromArgb(0, 255, 127);
        private readonly Color BrightBlue = Color.FromArgb(0, 174, 255);
        private readonly Color BrightGold = Color.FromArgb(255, 215, 0);
        private readonly Color TextColor = Color.FromArgb(230, 230, 230);

        public QuestForm(GameManager gameManager, Action onQuestClaimed)
        {
            _gameManager = gameManager;
            _onQuestClaimed = onQuestClaimed;
            InitializeComponent();
            ApplyDarkMode();
            LoadQuests();
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            panelQuests.BackColor = DarkBackground;
            lblTitle.ForeColor = BrightGold;
        }

        private void LoadQuests()
        {
            // Clear existing quest panels
            panelQuests.Controls.Clear();

            var questService = _gameManager.GetQuestService();
            int yPosition = 10;

            foreach (var quest in questService.Quests)
            {
                var questPanel = CreateQuestPanel(quest);
                questPanel.Location = new Point(10, yPosition);
                panelQuests.Controls.Add(questPanel);
                yPosition += questPanel.Height + 10;
            }

            lblTotalGems.Text = $"?? Verfügbare Gems: {_gameManager.Stats.Gems}";
            lblTotalGems.ForeColor = BrightBlue;
        }

        private Panel CreateQuestPanel(Quest quest)
        {
            Color panelColor;
            if (quest.IsClaimed)
                panelColor = DarkAccent;
            else if (quest.IsCompleted)
                panelColor = Color.FromArgb(0, 80, 0);
            else
                panelColor = DarkPanel;

            var panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(540, 100),
                BackColor = panelColor
            };

            // Zeige die Anzahl der Wiederholungen und aktuelles Goal bei wiederholbaren Quests
            string description = quest.Description;
            if (quest.IsRepeatable && quest.TimesCompleted > 0)
            {
                description = $"{quest.Description.Replace($"{quest.InitialGoal:N0}", $"{quest.Goal:N0}")} (x{quest.TimesCompleted + 1})";
            }
            else if (quest.IsRepeatable)
            {
                description = $"{quest.Description} (Wiederholbar)";
            }

            var lblDescription = new Label
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(520, 25),
                Text = description,
                ForeColor = TextColor
            };

            var lblProgress = new Label
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(10, 40),
                Size = new Size(520, 20),
                Text = $"Fortschritt: {quest.CurrentProgress} / {quest.Goal} ({quest.ProgressPercentage:F1}%)",
                ForeColor = TextColor
            };

            var lblReward = new Label
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(10, 65),
                Size = new Size(300, 20),
                Text = $"Belohnung: {quest.RewardGems} Gems" + (quest.IsRepeatable ? " (wiederholbar)" : ""),
                ForeColor = BrightBlue
            };

            var btnClaim = new Button
            {
                Location = new Point(380, 60),
                Size = new Size(150, 30),
                Font = new Font("Segoe UI", 9F),
                Text = quest.IsClaimed ? "Beansprucht ?" : quest.IsCompleted ? "Belohnung holen!" : "Nicht verfügbar",
                Enabled = quest.IsCompleted && !quest.IsClaimed,
                Tag = quest,
                BackColor = (quest.IsCompleted && !quest.IsClaimed) ? BrightGreen : DarkAccent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClaim.FlatAppearance.BorderSize = 0;
            btnClaim.Click += BtnClaim_Click;

            panel.Controls.Add(lblDescription);
            panel.Controls.Add(lblProgress);
            panel.Controls.Add(lblReward);
            panel.Controls.Add(btnClaim);

            return panel;
        }

        private void BtnClaim_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Quest quest)
            {
                var questService = _gameManager.GetQuestService();
                string message = $"Quest abgeschlossen! {quest.RewardGems} Gems erhalten!";
                
                if (quest.IsRepeatable)
                {
                    message += "\n\nDiese Quest kann erneut abgeschlossen werden!";
                }

                questService.ClaimReward(quest, _gameManager.Stats);
                _gameManager.Save();
                LoadQuests();
                _onQuestClaimed?.Invoke();
                MessageBox.Show(message, "Quest Belohnung", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadQuests();
        }
    }
}
