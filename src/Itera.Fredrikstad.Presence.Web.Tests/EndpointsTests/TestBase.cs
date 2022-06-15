using Itera.Fredrikstad.Presence.Web.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;
using Ardalis.SmartEnum.SystemTextJson;
using Itera.Fredrikstad.Presence.Core;
using Microsoft.AspNetCore.Hosting;

namespace Itera.Fredrikstad.Presence.Web.Tests.EndpointsTests
{
    public class TestBase : IClassFixture<WebApplicationFactory<IApiMarker>>
    {
        protected readonly HttpClient Client;
        protected readonly JsonSerializerOptions JsonOptions = new();

        public TestBase(WebApplicationFactory<IApiMarker> factory)
        {
            JsonOptions.Converters.Add(new SmartEnumNameConverter<DayType, int>());
            JsonOptions.PropertyNameCaseInsensitive = true;

            Client = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTests");
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                });
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            //["someAppSetting"] = "someAppsettingValue"
                        }
                    );
                });
            })

            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }
    }
}