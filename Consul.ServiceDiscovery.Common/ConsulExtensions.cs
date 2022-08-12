using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace Consul.ServiceDiscovery.Common
{
    public static class ConsulExtensions
    {
        public static IServiceCollection AddConsul(this IServiceCollection serviceCollection)
        {
            IConfiguration configuration;
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }
            ConsulOptions consulConfigOptions = configuration.Get<ConsulOptions>(options => configuration.GetSection("Consul").Bind(options));

            serviceCollection.Configure<ConsulOptions>(configuration.GetSection("Consul"));
            serviceCollection.AddTransient<IConsulServices, ConsulServices>();
            serviceCollection.AddTransient<ConsulServiceDiscoveryMessageHandler>();
            serviceCollection.AddHttpClient<IConsulHttpClient, ConsulHttpClient>()
                .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>();

            var consulConfig = configuration.GetSection("Consul");
            var host = consulConfig.GetValue<string>("Host");

            return serviceCollection.AddSingleton<IConsulClient>(c => new ConsulClient(cfg =>
            {
                if (!string.IsNullOrEmpty(host))
                {
                    cfg.Address = new Uri(host);
                }
            }));
        }

        public static string UseConsul(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var Iconfig = scope.ServiceProvider.GetService<IConfiguration>();

                var consulConfig = app.ApplicationServices.GetRequiredService<IOptions<ConsulOptions>>();
                //var config = Iconfig.Get<ConsulOptions>(options => Iconfig.GetSection("Consul").Bind(options));
                var config = consulConfig.Value;

                if (!config.Enabled)
                    return String.Empty;

                Guid serviceId = Guid.NewGuid();
                string consulServiceID = $"{config.Service}:{serviceId}";

                var client = scope.ServiceProvider.GetService<IConsulClient>();

                // Get server IP address
                var features = app.Properties["server.Features"] as FeatureCollection;
                var addresses = features.Get<IServerAddressesFeature>();
                var address = addresses.Addresses.First();

                // Register service with consul
                var uri = new Uri(address);

                var consulServiceRistration = new AgentServiceRegistration
                {
                    Name = config.Service,
                    ID = consulServiceID,
                    Address = $"{uri.Scheme}://{uri.Host}",
                    Port = uri.Port,
                    //TODO : Add Tags Tags = fabioOptions.Value.Enabled ? GetFabioTags(serviceName, fabioOptions.Value.Service) : null
                };

                if (config.PingEnabled)
                {
                    var healthService = scope.ServiceProvider.GetService<HealthCheckService>();

                    if (healthService != null)
                    {
                        var scheme = address.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)
                       ? string.Empty
                       : "http://";
                        var check = new AgentServiceCheck
                        {
                            Interval = TimeSpan.FromSeconds(5),
                            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(10),
                            HTTP = $"{scheme}{address}/health"
                        };

                        consulServiceRistration.Checks = new[] { check };
                    }
                    else
                    {
                        throw new Exception("Please ensure that Healthchecks has been added before adding checks to Consul.");
                    }
                }

                client.Agent.ServiceRegister(consulServiceRistration);

                return consulServiceID;
            }
        }
    }
}
