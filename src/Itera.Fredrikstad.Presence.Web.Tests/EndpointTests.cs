using Itera.Fredrikstad.Presence.Web.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Itera.Fredrikstad.Presence.Web.Tests
{
    public class EndpointTests : IClassFixture<WebApplicationFactory<IApiMarker>>
    {
        private readonly WebApplicationFactory<IApiMarker> _factory;

        public EndpointTests(WebApplicationFactory<IApiMarker> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Test1()
        {
            //TODO: this test requires that " app.UseAuthorization(); " is commented out in program.cs
            var httpClient = _factory.CreateClient();
            var response = await httpClient.GetAsync("api/daySummary?date=06.08.2022");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);

        }
    }
}