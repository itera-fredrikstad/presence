using Itera.Fredrikstad.Presence.Web.Api;
using Itera.Fredrikstad.Presence.Web.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Xunit;
using FluentAssertions;

namespace Itera.Fredrikstad.Presence.Web.Tests.EndpointsTests
{
    public class GetUserShould : TestBase
    {
        public GetUserShould(WebApplicationFactory<IApiMarker> factory) : base(factory)
        {

        }

        [Fact]
        public async Task ReturnUser_WithUserIdSetTo_ClaimOfTypeName()
        {
            var response = await Client.GetAsync("api/user");

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<User>(result, JsonOptions);
            Console.WriteLine(result);
            user.UserId.Should().Be("Test user");
        }

    }
}