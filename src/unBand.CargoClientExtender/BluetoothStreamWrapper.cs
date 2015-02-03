using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace unBand.CargoClientExtender
{
    /// <summary>
    /// Small StreamSocket wrapper (really around Stream, but implemented on top of a StreamSocket)
    /// that implemented Read/Write Timeouts
    /// </summary>
    internal class BluetoothStreamWrapper : Stream
    {
        const int DEFAULT_WRITE_TIMEOUT = 1000;
        const int DEFAULT_READ_TIMEOUT = 1000;

        private StreamSocket _streamSocket;
        private Stream _readStream;
        private Stream _writeStream;

        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override bool CanTimeout { get { return true; } }

        public override int ReadTimeout { get; set; }
        public override int WriteTimeout { get; set; }

        public override long Length { get { throw new NotImplementedException(); } }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public BluetoothStreamWrapper(StreamSocket streamSocket)
        {
            _streamSocket = streamSocket;

            _readStream = streamSocket.InputStream.AsStreamForRead(0);
            _writeStream = streamSocket.OutputStream.AsStreamForWrite(0);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var timeout = (ReadTimeout < 1 ? DEFAULT_READ_TIMEOUT : ReadTimeout);

            var cancellationToken = new CancellationTokenSource(timeout).Token;

            try
            {
                //NOTE: if we read past the end of the stream the BT connection will be cancelled!
                //      this can happen if you (for example) request the Background and none is set

                var readTask = _readStream.ReadAsync(buffer, offset, count, cancellationToken);
                readTask.Wait();  // can't just use Read() since it doesn't support cancellation

                return readTask.Result;
            }
            catch
            {
                // TODO: exception / log
                throw new TimeoutException();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _writeStream.WriteAsync(buffer, offset, count, GetWriteCancellationToken()).Wait();
        }

        public override void Flush()
        {
            _writeStream.FlushAsync(GetWriteCancellationToken()).Wait();
        }

        private CancellationToken GetWriteCancellationToken()
        {
            var timeout = (WriteTimeout < 1 ? DEFAULT_WRITE_TIMEOUT : WriteTimeout);

            return new CancellationTokenSource(timeout).Token;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
