namespace SpinARayan
{
    partial class FullInventoryForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Label lblTotalRayans;
        private Panel panelInventory;

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
            this.lblTotalRayans = new System.Windows.Forms.Label();
            this.panelInventory = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Size = new System.Drawing.Size(800, 40);
            this.lblTitle.Text = "Vollständiges Inventar";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTotalRayans
            // 
            this.lblTotalRayans.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblTotalRayans.Location = new System.Drawing.Point(12, 49);
            this.lblTotalRayans.Size = new System.Drawing.Size(800, 30);
            this.lblTotalRayans.Text = "Gesamt Rayans: 0";
            this.lblTotalRayans.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelInventory
            // 
            this.panelInventory.AutoScroll = true;
            this.panelInventory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelInventory.Location = new System.Drawing.Point(12, 90);
            this.panelInventory.Size = new System.Drawing.Size(800, 630);
            this.panelInventory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // FullInventoryForm
            // 
            this.ClientSize = new System.Drawing.Size(824, 750);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblTotalRayans);
            this.Controls.Add(this.panelInventory);
            this.Text = "Vollständiges Inventar";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ResumeLayout(false);
        }
    }
}
