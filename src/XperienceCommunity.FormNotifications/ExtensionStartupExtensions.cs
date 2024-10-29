using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.FormNotifications.Services;

namespace XperienceCommunity.FormNotifications;

public static class ExtensionStartupExtensions
{
    /// <summary>
    /// Adds page type restrictions extension dependencies
    /// </summary>
    /// <param name="serviceCollection">the <see cref="IServiceCollection"/> which will be modified</param>
    /// <returns>Returns this instance of <see cref="IServiceCollection"/>, allowing for further configuration in a fluent manner.</returns>
    public static IServiceCollection AddFormNotificationsExtensionServices(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<ExtensionModuleInstaller>();
        serviceCollection
            .AddTransient<IFormNotificationEmailService, FormNotificationEmailService>();

        return serviceCollection;
    }
}