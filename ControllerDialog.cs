using System;
using System.Drawing;
using System.Windows.Forms;

namespace SwitchTestingMode
{
    public sealed class ControllerDialog : Form
    {
        readonly XboxController _controller;
        readonly Timer _timer;
        readonly Button[] _buttons;
        int _selectedIndex;

        public DialogResult DialogResultValue { get; private set; } = DialogResult.None;

        public ControllerDialog(string title, string message, MessageBoxButtons buttons, XboxController controller)
        {
            _controller = controller;

            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ShowIcon = false;
            BackColor = Color.FromArgb(245, 245, 245);
            ClientSize = new Size(420, 180);

            var msgLabel = new Label
            {
                Text = message,
                Location = new Point(20, 20),
                Size = new Size(380, 70),
                Font = new Font("Microsoft YaHei", 11),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(msgLabel);

            int btnY = 110;

            if (buttons == MessageBoxButtons.OK)
            {
                var okBtn = new Button
                {
                    Text = "确定",
                    Size = new Size(120, 36),
                    Location = new Point((ClientSize.Width - 120) / 2, btnY),
                    Font = new Font("Microsoft YaHei", 11),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 2, BorderColor = Color.FromArgb(0, 120, 215) },
                    BackColor = Color.White,
                    UseVisualStyleBackColor = true
                };
                okBtn.Click += (s, e) => { DialogResultValue = DialogResult.OK; Close(); };
                Controls.Add(okBtn);
                _buttons = new[] { okBtn };
            }
            else
            {
                var yesBtn = new Button
                {
                    Text = "是(&Y)",
                    Size = new Size(120, 36),
                    Location = new Point(ClientSize.Width / 2 - 140, btnY),
                    Font = new Font("Microsoft YaHei", 11),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 2, BorderColor = Color.FromArgb(0, 120, 215) },
                    BackColor = Color.White,
                    UseVisualStyleBackColor = true
                };
                yesBtn.Click += (s, e) => { DialogResultValue = DialogResult.Yes; Close(); };

                var noBtn = new Button
                {
                    Text = "否(&N)",
                    Size = new Size(120, 36),
                    Location = new Point(ClientSize.Width / 2 + 20, btnY),
                    Font = new Font("Microsoft YaHei", 11),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(180, 180, 180) },
                    BackColor = Color.White,
                    UseVisualStyleBackColor = true
                };
                noBtn.Click += (s, e) => { DialogResultValue = DialogResult.No; Close(); };

                Controls.Add(yesBtn);
                Controls.Add(noBtn);
                _buttons = new[] { yesBtn, noBtn };
            }

            _selectedIndex = 0;
            UpdateButtonStyles();

            _controller.AButton += OnA;
            _controller.BButton += OnB;
            _controller.DPadUp += OnUp;
            _controller.DPadDown += OnDown;

            _timer = new Timer { Interval = 50 };
            _timer.Tick += (s, e) => _controller.Poll();
            _timer.Start();

            FormClosed += (s, e) =>
            {
                _timer.Stop();
                _timer.Dispose();
                _controller.AButton -= OnA;
                _controller.BButton -= OnB;
                _controller.DPadUp -= OnUp;
                _controller.DPadDown -= OnDown;
            };
        }

        void OnA()
        {
            if (IsHandleCreated)
                _buttons[_selectedIndex].PerformClick();
        }

        void OnB()
        {
            if (IsHandleCreated)
            {
                DialogResultValue = DialogResult.Cancel;
                Close();
            }
        }

        void OnUp()
        {
            if (IsHandleCreated)
            {
                _selectedIndex = (_selectedIndex - 1 + _buttons.Length) % _buttons.Length;
                UpdateButtonStyles();
            }
        }

        void OnDown()
        {
            if (IsHandleCreated)
            {
                _selectedIndex = (_selectedIndex + 1) % _buttons.Length;
                UpdateButtonStyles();
            }
        }

        void UpdateButtonStyles()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                var app = _buttons[i].FlatAppearance;
                if (i == _selectedIndex)
                {
                    app.BorderColor = Color.FromArgb(0, 120, 215);
                    app.BorderSize = 2;
                }
                else
                {
                    app.BorderColor = Color.FromArgb(180, 180, 180);
                    app.BorderSize = 1;
                }
            }
        }
    }
}
