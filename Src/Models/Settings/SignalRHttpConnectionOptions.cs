namespace Ccf.Ck.Models.Settings
{
    public class SignalRHttpConnectionOptions
    {
        public long ApplicationMaxBufferSize { get; set; } = 65000; //The default value is 65KB.
        public long TransportMaxBufferSize { get; set; } = 65000; //The default value is 65KB.
    }
}
