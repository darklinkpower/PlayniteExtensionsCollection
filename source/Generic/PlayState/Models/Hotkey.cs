using System;
using System.Windows.Input;

namespace PlayState.Models
{
    // Obtained from https://github.com/felixkmh/QuickSearch-for-Playnite
    public class Hotkey : IEquatable<Hotkey>
    {
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public Hotkey(Key key, ModifierKeys modifier)
        {
            this.Key = key;
            this.Modifiers = modifier;
        }

        public bool Equals(Hotkey other)
        {
            return Key == other.Key && Modifiers == other.Modifiers;
        }

        public override string ToString()
        {
            return $"{Modifiers} + {Key}";
        }
    }
}
