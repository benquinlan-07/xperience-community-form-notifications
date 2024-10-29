using CMS.Automation;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.EmailEngine;
using CMS.EmailLibrary.Internal;
using CMS.EmailLibrary;
using CMS.EmailMarketing.Internal;
using CMS.OnlineForms;
using Kentico.Xperience.Admin.Base;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;
using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.FormNotifications;
using XperienceCommunity.FormNotifications.Services;

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

        BizFormItemEvents.Insert.After += OnAfterInsertBizFormItem;
    }

    private void InitializeModule(object? sender, EventArgs e) =>
        _installer?.Install();

    private void OnAfterInsertBizFormItem(object sender, BizFormItemEventArgs e)
    {
        var emailTask = Task.Run(async () =>
        {
            var formNotificationEmailService = Service.Resolve<IFormNotificationEmailService>();
            await formNotificationEmailService.SendFormEmails(e.Item);
        });
        emailTask.Wait();
    }
}