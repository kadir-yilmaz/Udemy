namespace Udemy.Basket.API.Options
{
    public class RedisOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSentinel { get; set; }
        public string? ServiceName { get; set; }
        public string? SentinelHosts { get; set; }
        public int SentinelPort { get; set; } = 26379;
    }
}
