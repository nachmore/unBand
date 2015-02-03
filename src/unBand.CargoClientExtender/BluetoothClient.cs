using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;

namespace unBand.CargoClientExtender
{
    public static class BluetoothClient
    {
        public static async Task<CargoClient> CreateAsync(DeviceInfo deviceInfo)
        {
            if (deviceInfo.Protocol != TransportProtocol.Bluetooth)
            {
                return await CargoClient.CreateAsync(deviceInfo);
            }

            // since constructors can't be async (and we can't use the builtin CreateAsync since it will
            // choke on Bluetooth) wrap the creation in a Task
            return await Task.Run<CargoClient>(() =>
            {
                var client = new CargoClient(new BluetoothDeviceTransport((BluetoothDeviceInfo)deviceInfo), null);

                // if we don't set this most actions will fail. I assume this causes weirdness if the Band is not in
                // App mode (perhaps during initial setup?), but in reality we always seem to be in App mode
                client.deviceTransportApp = RunningAppType.App;

                return client;
            });
        }

        public static async Task<DeviceInfo[]> GetConnectedDevicesAsync()
        {
            // In case the GUID ever stops working, you can grab the updated one from Device Manager (after pairing)
            return await Task.Run<DeviceInfo[]>(() => { return GetConnectedDevices(new Guid("{A502CA97-2BA5-413C-A4E0-13804E47B38F}")); });
        }

        private static DeviceInfo[] GetConnectedDevices(Guid serviceGuid)
        {
            var deviceSelector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.FromUuid(serviceGuid));
            var findAllAsync = DeviceInformation.FindAllAsync(deviceSelector);

            if (!findAllAsync.AsTask<DeviceInformationCollection>().Wait(1000))
            {
                findAllAsync.Cancel();

                // oh well, no devices
                return new DeviceInfo[]{};
            }

            var rv = findAllAsync.GetResults();

            List<BluetoothDeviceInfo> list = new List<BluetoothDeviceInfo>();

            foreach (var device in rv)
            {
                list.Add(new BluetoothDeviceInfo(serviceGuid, device));
            }

            return list.ToArray();
        }

    }
}
