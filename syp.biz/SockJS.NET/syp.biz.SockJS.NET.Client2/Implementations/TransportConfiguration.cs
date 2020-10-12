using System;
using syp.biz.SockJS.NET.Client2.Interfaces;
using syp.biz.SockJS.NET.Common.DTO;

namespace syp.biz.SockJS.NET.Client2.Implementations
{
    internal class TransportConfiguration : ITransportConfiguration
    {
        private readonly SockJsConfiguration.Factory.ReadOnlySockJsConfiguration _config;

        public TransportConfiguration(SockJsConfiguration.Factory.ReadOnlySockJsConfiguration config, InfoDto info)
        {
            this.Info = info;
            this._config = config;
        }

        #region Implementation of ITransportConfiguration
        public Uri BaseEndpoint => this._config.BaseEndpoint;
        public ILogger Logger => this._config.Logger;
        public InfoDto Info { get; }
        #endregion Implementation of ITransportConfiguration
    }
}
