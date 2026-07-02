using System;
using System.Drawing;
using System.Windows.Forms;

namespace SwitchTestingMode
{
    public sealed class MainForm : Form
    {
        Panel _statusPanel;
        Label _statusIcon;
        Label _statusText;
        Label _versionText;
        Button _toggleButton;
        Button _exitButton;
        Label _controllerStatus;
        Timer _controllerTimer;
        XboxController _controller;
        bool _testMode;
        bool _showingDialog;

        public MainForm()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Text = "Windows 测试模式切换工具";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(520, 350);
            BackColor = Color.FromArgb(245, 245, 245);

            _statusPanel = new Panel
            {
                Size = new Size(480, 110),
                Location = new Point(20, 20)
            };

            _statusIcon = new Label
            {
                Size = new Size(48, 48),
                Location = new Point(216, 8),
                Font = new Font("Segoe UI", 28),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "●"
            };

            _statusText = new Label
            {
                Size = new Size(460, 30),
                Location = new Point(10, 58),
                Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _versionText = new Label
            {
                Size = new Size(460, 22),
                Location = new Point(10, 84),
                Font = new Font("Microsoft YaHei", 9),
                ForeColor = Color.FromArgb(200, 220, 255),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _statusPanel.Controls.AddRange(new Control[] { _statusIcon, _statusText, _versionText });

            _toggleButton = new Button
            {
                Size = new Size(300, 48),
                Location = new Point(110, 150),
                Font = new Font("Microsoft YaHei", 13, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 2 },
                BackColor = Color.White,
                UseVisualStyleBackColor = true
            };
            _toggleButton.Click += (s, e) => ToggleMode();

            _exitButton = new Button
            {
                Size = new Size(300, 40),
                Location = new Point(110, 215),
                Font = new Font("Microsoft YaHei", 11),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1 },
                BackColor = Color.WhiteSmoke,
                Text = "退出",
                ForeColor = Color.FromArgb(100, 100, 100),
                UseVisualStyleBackColor = true
            };
            _exitButton.Click += (s, e) => Close();

            _controllerStatus = new Label
            {
                Size = new Size(480, 24),
                Location = new Point(20, 270),
                Font = new Font("Microsoft YaHei", 9),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "🎮 未检测到手柄"
            };

            Controls.AddRange(new Control[] { _statusPanel, _toggleButton, _exitButton, _controllerStatus });

            _controller = new XboxController();
            _controller.AButton += () => { if (!_showingDialog) Invoke(new Action(ToggleMode)); };
            _controller.BButton += () => { if (!_showingDialog) Invoke(new Action(Close)); };
            _controller.DPadUp += () => { if (!_showingDialog) Invoke(new Action(() =>
                SelectNextControl(ActiveControl, false, true, true, true))); };
            _controller.DPadDown += () => { if (!_showingDialog) Invoke(new Action(() =>
                SelectNextControl(ActiveControl, true, true, true, true))); };
            _controller.ConnectionChanged += () => Invoke(new Action(() =>
            {
                _controllerStatus.Text = _controller.Connected
                    ? $"🎮 手柄已连接（端口 {_controller.ControllerIndex + 1}）▲▼选择  A确认  B退出"
                    : "🎮 未检测到手柄";
            }));

            _controllerTimer = new Timer { Interval = 50 };
            _controllerTimer.Tick += (s, e) => _controller.Poll();

            Load += (s, e) =>
            {
                RefreshStatus();
                _controllerTimer.Start();
            };
        }

        void RefreshStatus()
        {
            try { _testMode = TestModeManager.IsTestModeEnabled(); }
            catch { _testMode = false; }

            if (_testMode)
            {
                _statusPanel.BackColor = Color.FromArgb(0, 150, 70);
                _statusText.Text = "当前状态：测试模式已开启";
                _toggleButton.Text = "关闭测试模式";
                _toggleButton.ForeColor = Color.FromArgb(200, 50, 50);
                _toggleButton.FlatAppearance.BorderColor = Color.FromArgb(200, 50, 50);
            }
            else
            {
                _statusPanel.BackColor = Color.FromArgb(190, 60, 50);
                _statusText.Text = "当前状态：测试模式已关闭";
                _toggleButton.Text = "开启测试模式";
                _toggleButton.ForeColor = Color.FromArgb(0, 140, 60);
                _toggleButton.FlatAppearance.BorderColor = Color.FromArgb(0, 140, 60);
            }

            _versionText.Text = TestModeManager.GetWindowsVersion();
        }

        void ToggleMode()
        {
            _showingDialog = true;
            _controllerTimer.Enabled = false;

            try
            {
                _toggleButton.Enabled = false;
                _toggleButton.Text = "执行中...";

                TestModeManager.SetTestMode(!_testMode);

                RefreshStatus();
                TestModeManager.RestartSystem();
            }
            catch (Exception ex)
            {
                using (var dlg = new ControllerDialog("错误",
                    $"操作失败：{ex.Message}",
                    MessageBoxButtons.OK, _controller))
                {
                    dlg.ShowDialog(this);
                }
            }
            finally
            {
                _showingDialog = false;
                _toggleButton.Enabled = true;
                _controllerTimer.Enabled = true;
            }
        }
    }
}
