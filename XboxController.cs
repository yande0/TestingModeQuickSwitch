using System;
using System.Runtime.InteropServices;

namespace SwitchTestingMode
{
    public sealed class XboxController : IDisposable
    {
        const int MAX_CONTROLLERS = 4;
        const uint ERROR_SUCCESS = 0;

        [DllImport("xinput1_4.dll")]
        static extern uint XInputGetState(int dwUserIndex, out XINPUT_STATE pState);

        [StructLayout(LayoutKind.Sequential)]
        struct XINPUT_STATE
        {
            public uint dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct XINPUT_GAMEPAD
        {
            public ushort wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }

        public const ushort DPAD_UP = 0x0001;
        public const ushort DPAD_DOWN = 0x0002;
        public const ushort DPAD_LEFT = 0x0004;
        public const ushort DPAD_RIGHT = 0x0008;
        public const ushort START = 0x0010;
        public const ushort BACK = 0x0020;
        public const ushort A = 0x1000;
        public const ushort B = 0x2000;
        public const ushort X = 0x4000;
        public const ushort Y = 0x8000;

        XINPUT_GAMEPAD _prev;
        int _index = -1;
        bool _connected;

        public bool Connected => _connected;
        public int ControllerIndex => _index;

        public event Action AButton;
        public event Action BButton;
        public event Action StartButton;
        public event Action DPadUp;
        public event Action DPadDown;
        public event Action DPadLeft;
        public event Action DPadRight;
        public event Action ConnectionChanged;

        public bool Poll()
        {
            for (int i = 0; i < MAX_CONTROLLERS; i++)
            {
                uint result = XInputGetState(i, out XINPUT_STATE state);
                if (result == ERROR_SUCCESS)
                {
                    if (!_connected)
                    {
                        _connected = true;
                        _index = i;
                        ConnectionChanged?.Invoke();
                    }

                    var cur = state.Gamepad;
                    ushort pressed = (ushort)(cur.wButtons & ~_prev.wButtons);

                    if ((pressed & A) != 0) AButton?.Invoke();
                    if ((pressed & B) != 0) BButton?.Invoke();
                    if ((pressed & START) != 0) StartButton?.Invoke();
                    if ((pressed & DPAD_UP) != 0) DPadUp?.Invoke();
                    if ((pressed & DPAD_DOWN) != 0) DPadDown?.Invoke();
                    if ((pressed & DPAD_LEFT) != 0) DPadLeft?.Invoke();
                    if ((pressed & DPAD_RIGHT) != 0) DPadRight?.Invoke();

                    if (Math.Abs((int)cur.sThumbLY) > 20000 && Math.Abs((int)_prev.sThumbLY) <= 20000)
                    {
                        if (cur.sThumbLY > 0) DPadUp?.Invoke();
                        else DPadDown?.Invoke();
                    }

                    _prev = cur;
                    return true;
                }
            }

            if (_connected)
            {
                _connected = false;
                _index = -1;
                _prev = default;
                ConnectionChanged?.Invoke();
            }

            return false;
        }

        public void Dispose() { }
    }
}
