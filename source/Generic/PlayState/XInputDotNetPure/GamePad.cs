using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayState.XInputDotNetPure
{
    // Based on https://github.com/speps/XInputDotNet
    class Imports
    {
        internal const string DLLName = "XInputInterface";

        [DllImport(DLLName)]
        public static extern uint XInputGamePadGetState(uint playerIndex, out GamePadState.RawState state);
        [DllImport(DLLName)]
        public static extern void XInputGamePadSetState(uint playerIndex, float leftMotor, float rightMotor);
    }

    public enum ButtonState
    {
        Pressed,
        Released
    }

    public struct GamePadButtons : IEquatable<GamePadButtons>
    {
        ButtonState start, back, leftStick, rightStick, leftShoulder, rightShoulder, guide, a, b, x, y;

        public GamePadButtons(ButtonState start, ButtonState back, ButtonState leftStick, ButtonState rightStick,
                                ButtonState leftShoulder, ButtonState rightShoulder, ButtonState guide,
                                ButtonState a, ButtonState b, ButtonState x, ButtonState y)
        {
            this.start = start;
            this.back = back;
            this.leftStick = leftStick;
            this.rightStick = rightStick;
            this.leftShoulder = leftShoulder;
            this.rightShoulder = rightShoulder;
            this.guide = guide;
            this.a = a;
            this.b = b;
            this.x = x;
            this.y = y;
        }

        public ButtonState Start
        {
            get { return start; }
        }

        public ButtonState Back
        {
            get { return back; }
        }

        public ButtonState LeftStick
        {
            get { return leftStick; }
        }

        public ButtonState RightStick
        {
            get { return rightStick; }
        }

        public ButtonState LeftShoulder
        {
            get { return leftShoulder; }
        }

        public ButtonState RightShoulder
        {
            get { return rightShoulder; }
        }

        public ButtonState Guide
        {
            get { return guide; }
        }

        public ButtonState A
        {
            get { return a; }
        }

        public ButtonState B
        {
            get { return b; }
        }

        public ButtonState X
        {
            get { return x; }
        }

        public ButtonState Y
        {
            get { return y; }
        }

        public bool IsAnyPressed()
        {
            return start == ButtonState.Pressed ||
                   back == ButtonState.Pressed ||
                   leftStick == ButtonState.Pressed ||
                   rightStick == ButtonState.Pressed ||
                   leftShoulder == ButtonState.Pressed ||
                   rightShoulder == ButtonState.Pressed ||
                   a == ButtonState.Pressed ||
                   b == ButtonState.Pressed ||
                   x == ButtonState.Pressed ||
                   y == ButtonState.Pressed;
        }

        public bool Equals(GamePadButtons other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return this == (GamePadButtons)obj;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"Start:{Start}, Back:{Back}, LeftStick:{LeftStick}, RightStick:{RightStick}, LeftShoulder:{LeftShoulder}, RightShoulder:{RightShoulder}, Guide:{Guide}, A:{A}, B:{B}, X:{X}, Y:{Y}";
        }

        public List<string> GetPressedButtonsList()
        {
            var buttons = new List<string>();
            if (Start == ButtonState.Pressed)
            {
                buttons.Add("Start");
            }
            if (Back == ButtonState.Pressed)
            {
                buttons.Add("Back");
            }
            if (LeftStick == ButtonState.Pressed)
            {
                buttons.Add("LeftStick");
            }
            if (RightStick == ButtonState.Pressed)
            {
                buttons.Add("RightStick");
            }
            if (LeftShoulder == ButtonState.Pressed)
            {
                buttons.Add("LeftShoulder");
            }
            if (RightShoulder == ButtonState.Pressed)
            {
                buttons.Add("RightShoulder");
            }
            if (Guide == ButtonState.Pressed)
            {
                buttons.Add("Guide");
            }
            if (A == ButtonState.Pressed)
            {
                buttons.Add("A");
            }
            if (B == ButtonState.Pressed)
            {
                buttons.Add("B");
            }
            if (X == ButtonState.Pressed)
            {
                buttons.Add("X");
            }
            if (Y == ButtonState.Pressed)
            {
                buttons.Add("Y");
            }

            return buttons;
        }

        public static bool operator ==(GamePadButtons obj1, GamePadButtons obj2)
        {
            return obj1.Start == obj2.Start &&
                   obj1.Back == obj2.Back &&
                   obj1.LeftStick == obj2.LeftStick &&
                   obj1.RightStick == obj2.RightStick &&
                   obj1.LeftShoulder == obj2.LeftShoulder &&
                   obj1.RightShoulder == obj2.RightShoulder &&
                   obj1.Guide == obj2.Guide &&
                   obj1.A == obj2.A &&
                   obj1.B == obj2.B &&
                   obj1.X == obj2.X &&
                   obj1.Y == obj2.Y;
        }

        public static bool operator !=(GamePadButtons obj1, GamePadButtons obj2)
        {
            return !(obj1.Start == obj2.Start &&
                   obj1.Back == obj2.Back &&
                   obj1.LeftStick == obj2.LeftStick &&
                   obj1.RightStick == obj2.RightStick &&
                   obj1.LeftShoulder == obj2.LeftShoulder &&
                   obj1.RightShoulder == obj2.RightShoulder &&
                   obj1.Guide == obj2.Guide &&
                   obj1.A == obj2.A &&
                   obj1.B == obj2.B &&
                   obj1.X == obj2.X &&
                   obj1.Y == obj2.Y);
        }
    }

    public struct GamePadDPad : IEquatable<GamePadDPad>
    {
        ButtonState up, down, left, right;

        public GamePadDPad(ButtonState up, ButtonState down, ButtonState left, ButtonState right)
        {
            this.up = up;
            this.down = down;
            this.left = left;
            this.right = right;
        }

        public ButtonState Up
        {
            get { return up; }
        }

        public ButtonState Down
        {
            get { return down; }
        }

        public ButtonState Left
        {
            get { return left; }
        }

        public ButtonState Right
        {
            get { return right; }
        }

        public bool IsAnyPressed()
        {
            return up == ButtonState.Pressed ||
                   down == ButtonState.Pressed ||
                   left == ButtonState.Pressed ||
                   right == ButtonState.Pressed;
        }

        public List<string> GetPressedButtonsList()
        {
            var buttons = new List<string>();
            if (up == ButtonState.Pressed)
            {
                buttons.Add("Up");
            }
            if (down == ButtonState.Pressed)
            {
                buttons.Add("Down");
            }
            if (left == ButtonState.Pressed)
            {
                buttons.Add("Left");
            }
            if (right == ButtonState.Pressed)
            {
                buttons.Add("Right");
            }

            return buttons;
        }

        public bool Equals(GamePadDPad other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return this == (GamePadDPad)obj;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"Up:{Up}, Down:{Down}, Left:{Left}, Right:{Right}";
        }

        public static bool operator ==(GamePadDPad obj1, GamePadDPad obj2)
        {
            return obj1.Up == obj2.Up &&
                   obj1.Down == obj2.Down &&
                   obj1.Left == obj2.Left &&
                   obj1.Right == obj2.Right;
        }

        public static bool operator !=(GamePadDPad obj1, GamePadDPad obj2)
        {
            return !(obj1.Up == obj2.Up &&
                   obj1.Down == obj2.Down &&
                   obj1.Left == obj2.Left &&
                   obj1.Right == obj2.Right);
        }
    }

    public struct GamePadThumbSticks
    {
        public struct StickValue
        {
            float x, y;

            internal StickValue(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public float X
            {
                get { return x; }
            }

            public float Y
            {
                get { return y; }
            }
        }

        StickValue left, right;

        internal GamePadThumbSticks(StickValue left, StickValue right)
        {
            this.left = left;
            this.right = right;
        }

        public StickValue Left
        {
            get { return left; }
        }

        public StickValue Right
        {
            get { return right; }
        }
    }

    public struct GamePadTriggers : IEquatable<GamePadTriggers>
    {
        float left;
        float right;

        internal GamePadTriggers(float left, float right)
        {
            this.left = left;
            this.right = right;
        }

        public float Left
        {
            get { return left; }
        }

        public float Right
        {
            get { return right; }
        }

        public bool IsAnyPressed()
        {
            return left != 0 ||
                   right != 0;
        }

        public bool Equals(GamePadTriggers other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return this == (GamePadTriggers)obj;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"Left:{Left}, Right:{Right}";
        }

        public static bool operator ==(GamePadTriggers obj1, GamePadTriggers obj2)
        {
            return obj1.Left == obj2.Left &&
                   obj1.Right == obj2.Right;
        }

        public static bool operator !=(GamePadTriggers obj1, GamePadTriggers obj2)
        {
            return !(obj1.Left == obj2.Left &&
                   obj1.Right == obj2.Right);
        }
    }

    public struct GamePadState
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct RawState
        {
            public uint dwPacketNumber;
            public GamePad Gamepad;

            [StructLayout(LayoutKind.Sequential)]
            public struct GamePad
            {
                public ushort wButtons;
                public byte bLeftTrigger;
                public byte bRightTrigger;
                public short sThumbLX;
                public short sThumbLY;
                public short sThumbRX;
                public short sThumbRY;
            }
        }

        bool isConnected;
        uint packetNumber;
        GamePadButtons buttons;
        GamePadDPad dPad;
        GamePadThumbSticks thumbSticks;
        GamePadTriggers triggers;

        enum ButtonsConstants
        {
            DPadUp = 0x00000001,
            DPadDown = 0x00000002,
            DPadLeft = 0x00000004,
            DPadRight = 0x00000008,
            Start = 0x00000010,
            Back = 0x00000020,
            LeftThumb = 0x00000040,
            RightThumb = 0x00000080,
            LeftShoulder = 0x0100,
            RightShoulder = 0x0200,
            Guide = 0x0400,
            A = 0x1000,
            B = 0x2000,
            X = 0x4000,
            Y = 0x8000
        }

        internal GamePadState(bool isConnected, RawState rawState, GamePadDeadZone deadZone)
        {
            this.isConnected = isConnected;

            if (!isConnected)
            {
                rawState.dwPacketNumber = 0;
                rawState.Gamepad.wButtons = 0;
                rawState.Gamepad.bLeftTrigger = 0;
                rawState.Gamepad.bRightTrigger = 0;
                rawState.Gamepad.sThumbLX = 0;
                rawState.Gamepad.sThumbLY = 0;
                rawState.Gamepad.sThumbRX = 0;
                rawState.Gamepad.sThumbRY = 0;
            }

            packetNumber = rawState.dwPacketNumber;
            buttons = new GamePadButtons(
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Start) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Back) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.LeftThumb) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.RightThumb) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.LeftShoulder) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.RightShoulder) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Guide) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.A) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.B) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.X) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.Y) != 0 ? ButtonState.Pressed : ButtonState.Released
            );
            dPad = new GamePadDPad(
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadUp) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadDown) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadLeft) != 0 ? ButtonState.Pressed : ButtonState.Released,
                (rawState.Gamepad.wButtons & (uint)ButtonsConstants.DPadRight) != 0 ? ButtonState.Pressed : ButtonState.Released
            );

            thumbSticks = new GamePadThumbSticks(
                Utils.ApplyLeftStickDeadZone(rawState.Gamepad.sThumbLX, rawState.Gamepad.sThumbLY, deadZone),
                Utils.ApplyRightStickDeadZone(rawState.Gamepad.sThumbRX, rawState.Gamepad.sThumbRY, deadZone)
            );
            triggers = new GamePadTriggers(
                Utils.ApplyTriggerDeadZone(rawState.Gamepad.bLeftTrigger, deadZone),
                Utils.ApplyTriggerDeadZone(rawState.Gamepad.bRightTrigger, deadZone)
            );
        }

        public uint PacketNumber
        {
            get { return packetNumber; }
        }

        public bool IsConnected
        {
            get { return isConnected; }
        }

        public GamePadButtons Buttons
        {
            get { return buttons; }
        }

        public GamePadDPad DPad
        {
            get { return dPad; }
        }

        public GamePadTriggers Triggers
        {
            get { return triggers; }
        }

        public GamePadThumbSticks ThumbSticks
        {
            get { return thumbSticks; }
        }
    }

    public enum PlayerIndex
    {
        One = 0,
        Two = 1,
        Three = 2,
        Four = 3
    }

    public enum GamePadDeadZone
    {
        Circular,
        IndependentAxes,
        None
    }

    public class GamePad
    {
        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            return GetState(playerIndex, GamePadDeadZone.IndependentAxes);
        }

        public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZone)
        {
            GamePadState.RawState state;
            uint result = Imports.XInputGamePadGetState((uint)playerIndex, out state);
            return new GamePadState(result == Utils.Success, state, deadZone);
        }

        public static void SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {
            Imports.XInputGamePadSetState((uint)playerIndex, leftMotor, rightMotor);
        }
    }
}