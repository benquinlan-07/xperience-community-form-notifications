using CMS.Core;
using CMS.EmailLibrary;
using CMS.EmailLibrary.Internal;
using CMS.Helpers;
using CMS.Helpers.Internal;
using CMS.Websites.Internal;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using XperienceCommunity.FormNotifications.Models;

namespace XperienceCommunity.FormNotifications.Filters;

internal sealed class UrlResolverEmailContentFilter : IEmailContentFilter
{
    public async Task<string> Apply(string emailText, EmailConfigurationInfo email, IEmailDataContext dataContext)
    {
        if (dataContext is not NotificationFormAutoresponderEmailDataContext notificationDataContext || notificationDataContext.FormEmailTemplate == null)
            return emailText;

        var domain = await ResolveDomain(notificationDataContext.FormEmailTemplate.FormEmailTemplateServiceDomain);
        var baseUrl = Service.Resolve<IAbsoluteUrlBuilder>().BuildAbsoluteUrl(domain, "");
        return HTMLHelper.ResolveUrls(emailText, baseUrl);
    }

    // Service domain is optional; fall back to the current channel's primary domain, then the request domain.
    private static async Task<string> ResolveDomain(string serviceDomain)
    {
        if (!string.IsNullOrWhiteSpace(serviceDomain))
            return serviceDomain;

        var websiteChannelContext = Service.Resolve<IWebsiteChannelContext>();
        if (websiteChannelContext.WebsiteChannelID > 0)
        {
            try
            {
                return await Service.Resolve<IWebsiteChannelDomainProvider>().GetDomain(websiteChannelContext.WebsiteChannelID, CancellationToken.None);
            }
            catch (InvalidOperationException)
            {
                // Channel has no resolvable domain; fall back to the request domain below.
            }
        }

        return Service.Resolve<IHttpContextAccessor>().HttpContext?.Request.Host.Value ?? string.Empty;
    }
}
