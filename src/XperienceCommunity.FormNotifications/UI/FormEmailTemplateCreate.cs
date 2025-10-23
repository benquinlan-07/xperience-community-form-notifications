using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XperienceCommunity.FormNotifications.Models;
using XperienceCommunity.FormNotifications.UI;

[assembly: UIPage(typeof(FormEmailTemplateListingPage), "create", typeof(FormEmailTemplateCreate), "Create email template", TemplateNames.EDIT, 100, Icon = "xp-message")]

namespace XperienceCommunity.FormNotifications.UI;

public class FormEmailTemplateCreate : ModelEditPage<FormEmailTemplateModel>
{
    private readonly IInfoProvider<FormEmailTemplateInfo> _formEmailTemplateInfoProvider;
    private FormEmailTemplateModel _model;

    public FormEmailTemplateCreate(IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IInfoProvider<FormEmailTemplateInfo> formEmailTemplateInfoProvider)
        : base(formItemCollectionProvider, formDataBinder)
    {
        _formEmailTemplateInfoProvider = formEmailTemplateInfoProvider;
    }

    /// <inheritdoc />
    public override async Task ConfigurePage()
    {
        PageConfiguration.Headline = "Email Template";

        await base.ConfigurePage();
    }


    /// <inheritdoc />
    protected override async Task<ICommandResponse> ProcessFormData(FormEmailTemplateModel model, ICollection<IFormItem> formItems)
    {
        var formNotification = GetFormEmailTemplateInfo() ?? new FormEmailTemplateInfo();

        formNotification.FormEmailTemplateDisplayName = model.DisplayName;
        formNotification.FormEmailTemplateName = model.Name == "__AUTO__" ? Regex.Replace(model.DisplayName, "[^a-zA-Z0-9]", "") : model.Name;
        formNotification.FormEmailTemplateSubject = model.Subject;
        formNotification.FormEmailTemplateSender = model.Sender;
        formNotification.FormEmailTemplateSourceCode = model.Template;

        _formEmailTemplateInfoProvider.Set(formNotification);

        // Initializes a client response
        var response = ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationSuccess)
        {
            // Returns the submitted field values to the client (repopulates the form)
            Items = await formItems.OnlyVisible().GetClientProperties(),
        });

        response.AddSuccessMessage("Email notification settings have been saved.");

        return response;
    }

    protected virtual FormEmailTemplateInfo GetFormEmailTemplateInfo()
    {
        return null;
    }

    protected override FormEmailTemplateModel Model
    {
        get { return _model ??= InitializeModel(); }
    }

    private FormEmailTemplateModel InitializeModel()
    {
        var model = new FormEmailTemplateModel();

        var formNotification = GetFormEmailTemplateInfo();
        model.DisplayName = formNotification?.FormEmailTemplateDisplayName;
        model.Name = formNotification?.FormEmailTemplateName;
        model.Subject = formNotification?.FormEmailTemplateSubject;
        model.Sender = formNotification?.FormEmailTemplateSender;
        model.Template = formNotification?.FormEmailTemplateSourceCode;
        return model;
    }
}