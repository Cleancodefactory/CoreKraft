namespace Ccf.Ck.Models.Settings
{
    public class SignalRHubOptions
    {
        public int ClientTimeoutInterval { get; set; } = 30; //The default timeout is 30 seconds.
        public int KeepAliveInterval { get; set; } = 15; //The default timeout is 30 seconds.
        public bool EnableDetailedErrors { get; set; } = false;
        public long? MaximumReceiveMessageSize { get; set; } = 32; //The default is 32KB.
        public int? StreamBufferCapacity { get; set; } = 10; //The default size is 10.
        public int HandshakeTimeout { get; set; } = 15; //The default timeout is 15 seconds.
    }
}
