using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Common.Extensions;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Lib.Receiver
{
    internal class XhrReceiver : EventEmitter, IReceiver
    {
        private int _bufferPosition;
        private IAjaxObject _xo;

        private XhrReceiver(string url, AjaxObjectFactory ajaxObject)
        {
            Log.Debug($"{nameof(XhrReceiver)}: {url}");
            this._bufferPosition = 0;
            this._xo = ajaxObject("POST", url, null);
            this._xo.On("chunk", this.ChunkHandler);
            this._xo.Once("finish", this.OnFinish);
        }

        public static IReceiver Build(string url, AjaxObjectFactory ajaxObject)
        {
            var receiver = new XhrReceiver(url, ajaxObject);
            return receiver;
        }

        private void ChunkHandler(object sender, params object[] args)
        {
            var status = (int)args[0];
            var text = args[1] as string;

            Log.Debug($"{nameof(this.ChunkHandler)}: {status}");
            if (status != 200 || text.IsNullOrWhiteSpace()) return;

            for (var idx = -1; ; this._bufferPosition += idx + 1)
            {
                var buf = text.Substring(this._bufferPosition);
                idx = buf.IndexOf('\n');
                if (idx == -1) break;

                var msg = buf.Substring(0, idx);
                if (msg.IsNullOrEmpty()) continue;

                Log.Debug($"{nameof(this.ChunkHandler)}: {msg}");
                this.Emit("message", msg);
            }
        }

        private void Cleanup()
        {
            Log.Debug(nameof(this.Cleanup));
            this.RemoveAllListeners();
        }

        private void OnFinish(object sender, object[] args)
        {
            var status = (int)args[0];
            var text = args[1] as string;
            Log.Debug($"{nameof(this.OnFinish)}: Finish {status} {text}");
            this.ChunkHandler(this, status, text);
            this._xo = null;
            var reason = status == 200 ? "network" : "permanent";
            Log.Debug($"{nameof(this.OnFinish)}: Close {reason}");
            this.Emit("close", null, reason);
            this.Cleanup();
        }

        public void Abort()
        {
            Log.Debug(nameof(this.Abort));
            if (this._xo != null)
            {
                this._xo.Close();
                Log.Debug($"{nameof(this.Abort)}: Close");
                this.Emit("close", null, "user");
                this._xo = null;
            }
            this.Cleanup();
        }
    }
}
