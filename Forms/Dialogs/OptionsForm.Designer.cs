using System.Drawing;
using System.Windows.Forms;

namespace SpinARayan
{
    partial class OptionsForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Panel panelStats;
        private Panel panelActions;
        
        // Stats Labels
        private Label lblTotalRolls;
        private Label lblPlayTime;
        private Label lblRebirths;
        private Label lblInventoryCount;
        private Label lblUniqueRayans;
        private Label lblBestRayan;
        private Label lblBestRarity;
        private Label lblBestValue;
        private Label lblPlotSlots;
        private Label lblEquipped;
        private Label lblTotalIncome;
        private Label lblMoneyMultiplier;
        private Label lblLuckMultiplier;
        private Label lblLuckBoosterLevel;
        private Label lblDicesOwned;
        private Label lblCurrentDice;
        private Label lblMoney;
        private Label lblGems;
        
        private Button btnResetGame;
        private Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.panelStats = new Panel();
            this.panelActions = new Panel();
            
            this.lblTotalRolls = new Label();
            this.lblPlayTime = new Label();
            this.lblRebirths = new Label();
            this.lblInventoryCount = new Label();
            this.lblUniqueRayans = new Label();
            this.lblBestRayan = new Label();
            this.lblBestRarity = new Label();
            this.lblBestValue = new Label();
            this.lblPlotSlots = new Label();
            this.lblEquipped = new Label();
            this.lblTotalIncome = new Label();
            this.lblMoneyMultiplier = new Label();
            this.lblLuckMultiplier = new Label();
            this.lblLuckBoosterLevel = new Label();
            this.lblDicesOwned = new Label();
            this.lblCurrentDice = new Label();
            this.lblMoney = new Label();
            this.lblGems = new Label();
            
            this.btnResetGame = new Button();
            this.btnClose = new Button();
            
            this.panelStats.SuspendLayout();
            this.panelActions.SuspendLayout();
            this.SuspendLayout();
            
            // lblTitle
            this.lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTitle.Location = new Point(12, 12);
            this.lblTitle.Size = new Size(720, 40);
            this.lblTitle.Text = "?? OPTIONS & STATISTICS";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            
            // panelStats
            this.panelStats.BorderStyle = BorderStyle.FixedSingle;
            this.panelStats.Location = new Point(12, 60);
            this.panelStats.Size = new Size(720, 550);
            this.panelStats.AutoScroll = true;
            
            // Stats Layout (3 columns)
            int col1X = 20;
            int col2X = 240;
            int col3X = 460;
            int yPos = 20;
            int lineHeight = 30;
            
            // Column 1: Basic Stats
            AddStatLabel(lblTotalRolls, col1X, yPos, "Total Rolls: 0");
            AddStatLabel(lblPlayTime, col1X, yPos += lineHeight, "Spielzeit: 0");
            AddStatLabel(lblRebirths, col1X, yPos += lineHeight, "Rebirths: 0");
            AddStatLabel(lblMoney, col1X, yPos += lineHeight, "Money: 0");
            AddStatLabel(lblGems, col1X, yPos += lineHeight, "Gems: 0");
            
            // Add section header
            yPos += lineHeight + 10;
            var lblInventoryHeader = new Label
            {
                Location = new Point(col1X, yPos),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "?? Inventar"
            };
            panelStats.Controls.Add(lblInventoryHeader);
            yPos += lineHeight;
            
            AddStatLabel(lblInventoryCount, col1X, yPos, "Rayans: 0");
            AddStatLabel(lblUniqueRayans, col1X, yPos += lineHeight, "Typen: 0");
            
            // Column 2: Best Rayan
            yPos = 20;
            var lblBestHeader = new Label
            {
                Location = new Point(col2X, yPos),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "?? Bester Rayan"
            };
            panelStats.Controls.Add(lblBestHeader);
            yPos += lineHeight;
            
            AddStatLabel(lblBestRayan, col2X, yPos, "Name: -");
            AddStatLabel(lblBestRarity, col2X, yPos += lineHeight, "Rarity: -");
            AddStatLabel(lblBestValue, col2X, yPos += lineHeight, "Wert: -");
            
            yPos += lineHeight + 10;
            var lblPlotHeader = new Label
            {
                Location = new Point(col2X, yPos),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "?? Plots"
            };
            panelStats.Controls.Add(lblPlotHeader);
            yPos += lineHeight;
            
            AddStatLabel(lblPlotSlots, col2X, yPos, "Slots: 3/10");
            AddStatLabel(lblEquipped, col2X, yPos += lineHeight, "Ausgerüstet: 0");
            AddStatLabel(lblTotalIncome, col2X, yPos += lineHeight, "Income: 0/s");
            AddStatLabel(lblMoneyMultiplier, col2X, yPos += lineHeight, "Multiplier: 1.0x");
            
            // Column 3: Luck & Dices
            yPos = 20;
            var lblLuckHeader = new Label
            {
                Location = new Point(col3X, yPos),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "?? Glück"
            };
            panelStats.Controls.Add(lblLuckHeader);
            yPos += lineHeight;
            
            AddStatLabel(lblLuckMultiplier, col3X, yPos, "Luck: 1.0x");
            AddStatLabel(lblLuckBoosterLevel, col3X, yPos += lineHeight, "Booster: 0");
            
            yPos += lineHeight + 10;
            var lblDiceHeader = new Label
            {
                Location = new Point(col3X, yPos),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "?? Dices"
            };
            panelStats.Controls.Add(lblDiceHeader);
            yPos += lineHeight;
            
            AddStatLabel(lblDicesOwned, col3X, yPos, "Besessen: 1");
            AddStatLabel(lblCurrentDice, col3X, yPos += lineHeight, "Aktuell: Basic");
            
            // panelActions
            this.panelActions.BorderStyle = BorderStyle.FixedSingle;
            this.panelActions.Location = new Point(12, 620);
            this.panelActions.Size = new Size(720, 80);
            
            // btnResetGame
            this.btnResetGame.Location = new Point(20, 20);
            this.btnResetGame.Size = new Size(300, 40);
            this.btnResetGame.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnResetGame.Text = "?? SPIELSTAND ZURÜCKSETZEN";
            this.btnResetGame.BackColor = Color.FromArgb(255, 69, 58);
            this.btnResetGame.ForeColor = Color.White;
            this.btnResetGame.FlatStyle = FlatStyle.Flat;
            this.btnResetGame.FlatAppearance.BorderSize = 0;
            this.btnResetGame.Click += btnResetGame_Click;
            this.panelActions.Controls.Add(this.btnResetGame);
            
            // btnClose
            this.btnClose.Location = new Point(340, 20);
            this.btnClose.Size = new Size(300, 40);
            this.btnClose.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnClose.Text = "SCHLIESSEN";
            this.btnClose.BackColor = Color.FromArgb(60, 60, 65);
            this.btnClose.ForeColor = Color.FromArgb(230, 230, 230);
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.FlatAppearance.BorderSize = 1;
            this.btnClose.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 85);
            this.btnClose.Click += btnClose_Click;
            this.panelActions.Controls.Add(this.btnClose);
            
            // OptionsForm
            this.ClientSize = new Size(744, 720);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.panelStats);
            this.Controls.Add(this.panelActions);
            this.Text = "Options & Statistics";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            this.panelStats.ResumeLayout(false);
            this.panelActions.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        
        private void AddStatLabel(Label label, int x, int y, string text)
        {
            label.Location = new Point(x, y);
            label.Size = new Size(200, 25);
            label.Font = new Font("Segoe UI", 10F);
            label.Text = text;
            panelStats.Controls.Add(label);
        }
    }
}
