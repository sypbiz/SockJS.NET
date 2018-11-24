using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Lib
{
    internal class Polling : EventEmitter
    {
        public delegate IReceiver ReceiverFactory(string url, AjaxObjectFactory ajaxObject);
        
        private readonly ReceiverFactory _receiver;
        private readonly string _receiveUrl;
        private readonly AjaxObjectFactory _ajaxObject;
        private IReceiver _poll;
        private bool _pollIsClosing;

        public Polling(ReceiverFactory receiver, string receiveUrl, AjaxObjectFactory ajaxObject)
        {
            Log.Debug($"{nameof(Polling)}: {receiveUrl}");
            this._receiver = receiver;
            this._receiveUrl = receiveUrl;
            this._ajaxObject = ajaxObject;
            this.ScheduleReceiver();
        }

        private void ScheduleReceiver()
        {
            Log.Debug($"{nameof(this.ScheduleReceiver)}");
            var poll = this._poll = this._receiver(this._receiveUrl, this._ajaxObject);
            poll.On("message", this.OnMessage);
            poll.Once("close", this.OnClose);
        }

        private void OnMessage(object sender, object[] args)
        {
            var msg = args[0] as string;
            Log.Debug($"{nameof(this.OnMessage)}: {msg}");
            this.Emit("message", msg);
        }

        private void OnClose(object sender, object[] args) 
        {
            var code = (int?) args[0];
            var reason = args[1] as string;
            Log.Debug($"{nameof(this.OnClose)}: Close {code} {reason} {this._pollIsClosing}");
            this._poll = null;
            if (!this._pollIsClosing)
            {
                if (reason == "network")
                {
                    this.ScheduleReceiver();
                }
                else
                {
                    this.Emit("close", code ?? 1006, reason);
                    this.RemoveAllListeners();
                }
            }
        }

        public void Abort()
        {
            Log.Debug($"{nameof(this.Abort)}");
            this.RemoveAllListeners();
            this._pollIsClosing = true;
            this._poll?.Abort();
        }
    }
}
