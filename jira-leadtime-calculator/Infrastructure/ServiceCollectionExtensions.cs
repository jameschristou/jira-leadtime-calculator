using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace jira_leadtime_calculator.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfig<T>(this IServiceCollection services, IConfiguration configuration) where T : class, new()
        {
            var config = new T();
            configuration.Bind(config);
            return services.AddSingleton(config);
        }

        public static IServiceCollection AddConfig<T>(this IServiceCollection services, IConfiguration configuration, string configurationKey) where T : class, new()
        {
            var config = new T();
            configuration.GetSection(configurationKey).Bind(config);

            return services.AddSingleton(config);
        }
    }
}
