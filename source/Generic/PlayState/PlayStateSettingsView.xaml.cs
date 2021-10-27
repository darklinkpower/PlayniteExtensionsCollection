using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlayState
{
    public partial class PlayStateSettingsView : UserControl
    {
        public PlayStateSettingsView()
        {
            InitializeComponent();
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            var modifierKeys = new List<Key>
            {
                Key.LeftCtrl,
                Key.RightCtrl,
                Key.LeftShift,
                Key.RightShift,
                Key.LeftAlt,
                Key.RightAlt,
                Key.LWin,
                Key.RWin
            };

            if (modifierKeys.Contains(e.Key) || Keyboard.Modifiers == ModifierKeys.None)
            {
                return;
            }

            try
            {
                var keyGesture = new KeyGesture(e.Key, Keyboard.Modifiers);
                var converter = new KeyGestureConverter();
                //Settings.GestureString = converter.ConvertToInvariantString(keyGesture);
            }
            catch (Exception)
            {

            }
        }
    }
}