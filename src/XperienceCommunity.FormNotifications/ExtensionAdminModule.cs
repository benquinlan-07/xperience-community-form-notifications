using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailLibrary;
using CMS.EmailLibrary.Internal;
using CMS.OnlineForms;
using Kentico.Xperience.Admin.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using XperienceCommunity.FormNotifications;
using XperienceCommunity.FormNotifications.EventHandlers;
using XperienceCommunity.FormNotifications.Filters;

[assembly: CMS.AssemblyDiscoverable]
[assembly: CMS.RegisterModule(typeof(ExtensionAdminModule))]

namespace XperienceCommunity.FormNotifications;

internal class ExtensionAdminModule : AdminModule
{
    private ExtensionModuleInstaller? _installer;

    public ExtensionAdminModule()
        : base(Constants.ModuleName)
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        // Register the module installer
        var services = parameters.Services;
        _installer = services.GetRequiredService<ExtensionModuleInstaller>();

        // Add the URL resolver for the email urls
        var resolver = new UrlResolverEmailContentFilter();
        EmailContentFilterRegister.Instance.Register(() => resolver, EmailContentFilterType.Sending, 1000);

        // Add the event handler to run the install process on initialised event
        ApplicationEvents.Initialized.Execute += InitializeModule;
    }

    // OnPreInit allows you to access IServiceCollection via ModulePreInitParameters
    protected override void OnPreInit(ModulePreInitParameters parameters)
    {
        base.OnPreInit(parameters);

        // Registers an object event handler
        parameters.Services.AddInfoObjectEventHandler<InfoObjectAfterInsertEvent<BizFormItem>, BizFormItemAfterInsertHandler>();
    }

    private void InitializeModule(object? sender, EventArgs e) =>
        _installer?.Install();
}