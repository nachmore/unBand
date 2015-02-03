using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Devices.Enumeration;

namespace unBand.CargoClientExtender
{
    public class BluetoothDeviceInfo : DeviceInfo
    {
        public DeviceInformation Device { get; private set;}

        public BluetoothDeviceInfo(Guid serviceGuid, DeviceInformation device) : base(device.Name, serviceGuid, TransportProtocol.Bluetooth)
        {
            Device = device;
        }
    }
}
