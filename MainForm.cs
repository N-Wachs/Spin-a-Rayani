using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows.Forms;
using SpinARayan.Models;
using SpinARayan.Services;

namespace SpinARayan
{
    public partial class MainForm : Form
    {
        private readonly GameManager _gameManager;
        private double _rollCooldownRemaining = 0;
        private System.Windows.Forms.Timer _rollCooldownTimer;
        private bool _plotsDirty = true;

        private readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
        private readonly Color DarkPanel = Color.FromArgb(45, 45, 48);
        private readonly Color DarkAccent = Color.FromArgb(60, 60, 65);
        private readonly Color BrightGreen = Color.FromArgb(0, 255, 127);
        private readonly Color BrightBlue = Color.FromArgb(0, 174, 255);
        private readonly Color BrightGold = Color.FromArgb(255, 215, 0);
        private readonly Color BrightRed = Color.FromArgb(255, 69, 58);
        private readonly Color TextColor = Color.FromArgb(230, 230, 230);

        public MainForm()
        {
            _gameManager = new GameManager();
            InitializeComponent();
            SetupCustomUI();
            
            _gameManager.OnStatsChanged += () => {
                if (this.InvokeRequired) this.Invoke(new Action(UpdateUI));
                else UpdateUI();
            };
            
            _gameManager.OnRayanRolled += (rayan) => {
                if (this.InvokeRequired) this.Invoke(new Action(() => ShowRollResult(rayan)));
                else ShowRollResult(rayan);
            };

            _rollCooldownTimer = new System.Windows.Forms.Timer();
            _rollCooldownTimer.Interval = 100;
            _rollCooldownTimer.Tick += (s, e) => {
                if (_rollCooldownRemaining > 0)
                {
                    _rollCooldownRemaining -= 0.1;
                    if (_rollCooldownRemaining <= 0)
                    {
                        _rollCooldownRemaining = 0;
                        _rollCooldownTimer.Stop();
                        btnRoll.Enabled = true;
                    }
                    UpdateRollButtonText();
                }
            };

            UpdateUI();
        }

        private void SetupCustomUI()
        {
            this.BackColor = DarkBackground;
            panelLeft.BackColor = DarkPanel;
            panelCenter.BackColor = DarkBackground;
            panelRight.BackColor = DarkPanel;
            
            SetDoubleBuffered(panelCenter);
        }

        private void UpdateUI()
        {
            lblMoney.Text = $"Money: {FormatBigInt(_gameManager.Stats.Money)}";
            lblGems.Text = $"Gems: {FormatBigInt(_gameManager.Stats.Gems)}";
            lblRebirths.Text = $"Rebirths: {_gameManager.Stats.Rebirths}";
            
            UpdateRollButtonText();
            
            if (_plotsDirty)
            {
                UpdatePlotsUI();
                _plotsDirty = false;
            }
        }

        private void UpdateRollButtonText()
        {
            if (_rollCooldownRemaining > 0)
            {
                btnRoll.Text = $"WAIT ({_rollCooldownRemaining:F1}s)";
            }
            else
            {
                var selectedDice = _gameManager.Stats.GetSelectedDice();
                btnRoll.Text = $"ROLL ({selectedDice.Name})";
                
                string dicePath = Path.Combine(Application.StartupPath, selectedDice.ImagePath.TrimStart('/'));
                if (File.Exists(dicePath))
                {
                    btnRoll.Image = Image.FromFile(dicePath);
                    btnRoll.ImageAlign = ContentAlignment.MiddleLeft;
                    btnRoll.TextImageRelation = TextImageRelation.ImageBeforeText;
                }
            }
        }

        private void UpdatePlotsUI()
        {
            // Simplified plot update logic
        }

        private void ShowRollResult(Rayan rayan)
        {
            lblLastRoll.Text = $"Last Roll: {rayan.FullName} (1:{rayan.Rarity:N0})";
            lblLastRoll.ForeColor = rayan.Rarity > 1000000 ? BrightGold : TextColor;
        }

        private void btnRoll_Click(object sender, EventArgs e)
        {
            _gameManager.Roll();
            btnRoll.Enabled = false;
            _rollCooldownRemaining = _gameManager.Stats.RollCooldown;
            _rollCooldownTimer.Start();
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            var fullInventoryForm = new FullInventoryForm(_gameManager.Stats.Inventory, () => {
                _gameManager.Save();
                UpdateUI();
            });
            fullInventoryForm.ShowDialog();
        }

        private void btnShop_Click(object sender, EventArgs e)
        {
            var diceShopForm = new DiceShopForm(_gameManager, () => {
                UpdateUI();
            });
            diceShopForm.ShowDialog();
        }

        private void btnUpgrades_Click(object sender, EventArgs e)
        {
            var upgradeForm = new UpgradeForm(_gameManager, () => {
                UpdateUI();
            });
            upgradeForm.ShowDialog();
        }

        private void btnQuests_Click(object sender, EventArgs e)
        {
            var questForm = new QuestForm(_gameManager, () => {
                UpdateUI();
            });
            questForm.ShowDialog();
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            var optionsForm = new OptionsForm(_gameManager, () => {
                UpdateUI();
            });
            optionsForm.ShowDialog();
        }

        private string FormatBigInt(BigInteger value)
        {
            if (value < 1000) return value.ToString();
            if (value < 1000000) return ((double)value / 1000).ToString("F1") + "K";
            if (value < 1000000000) return ((double)value / 1000000).ToString("F1") + "M";
            if (value < 1000000000000) return ((double)value / 1000000000).ToString("F1") + "B";
            return value.ToString("E1");
        }

        private static void SetDoubleBuffered(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }
    }
}
