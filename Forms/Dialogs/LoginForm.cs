using System;
using System.Drawing;
using System.Windows.Forms;
using SpinARayan.Config;

namespace SpinARayan.Forms.Dialogs
{
    public partial class LoginForm : Form
    {
        public string Username { get; private set; } = "";
        public string Password { get; private set; } = "";
        public bool RememberMe { get; private set; } = true;

        private readonly Color DarkBackground = ModernTheme.BackgroundElevated;
        private readonly Color DarkPanel = ModernTheme.BackgroundPanel;
        private readonly Color BrightGreen = ModernTheme.Success;
        private readonly Color BrightBlue = ModernTheme.AccentBlue;
        private readonly Color TextColor = ModernTheme.TextPrimary;

        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckBox chkRememberMe;
        private Button btnLogin;
        private Button btnRegister;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;

        public LoginForm()
        {
            InitializeComponent();
            ApplyDarkMode();
        }

        private void InitializeComponent()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi; // DPI-Aware: Skalierung an DPI anpassen
            this.Text = "Login - Spin a Rayan";
            this.ClientSize = new Size(400, 340);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new Size(350, 300); // DPI-Aware: Minimale Größe

            // Title
            lblTitle = new Label
            {
                Text = "🎲 Spin a Rayan - Login",
                Location = new Point(20, 20),
                Size = new Size(360, 40),
                Font = new Font("Segoe UI Emoji", 16F, FontStyle.Bold, GraphicsUnit.Point),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Username Label
            lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(20, 80),
                Size = new Size(360, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point)
            };
            this.Controls.Add(lblUsername);

            // Username TextBox
            txtUsername = new TextBox
            {
                Location = new Point(20, 105),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point)
            };
            this.Controls.Add(txtUsername);

            // Password Label
            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(20, 145),
                Size = new Size(360, 20),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblPassword);

            // Password TextBox
            txtPassword = new TextBox
            {
                Location = new Point(20, 170),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 12F),
                PasswordChar = '\u2022',
                UseSystemPasswordChar = false
            };
            this.Controls.Add(txtPassword);

            // Remember Me Checkbox
            chkRememberMe = new CheckBox
            {
                Text = "Login merken",
                Location = new Point(20, 210),
                Size = new Size(360, 25),
                Font = new Font("Segoe UI", 10F),
                Checked = true
            };
            this.Controls.Add(chkRememberMe);

            // Login Button
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(20, 245),
                Size = new Size(170, 35),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            // Cancel Button
            btnCancel = new Button
            {
                Text = "Abbrechen",
                Location = new Point(210, 245),
                Size = new Size(170, 35),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);
            
            // Register Button
            btnRegister = new Button
            {
                Text = "\U00002795 Neuen Account erstellen",
                Location = new Point(20, 290),
                Size = new Size(360, 35),
                Font = new Font("Segoe UI Emoji", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;
            this.Controls.Add(btnRegister);

            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;
        }

        private void ApplyDarkMode()
        {
            this.BackColor = DarkBackground;
            lblTitle.ForeColor = BrightBlue;
            lblUsername.ForeColor = TextColor;
            lblPassword.ForeColor = TextColor;
            chkRememberMe.ForeColor = TextColor;
            chkRememberMe.BackColor = DarkBackground;
            
            btnLogin.BackColor = BrightGreen;
            btnLogin.ForeColor = Color.White;
            
            btnCancel.BackColor = DarkPanel;
            btnCancel.ForeColor = TextColor;
            
            btnRegister.BackColor = BrightBlue;
            btnRegister.ForeColor = Color.White;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            Username = txtUsername.Text.Trim();
            Password = txtPassword.Text;
            RememberMe = chkRememberMe.Checked;

            if (string.IsNullOrEmpty(Username))
            {
                MessageBox.Show("Bitte Username eingeben!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Bitte Passwort eingeben!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            Username = txtUsername.Text.Trim();
            Password = txtPassword.Text;
            RememberMe = chkRememberMe.Checked;

            if (string.IsNullOrEmpty(Username))
            {
                MessageBox.Show("Bitte Username eingeben!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Bitte Passwort eingeben!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            if (Password.Length < 4)
            {
                MessageBox.Show("Passwort muss mindestens 4 Zeichen lang sein!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.DialogResult = DialogResult.Retry; // Use Retry for Register action
            this.Close();
        }
    }
}
