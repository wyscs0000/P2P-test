using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

#if NETFX_CORE
using System.Threading.Tasks;
using System.Threading;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Foundation;
using System.Runtime.ExceptionServices;
#else
using System.Net.Sockets;
using System.Threading;
#endif


public class Endian
{
    public static int Switch(int nIn)
    {
        uint n = (uint)nIn;
        uint ret =
            n / 0x100 / 0x100 / 0x100 % 0x100 +
            n / 0x100 / 0x100 % 0x100 * 0x100 +
            n / 0x100 % 0x100 * 0x100 * 0x100 +
            n % 0x100 * 0x100 * 0x100 * 0x100;
        return (int)ret;
    }

    public static short Switch(short nIn)
    {
        ushort n = (ushort)nIn;
        int ret = n / 0x100 + n % 0x100 * 0x100;
        return (short)ret;
    }

    public static void WriteInt(byte[] des, int offset, int value)
    {
        des[offset + 0] = (byte)(value >> 24);
        des[offset + 1] = (byte)(value << 8 >> 24);
        des[offset + 2] = (byte)(value << 16 >> 24);
        des[offset + 3] = (byte)(value << 24 >> 24);
    }

    public static void WriteShort(byte[] des, int offset, short value)
    {
        des[offset + 0] = (byte)(value >> 8);
        des[offset + 1] = (byte)(value << 8 >> 8);
    }
}

#if NETFX_CORE
namespace System.Net.Sockets
{
    /// <summary>
    /// Single stream supporting reading and writing (used as part of TcpClient implementation).
    /// </summary>
//    public class ReadWriteStream : Stream
//    {
//        private readonly Stream readStream;
//        private readonly Stream writeStream;

//        public ReadWriteStream(Stream readStream, Stream writeStream)
//        {
//            this.readStream = readStream;
//            this.writeStream = writeStream;
//        }

//        public override bool CanRead { get { return readStream.CanRead; } }
//        public override bool CanSeek { get { return readStream.CanSeek; } }
//        public override bool CanTimeout { get { return readStream.CanTimeout && writeStream.CanTimeout; } }
//        public override bool CanWrite { get { return writeStream.CanWrite; } }

//        /// <summary>
//        /// Length (uses internal read stream).
//        /// </summary>
//        public override long Length { get { return readStream.Length; } }

//        /// <summary>
//        /// Position (uses internal read stream).
//        /// </summary>
//        public override long Position
//        {
//            get { return readStream.Position; }
//            set { readStream.Position = value; }
//        }

//        public override int ReadTimeout
//        {
//            get { return readStream.ReadTimeout; }
//            set { readStream.ReadTimeout = value; }
//        }
//        public override int WriteTimeout
//        {
//            get { return writeStream.WriteTimeout; }
//            set { writeStream.WriteTimeout = value; }
//        }
//#if NETFX_CORE
//        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
//        {
//            return readStream.CopyToAsync(destination, bufferSize, cancellationToken);
//        }

//        public override async Task FlushAsync(CancellationToken cancellationToken)
//        {
//            await readStream.FlushAsync(cancellationToken);
//            await writeStream.FlushAsync(cancellationToken);
//        }

//        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//        {
//            return readStream.ReadAsync(buffer, offset, count, cancellationToken);
//        }

//        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//        {
//            return writeStream.WriteAsync(buffer, offset, count, cancellationToken);
//        }
//#endif
//        public override int Read(byte[] buffer, int offset, int count)
//        {
//            return readStream.Read(buffer, offset, count);
//        }

//        public override void Flush()
//        {
//            readStream.Flush();
//            writeStream.Flush();
//        }

//        public override int ReadByte()
//        {
//            return readStream.ReadByte();
//        }

//        public override long Seek(long offset, SeekOrigin origin)
//        {
//            return readStream.Seek(offset, origin);
//        }

//        public override void SetLength(long value)
//        {
//            writeStream.SetLength(value);
//        }

//        public override void Write(byte[] buffer, int offset, int count)
//        {
//            writeStream.Write(buffer, offset, count);
//        }

//        public override void WriteByte(byte value)
//        {
//            writeStream.WriteByte(value);
//        }
//    }

    public class AsyncResult : IAsyncResult
    {
        public object AsyncState { get; set; }
        public System.Threading.WaitHandle AsyncWaitHandle { get { return null; } }
        public bool CompletedSynchronously { get { return false; } }
        public bool IsCompleted { get; set; }
    }

#if NETFX_CORE
    public class TaskStateAsyncResult : IAsyncResult
    {
        public readonly Task InnerTask;
        protected IAsyncResult taskAsAR;
        public object AsyncState { get; protected set; }
        public WaitHandle AsyncWaitHandle { get { return taskAsAR.AsyncWaitHandle; } }
        public bool CompletedSynchronously { get { return taskAsAR.CompletedSynchronously; } }
        public bool IsCompleted { get { return taskAsAR.IsCompleted; } }

        public int len;
        public TaskStateAsyncResult(Task task, object state)
        {
            InnerTask = task;
            taskAsAR = task;
            AsyncState = state;
        }
    }

    public class TaskStateAsyncResult<T> : TaskStateAsyncResult
    {
        public readonly new Task<T> InnerTask;

        public TaskStateAsyncResult(Task<T> task, object state)
            : base(task, state)
        {
            InnerTask = task;
            taskAsAR = task;
            AsyncState = state;
        }
    }
#endif

    /// <summary>
    /// MSDN reference: http://msdn.microsoft.com/en-us/library/system.net.sockets.tcpclient.aspx.
    /// </summary>
    public class Socket
    {
        public bool UsePlainSocket = true;

#if NETFX_CORE
        private StreamSocket _socket = null;
        DataWriter _writer;
        bool _isConnected = false;
        //ReadWriteStream _readWriteStream = null;


        private async Task EnsureSocket(string hostName, int port)
        {
            try
            {
                var host = new HostName(hostName);
                _socket = new StreamSocket();
                await _socket.ConnectAsync(host, port.ToString(), SocketProtectionLevel.PlainSocket);
                //_readWriteStream = new ReadWriteStream(_socket.InputStream.AsStreamForRead(), _socket.OutputStream.AsStreamForWrite());
                _isConnected = true;
            }
            catch (Exception ex)
            {
                Close();
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown)
                {
                    // TODO abort any retry attempts on Unity side
                    throw;
                }
            }
        }
#endif
        public bool Connected
        {
            get
            {
#if NETFX_CORE
                return _socket != null && _isConnected;
#else
                throw new NotImplementedException();
#endif
            }
        }

        public int SendTimeout { get; set; }
        public int ReceiveTimeout { get; set; }

        public void Connect(string hostName, int port)
        {
#if NETFX_CORE
            var thread = EnsureSocket(hostName, port);
            thread.Wait();
#else
            throw new PlatformNotSupportedException();
#endif
        }

        public IAsyncResult BeginConnect(string host, int port, AsyncCallback requestCallback, object state)
        {
#if NETFX_CORE

            var task = EnsureSocket(host, port);
            TaskStateAsyncResult res = new TaskStateAsyncResult(task, state);

            task.ContinueWith((t) =>
            {
                requestCallback(res);
            });

            return res;
#else
            throw new PlatformNotSupportedException();
#endif
        }

        public void EndConnect(IAsyncResult result)
        {
#if NETFX_CORE
            var ar = (TaskStateAsyncResult)result;
            if (ar.InnerTask.IsFaulted)
                throw ar.InnerTask.Exception;
            return;
#else
            throw new PlatformNotSupportedException();
#endif
        }

        public Stream GetStream()
        {
#if NETFX_CORE
            if (_socket == null || !_isConnected) return null;
            return null; //_readWriteStream;
#else
            throw new PlatformNotSupportedException();
#endif
        }

        public void Close()
        {
#if NETFX_CORE  
            _isConnected = false;
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
            //if (_readWriteStream != null)
            //{
            //    _readWriteStream.Dispose();
            //    _readWriteStream = null;
            //}
#else
            throw new PlatformNotSupportedException();
#endif
        }

        public void ReadFromInputStreamAsync(int size, Action<byte[]> successCallback, Action<Exception> failureCallback)
        {
#if NETFX_CORE
            var task = ReadFromInputStreamAsyncInner(size);
            task.ContinueWith((t) =>
            {
                if (t.IsFaulted && failureCallback != null)
                    failureCallback(t.Exception);
                else if (!t.IsFaulted && successCallback != null)
                    successCallback(t.Result);
            });
            
#else
            throw new PlatformNotSupportedException("TcpClient.ReadFromInputStream");
#endif
        }

#if NETFX_CORE
        //BeginReceive (msgcls.byteLinShi,0,msgcls.byteLinShi.Length,0,new AsyncCallback(ReceiveSorket),msgcls)
        public IAsyncResult BeginReceive(byte[] buf, int index, int len, int nouse, AsyncCallback requestCallback, object state)
        {
#if NETFX_CORE

            var task = ReadFromInputStreamAsyncInner(len);

            TaskStateAsyncResult res = new TaskStateAsyncResult(task, state);

            task.ContinueWith((t) =>
            {
                if (t.Result == null)
                    return;

                t.Result.CopyTo(buf, index);

                res.len = t.Result.Length;
                requestCallback(res);
            });

            return res;
#else
            throw new PlatformNotSupportedException();
#endif
        }

        public int EndReceive(IAsyncResult ret)
        {
            TaskStateAsyncResult res = (TaskStateAsyncResult)(ret);
            return res.len;
        }

        public IAsyncResult BeginSend(byte[] buf, int index, int len, int nouse, AsyncCallback requestCallback, object state)
        {
#if NETFX_CORE
            byte[] sendbuf = new byte[len];
            Array.Copy(buf, index, sendbuf, 0, len);
            var task = WriteToOutputStreamAsyncInner(sendbuf);

            TaskStateAsyncResult res = new TaskStateAsyncResult(task, state);

            task.ContinueWith((t) =>
            {
                res.len = len;
                requestCallback(res);
            });

            return res;
#else
            throw new PlatformNotSupportedException();
#endif
        }

        public int EndSend(IAsyncResult ret)
        {
            TaskStateAsyncResult res = (TaskStateAsyncResult)(ret);
            return res.len;
        }

        private async Task<byte[]> ReadFromInputStreamAsyncInner(int size)
        {
            if (_socket == null) return null;
            try
            {
                using (DataReader reader = new DataReader(_socket.InputStream))
                {
                    reader.InputStreamOptions = InputStreamOptions.Partial;
                    var count = await reader.LoadAsync((uint)size);
                    byte[] bytesRead = null;
                    if (count > 0)
                    {
                        bytesRead = new byte[count];
                        reader.ReadBytes(bytesRead);
                    }
                    reader.DetachStream();

                    return bytesRead;
                }
            }
            catch(Exception e)
            {
                return null;
            }
        }
#endif

        public void WriteToOutputStream(byte[] bytes)
        {
#if NETFX_CORE
            var thread = WriteToOutputStreamAsyncInner(bytes);
            thread.Wait();
#else
            throw new PlatformNotSupportedException();
#endif
        }

        public void WriteToOutputStreamAsync(byte[] bytes, Action successCallback, Action<Exception> failureCallback)
        {
#if NETFX_CORE
            var task = WriteToOutputStreamAsyncInner(bytes);
            task.ContinueWith((t) =>
            {
                if (t.IsFaulted && failureCallback != null)
                    failureCallback(t.Exception);
                else if (!t.IsFaulted && successCallback != null)
                    successCallback();
            });
#else
            throw new PlatformNotSupportedException();
#endif
        }

#if NETFX_CORE
        private async Task WriteToOutputStreamAsyncInner(byte[] bytes)
        {
            if (_socket == null) return;

            if (_writer == null)
                _writer = new DataWriter(_socket.OutputStream);

            _writer.WriteBytes(bytes);

            try
            {
                await _writer.StoreAsync();
                await _socket.OutputStream.FlushAsync();

                //_writer.DetachStream();
                //_writer.Dispose();
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error if fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    // TODO abort any retry attempts on Unity side
                    throw;
                }
            }
        }
#endif

    }
}
#endif