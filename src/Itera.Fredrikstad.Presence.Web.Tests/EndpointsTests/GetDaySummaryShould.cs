using Itera.Fredrikstad.Presence.Web.Api;
using Itera.Fredrikstad.Presence.Web.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Xunit;
using FluentAssertions;

namespace Itera.Fredrikstad.Presence.Web.Tests.EndpointsTests
{
    public class GetDaySummaryShould : TestBase
    {
        public GetDaySummaryShould(WebApplicationFactory<IApiMarker> factory) : base(factory)
        {
        }

        [Fact]
        public async Task ReturnDaySummary()
        {
            var response = await Client.GetAsync("api/daySummary?date=06.08.2022");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine(result);

            var daySummary = JsonSerializer.Deserialize<DaySummary>(result, JsonOptions);
            daySummary.Date.Date.Equals(new DateTime(2022, 08, 06));
            daySummary.Attendees.Should().NotBeEmpty();
        }
    }
}