using System;
using syp.biz.SockJS.NET.Common.Extensions;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Lib
{
    internal abstract class SenderReceiver : BufferedSender
    {
        private Polling _poll;

        protected SenderReceiver(string transUrl, string urlSuffix, SenderDelegate senderFunc, Polling.ReceiverFactory receiver, AjaxObjectFactory ajaxObject)
            : base(transUrl, senderFunc)
        {
            var pollUrl = new Uri(transUrl).AddPath(urlSuffix);
            Log.Debug($"{nameof(SenderReceiver)}: {pollUrl}");
            this._poll = new Polling(receiver, pollUrl.OriginalString, ajaxObject);
            this._poll.On("message", this.OnMessage);
            this._poll.Once("close", this.OnPollClose);
        }

        protected SenderReceiver() { }

        private void OnMessage(object sender, object[] args)
        {
            var msg = args[0] as string;
            Log.Debug($"{nameof(this.OnMessage)}: Poll message {msg}");
            this.Emit("message", msg);
        }

        private void OnPollClose(object sender, object[] args)
        {
            var code = (int)args[0];
            var reason = args[1] as string;
            Log.Debug($"{nameof(this.OnPollClose)}: Poll close {code} {reason}");
            this._poll = null;
            this.Emit("close", code, reason);
            this.Close();
        }

        public override void Close()
        {
            base.Close();
            Log.Debug($"{nameof(this.Close)}: Close");
            this.RemoveAllListeners();
            this._poll?.Abort();
            this._poll = null;
        }
    }
}