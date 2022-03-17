using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchCollection.Models
{
    public class SearchDefinition : ObservableObject
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }

        private string icon;
        public string Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private string searchTemplate;
        public string SearchTemplate
        {
            get => searchTemplate;
            set
            {
                searchTemplate = value;
                OnPropertyChanged();
            }
        }

    }
}
