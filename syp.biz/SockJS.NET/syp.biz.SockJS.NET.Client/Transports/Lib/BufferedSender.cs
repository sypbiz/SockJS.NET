using System;
using System.Collections.Generic;
using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Client.Polyfills;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Lib
{
    internal abstract class BufferedSender : EventEmitter, ITransport
    {
        protected delegate Action SenderDelegate(string url, string payload, Action<Exception> callback);

        private readonly Queue<string> _sendBuffer = new Queue<string>();
        private readonly SenderDelegate _sender;
        private readonly string _url;
        private Action _sendStop;

        protected BufferedSender(string url, SenderDelegate sender)
        {
            Log.Debug($"{nameof(BufferedSender)}: {url}");
            this._sender = sender;
            this._url = url;
        }

        protected BufferedSender() { }

        public abstract string TransportName { get; }

        public void Send(string message)
        {
            Log.Debug($"{nameof(this.Send)}: Send {message}");
            this._sendBuffer.Enqueue(message);
            if (this._sendStop is null) this.SendSchedule();
        }

        // For polling transports in a situation when in the message callback,
        // new message is being send. If the sending connection was started
        // before receiving one, it is possible to saturate the network and
        // timeout due to the lack of receiving socket. To avoid that we delay
        // sending messages by some small time, in order to let receiving
        // connection be started beforehand. This is only a halfmeasure and
        // does not fix the big problem, but it does make the tests go more
        // stable on slow networks.
        private void SendScheduleWait()
        {
            Log.Debug(nameof(this.SendScheduleWait));
            int? tref = null;
            this._sendStop = () =>
            {
                Log.Debug("SendStop");
                this._sendStop = null;
                Timers.ClearInterval(tref);
            };

            tref = Timers.SetTimeout(() =>
            {
                Log.Debug("Timeout");
                this._sendStop = null;
                this.SendSchedule();
            }, 25);
        }

        private void SendSchedule()
        {
            Log.Debug($"{nameof(this.SendSchedule)}: {this._sendBuffer.Count}");
            if (this._sendBuffer.Count > 0)
            {
                var payload = $"[{string.Join(",", this._sendBuffer)}]";
                this._sendStop = this._sender(this._url, payload, err =>
                {
                    this._sendStop = null;
                    if (err != null)
                    {
                        Log.Debug($"{nameof(this.SendSchedule)}: Error {err}");
                        this.Emit("close", /* err.code || */ 1006, $"Sending error: {err}");
                        this.Close();
                    }
                    else
                    {
                        this.SendScheduleWait();
                    }
                });
                this._sendBuffer.Clear();
            }
        }

        private void Cleanup()
        {
            Log.Debug(nameof(this.Cleanup));
            this.RemoveAllListeners();
        }

        public virtual void Close()
        {
            Log.Debug(nameof(this.Close));
            this.Cleanup();
            if (this._sendStop is null) return;
            this._sendStop();
            this._sendStop = null;
        }
    }
}