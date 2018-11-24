namespace syp.biz.SockJS.NET.Client.Event
{
    public class CloseEvent: Event
    {
        public CloseEvent() : base("close")
        {
            this.InitEvent(this.Type, false, false);
            this.WasClean = false;
            this.Code = 0;
            this.Reason = string.Empty;
        }

        public bool WasClean { get; set; }
        public int Code { get; set; }
        public string Reason { get; set; }
    }
}
