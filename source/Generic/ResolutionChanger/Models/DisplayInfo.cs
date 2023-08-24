using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolutionChanger.Models
{
    public class DisplayInfo : ObservableObject
    {
        public string DeviceName;
        public string DeviceString;
        public string DeviceID;
        public string DeviceKey;

        [DontSerialize]
        public string DisplayName => $"{DeviceString} - {DeviceName}";
    }
}