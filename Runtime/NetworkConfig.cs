namespace GameFoundation.Scripts.Network
{
    public class NetworkConfig
    {
        public string Host                    { get; set; } // URI của server web service
        public double HttpRequestTimeout      { get; set; } = 30; // Timeout mặc định cho các request HTTP
        public double DownloadRequestTimeout  { get; set; } = 600; // Timeout mặc định cho download
        public string BattleWebsocketUri      { get; set; } // URI của websocket service
        public int    MaximumRetryStatusCode0 { get; set; } = 5; // Số lần retry tối đa cho status code 0
        public float  RetryDelay              { get; set; } = 0.1f;
        public bool   AllowRetry              { get; set; } = true;
        public string ParamLink      = "&";
        public string ParamDelimiter = "?";

        public static string WrapFull     => "wrap";
        public static string WrapRequest  => "wrap_request";
        public static string WrapResponse => "wrap_response";
    }
}