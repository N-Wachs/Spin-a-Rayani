using System.Drawing;
using System.Windows.Forms;

namespace SpinARayan
{
    partial class UpgradeForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Label lblGems;
        private Label lblMoney;
        private TabControl tabControl;
        private TabPage tabGemsUpgrades;
        private TabPage tabMoneyUpgrades;
        private Panel panelAutoRoll;
        private Panel panelRollCooldown;
        private Panel panelLuckBooster;
        private Label lblAutoRollTitle;
        private Button btnAutoRollUnlock;
        private Button btnAutoRollToggle;
        private Label lblLuckBooster;
        private Label lblLuckBoosterTitle;
        private Button btnBuyLuckBooster;
        private Panel panelSkipNextRebirth;
        private Label lblSkipNextRebirthTitle;
        private Label lblSkipNextRebirth;
        private Button btnBuySkipNextRebirth;
        private Label lblRollCooldown;
        private Label lblRollCooldownTitle;
        private Button btnReduceCooldown;
        private Button btnRefresh;

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
            this.lblGems = new Label();
            this.lblMoney = new Label();
            this.tabControl = new TabControl();
            this.tabGemsUpgrades = new TabPage();
            this.tabMoneyUpgrades = new TabPage();
            this.panelAutoRoll = new Panel();
            this.lblAutoRollTitle = new Label();
            this.btnAutoRollUnlock = new Button();
            this.btnAutoRollToggle = new Button();
            this.panelRollCooldown = new Panel();
            this.lblRollCooldownTitle = new Label();
            this.lblRollCooldown = new Label();
            this.btnReduceCooldown = new Button();
            this.panelLuckBooster = new Panel();
            this.lblLuckBoosterTitle = new Label();
            this.lblLuckBooster = new Label();
            this.btnBuyLuckBooster = new Button();
            this.panelSkipNextRebirth = new Panel();
            this.lblSkipNextRebirthTitle = new Label();
            this.lblSkipNextRebirth = new Label();
            this.btnBuySkipNextRebirth = new Button();
            this.btnRefresh = new Button();
            this.tabControl.SuspendLayout();
            this.tabGemsUpgrades.SuspendLayout();
            this.tabMoneyUpgrades.SuspendLayout();
            this.panelAutoRoll.SuspendLayout();
            this.panelRollCooldown.SuspendLayout();
            this.panelLuckBooster.SuspendLayout();
            this.panelSkipNextRebirth.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new Font("Segoe UI Emoji", 16F, FontStyle.Bold);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Size = new Size(660, 40);
            this.lblTitle.Text = "Upgrades";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // lblGems
            // 
            this.lblGems.Font = new Font("Segoe UI Emoji", 12F, FontStyle.Bold);
            this.lblGems.ForeColor = Color.Blue;
            this.lblGems.Location = new Point(12, 49);
            this.lblGems.Size = new Size(300, 30);
            this.lblGems.Text = "Verfuegbare Gems: 0";
            
            // 
            // lblMoney
            // 
            this.lblMoney.Font = new Font("Segoe UI Emoji", 12F, FontStyle.Bold);
            this.lblMoney.ForeColor = Color.Green;
            this.lblMoney.Location = new Point(372, 49);
            this.lblMoney.Size = new Size(300, 30);
            this.lblMoney.Text = "Verfuegbares Geld: 0";
            
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabGemsUpgrades);
            this.tabControl.Controls.Add(this.tabMoneyUpgrades);
            this.tabControl.Location = new Point(12, 90);
            this.tabControl.Size = new Size(660, 420);
            this.tabControl.Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular);
            
            // 
            // tabGemsUpgrades
            // 
            this.tabGemsUpgrades.Controls.Add(this.panelAutoRoll);
            this.tabGemsUpgrades.Controls.Add(this.panelRollCooldown);
            this.tabGemsUpgrades.Text = "💎 Gems Upgrades";
            this.tabGemsUpgrades.BackColor = Color.FromArgb(230, 240, 255);
            this.tabGemsUpgrades.Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular);
            
            // 
            // tabMoneyUpgrades
            // 
            this.tabMoneyUpgrades.Controls.Add(this.panelLuckBooster);
            this.tabMoneyUpgrades.Controls.Add(this.panelSkipNextRebirth);
            this.tabMoneyUpgrades.Text = "💰 Geld Upgrades";
            this.tabMoneyUpgrades.BackColor = Color.FromArgb(230, 255, 230);
            this.tabMoneyUpgrades.Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular);
            
            // 
            // panelAutoRoll
            // 
            this.panelAutoRoll.BorderStyle = BorderStyle.FixedSingle;
            this.panelAutoRoll.Controls.Add(this.lblAutoRollTitle);
            this.panelAutoRoll.Controls.Add(this.btnAutoRollUnlock);
            this.panelAutoRoll.Controls.Add(this.btnAutoRollToggle);
            this.panelAutoRoll.Location = new Point(10, 10);
            this.panelAutoRoll.Size = new Size(630, 120);
            this.panelAutoRoll.BackColor = Color.White;
            
            // 
            // lblAutoRollTitle
            // 
            this.lblAutoRollTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblAutoRollTitle.Location = new Point(10, 10);
            this.lblAutoRollTitle.Size = new Size(610, 30);
            this.lblAutoRollTitle.Text = "Auto Roll System";
            
            // 
            // btnAutoRollUnlock
            // 
            this.btnAutoRollUnlock.Font = new Font("Segoe UI", 10F);
            this.btnAutoRollUnlock.Location = new Point(10, 50);
            this.btnAutoRollUnlock.Size = new Size(610, 25);
            this.btnAutoRollUnlock.Text = "AutoRoll freischalten (100 Gems)";
            this.btnAutoRollUnlock.Click += new System.EventHandler(this.btnAutoRollUnlock_Click);
            
            // 
            // btnAutoRollToggle
            // 
            this.btnAutoRollToggle.Font = new Font("Segoe UI", 10F);
            this.btnAutoRollToggle.Location = new Point(10, 80);
            this.btnAutoRollToggle.Size = new Size(610, 25);
            this.btnAutoRollToggle.Text = "AutoRoll: AUS";
            this.btnAutoRollToggle.Click += new System.EventHandler(this.btnAutoRollToggle_Click);
            
            // 
            // panelRollCooldown
            // 
            this.panelRollCooldown.BorderStyle = BorderStyle.FixedSingle;
            this.panelRollCooldown.Controls.Add(this.lblRollCooldownTitle);
            this.panelRollCooldown.Controls.Add(this.lblRollCooldown);
            this.panelRollCooldown.Controls.Add(this.btnReduceCooldown);
            this.panelRollCooldown.Location = new Point(10, 140);
            this.panelRollCooldown.Size = new Size(630, 120);
            this.panelRollCooldown.BackColor = Color.White;
            
            // 
            // lblRollCooldownTitle
            // 
            this.lblRollCooldownTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblRollCooldownTitle.Location = new Point(10, 10);
            this.lblRollCooldownTitle.Size = new Size(610, 30);
            this.lblRollCooldownTitle.Text = "Roll Cooldown";
            
            // 
            // lblRollCooldown
            // 
            this.lblRollCooldown.Font = new Font("Segoe UI", 10F);
            this.lblRollCooldown.Location = new Point(10, 50);
            this.lblRollCooldown.Size = new Size(610, 25);
            this.lblRollCooldown.Text = "Roll Cooldown: 2.0s (Level 0)";
            
            // 
            // btnReduceCooldown
            // 
            this.btnReduceCooldown.Font = new Font("Segoe UI", 10F);
            this.btnReduceCooldown.Location = new Point(10, 80);
            this.btnReduceCooldown.Size = new Size(610, 25);
            this.btnReduceCooldown.Text = "Cooldown reduzieren";
            this.btnReduceCooldown.Click += new System.EventHandler(this.btnReduceCooldown_Click);
            
            // 
            // panelLuckBooster
            // 
            this.panelLuckBooster.BorderStyle = BorderStyle.FixedSingle;
            this.panelLuckBooster.Controls.Add(this.lblLuckBoosterTitle);
            this.panelLuckBooster.Controls.Add(this.lblLuckBooster);
            this.panelLuckBooster.Controls.Add(this.btnBuyLuckBooster);
            this.panelLuckBooster.Location = new Point(10, 10);
            this.panelLuckBooster.Size = new Size(630, 120);
            this.panelLuckBooster.BackColor = Color.White;
            
            // 
            // lblLuckBoosterTitle
            // 
            this.lblLuckBoosterTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblLuckBoosterTitle.Location = new Point(10, 10);
            this.lblLuckBoosterTitle.Size = new Size(610, 30);
            this.lblLuckBoosterTitle.Text = "Luck Booster (+25% pro Level)";
            
            // 
            // lblLuckBooster
            // 
            this.lblLuckBooster.Font = new Font("Segoe UI", 10F);
            this.lblLuckBooster.Location = new Point(10, 50);
            this.lblLuckBooster.Size = new Size(610, 25);
            this.lblLuckBooster.Text = "Luck Multiplier: 1.00x (Level 0)";
            
            // 
            // btnBuyLuckBooster
            // 
            this.btnBuyLuckBooster.Font = new Font("Segoe UI", 10F);
            this.btnBuyLuckBooster.Location = new Point(10, 80);
            this.btnBuyLuckBooster.Size = new Size(610, 25);
            this.btnBuyLuckBooster.Text = "Luck +25% kaufen (5000 Money)";
            this.btnBuyLuckBooster.Click += new System.EventHandler(this.btnBuyLuckBooster_Click);
            
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new Point(12, 520);
            this.btnRefresh.Size = new Size(660, 30);
            this.btnRefresh.Text = "Aktualisieren";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            
            // 
            // UpgradeForm
            // 
            this.ClientSize = new Size(684, 561);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblGems);
            this.Controls.Add(this.lblMoney);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnRefresh);
            this.Text = "Upgrades";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            // Initialize Skip Next Rebirth panel
            InitializeSkipNextRebirthPanel();
            
            this.tabControl.ResumeLayout(false);
            this.tabGemsUpgrades.ResumeLayout(false);
            this.tabMoneyUpgrades.ResumeLayout(false);
            this.panelAutoRoll.ResumeLayout(false);
            this.panelRollCooldown.ResumeLayout(false);
            this.panelLuckBooster.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
