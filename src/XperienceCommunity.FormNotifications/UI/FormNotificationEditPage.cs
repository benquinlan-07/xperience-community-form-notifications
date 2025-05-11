using System.Collections.Generic;
using System.Threading.Tasks;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.FormNotifications.UI;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using CMS.EmailLibrary;
using Kentico.Xperience.Admin.DigitalMarketing.FormAnnotations;
using System;
using System.Linq;
using XperienceCommunity.FormNotifications.Models;
using CMS.DataEngine;
using XperienceCommunity.FormNotifications.ValidationRules;
using CMS.OnlineForms;
using XperienceCommunity.FormNotifications.Controls;
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
        formNotification.FormNotificationEmailAutoresponderTemplate = model.AutoresponderEnabled ? model.AutoresponderEmailTemplate.First().EmailGuid : Guid.Empty;
        formNotification.FormNotificationEmailAutoresponderIncludeAttachments = model.AutoresponderEnabled && model.AutoresponderIncludeAttachments;

        formNotification.FormNotificationSendEmailNotification = model.NotificationEnabled;
        formNotification.FormNotificationEmailNotificationRecipient = (model.NotificationEnabled ? model.NotificationRecipient : null) ?? string.Empty;
        formNotification.FormNotificationEmailNotificationSubject = (model.NotificationEnabled ? model.NotificationSubject : null) ?? string.Empty;
        formNotification.FormNotificationEmailNotificationTemplate = model.NotificationEnabled ? model.NotificationEmailTemplate.First().EmailGuid : Guid.Empty;
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
        model.AutoresponderEmailTemplate = formNotification != null && model.AutoresponderEnabled
            ? new[] { new EmailRelatedItem() { EmailGuid = formNotification.FormNotificationEmailAutoresponderTemplate } }
            : Array.Empty<EmailRelatedItem>();
        model.AutoresponderIncludeAttachments = model.AutoresponderEnabled && (formNotification?.FormNotificationEmailAutoresponderIncludeAttachments ?? false);

        model.NotificationEnabled = formNotification?.FormNotificationSendEmailNotification ?? false;
        model.NotificationRecipient = model.NotificationEnabled ? formNotification?.FormNotificationEmailNotificationRecipient : null;
        model.NotificationSubject = model.NotificationEnabled ? formNotification?.FormNotificationEmailNotificationSubject : null;
        model.NotificationEmailTemplate = formNotification != null && model.NotificationEnabled
            ? new[] { new EmailRelatedItem() { EmailGuid = formNotification.FormNotificationEmailNotificationTemplate } }
            : Array.Empty<EmailRelatedItem>();
        model.NotificationIncludeAttachments = model.NotificationEnabled && (formNotification?.FormNotificationEmailNotificationIncludeAttachments ?? false);

        return model;
    }

    /// <summary>Represent a general properties form.</summary>
    [FormCategory(Label = "Autoresponder", Order = 0)]
    [FormCategory(Label = "Notification", Order = 10)]
    public class EditModel
    {
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

        /// <summary>Email to be sent as autoresponder mail.</summary>
        [EmailSelectorComponent(AllowedEmailPurpose = "FormAutoresponder", Label = "Email", MaximumEmails = 1, Order = 4)]
        [RequiredIfTrueValidationRule(nameof(AutoresponderEnabled), FieldName = "Email")]
        [VisibleIfTrue(nameof(AutoresponderEnabled))]
        public IEnumerable<EmailRelatedItem> AutoresponderEmailTemplate { get; set; }

        [CheckBoxComponent(Label = "Include attachments", Order = 5)]
        [VisibleIfTrue(nameof(AutoresponderEnabled))]
        public bool AutoresponderIncludeAttachments { get; set; }

        [CheckBoxComponent(Label = "Send email notification", Order = 11)]
        public bool NotificationEnabled { get; set; }

        /// <summary>Indicates the source of autoresponder email.</summary>
        [TextInputComponent(Label = "Recipients", Order = 12, ExplanationText = "Additional recipients can be separated by comma or semi-colon")]
        [RequiredIfTrueValidationRule(nameof(NotificationEnabled), FieldName = "Recipients")]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public string NotificationRecipient { get; set; }

        /// <summary>Indicates the source of autoresponder email.</summary>
        [TextInputComponent(Label = "Subject", Order = 13, ExplanationText = "Defaults to selected email subject if not specified")]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public string NotificationSubject { get; set; }

        /// <summary>Email to be sent as autoresponder mail.</summary>
        [EmailSelectorComponent(AllowedEmailPurpose = "FormAutoresponder", Label = "Email", MaximumEmails = 1, Order = 14)]
        [RequiredIfTrueValidationRule(nameof(NotificationEnabled), FieldName = "Email")]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public IEnumerable<EmailRelatedItem> NotificationEmailTemplate { get; set; }

        [CheckBoxComponent(Label = "Include attachments", Order = 15)]
        [VisibleIfTrue(nameof(NotificationEnabled))]
        public bool NotificationIncludeAttachments { get; set; }

    }
}