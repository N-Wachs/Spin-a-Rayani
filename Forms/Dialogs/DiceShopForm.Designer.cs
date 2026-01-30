namespace SpinARayan
{
    partial class DiceShopForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Label lblMoney;
        private Panel panelDices;

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
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Size = new System.Drawing.Size(820, 40);
            this.lblTitle.Text = "?? DICE SHOP";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMoney
            // 
            this.lblMoney.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblMoney.Location = new System.Drawing.Point(12, 49);
            this.lblMoney.Size = new System.Drawing.Size(820, 30);
            this.lblMoney.Text = "?? Money: 0";
            this.lblMoney.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelDices
            // 
            this.panelDices.AutoScroll = true;
            this.panelDices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDices.Location = new System.Drawing.Point(12, 90);
            this.panelDices.Size = new System.Drawing.Size(820, 680);
            // 
            // DiceShopForm
            // 
            this.ClientSize = new System.Drawing.Size(844, 800);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblMoney);
            this.Controls.Add(this.panelDices);
            this.Text = "Dice Shop";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ResumeLayout(false);
        }
    }
}
