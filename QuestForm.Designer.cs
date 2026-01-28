namespace SpinARayan
{
    partial class QuestForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Label lblTotalGems;
        private Panel panelQuests;
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblTotalGems = new System.Windows.Forms.Label();
            this.panelQuests = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Size = new System.Drawing.Size(570, 40);
            this.lblTitle.Text = "Quests";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTotalGems
            // 
            this.lblTotalGems.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblTotalGems.Location = new System.Drawing.Point(12, 49);
            this.lblTotalGems.Size = new System.Drawing.Size(570, 30);
            this.lblTotalGems.Text = "Verfügbare Gems: 0";
            this.lblTotalGems.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelQuests
            // 
            this.panelQuests.AutoScroll = true;
            this.panelQuests.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelQuests.Location = new System.Drawing.Point(12, 90);
            this.panelQuests.Size = new System.Drawing.Size(570, 530);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(12, 630);
            this.btnRefresh.Size = new System.Drawing.Size(570, 30);
            this.btnRefresh.Text = "?? Aktualisieren";
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
            this.btnRefresh.ForeColor = System.Drawing.Color.FromArgb(230, 230, 230);
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.FlatAppearance.BorderSize = 1;
            this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // QuestForm
            // 
            this.ClientSize = new System.Drawing.Size(594, 680);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblTotalGems);
            this.Controls.Add(this.panelQuests);
            this.Controls.Add(this.btnRefresh);
            this.Text = "Quests";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ResumeLayout(false);
        }
    }
}
