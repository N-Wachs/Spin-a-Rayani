namespace SpinARayan
{
    partial class DiceShopForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblMoney;
        private System.Windows.Forms.Panel panelDices;

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
            this.lblMoney = new System.Windows.Forms.Label();
            this.panelDices = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Emoji", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Size = new System.Drawing.Size(820, 40);
            this.lblTitle.Text = "\U0001F3B2 DICE SHOP";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            // 
            // lblMoney
            // 
            this.lblMoney.Font = new System.Drawing.Font("Segoe UI Emoji", 14F, System.Drawing.FontStyle.Bold);
            this.lblMoney.Location = new System.Drawing.Point(12, 49);
            this.lblMoney.Size = new System.Drawing.Size(820, 30);
            this.lblMoney.Text = "💰 Money: 0";
            this.lblMoney.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMoney.ForeColor = System.Drawing.Color.FromArgb(76, 175, 80);
            // 
            // panelDices
            // 
            this.panelDices.AutoScroll = true;
            this.panelDices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDices.Location = new System.Drawing.Point(12, 90);
            this.panelDices.Size = new System.Drawing.Size(820, 680);
            this.panelDices.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            // 
            // DiceShopForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi; // DPI-Aware: Skalierung an DPI anpassen
            this.ClientSize = new System.Drawing.Size(844, 800);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblMoney);
            this.Controls.Add(this.panelDices);
            this.Text = "Dice Shop";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 400); // DPI-Aware: Minimale Größe
            this.BackColor = System.Drawing.Color.FromArgb(18, 18, 18);
            this.ResumeLayout(false);
        }
    }
}
