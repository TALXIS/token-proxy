
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TokenProxy.API;
using TokenProxy.API.Interfaces;
using TokenProxy.API.Options;
using TokenProxy.API.Services;

[assembly: FunctionsStartup(typeof(Startup))]
namespace TokenProxy.API
{
    public class Startup : FunctionsStartup
    {
        private static IConfiguration GetConfiguration(IFunctionsHostBuilder builder) =>
            builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // options, settings
            var config = GetConfiguration(builder);
            builder.Services
                .AddOptions<ApiOptions>()
                .Configure<IConfiguration>((options, configuration) => configuration.Bind(options))
                .ValidateDataAnnotations();
            builder.Services
                .AddOptions<oAuth2Options>()
                .Configure<IConfiguration>((options, configuration) => configuration.Bind(options))
                .ValidateDataAnnotations();

            // Http
            builder.Services.AddHttpClient();

            // token service
            builder.Services
                .AddHttpClient<ITokenService, TokenService>();

            // cache
            builder.Services.AddMemoryCache();
            builder.Services.Decorate<ITokenService, TokenServiceMemoryCache>();
        }
    }
}
