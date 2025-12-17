using Xunit;

namespace Gateway.Tests.Pipeline;

public class AclMiddlewareTests
{
    public class HealthEndpointTests : IClassFixture<GatewayFactory>
    {
        private readonly HttpClient _client;

        public HealthEndpointTests(GatewayFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Health_ReturnsOk()
        {
            var resp = await _client.GetAsync("/health");
            resp.EnsureSuccessStatusCode();
        }
    }

}