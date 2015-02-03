using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using System.IO; // gets us nifty extension methods like AsStreamForRead/Write

namespace unBand.CargoClientExtender
{
    public class BluetoothDeviceTransport : IDeviceTransport, IDisposable
    {
        public event EventHandler Disconnected; 
        
        private bool _disposed;

        public CargoStreamReader CargoReader {get; private set;}
        public Stream CargoStream {get; private set;}
        public CargoStreamWriter CargoWriter {get; private set; }
        public bool IsConnected {get; private set;}
        public int MaxDataPayloadSize {get; private set;}
        public TransportProtocol TransportProtocol {get; private set;}
    
        private BluetoothDeviceInfo _deviceInfo;
        public BluetoothDeviceTransport(BluetoothDeviceInfo deviceInfo)
        {
            _deviceInfo = deviceInfo;

            Connect();
        }

        public void Connect(ushort maxConnectAttempts = 1)
        {
            ConnectAsync().Wait();
        }

        public async Task ConnectAsync(ushort maxConnectAttempts = 1)
        {
            System.Diagnostics.Debug.WriteLine("BluetoothDeviceTransport::Connect() called");

            var deviceService = await RfcommDeviceService.FromIdAsync(_deviceInfo.Device.Id);

            if (deviceService == null)
            {
                throw new Exception("Failed to create RfcommDeviceService with id: " + _deviceInfo.Id.ToString());
            }

            var streamSocket = new StreamSocket();

            await streamSocket.ConnectAsync(deviceService.ConnectionHostName, deviceService.ConnectionServiceName);
            
            CargoStream = new BluetoothStreamWrapper(streamSocket);

            // we could have used streamSocket.Input/OutputStream.AsStreamForRead/Write but they don't support timeouts
            // and it turns out those are super important if you don't want to lock up constantly on random calls (like
            // trying to get the background when there isn't one :( ).
            CargoReader = new CargoStreamReader(CargoStream);
            CargoWriter = new CargoStreamWriter(CargoStream);
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void WriteCommandPacket(CargoCommand packetHeader, byte[] argBuf, bool flush)
        {
            // Note: turns out argBuf can be null
            var bufLength = (argBuf == null ? 0 : argBuf.Length);
            
            this.CargoWriter.WriteByte((byte)((System.Runtime.InteropServices.Marshal.SizeOf(packetHeader)) + bufLength));

            this.CargoWriter.WriteStruct<CargoCommand>(ref packetHeader);

            if (bufLength > 0)
            {
                this.CargoWriter.Write(argBuf);
            }

            if (flush)
            {
                this.CargoWriter.Flush();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Disconnect();
            }
        }
    }
}
