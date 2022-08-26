using System;
using System.Windows.Input;

namespace PlayState.Models
{
    public class HotKey : IEquatable<HotKey>
    {
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public HotKey(Key key, ModifierKeys modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public bool Equals(HotKey other)
        {
            return Key == other.Key && Modifiers == other.Modifiers;
        }

        public override string ToString()
        {
            var str = string.Empty;
            if (Modifiers.HasFlag(ModifierKeys.Control))
            {
                str += "Ctrl + ";
            }

            if (Modifiers.HasFlag(ModifierKeys.Shift))
            {
                str += "Shift + ";
            }

            if (Modifiers.HasFlag(ModifierKeys.Alt))
            {
                str += "Alt + ";
            }

            if (Modifiers.HasFlag(ModifierKeys.Windows))
            {
                str += "Win + ";
            }

            return str += Key.ToString();
        }

    }
}