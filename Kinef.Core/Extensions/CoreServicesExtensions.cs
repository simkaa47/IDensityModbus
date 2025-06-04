using Kinef.Core.Services;
using Kinef.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Kinef.Core.Extensions;

public static class CoreServicesExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddLogging();
        services.AddSingleton<IdensityService>();
        return services;
    }
}