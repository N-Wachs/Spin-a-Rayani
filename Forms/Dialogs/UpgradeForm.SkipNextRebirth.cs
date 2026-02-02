using System.Drawing;
using System.Windows.Forms;

namespace SpinARayan
{
    partial class UpgradeForm
    {
        /// <summary>
        /// Initialize the Skip Next Rebirth panel
        /// </summary>
        private void InitializeSkipNextRebirthPanel()
        {
            this.panelSkipNextRebirth = new Panel();
            this.lblSkipNextRebirthTitle = new Label();
            this.lblSkipNextRebirth = new Label();
            this.btnBuySkipNextRebirth = new Button();
            
            // 
            // panelSkipNextRebirth
            // 
            this.panelSkipNextRebirth.BorderStyle = BorderStyle.FixedSingle;
            this.panelSkipNextRebirth.Controls.Add(this.lblSkipNextRebirthTitle);
            this.panelSkipNextRebirth.Controls.Add(this.lblSkipNextRebirth);
            this.panelSkipNextRebirth.Controls.Add(this.btnBuySkipNextRebirth);
            this.panelSkipNextRebirth.Location = new Point(10, 140);
            this.panelSkipNextRebirth.Size = new Size(630, 140);
            this.panelSkipNextRebirth.BackColor = Color.White;
            
            // 
            // lblSkipNextRebirthTitle
            // 
            this.lblSkipNextRebirthTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblSkipNextRebirthTitle.Location = new Point(10, 10);
            this.lblSkipNextRebirthTitle.Size = new Size(610, 30);
            this.lblSkipNextRebirthTitle.Text = "? Skip Next Rebirth (+2 Rebirths statt +1)";
            
            // 
            // lblSkipNextRebirth
            // 
            this.lblSkipNextRebirth.Font = new Font("Segoe UI", 9F);
            this.lblSkipNextRebirth.Location = new Point(10, 45);
            this.lblSkipNextRebirth.Size = new Size(610, 60);
            this.lblSkipNextRebirth.Text = "Kosten: 60% Aufpreis vom aktuellen Rebirth-Preis.\n" +
                                           "Beim n‰chsten Rebirth erh‰ltst du +2 Rebirths statt +1.\n" +
                                           "?? Verliert alle Dices (auﬂer Basic) und Rayans SOFORT!\n" +
                                           "?? Zieht nur den Upgrade-Preis ab, nicht den Rebirth-Preis.";
            
            // 
            // btnBuySkipNextRebirth
            // 
            this.btnBuySkipNextRebirth.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnBuySkipNextRebirth.Location = new Point(10, 105);
            this.btnBuySkipNextRebirth.Size = new Size(610, 25);
            this.btnBuySkipNextRebirth.Text = "Upgrade kaufen (Berechnung l‰uft...)";
            this.btnBuySkipNextRebirth.Click += new System.EventHandler(this.btnBuySkipNextRebirth_Click);
            
            // Add panel to tabMoneyUpgrades
            this.tabMoneyUpgrades.Controls.Add(this.panelSkipNextRebirth);
        }
    }
}
