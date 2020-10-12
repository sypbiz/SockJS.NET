using System;
using System.Threading;
using System.Threading.Tasks;
using syp.biz.SockJS.NET.Client2.Interfaces;

namespace syp.biz.SockJS.NET.Client2.Implementations.Transports
{
    internal class WebSocketTransportFactory : ITransportFactory
    {
        #region Implementation of ITransportFactory
        public string Name => "websocket";
        public bool Enabled { get; set; } = true;
        public uint Priority { get; set; } = 100;
        public Task<ITransport> Build(ITransportConfiguration config)
        {
            config.Logger.Debug($"{nameof(Build)}: '{this.Name}' transport");
            var transport = new WebSocketTransport(config);
            return Task.FromResult<ITransport>(transport);
        }
        #endregion Implementation of ITransportFactory
    }

    internal class WebSocketTransport : ITransport
    {
        private readonly ITransportConfiguration _config;
        private readonly ILogger _log;

        public WebSocketTransport(ITransportConfiguration config)
        {
            this._config = config;
            this._log = config.Logger;
        }

        #region Implementation of IDisposable
        public void Dispose() => throw new NotImplementedException();
        #endregion Implementation of IDisposable

        #region Implementation of ITransport
        public event EventHandler<string>? Message;

        public string Name => "websocket";
        
        public async Task Connect(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Connect() => this.Connect(CancellationToken.None);

        public async Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task Send(string data) => this.Send(data, CancellationToken.None);

        public async Task Send(string data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        #endregion Implementation of ITransport
    }
}
