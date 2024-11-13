using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebViewCore.Domain.Events
{
    public class AddressChangedEventArgs : EventArgs
    {
        public string OldAddress { get; }
        public string NewAddress { get; }

        public AddressChangedEventArgs(string oldAddress, string newAddress)
        {
            OldAddress = oldAddress;
            NewAddress = newAddress;
        }
    }
}
