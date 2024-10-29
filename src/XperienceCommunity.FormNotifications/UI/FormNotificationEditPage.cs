using System.Collections.Generic;
using System.Threading.Tasks;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.FormNotifications.UI;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using CMS.EmailLibrary;
using Kentico.Xperience.Admin.DigitalMarketing.FormAnnotations;
using System;
using System.Linq;
using XperienceCommunity.FormNotifications.Models;
using CMS.DataEngine;

[assembly: UIPage(typeof(FormBuilderTab), "emails", typeof(FormNotificationEditPage), "Emails", TemplateNames.EDIT, 210, Icon = "xp-message")]

namespace XperienceCommunity.FormNotifications.UI;

[UIPageLocation(PageLocationEnum.SidePanel)]
public class FormNotificationEditPage : ModelEditPage<FormNotificationEditPage.EditModel>
{
    private readonly IInfoProvider<FormNotificationInfo> _formNotificationInfoProvider;
    private EditModel _model;

    public FormNotificationEditPage(IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IInfoProvider<FormNotificationInfo> formNotificationInfoProvider)
        : base(formItemCollectionProvider, formDataBinder)
    {
        _formNotificationInfoProvider = formNotificationInfoProvider;
    }

    [PageParameter(typeof(IntPageModelBinder), typeof(FormEditSection))]
    public int FormId { get; set; }

    /// <inheritdoc />
    public override async Task ConfigurePage()
    {
        this.PageConfiguration.Headline = "Emails";

        var formNotification = GetFormNotificationInfo();
        Model.AutoresponderEnabled = formNotification?.FormNotificationSendEmailAutoresponder ?? false;
        Model.AutoresponderSubject = Model.AutoresponderEnabled ? formNotification?.FormNotificationEmailAutoresponderSubject : null;
        Model.AutoresponderEmailTemplate = formNotification != null && Model.AutoresponderEnabled
            ? new[] { new EmailRelatedItem() { EmailGuid = formNotification.FormNotificationEmailAutoresponderTemplate } }
            : Array.Empty<EmailRelatedItem>();

        Model.NotificationEnabled = formNotification?.FormNotificationSendEmailNotification ?? false;
        Model.NotificationRecipient = Model.NotificationEnabled ? formNotification?.FormNotificationEmailNotificationRecipient : null;
        Model.NotificationSubject = Model.NotificationEnabled ? formNotification?.FormNotificationEmailNotificationSubject : null;
        Model.NotificationEmailTemplate = formNotification != null && Model.NotificationEnabled
            ? new[] { new EmailRelatedItem() { EmailGuid = formNotification.FormNotificationEmailNotificationTemplate } }
            : Array.Empty<EmailRelatedItem>();

        await base.ConfigurePage();
    }

    /// <inheritdoc />
    protected override async Task<ICommandResponse> ProcessFormData(EditModel model, ICollection<IFormItem> formItems)
    {
        var formNotification = GetFormNotificationInfo();

        if (formNotification == null)
        {
            formNotification = new FormNotificationInfo();
            formNotification.FormNotificationFormId = FormId;
        }

        formNotification.FormNotificationSendEmailAutoresponder = model.AutoresponderEnabled;
        formNotification.FormNotificationEmailAutoresponderSubject = model.AutoresponderEnabled ? model.AutoresponderSubject : null;
        formNotification.FormNotificationEmailAutoresponderTemplate = model.AutoresponderEnabled ? model.AutoresponderEmailTemplate.First().EmailGuid : Guid.Empty;

        formNotification.FormNotificationSendEmailNotification = model.NotificationEnabled;
        formNotification.FormNotificationEmailNotificationRecipient = model.NotificationEnabled ? model.NotificationRecipient : null;
        formNotification.FormNotificationEmailNotificationSubject = model.NotificationEnabled ? model.NotificationSubject : null;
        formNotification.FormNotificationEmailNotificationTemplate = model.NotificationEnabled ? model.NotificationEmailTemplate.First().EmailGuid : Guid.Empty;

        _formNotificationInfoProvider.Set(formNotification);

        // Initializes a client response
        var response = ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationSuccess)
        {
            // Returns the submitted field values to the client (repopulates the form)
            Items = await formItems.OnlyVisible().GetClientProperties(),
        });

        return response;
    }

    private FormNotificationInfo GetFormNotificationInfo()
    {
        return _formNotificationInfoProvider.Get()
            .WhereEquals(nameof(FormNotificationInfo.FormNotificationFormId), FormId)
            .FirstOrDefault();
    }

    protected override EditModel Model
    {
        get { return _model ??= new EditModel(); }
    }


    /// <summary>Represent a general properties form.</summary>
    [FormCategory(Label = "Autoresponder", Order = 1)]
    [FormCategory(Label = "Notification", Order = 5)]
    public class EditModel
    {
        [CheckBoxComponent(Label = "Send autoresponder email", Order = 2)]
        [RequiredValidationRule]
        public bool AutoresponderEnabled { get; set; }

        /// <summary>Indicates the source of autoresponder email.</summary>
        [TextInputComponent(Label = "Subject", Order = 3, ExplanationText = "Defaults to selected email subject if not specified")]
        [VisibleIfTrue(nameof(AutoresponderEnabled))]
        public string AutoresponderSubject { get; set; }

        /// <summary>Email to be sent as autoresponder mail.</summary>
        [EmailSelectorComponent(AllowedEmailPurpose = "FormAutoresponder", Label = "Email", MaximumEmails = 1, Order = 4)]
        [RequiredValidationRule]
        [VisibleIfTrue(nameof(AutoresponderEnabled))]
        public IEnumerable<EmailRelatedItem> AutoresponderEmailTemplate { get; set; }

        [CheckBoxComponent(Label = "Send email notification", Order = 6)]
        [RequiredValidationRule]
        public bool NotificationEnabled { get; set; }

        /// <summary>Indicates the source of autoresponder email.</summary>
        [TextInputComponent(Label = "Recipients", Order = 7, ExplanationText = "Additional recipients can be separated by comma or semi-colon")]
        [RequiredValidationRule]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public string NotificationRecipient { get; set; }

        /// <summary>Indicates the source of autoresponder email.</summary>
        [TextInputComponent(Label = "Subject", Order = 8, ExplanationText = "Defaults to selected email subject if not specified")]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public string NotificationSubject { get; set; }

        /// <summary>Email to be sent as autoresponder mail.</summary>
        [EmailSelectorComponent(AllowedEmailPurpose = "FormAutoresponder", Label = "Email", MaximumEmails = 1, Order = 9)]
        [RequiredValidationRule]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public IEnumerable<EmailRelatedItem> NotificationEmailTemplate { get; set; }

    }
}