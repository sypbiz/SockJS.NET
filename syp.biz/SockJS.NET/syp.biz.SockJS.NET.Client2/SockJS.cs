using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using syp.biz.SockJS.NET.Client.Implementations;
using syp.biz.SockJS.NET.Client.Interfaces;
using syp.biz.SockJS.NET.Common.DTO;

namespace syp.biz.SockJS.NET.Client
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SockJS : IClient
    {
        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);
        private readonly SockJsConfiguration.Factory.ReadOnlySockJsConfiguration _config;
        private readonly ILogger _log;
        private ITransport? _transport;
        private ConnectionState _state = ConnectionState.Initial;

        public SockJS(SockJsConfiguration? config = default)
        {
            this._config = (config ?? SockJsConfiguration.Factory.BuildDefault()).AsReadonly();
            this._log = this._config.Logger;
        }

        #region Implementation of IDisposable
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion Implementation of IDisposable

        #region Implementation of IClient
        public event EventHandler? Connected;
        public event EventHandler? Disconnected;
        public event EventHandler<string>? Message;

        public ConnectionState State
        {
            get => this._state;
            private set
            {
                var current = this._state;
                if (current == value) return;
                this._log.Debug($"{nameof(this.State)}: {current} -> {value}");
                this._state = value;
            }
        }

        public async Task Connect(CancellationToken token)
        {
            this._log.Info(nameof(Connect));
            try
            {
                await this._sync.WaitAsync(token);
                if (this.State != ConnectionState.Initial) throw new Exception($"Cannot connect while state is '{this.State}'");
                this.State = ConnectionState.Connecting;

                var info = await new InfoReceiver(this._config).GetInfo();

                ITransport? selectedTransport = null;
                var factories = this._config.TransportFactories.Where(t => t.Enabled).ToArray();
                this._log.Debug($"{nameof(Connect)}: Transports: {factories.Length}/{this._config.TransportFactories.Count} (enabled/total)");

                foreach (var factory in factories)
                {
                    selectedTransport = await this.TryTransport(factory, info, token);
                    if (selectedTransport is null) continue;
                    break;
                }

                this._transport = selectedTransport ?? throw new Exception("No available transports");
                this._transport.Message += this.TransportOnMessage;
                this._transport.Disconnected += this.TransportOnDisconnected;
                this.State = ConnectionState.Established;
                this.Connected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                this._log.Error($"{nameof(Connect)}: {e.Message}");
                this._log.Debug($"{nameof(Connect)}: {e}");
                this.State = ConnectionState.Error;
                throw;
            }
            finally
            {
                this._sync.Release();
            }
        }

        public Task Connect() => this.Connect(CancellationToken.None);

        public async Task Disconnect()
        {
            this._log.Info(nameof(this.Disconnect));
            this.VerifyEstablished();
            await this._transport!.Disconnect();
        }

        public Task Send(string data) => this.Send(data, CancellationToken.None);

        public async Task Send(string data, CancellationToken token)
        {
            this.VerifyEstablished();
            await this._transport!.Send(data, token);
        }

        #endregion Implementation of IClient

        private async Task<ITransport?> TryTransport(
            ITransportFactory factory,
            InfoDto info,
            CancellationToken token)
        {
            try
            {
                this._log.Debug($"{nameof(this.TryTransport)}: {factory.Name}");
                var transport = await factory.Build(new TransportConfiguration(this._config, info));
                await transport.Connect(token);
                this._log.Info($"{nameof(this.TryTransport)}: {factory.Name} - Success");
                return transport;
            }
            catch (Exception e)
            {
                this._log.Error($"{nameof(this.TryTransport)}: {factory.Name} - Failed: {e.Message}");
                this._log.Error($"{nameof(this.TryTransport)}: {e}");
                return null;
            }
        }

        private void TransportOnMessage(object sender, string message)
        {
            this._log.Debug($"{nameof(this.TransportOnMessage)}: {message}");
            this.Message?.Invoke(this, message);
        }

        private void TransportOnDisconnected(object sender, EventArgs e)
        {
            this._log.Debug($"{nameof(this.TransportOnDisconnected)}");
            this.Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private void VerifyEstablished()
        {
            if (this._transport is null || this.State != ConnectionState.Established)
                throw new InvalidOperationException("Connection not established");
        }
    }
}