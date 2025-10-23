using CMS.Core;
using CMS.DataEngine;
using CMS.EmailLibrary;
using CMS.OnlineForms;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.DigitalMarketing.FormAnnotations;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XperienceCommunity.FormNotifications.Controls;
using XperienceCommunity.FormNotifications.Models;
using XperienceCommunity.FormNotifications.UI;
using XperienceCommunity.FormNotifications.ValidationRules;
using static System.Net.Mime.MediaTypeNames;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

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
        PageConfiguration.Headline = "Emails";

        await base.ConfigurePage();
    }

    public override async Task<ICommandResponse<FormChangeResult>> Change(FormChangeCommandArguments args)
    {
	    var commandResponse = await base.Change(args);
	    AssignOptionsToRecipientEmailField(commandResponse.Result.Items);
		return commandResponse;
    }

    public override async Task<EditTemplateClientProperties> ConfigureTemplateProperties(EditTemplateClientProperties properties)
    {
        properties = await base.ConfigureTemplateProperties(properties);
        AssignOptionsToRecipientEmailField(properties.Items);
		return properties;
    }

    public override async Task<ICommandResponse> Submit(FormSubmissionCommandArguments args)
    {
	    var commandResponse = await base.Submit(args);
	    if (commandResponse is ICommandResponse<FormSubmissionResult> formSubmissionResultResponse)
			AssignOptionsToRecipientEmailField(formSubmissionResultResponse.Result.Items);
		return commandResponse;
    }

    protected override async Task<ICommandResponse> SubmitInternal(FormSubmissionCommandArguments args, ICollection<IFormItem> items, IFormFieldValueProvider formFieldValueProvider)
	{
		AssignOptionsToRecipientEmailField(items);
		return await base.SubmitInternal(args, items, formFieldValueProvider);
	}

    private void AssignOptionsToRecipientEmailField(ICollection<IFormItem> formItems)
	{
		var recipientFieldDropDownOptions = formItems.OfType<DynamicOptionsDropDownComponent>().FirstOrDefault(x => x.Name == nameof(EditModel.AutoresponderRecipientEmailField));
		if (recipientFieldDropDownOptions != null)
			recipientFieldDropDownOptions.Properties.Options = string.Join(Environment.NewLine, GetRecipientFieldOptions().Select(x => $"{x.Value}"));
	}

    private void AssignOptionsToRecipientEmailField(ICollection<IFormItemClientProperties> clientProperties)
    {
	    var recipientFieldDropDownOptions = clientProperties.OfType<DropDownClientProperties>().FirstOrDefault(x => x.Name == nameof(EditModel.AutoresponderRecipientEmailField));
	    if (recipientFieldDropDownOptions != null)
		    recipientFieldDropDownOptions.Options = GetRecipientFieldOptions();
    }

	private DropDownOptionItem[] GetRecipientFieldOptions()
    {
        var bizForm = BizFormInfoProvider.ProviderObject.Get(FormId);
        var formFields = bizForm.Form.GetFields(true, false);
        return formFields.Select(x => new DropDownOptionItem() { Value = x.Name, Text = x.Caption }).ToArray();
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
        formNotification.FormNotificationEmailAutoresponderRecipientEmailField = (model.AutoresponderEnabled ? model.AutoresponderRecipientEmailField : null) ?? string.Empty;
        formNotification.FormNotificationEmailAutoresponderSubject = (model.AutoresponderEnabled ? model.AutoresponderSubject : null) ?? string.Empty;
        formNotification.FormNotificationEmailAutoresponderTemplate = model.AutoresponderEnabled && model.AutoresponderEmailSource == EditModel.EMAIL_SOURCE_EMAIL_CHANNEL && model.AutoresponderEmailChannelConfiguration.Any()
            ? model.AutoresponderEmailChannelConfiguration.First().EmailGuid 
            : Guid.Empty;
        formNotification.FormNotificationEmailAutoresponderEmailTemplate = model.AutoresponderEnabled && model.AutoresponderEmailSource == EditModel.EMAIL_SOURCE_INLINE_MESSAGE 
            ? model.AutoresponderTemplate 
            : string.Empty;
        formNotification.FormNotificationEmailAutoresponderEmailMessage = model.AutoresponderEnabled && model.AutoresponderEmailSource == EditModel.EMAIL_SOURCE_INLINE_MESSAGE
            ? model.AutoresponderMessage
            : string.Empty;
        formNotification.FormNotificationEmailAutoresponderIncludeAttachments = model.AutoresponderEnabled && model.AutoresponderIncludeAttachments;

        formNotification.FormNotificationSendEmailNotification = model.NotificationEnabled;
        formNotification.FormNotificationEmailNotificationRecipient = (model.NotificationEnabled ? model.NotificationRecipient : null) ?? string.Empty;
        formNotification.FormNotificationEmailNotificationSubject = (model.NotificationEnabled ? model.NotificationSubject : null) ?? string.Empty;
        formNotification.FormNotificationEmailNotificationTemplate = model.NotificationEnabled && model.NotificationEmailSource == EditModel.EMAIL_SOURCE_EMAIL_CHANNEL && model.NotificationEmailChannelConfiguration.Any()
            ? model.NotificationEmailChannelConfiguration.First().EmailGuid
            : Guid.Empty;
        formNotification.FormNotificationEmailNotificationEmailTemplate = model.NotificationEnabled && model.NotificationEmailSource == EditModel.EMAIL_SOURCE_INLINE_MESSAGE
            ? model.NotificationTemplate
            : string.Empty;
        formNotification.FormNotificationEmailNotificationEmailMessage = model.NotificationEnabled && model.NotificationEmailSource == EditModel.EMAIL_SOURCE_INLINE_MESSAGE
            ? model.NotificationMessage
            : string.Empty;
        formNotification.FormNotificationEmailNotificationIncludeAttachments = model.NotificationEnabled && model.NotificationIncludeAttachments;

        _formNotificationInfoProvider.Set(formNotification);

        // Initializes a client response
        var response = ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationSuccess)
        {
            // Returns the submitted field values to the client (repopulates the form)
            Items = await formItems.OnlyVisible().GetClientProperties(),
        });

        response.AddSuccessMessage("Email notification settings have been saved.");

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
        get { return _model ??= InitializeModel(); }
    }

    private EditModel InitializeModel()
    {
        var model = new EditModel();

        var formNotification = GetFormNotificationInfo();
        model.AutoresponderEnabled = formNotification?.FormNotificationSendEmailAutoresponder ?? false;
        model.AutoresponderRecipientEmailField = model.AutoresponderEnabled ? formNotification?.FormNotificationEmailAutoresponderRecipientEmailField : null;
        model.AutoresponderSubject = model.AutoresponderEnabled ? formNotification?.FormNotificationEmailAutoresponderSubject : null;
        model.AutoresponderEmailSource = model.AutoresponderEnabled && formNotification != null
            ? formNotification.FormNotificationEmailAutoresponderTemplate != Guid.Empty
                ? EditModel.EMAIL_SOURCE_EMAIL_CHANNEL
                : !string.IsNullOrWhiteSpace(formNotification.FormNotificationEmailAutoresponderEmailTemplate)
                    ? EditModel.EMAIL_SOURCE_INLINE_MESSAGE
                    : null
            : null;
        model.AutoresponderEmailChannelConfiguration = formNotification != null && model.AutoresponderEnabled
            ? new[] { new EmailRelatedItem() { EmailGuid = formNotification.FormNotificationEmailAutoresponderTemplate } }
            : Array.Empty<EmailRelatedItem>();
        model.AutoresponderTemplate = formNotification != null && model.AutoresponderEnabled
            ? formNotification.FormNotificationEmailAutoresponderEmailTemplate
            : null;
        model.AutoresponderMessage = formNotification != null && model.AutoresponderEnabled
            ? formNotification.FormNotificationEmailAutoresponderEmailMessage
            : null;
        model.AutoresponderIncludeAttachments = model.AutoresponderEnabled && (formNotification?.FormNotificationEmailAutoresponderIncludeAttachments ?? false);

        model.NotificationEnabled = formNotification?.FormNotificationSendEmailNotification ?? false;
        model.NotificationRecipient = model.NotificationEnabled ? formNotification?.FormNotificationEmailNotificationRecipient : null;
        model.NotificationSubject = model.NotificationEnabled ? formNotification?.FormNotificationEmailNotificationSubject : null;
        model.NotificationEmailSource = model.NotificationEnabled && formNotification != null
            ? formNotification.FormNotificationEmailNotificationTemplate != Guid.Empty
                ? EditModel.EMAIL_SOURCE_EMAIL_CHANNEL
                : !string.IsNullOrWhiteSpace(formNotification.FormNotificationEmailNotificationEmailTemplate)
                    ? EditModel.EMAIL_SOURCE_INLINE_MESSAGE
                    : null
            : null;
        model.NotificationEmailChannelConfiguration = formNotification != null && model.NotificationEnabled
            ? new[] { new EmailRelatedItem() { EmailGuid = formNotification.FormNotificationEmailNotificationTemplate } }
            : Array.Empty<EmailRelatedItem>();
        model.NotificationTemplate = formNotification != null && model.NotificationEnabled
            ? formNotification.FormNotificationEmailNotificationEmailTemplate
            : null;
        model.NotificationMessage = formNotification != null && model.NotificationEnabled
            ? formNotification.FormNotificationEmailNotificationEmailMessage
            : null;
        model.NotificationIncludeAttachments = model.NotificationEnabled && (formNotification?.FormNotificationEmailNotificationIncludeAttachments ?? false);

        return model;
    }

    /// <summary>Represent a general properties form.</summary>
    [FormCategory(Label = "Autoresponder", Order = 0)]
    [FormCategory(Label = "Notification", Order = 10)]
    public class EditModel
    {
        internal const string EMAIL_SOURCE_EMAIL_CHANNEL = "Email channel";
        internal const string EMAIL_SOURCE_INLINE_MESSAGE = "Inline message";
        internal const string EMAIL_SOURCE_OPTIONS = ";Select\n" + EMAIL_SOURCE_EMAIL_CHANNEL + "\n" + EMAIL_SOURCE_INLINE_MESSAGE;

        [CheckBoxComponent(Label = "Send autoresponder email", Order = 1)]
        public bool AutoresponderEnabled { get; set; }

        /// <summary>Indicates the source of autoresponder email.</summary>
        [DynamicOptionsDropDownComponent(Label = "Recipient email field", Order = 2, ExplanationText = "Please select the field on the form that represents the recipients email")]
        [RequiredIfTrueValidationRule(nameof(AutoresponderEnabled), FieldName = "Recipient email field")]
        [VisibleIfTrue(nameof(AutoresponderEnabled))]
        public string AutoresponderRecipientEmailField { get; set; }

        /// <summary>Indicates the source of autoresponder email.</summary>
        [TextInputComponent(Label = "Subject", Order = 3, ExplanationText = "Defaults to selected email subject if not specified")]
        [VisibleIfTrue(nameof(AutoresponderEnabled))]
        public string AutoresponderSubject { get; set; }

        [DropDownComponent(Label = "Email source", Options = EMAIL_SOURCE_OPTIONS, Order = 4)]
        [RequiredIfTrueValidationRule(nameof(AutoresponderEnabled), FieldName = "Recipient email field")]
        [VisibleIfTrue(nameof(AutoresponderEnabled))]
        public string AutoresponderEmailSource { get; set; }

        /// <summary>Email to be sent as autoresponder mail.</summary>
        [EmailSelectorComponent(AllowedEmailPurpose = "FormAutoresponder", Label = "Email", MaximumEmails = 1, Order = 5)]
        [RequiredIfTrueValidationRule(nameof(AutoresponderEnabled), FieldName = "Email")]
        [VisibleIfEqualTo(nameof(AutoresponderEmailSource), "Email channel")]
        public IEnumerable<EmailRelatedItem> AutoresponderEmailChannelConfiguration { get; set; }

        [DropDownComponent(Label = "Email template", DataProviderType = typeof(EmailTemplateDataProvider), Order = 5)]
        [RequiredIfTrueValidationRule(nameof(AutoresponderEnabled), FieldName = "Recipient email field")]
        [VisibleIfEqualTo(nameof(AutoresponderEmailSource), "Inline message")]
        public string AutoresponderTemplate { get; set; }

        [RichTextEditorComponent(Label = "Message", Order = 6)]
        [RequiredIfTrueValidationRule(nameof(AutoresponderEnabled), FieldName = "Recipient email field")]
        [VisibleIfEqualTo(nameof(AutoresponderEmailSource), "Inline message")]
        public string AutoresponderMessage { get; set; }

        [CheckBoxComponent(Label = "Include attachments", Order = 7)]
        [VisibleIfTrue(nameof(AutoresponderEnabled))]
        public bool AutoresponderIncludeAttachments { get; set; }

        [CheckBoxComponent(Label = "Send email notification", Order = 11)]
        public bool NotificationEnabled { get; set; }

        /// <summary>Indicates the source of autoresponder email.</summary>
        [TextInputComponent(Label = "Recipients", Order = 12, ExplanationText = "Additional recipients can be separated by comma or semi-colon")]
        [RequiredIfTrueValidationRule(nameof(NotificationEnabled), FieldName = "Recipients")]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public string NotificationRecipient { get; set; }

        /// <summary>Indicates the source of Notification email.</summary>
        [TextInputComponent(Label = "Subject", Order = 13, ExplanationText = "Defaults to selected email subject if not specified")]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public string NotificationSubject { get; set; }

        [DropDownComponent(Label = "Email source", Options = EMAIL_SOURCE_OPTIONS, Order = 14)]
        [RequiredIfTrueValidationRule(nameof(NotificationEnabled), FieldName = "Recipient email field")]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public string NotificationEmailSource { get; set; }

        /// <summary>Email to be sent as Notification mail.</summary>
        [EmailSelectorComponent(AllowedEmailPurpose = "FormAutoresponder", Label = "Email", MaximumEmails = 1, Order = 15)]
        [RequiredIfTrueValidationRule(nameof(NotificationEnabled), FieldName = "Email")]
        [VisibleIfEqualTo(nameof(NotificationEmailSource), "Email channel")]
        public IEnumerable<EmailRelatedItem> NotificationEmailChannelConfiguration { get; set; }

        [DropDownComponent(Label = "Email template", DataProviderType = typeof(EmailTemplateDataProvider), Order = 15)]
        [RequiredIfTrueValidationRule(nameof(NotificationEnabled), FieldName = "Recipient email field")]
        [VisibleIfEqualTo(nameof(NotificationEmailSource), "Inline message")]
        public string NotificationTemplate { get; set; }

        [RichTextEditorComponent(Label = "Message", Order = 16)]
        [RequiredIfTrueValidationRule(nameof(NotificationEnabled), FieldName = "Recipient email field")]
        [VisibleIfEqualTo(nameof(NotificationEmailSource), "Inline message")]
        public string NotificationMessage { get; set; }

        [CheckBoxComponent(Label = "Include attachments", Order = 17)]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public bool NotificationIncludeAttachments { get; set; }

    }

    public class EmailTemplateDataProvider : IDropDownOptionsProvider
    {
        public Task<IEnumerable<DropDownOptionItem>> GetOptionItems()
        {
            var provider = Service.Resolve<IInfoProvider<FormEmailTemplateInfo>>();
            var results = provider.Get()
                .OrderBy(nameof(FormEmailTemplateInfo.FormEmailTemplateDisplayName))
                .ToArray()
                .Select(x => new DropDownOptionItem { Value = x.FormEmailTemplateName, Text = x.FormEmailTemplateDisplayName })
                .ToArray();

            return Task.FromResult(results.AsEnumerable());
        }
    }
}