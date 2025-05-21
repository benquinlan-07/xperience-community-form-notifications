using CMS.Base;
using CMS.Core;
using CMS.OnlineForms;
using Kentico.Xperience.Admin.Base;
using System;
using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.FormNotifications;
using XperienceCommunity.FormNotifications.EventHandlers;

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

        var services = parameters.Services;

        _installer = services.GetRequiredService<ExtensionModuleInstaller>();

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