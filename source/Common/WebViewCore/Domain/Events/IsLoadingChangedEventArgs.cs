using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebViewCore.Domain.Events
{
    public class IsLoadingChangedEventArgs : EventArgs
    {
        public bool OldIsLoading { get; }
        public bool NewIsLoading { get; }

        public IsLoadingChangedEventArgs(bool oldIsLoading, bool newIsLoading)
        {
            OldIsLoading = oldIsLoading;
            NewIsLoading = newIsLoading;
        }
    }
}