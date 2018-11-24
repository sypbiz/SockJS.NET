namespace syp.biz.SockJS.NET.Client.Event
{
    public sealed class TransportMessageEvent : Event
    {
        public TransportMessageEvent(Newtonsoft.Json.Linq.JToken data) : base("message")
        {
            this.InitEvent(this.Type, false, false);
            this.Data = data;
        }

        public Newtonsoft.Json.Linq.JToken Data { get; }
    }
}
