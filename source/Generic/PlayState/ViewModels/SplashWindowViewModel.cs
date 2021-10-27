using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayState.ViewModels
{
    class SplashWindowViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string gameName { get; set; }
        public string GameName
        {
            get => gameName;
            set
            {
                gameName = value;
                OnPropertyChanged();
            }
        }

        public string suspendStatus { get; set; }
        public string SuspendStatus
        {
            get => suspendStatus;
            set
            {
                suspendStatus = value;
                OnPropertyChanged();
            }
        }
    }
}
