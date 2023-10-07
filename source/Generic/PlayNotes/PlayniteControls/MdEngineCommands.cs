using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlayNotes.MdEngineCommands
{
    public class HyperlinkCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is string url && !url.IsNullOrWhiteSpace())
            {
                ProcessStarter.StartUrl(url);
            }
        }
    }
}