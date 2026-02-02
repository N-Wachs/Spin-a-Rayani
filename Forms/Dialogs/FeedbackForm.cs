using System;
using System.Drawing;
using System.Windows.Forms;
using SpinARayan.Config;
using SpinARayan.Services;

namespace SpinARayan.Forms.Dialogs
{
    public class FeedbackForm : Form
    {
        private TextBox txtFeedback;
        private Button btnSend;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblInfo;
        private Label lblCharCount;
        
        private readonly DatabaseService _databaseService;
        private readonly string _username;
        
        // Modern Theme Colors
        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color DarkAccent = ModernTheme.PrimaryMedium;
        private readonly Color BrightGreen = ModernTheme.Success;
        private readonly Color TextColor = ModernTheme.TextPrimary;
        
        public FeedbackForm(DatabaseService databaseService, string username)
        {
            _databaseService = databaseService;
            _username = username;
            
            InitializeComponent();
            ApplyDarkMode();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Feedback senden";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = DarkBackground;
            
            // Title
            lblTitle = new Label
            {
                Text = "?? Feedback an das Entwicklerteam",
                Location = new Point(20, 20),
                Size = new Size(560, 35),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextColor,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);
            
            // Info Label
            lblInfo = new Label
            {
                Text = "Teile uns deine Meinung, Ideen oder Bugs mit!\n" +
                       "Max. 5 Zeilen mit je 100 Zeichen (500 Zeichen gesamt)",
                Location = new Point(20, 65),
                Size = new Size(560, 40),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(180, 180, 180),
                TextAlign = ContentAlignment.TopCenter
            };
            this.Controls.Add(lblInfo);
            
            // Feedback Textbox
            txtFeedback = new TextBox
            {
                Location = new Point(20, 115),
                Size = new Size(560, 200),
                Multiline = true,
                MaxLength = 500, // 5 lines * 100 chars
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F),
                BackColor = DarkAccent,
                ForeColor = TextColor
            };
            txtFeedback.TextChanged += TxtFeedback_TextChanged;
            this.Controls.Add(txtFeedback);
            
            // Character Count Label
            lblCharCount = new Label
            {
                Text = "0 / 500 Zeichen",
                Location = new Point(20, 320),
                Size = new Size(560, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(150, 150, 150),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(lblCharCount);
            
            // Send Button
            btnSend = new Button
            {
                Text = "?? Feedback senden",
                Location = new Point(20, 355),
                Size = new Size(270, 45),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = BrightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += BtnSend_Click;
            this.Controls.Add(btnSend);
            
            // Cancel Button
            btnCancel = new Button
            {
                Text = "Abbrechen",
                Location = new Point(310, 355),
                Size = new Size(270, 45),
                Font = new Font("Segoe UI", 12F),
                BackColor = DarkAccent,
                ForeColor = TextColor,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 85);
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }
        
        private void ApplyDarkMode()
        {
            // Already applied in InitializeComponent
        }
        
        private void TxtFeedback_TextChanged(object? sender, EventArgs e)
        {
            int charCount = txtFeedback.Text.Length;
            lblCharCount.Text = $"{charCount} / 500 Zeichen";
            
            // Change color based on usage
            if (charCount >= 500)
                lblCharCount.ForeColor = Color.FromArgb(244, 67, 54); // Red
            else if (charCount >= 400)
                lblCharCount.ForeColor = Color.FromArgb(255, 193, 7); // Orange/Yellow
            else
                lblCharCount.ForeColor = Color.FromArgb(150, 150, 150); // Gray
        }
        
        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            string feedback = txtFeedback.Text.Trim();
            
            if (string.IsNullOrEmpty(feedback))
            {
                MessageBox.Show(
                    "Bitte gib erst ein Feedback ein!",
                    "Kein Feedback",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            
            // Disable buttons while sending
            btnSend.Enabled = false;
            btnCancel.Enabled = false;
            btnSend.Text = "?? Wird gesendet...";
            
            try
            {
                bool success = await _databaseService.SaveFeedbackAsync(_username, feedback);
                
                if (success)
                {
                    MessageBox.Show(
                        "? Danke für dein Feedback!\n\n" +
                        "Deine Nachricht wurde erfolgreich gesendet.\n" +
                        "Wir werden uns dein Feedback ansehen!",
                        "Feedback gesendet",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "? Fehler beim Senden des Feedbacks!\n\n" +
                        "Bitte versuche es später erneut.",
                        "Fehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    
                    // Re-enable buttons
                    btnSend.Enabled = true;
                    btnCancel.Enabled = true;
                    btnSend.Text = "?? Feedback senden";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"? Fehler beim Senden:\n\n{ex.Message}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                
                // Re-enable buttons
                btnSend.Enabled = true;
                btnCancel.Enabled = true;
                btnSend.Text = "?? Feedback senden";
            }
        }
    }
}
