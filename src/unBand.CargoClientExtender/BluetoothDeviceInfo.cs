using Microsoft.Band;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Devices.Enumeration;

namespace unBand.CargoClientExtender
{
    public class BluetoothDeviceInfo : IBandInfo
    {
        public DeviceInformation Device { get; private set;}

        public BandConnectionType ConnectionType {get; private set; }

        public string Name { get; private set; }

        public BluetoothDeviceInfo(Guid serviceGuid, DeviceInformation device)
        {
            ConnectionType = BandConnectionType.Bluetooth;

            Device = device;
        }

    }
}
