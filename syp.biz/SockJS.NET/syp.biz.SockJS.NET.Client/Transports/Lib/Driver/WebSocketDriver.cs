using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Common.Extensions;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Lib.Driver
{
    internal class WebSocketDriver : EventEmitter, IDisposable
    {
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly Uri _url;
        private readonly ClientWebSocket _socket;

        public WebSocketDriver(Uri url, string[] protocols, ITransportOptions options)
        {
            this._url = url;
            this._socket = new ClientWebSocket();
            this.Connect().Wait();
        }

        public static bool IsSupported { get; } = CheckIfWebSocketIsSupported();

        private static bool CheckIfWebSocketIsSupported()
        {
            Log.Debug(nameof(CheckIfWebSocketIsSupported));
            try
            {
                using (var socket = new ClientWebSocket()) return true;
            }
            catch (PlatformNotSupportedException ex)
            {
                Log.Debug($"{nameof(CheckIfWebSocketIsSupported)}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(CheckIfWebSocketIsSupported)}: {ex}");
                throw;
            }
        }

        private async Task Connect()
        {
            try
            {
                Log.Debug(nameof(this.Connect));
                await this._socket.ConnectAsync(this._url, this._cancel.Token);
                Task.Factory.StartNew(this.ReceiveLoop, this._cancel.Token, TaskCreationOptions.LongRunning).IgnoreAwait();
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(this.Connect)}: {ex}");
                throw;
            }
        }

        private async void ReceiveLoop(object obj)
        {
            try
            {
                Log.Debug(nameof(this.ReceiveLoop));
                while (!this._cancel.IsCancellationRequested && this._socket.State == WebSocketState.Open)
                {
                    var builder = new StringBuilder();
                    var buffer = new byte[1024];
                    var segment = new ArraySegment<byte>(buffer);
                    var result = await this._socket.ReceiveAsync(segment, this._cancel.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        this.Emit("close", 1000, "Server sent close message");
                        break;
                    }
                    var data = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    builder.Append(data);
                    if (!result.EndOfMessage) continue;
                    var message = builder.ToString();
                    builder = new StringBuilder();
                    this.Emit("message", message);
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"{nameof(this.ReceiveLoop)}: {ex}");
            }
        }

        public async Task Send(string msg)
        {
            Log.Debug($"{nameof(this.Send)}: {msg}");
            try
            {
                var buffer = Encoding.UTF8.GetBytes(msg);
                var segment = new ArraySegment<byte>(buffer);
                await this._socket.SendAsync(segment, WebSocketMessageType.Text, true, this._cancel.Token);
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(this.Send)}: {ex}");
            }
        }

        public async Task Close()
        {
            Log.Debug(nameof(this.Close));
            await this.Close(WebSocketCloseStatus.NormalClosure, string.Empty);
        }

        private async Task Close(WebSocketCloseStatus status, string reason)
        {
            Log.Debug($"{nameof(this.Close)}: {status} {reason}");
            try
            {
                switch (this._socket.State)
                {
                    case WebSocketState.Aborted:
                    case WebSocketState.Closed:
                    case WebSocketState.CloseReceived:
                    case WebSocketState.CloseSent:
                    case WebSocketState.None:
                        return;
                }
                await this._socket.CloseAsync(status, reason, this._cancel.Token);
                this._cancel.Cancel();
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(this.Close)}: {ex}");
            }
        }

        public void Dispose()
        {
            this._cancel.Cancel();
            this._cancel.Dispose();
            this._socket.Dispose();
        }
    }
}
