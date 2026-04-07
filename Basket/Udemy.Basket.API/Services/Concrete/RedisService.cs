using StackExchange.Redis;
using Udemy.Basket.API.Options;

namespace Udemy.Basket.API.Services.Concrete
{
    public class RedisService
    {
        private readonly RedisOptions _options;
        private ConnectionMultiplexer _connectionMultiplexer;

        public RedisService(RedisOptions options)
        {
            _options = options;
        }

        public void Connect()
        {
            if (_options.UseSentinel && !string.IsNullOrWhiteSpace(_options.ServiceName) && !string.IsNullOrWhiteSpace(_options.SentinelHosts))
            {
                var sentinelOptions = new ConfigurationOptions
                {
                    ServiceName = _options.ServiceName,
                    AbortOnConnectFail = false,
                    ConnectRetry = 5,
                    ConnectTimeout = 5000
                };

                foreach (var host in _options.SentinelHosts.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    sentinelOptions.EndPoints.Add(host, _options.SentinelPort);
                }

                _connectionMultiplexer = ConnectionMultiplexer.Connect(sentinelOptions);
                return;
            }

            var options = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                ConnectRetry = 5,
                ConnectTimeout = 5000
            };
            options.EndPoints.Add(_options.Host, _options.Port);

            _connectionMultiplexer = ConnectionMultiplexer.Connect(options);
        }

        public IDatabase GetDb(int db = 0) => _connectionMultiplexer.GetDatabase(db);
    }
}
