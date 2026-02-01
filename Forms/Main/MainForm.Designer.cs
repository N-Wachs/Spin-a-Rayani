namespace SpinARayan
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel panelLeft;
        private Panel panelCenter;
        private Panel panelRight;
        private Button btnRoll;
        private Button btnAutoEquip;
        private Button btnRebirth;
        private Button btnInventory;
        private Button btnUpgrades;
        private Button btnQuests;
        private Button btnShop;
        private Button btnOptions;
        private Label lblMoney;
        private Label lblGems;
        private Label lblRebirths;
        private Label lblLastRoll;
        private Label lblAutoRollStatus;
        private Label lblLuck;
        private Label lblRebirthBonus;

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
            this.panelLeft = new Panel();
            this.panelCenter = new Panel();
            this.panelRight = new Panel();
            this.btnRoll = new Button();
            this.btnAutoEquip = new Button();
            this.btnRebirth = new Button();
            this.btnInventory = new Button();
            this.btnUpgrades = new Button();
            this.btnQuests = new Button();
            this.btnShop = new Button();
            this.btnOptions = new Button();
            this.lblMoney = new Label();
            this.lblGems = new Label();
            this.lblRebirths = new Label();
            this.lblLastRoll = new Label();
            this.lblAutoRollStatus = new Label();
            this.lblLuck = new Label();
            this.lblRebirthBonus = new Label();
            this.panelCenter.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // panelLeft
            // 
            this.panelLeft.Location = new Point(0, 0);
            this.panelLeft.Size = new Size(300, 700);
            this.panelLeft.BackColor = Color.FromArgb(38, 38, 38); // BackgroundPanel
            
            // 
            // panelCenter
            // 
            this.panelCenter.Location = new Point(300, 0);
            this.panelCenter.Size = new Size(600, 700);
            this.panelCenter.BackColor = Color.FromArgb(30, 30, 30); // BackgroundElevated
            
            // 
            // panelRight
            // 
            this.panelRight.Location = new Point(900, 0);
            this.panelRight.Size = new Size(300, 700);
            this.panelRight.BackColor = Color.FromArgb(38, 38, 38); // BackgroundPanel
            
            // 
            // btnRoll
            // 
            this.btnRoll.Location = new Point(150, 380); // Moved down from 270 to 380 for spacing
            this.btnRoll.Size = new Size(300, 120);
            this.btnRoll.BackColor = Color.FromArgb(33, 150, 243); // AccentBlue
            this.btnRoll.ForeColor = Color.White;
            this.btnRoll.Font = new Font("Segoe UI Emoji", 24F, FontStyle.Bold);
            this.btnRoll.Text = "🎲 ROLL\nDice 1";
            this.btnRoll.TextAlign = ContentAlignment.MiddleCenter;
            this.btnRoll.FlatStyle = FlatStyle.Flat;
            this.btnRoll.FlatAppearance.BorderSize = 2;
            this.btnRoll.FlatAppearance.BorderColor = Color.FromArgb(66, 165, 245); // AccentBlueLight
            this.btnRoll.Cursor = Cursors.Hand;
            this.btnRoll.Click += new EventHandler(this.btnRoll_Click);
            this.panelCenter.Controls.Add(this.btnRoll);
            
            // 
            // btnAutoEquip
            // 
            this.btnAutoEquip.Location = new Point(150, 360);
            this.btnAutoEquip.Size = new Size(145, 40);
            this.btnAutoEquip.BackColor = Color.FromArgb(48, 63, 159); // PrimaryMedium
            this.btnAutoEquip.ForeColor = Color.White;
            this.btnAutoEquip.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAutoEquip.Text = "Auto Equip";
            this.btnAutoEquip.FlatStyle = FlatStyle.Flat;
            this.btnAutoEquip.FlatAppearance.BorderSize = 1;
            this.btnAutoEquip.FlatAppearance.BorderColor = Color.FromArgb(92, 107, 192); // PrimaryLight
            this.btnAutoEquip.Click += new EventHandler(this.btnAutoEquip_Click);
            this.panelCenter.Controls.Add(this.btnAutoEquip);
            
            // 
            // btnRebirth
            // 
            this.btnRebirth.Location = new Point(305, 360);
            this.btnRebirth.Size = new Size(145, 40);
            this.btnRebirth.BackColor = Color.FromArgb(156, 39, 176); // Rebirth Purple
            this.btnRebirth.ForeColor = Color.White;
            this.btnRebirth.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnRebirth.Text = "Rebirth";
            this.btnRebirth.FlatStyle = FlatStyle.Flat;
            this.btnRebirth.FlatAppearance.BorderSize = 1;
            this.btnRebirth.FlatAppearance.BorderColor = Color.FromArgb(186, 104, 200); // Lighter purple
            this.btnRebirth.Click += new EventHandler(this.btnRebirth_Click);
            this.panelCenter.Controls.Add(this.btnRebirth);
            
            // 
            // lblMoney
            // 
            this.lblMoney.Location = new Point(20, 620);
            this.lblMoney.Size = new Size(260, 30);
            this.lblMoney.Font = new Font("Segoe UI Emoji", 16F, FontStyle.Bold);
            this.lblMoney.ForeColor = Color.FromArgb(76, 175, 80); // Success Green
            this.lblMoney.Text = "💰 0";
            this.panelCenter.Controls.Add(this.lblMoney);
            
            // 
            // lblGems
            // 
            this.lblGems.Location = new Point(20, 655);
            this.lblGems.Size = new Size(260, 30);
            this.lblGems.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold);
            this.lblGems.ForeColor = Color.FromArgb(33, 150, 243); // AccentBlue
            this.lblGems.Text = "💎 0";
            this.panelCenter.Controls.Add(this.lblGems);
            
            // 
            // lblLuck
            // 
            this.lblLuck.Location = new Point(320, 620);
            this.lblLuck.Size = new Size(260, 30);
            this.lblLuck.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold);
            this.lblLuck.ForeColor = Color.FromArgb(255, 193, 7); // Warning Amber
            this.lblLuck.Text = "🍀 Luck: +0%";
            this.lblLuck.TextAlign = ContentAlignment.MiddleRight;
            this.panelCenter.Controls.Add(this.lblLuck);
            
            // 
            // lblRebirthBonus
            // 
            this.lblRebirthBonus.Location = new Point(320, 655);
            this.lblRebirthBonus.Size = new Size(260, 30);
            this.lblRebirthBonus.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold);
            this.lblRebirthBonus.ForeColor = Color.FromArgb(156, 39, 176); // Rebirth Purple
            this.lblRebirthBonus.Text = "🔄 Rebirth: +0%";
            this.lblRebirthBonus.TextAlign = ContentAlignment.MiddleRight;
            this.panelCenter.Controls.Add(this.lblRebirthBonus);
            
            // 
            // lblLastRoll
            // 
            this.lblLastRoll.Location = new Point(50, 50);
            this.lblLastRoll.Size = new Size(500, 80);
            this.lblLastRoll.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold);
            this.lblLastRoll.ForeColor = Color.FromArgb(255, 193, 7); // Warning Amber
            this.lblLastRoll.TextAlign = ContentAlignment.MiddleCenter;
            this.lblLastRoll.Text = "🎲 Waiting for roll...";
            this.panelCenter.Controls.Add(this.lblLastRoll);
            
            // 
            // lblAutoRollStatus
            // 
            this.lblAutoRollStatus.Location = new Point(350, 655);
            this.lblAutoRollStatus.Size = new Size(200, 30);
            this.lblAutoRollStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblAutoRollStatus.ForeColor = Color.FromArgb(76, 175, 80); // Success Green
            this.lblAutoRollStatus.TextAlign = ContentAlignment.MiddleRight;
            this.lblAutoRollStatus.Text = "";
            this.panelCenter.Controls.Add(this.lblAutoRollStatus);
            
            // 
            // lblRebirths
            // 
            this.lblRebirths.Location = new Point(10, 670);
            this.lblRebirths.Size = new Size(280, 25);
            this.lblRebirths.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblRebirths.ForeColor = Color.FromArgb(230, 230, 230);
            this.lblRebirths.Text = "Rebirths: 0";
            this.lblRebirths.TextAlign = ContentAlignment.TopCenter;
            this.panelRight.Controls.Add(this.lblRebirths);
            
            // 
            // btnInventory
            // 
            this.btnInventory.Location = new Point(20, 20);
            this.btnInventory.Size = new Size(260, 60);
            this.btnInventory.BackColor = Color.FromArgb(48, 63, 159); // PrimaryMedium
            this.btnInventory.ForeColor = Color.White;
            this.btnInventory.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold);
            this.btnInventory.Text = "📦 INVENTORY";
            this.btnInventory.FlatStyle = FlatStyle.Flat;
            this.btnInventory.FlatAppearance.BorderSize = 1;
            this.btnInventory.FlatAppearance.BorderColor = Color.FromArgb(92, 107, 192); // PrimaryLight
            this.btnInventory.Click += new EventHandler(this.btnInventory_Click);
            this.panelRight.Controls.Add(this.btnInventory);
            
            // 
            // btnShop
            // 
            this.btnShop.Location = new Point(20, 90);
            this.btnShop.Size = new Size(260, 60);
            this.btnShop.BackColor = Color.FromArgb(48, 63, 159); // PrimaryMedium
            this.btnShop.ForeColor = Color.White;
            this.btnShop.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold);
            this.btnShop.Text = "🎲 DICE SHOP";
            this.btnShop.FlatStyle = FlatStyle.Flat;
            this.btnShop.FlatAppearance.BorderSize = 1;
            this.btnShop.FlatAppearance.BorderColor = Color.FromArgb(92, 107, 192); // PrimaryLight
            this.btnShop.Click += new EventHandler(this.btnShop_Click);
            this.panelRight.Controls.Add(this.btnShop);
            
            // 
            // btnUpgrades
            // 
            this.btnUpgrades.Location = new Point(20, 160);
            this.btnUpgrades.Size = new Size(260, 60);
            this.btnUpgrades.BackColor = Color.FromArgb(48, 63, 159); // PrimaryMedium
            this.btnUpgrades.ForeColor = Color.White;
            this.btnUpgrades.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold);
            this.btnUpgrades.Text = "⬆️ UPGRADES";
            this.btnUpgrades.FlatStyle = FlatStyle.Flat;
            this.btnUpgrades.FlatAppearance.BorderSize = 1;
            this.btnUpgrades.FlatAppearance.BorderColor = Color.FromArgb(92, 107, 192); // PrimaryLight
            this.btnUpgrades.Click += new EventHandler(this.btnUpgrades_Click);
            this.panelRight.Controls.Add(this.btnUpgrades);
            
            // 
            // btnQuests
            // 
            this.btnQuests.Location = new Point(20, 230);
            this.btnQuests.Size = new Size(260, 60);
            this.btnQuests.BackColor = Color.FromArgb(48, 63, 159); // PrimaryMedium
            this.btnQuests.ForeColor = Color.White;
            this.btnQuests.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold);
            this.btnQuests.Text = "📋 QUESTS";
            this.btnQuests.FlatStyle = FlatStyle.Flat;
            this.btnQuests.FlatAppearance.BorderSize = 1;
            this.btnQuests.FlatAppearance.BorderColor = Color.FromArgb(92, 107, 192); // PrimaryLight
            this.btnQuests.Click += new EventHandler(this.btnQuests_Click);
            this.panelRight.Controls.Add(this.btnQuests);
            
            // 
            // btnOptions
            // 
            this.btnOptions.Location = new Point(20, 300);
            this.btnOptions.Size = new Size(260, 60);
            this.btnOptions.BackColor = Color.FromArgb(48, 63, 159); // PrimaryMedium
            this.btnOptions.ForeColor = Color.White;
            this.btnOptions.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold);
            this.btnOptions.Text = "⚙️ OPTIONS";
            this.btnOptions.FlatStyle = FlatStyle.Flat;
            this.btnOptions.FlatAppearance.BorderSize = 1;
            this.btnOptions.FlatAppearance.BorderColor = Color.FromArgb(92, 107, 192); // PrimaryLight
            this.btnOptions.Click += new EventHandler(this.btnOptions_Click);
            this.panelRight.Controls.Add(this.btnOptions);
            
            // 
            // MainForm
            // 
            this.ClientSize = new Size(1200, 700);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.panelCenter);
            this.Controls.Add(this.panelRight);
            this.Text = "Spin a Rayan - Modern Edition";
            this.BackColor = Color.FromArgb(18, 18, 18); // BackgroundDark
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.panelCenter.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

