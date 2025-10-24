using AngleSharp.Css;
using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EmailLibrary;
using CMS.EmailLibrary.Internal;
using CMS.EmailMarketing.Internal;
using CMS.FormEngine;
using CMS.IO;
using CMS.MacroEngine;
using CMS.OnlineForms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using XperienceCommunity.FormNotifications.Extensions;
using XperienceCommunity.FormNotifications.Models;

namespace XperienceCommunity.FormNotifications.Services
{
    public interface IFormNotificationEmailService
    {
        public Task SendFormEmails(BizFormItem bizFormItem);
    }
    internal class FormNotificationEmailService : IFormNotificationEmailService
    {
        private readonly IEventLogService _eventLogService;
        private readonly IEmailService _emailService;
        private readonly IContentItemDataInfoRetriever _contentItemDataInfoRetriever;
        private readonly IEmailChannelLanguageRetriever _emailChannelLanguageRetriever;
        private readonly IEmailMarkupBuilderFactory _emailMarkupBuilderFactory;
        private readonly IInfoProvider<EmailConfigurationInfo> _emailConfigurationInfoProvider;
        private readonly IInfoProvider<FormNotificationInfo> _formNotificationInfoProvider;
        private readonly IInfoProvider<ContentItemInfo> _contentItemInfoProvider;
        private readonly IInfoProvider<EmailChannelSenderInfo> _emailChannelSenderInfoProvider;
        private readonly IInfoProvider<EmailChannelInfo> _emailChannelInfoProvider;
        private readonly IEnumerable<IFormNotificationEmailMessageHandler> _emailMessageHandlers;
        private readonly IInfoProvider<FormEmailTemplateInfo> _formEmailTemplateInfoProvider;

        public FormNotificationEmailService(IEventLogService eventLogService, 
            IEmailService emailService,
            IContentItemDataInfoRetriever contentItemDataInfoRetriever,
            IEmailChannelLanguageRetriever emailChannelLanguageRetriever,
            IEmailMarkupBuilderFactory emailMarkupBuilderFactory,
            IInfoProvider<EmailConfigurationInfo> emailConfigurationInfoProvider,
            IInfoProvider<FormNotificationInfo> formNotificationInfoProvider,
            IInfoProvider<ContentItemInfo> contentItemInfoProvider,
            IInfoProvider<EmailChannelSenderInfo> emailChannelSenderInfoProvider,
            IInfoProvider<EmailChannelInfo> emailChannelInfoProvider,
            IEnumerable<IFormNotificationEmailMessageHandler> emailMessageHandlers,
            IInfoProvider<FormEmailTemplateInfo> formEmailTemplateInfoProvider
            )
        {
            _eventLogService = eventLogService;
            _emailService = emailService;
            _contentItemDataInfoRetriever = contentItemDataInfoRetriever;
            _emailChannelLanguageRetriever = emailChannelLanguageRetriever;
            _emailMarkupBuilderFactory = emailMarkupBuilderFactory;
            _emailConfigurationInfoProvider = emailConfigurationInfoProvider;
            _formNotificationInfoProvider = formNotificationInfoProvider;
            _contentItemInfoProvider = contentItemInfoProvider;
            _emailChannelSenderInfoProvider = emailChannelSenderInfoProvider;
            _emailChannelInfoProvider = emailChannelInfoProvider;
            _emailMessageHandlers = emailMessageHandlers;
            _formEmailTemplateInfoProvider = formEmailTemplateInfoProvider;
        }

        public async Task SendFormEmails(BizFormItem bizFormItem)
        {
            try
            {
                var formNotification = _formNotificationInfoProvider.Get()
                    .WhereEquals(nameof(FormNotificationInfo.FormNotificationFormId), bizFormItem.BizFormInfo.FormID)
                    .FirstOrDefault();

                // Abort the process if nobody wants an email
                if (formNotification == null || (!formNotification.FormNotificationSendEmailAutoresponder && !formNotification.FormNotificationSendEmailNotification))
                    return;

                // Initialise the macro resolver with form data properties
                var formattedHtmlValuesBuilder = new StringBuilder();
                var macroResolver = MacroResolver.GetInstance();
                foreach (var propertyName in bizFormItem.Properties)
                {
                    var formField = bizFormItem.BizFormInfo.Form.GetFormField(propertyName);
                    if (formField == null)
                        continue;

                    var value = bizFormItem[propertyName];

                    macroResolver.SetNamedSourceData($"label_{propertyName}", formField.Caption);
                    macroResolver.SetNamedSourceData($"value_{propertyName}", value);
                    macroResolver.SetNamedSourceData(propertyName, value);

                    formattedHtmlValuesBuilder.Append($"<p><strong>{System.Web.HttpUtility.HtmlEncode(formField.Caption)}:</strong><br/>{System.Web.HttpUtility.HtmlEncode(value)}</p>");
                }

                macroResolver.SetNamedSourceData("FormData", formattedHtmlValuesBuilder.ToString());

                // Add the form to the resolver
                macroResolver.SetNamedSourceData("BizForm", bizFormItem.BizFormInfo);

                var bizformAttachments = new List<BizFormUploadFile>();
                if (formNotification.FormNotificationEmailNotificationIncludeAttachments || formNotification.FormNotificationEmailAutoresponderIncludeAttachments)
                {
                    var fileUploadFields = bizFormItem.BizFormInfo.Form.ItemsList
                        .OfType<FormFieldInfo>()
                        .Where(x => x.IsFileUpload())
                        .ToArray();

                    foreach (var fileUploadField in fileUploadFields)
                    {
                        var formValue = bizFormItem[fileUploadField.Name] as BizFormUploadFile;
                        if (string.IsNullOrWhiteSpace(formValue?.SystemFileName))
                            continue;
                        bizformAttachments.Add(formValue);
                    }
                }

                // Send the autoresponder email
                if (formNotification.FormNotificationSendEmailAutoresponder && !string.IsNullOrWhiteSpace(formNotification.FormNotificationEmailAutoresponderRecipientEmailField))
                {
                    var emailFormField = bizFormItem.BizFormInfo.Form.GetFormField(formNotification.FormNotificationEmailAutoresponderRecipientEmailField);
                    if (emailFormField != null)
                    {
                        var recipientEmail = bizFormItem[emailFormField.Name] as string;
                        if (!string.IsNullOrWhiteSpace(recipientEmail))
                        {
                            var recipient = new Recipient { Email = recipientEmail };
                            var dataContext = new FormAutoresponderEmailDataContext() { Recipient = recipient };
                            var attachments = formNotification.FormNotificationEmailAutoresponderIncludeAttachments
                                ? bizformAttachments
                                : null;
                            await SendEmail(formNotification, formNotification.FormNotificationEmailAutoresponderTemplate, formNotification.FormNotificationEmailAutoresponderEmailTemplate, formNotification.FormNotificationEmailAutoresponderEmailMessage, recipientEmail, formNotification.FormNotificationEmailAutoresponderSubject, dataContext, macroResolver, attachments, bizFormItem, true);
                        }
                        else
                        {
                            _eventLogService.LogWarning(nameof(FormNotificationEmailService), nameof(SendFormEmails), "Unable to send autoresponder as not value was provided in selected recipient email field.");
                        }
                    }
                    else
                    {
                        _eventLogService.LogWarning(nameof(FormNotificationEmailService), nameof(SendFormEmails), "Unable to send autoresponder as recipient email field was not set to a valid option. Please review your form email configuration.");
                    }
                }

                // Send the notification email
                if (formNotification.FormNotificationSendEmailNotification)
                {
                    var recipient = new Recipient { Email = formNotification.FormNotificationEmailNotificationRecipient };
                    var dataContext = new FormAutoresponderEmailDataContext() { Recipient = recipient };
                    var attachments = formNotification.FormNotificationEmailNotificationIncludeAttachments
                        ? bizformAttachments
                        : null;
                    await SendEmail(formNotification, formNotification.FormNotificationEmailNotificationTemplate, formNotification.FormNotificationEmailNotificationEmailTemplate, formNotification.FormNotificationEmailNotificationEmailMessage, formNotification.FormNotificationEmailNotificationRecipient, formNotification.FormNotificationEmailNotificationSubject, dataContext, macroResolver, attachments, bizFormItem, false);
                }
            }
            catch (Exception ex)
            {
                _eventLogService.LogException(nameof(FormNotificationEmailService), nameof(SendFormEmails), ex);
            }
        }

        private async Task SendEmail(FormNotificationInfo formNotification, Guid emailConfigurationGuid, string emailTemplateName, string emailMessage, string recipient, string subject, IEmailDataContext dataContext, MacroResolver macroResolver, ICollection<BizFormUploadFile> attachments, BizFormItem bizFormItem, bool isAutoresponder)
        {
            var emailSource = string.Empty;
            var emailSubject = string.Empty;
            var emailFrom = string.Empty;
            EmailConfigurationInfo emailConfiguration = null;

            if (emailConfigurationGuid != Guid.Empty)
            {
                emailConfiguration = await _emailConfigurationInfoProvider.GetAsync(emailConfigurationGuid);
                if (emailConfiguration == null)
                {
                    _eventLogService.LogWarning(nameof(FormNotificationEmailService), nameof(SendEmail), $"Could not find email configuration for '{emailConfigurationGuid}'");
                    return;
                }

                // Get the subject and from address
                var emailValues = await GetEmailValues(emailConfiguration);
                emailSubject = emailValues.EmailSubject;
                var senderMailAddress = await GetSenderMailAddress(emailValues);
                emailFrom = senderMailAddress.ToString();

                // Process macros in the email body, recipients and subject
                var emailMarkupBuilder = await _emailMarkupBuilderFactory.Create(emailConfiguration);
                emailSource = await emailMarkupBuilder.BuildEmailForSending(emailConfiguration);
            }
            else if (!string.IsNullOrWhiteSpace(emailTemplateName))
            {
                // Get the email template
                var emailTemplate = _formEmailTemplateInfoProvider.Get()
                    .WhereEquals(nameof(FormEmailTemplateInfo.FormEmailTemplateName), emailTemplateName)
                    .First();

                // Resolve the message into the template
                var templateResolver = MacroResolver.GetInstance();
                templateResolver.SetNamedSourceData("message", emailMessage);
                emailSource = templateResolver.ResolveMacros(emailTemplate.FormEmailTemplateSourceCode ?? string.Empty, null);

                emailFrom = emailTemplate.FormEmailTemplateSender;
                emailSubject = emailTemplate.FormEmailTemplateSubject;
            }

            var bodyContent = ResolveMacros(macroResolver, emailSource);

            foreach (IEmailContentFilter emailContentFilter in EmailContentFilterRegister.Instance.GetAll(EmailContentFilterType.Sending))
            {
                try
                {
                    bodyContent = await emailContentFilter.Apply(bodyContent, emailConfiguration, dataContext);
                }
                catch (ArgumentNullException ex)
                {
                    // Ignore this as it will only come about on filters expecting an email configuration when none is provided
                }
            }

            var recipients = ResolveMacros(macroResolver, recipient).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            subject = ResolveMacros(macroResolver, string.IsNullOrWhiteSpace(subject) ? emailSubject : subject);

            // Build the email message
            var toSend = new EmailMessage
            {
                Recipients = string.Join(";", recipients),
                Subject = subject,
                From = emailFrom,
                Body = bodyContent,
                MailoutGuid = Guid.NewGuid(),
            };

            if (emailConfiguration != null)
                toSend.EmailConfigurationID = emailConfiguration.EmailConfigurationID;

            // Include any attachments
            if (attachments != null)
            {
                foreach (var bizFormUploadFile in attachments)
                {
                    var filePath = FormHelper.GetFilePhysicalPath(bizFormUploadFile.SystemFileName);
                    var reader = File.OpenRead(filePath);
                    toSend.Attachments.Add(new Attachment(reader, bizFormUploadFile.OriginalFileName));
                }
            }

            // Apply custom transformations via registered handlers
            if (_emailMessageHandlers != null)
            {
                foreach (var handler in _emailMessageHandlers)
                {
                    toSend = await handler.TransformEmailMessageAsync(toSend, formNotification, emailConfiguration, bizFormItem, isAutoresponder);
                }
            }

            // Send email
            await _emailService.SendEmail(toSend);
        }

        private string ResolveMacros(MacroResolver macroResolver, string content)
        {
            // Include support for anyone copying form placeholders from previous environments by changing $$label:FieldName$$ and $$value:FieldName$$ to {% label_FieldName %} and {% value_FieldName %}
            var oldFormatRegex = new Regex("\\$\\$(value|label):([a-zA-Z_]+[a-zA-Z0-9_]*)\\$\\$");
            content = oldFormatRegex.Replace(content ?? "", "{% $1_$2 %}");
            return macroResolver.ResolveMacros(content ?? "", null);
        }

        private async Task<EmailContentTypeSpecificFieldValues> GetEmailValues(EmailConfigurationInfo email)
        {
            var contentItem = await _contentItemInfoProvider.GetAsync(email.EmailConfigurationContentItemID);
            var languageInfoOrThrow = await _emailChannelLanguageRetriever.GetEmailChannelLanguageInfoOrThrow(email.EmailConfigurationEmailChannelID);
            var contentItemData = await _contentItemDataInfoRetriever.GetContentItemData(contentItem.ContentItemContentTypeID, email.EmailConfigurationContentItemID, languageInfoOrThrow.ContentLanguageID, true);
            var emailSpecificFieldValues = new EmailContentTypeSpecificFieldValues(contentItemData);
            return emailSpecificFieldValues;
        }

        private async Task<MailAddress> GetSenderMailAddress(EmailContentTypeSpecificFieldValues values)
        {
            var sender = await _emailChannelSenderInfoProvider.GetAsync<EmailChannelSenderInfo>(values.EmailSenderID);
            return new MailAddress(await GetEmailAddress(sender.EmailChannelSenderName, sender.EmailChannelSenderEmailChannelID), sender.EmailChannelSenderDisplayName);
        }

        private async Task<string> GetEmailAddress(string senderName, int emailChannelID)
        {
            var emailChannel = await _emailChannelInfoProvider.GetAsync(emailChannelID);
            return $"{senderName}@{emailChannel.EmailChannelSendingDomain}";
        }
    }
}
