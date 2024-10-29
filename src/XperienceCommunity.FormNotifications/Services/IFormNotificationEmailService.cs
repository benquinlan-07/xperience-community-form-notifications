using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CMS.ContactManagement;
using CMS.ContentEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EmailLibrary;
using CMS.EmailLibrary.Internal;
using CMS.EmailMarketing.Internal;
using CMS.MacroEngine;
using CMS.OnlineForms;
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
        private readonly IEmailTemplateMergeService _emailTemplateMergeService;
        private readonly IEmailContentResolver _emailContentResolver;
        private readonly ICurrentContactProvider _currentContactProvider;
        private readonly IContentItemDataInfoRetriever _contentItemDataInfoRetriever;
        private readonly IEmailChannelLanguageRetriever _emailChannelLanguageRetriever;
        private readonly IInfoProvider<EmailConfigurationInfo> _emailConfigurationInfoProvider;
        private readonly IInfoProvider<FormNotificationInfo> _formNotificationInfoProvider;
        private readonly IInfoProvider<ContentItemInfo> _contentItemInfoProvider;
        private readonly IInfoProvider<EmailChannelSenderInfo> _emailChannelSenderInfoProvider;
        private readonly IInfoProvider<EmailChannelInfo> _emailChannelInfoProvider;

        public FormNotificationEmailService(IEventLogService eventLogService, 
            IEmailService emailService,
            IEmailTemplateMergeService emailTemplateMergeService,
            IEmailContentResolver emailContentResolver,
            ICurrentContactProvider currentContactProvider,
            IContentItemDataInfoRetriever contentItemDataInfoRetriever,
            IEmailChannelLanguageRetriever emailChannelLanguageRetriever,
            IInfoProvider<EmailConfigurationInfo> emailConfigurationInfoProvider,
            IInfoProvider<FormNotificationInfo> formNotificationInfoProvider,
            IInfoProvider<ContentItemInfo> contentItemInfoProvider,
            IInfoProvider<EmailChannelSenderInfo> emailChannelSenderInfoProvider,
            IInfoProvider<EmailChannelInfo> emailChannelInfoProvider)
        {
            _eventLogService = eventLogService;
            _emailService = emailService;
            _emailTemplateMergeService = emailTemplateMergeService;
            _emailContentResolver = emailContentResolver;
            _currentContactProvider = currentContactProvider;
            _contentItemDataInfoRetriever = contentItemDataInfoRetriever;
            _emailChannelLanguageRetriever = emailChannelLanguageRetriever;
            _emailConfigurationInfoProvider = emailConfigurationInfoProvider;
            _formNotificationInfoProvider = formNotificationInfoProvider;
            _contentItemInfoProvider = contentItemInfoProvider;
            _emailChannelSenderInfoProvider = emailChannelSenderInfoProvider;
            _emailChannelInfoProvider = emailChannelInfoProvider;
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
                }

                // Send the autoresponder email
                var contact = _currentContactProvider.GetCurrentContact();
                if (contact != null && formNotification.FormNotificationSendEmailAutoresponder)
                {
                    var recipient = new Recipient { FirstName = contact.ContactFirstName, LastName = contact.ContactLastName, Email = contact.ContactEmail };
                    var dataContext = new AutomationEmailDataContext() { Recipient = recipient, Contact = contact};
                    await SendEmail(formNotification.FormNotificationEmailAutoresponderTemplate, contact.ContactEmail, formNotification.FormNotificationEmailAutoresponderSubject, dataContext, macroResolver);
                }

                // Send the notification email
                if (formNotification.FormNotificationSendEmailNotification)
                {
                    var recipient = new Recipient { Email = formNotification.FormNotificationEmailNotificationRecipient };
                    var dataContext = new AutomationEmailDataContext() { Recipient = recipient };
                    await SendEmail(formNotification.FormNotificationEmailNotificationTemplate, formNotification.FormNotificationEmailNotificationRecipient, formNotification.FormNotificationEmailNotificationSubject, dataContext, macroResolver);
                }
            }
            catch (Exception ex)
            {
                _eventLogService.LogException(nameof(FormNotificationEmailService), nameof(SendFormEmails), ex);
            }
        }

        private async Task SendEmail(Guid emailConfigurationGuid, string recipient, string subject, IEmailDataContext dataContext, MacroResolver macroResolver)
        {
            var emailConfiguration = await _emailConfigurationInfoProvider.GetAsync(emailConfigurationGuid);
            if (emailConfiguration == null)
            {
                _eventLogService.LogWarning(nameof(FormNotificationEmailService), nameof(SendEmail), $"Could not find email configuration for '{emailConfigurationGuid}'");
                return;
            }

            // Get the subject and from address
            var emailValues = await GetEmailValues(emailConfiguration);
            var senderMailAddress = await GetSenderMailAddress(emailValues);

            // Process macros in the email body, recipients and subject
            var templateWithEmailData = await _emailTemplateMergeService.GetMergedTemplateWithEmailData(emailConfiguration, false);
            var bodyContent = ResolveMacros(macroResolver, templateWithEmailData);
            bodyContent = await _emailContentResolver.Resolve(emailConfiguration, bodyContent, EmailContentFilterType.Sending, dataContext);

            var recipients = ResolveMacros(macroResolver, recipient).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            subject = ResolveMacros(macroResolver, string.IsNullOrWhiteSpace(subject) ? emailValues.EmailSubject : subject);

            // Send the email
            var toSend = new EmailMessage
            {
                Recipients = string.Join(";", recipients),
                Subject = subject,
                From = senderMailAddress.ToString(),
                Body = bodyContent,
                EmailConfigurationID = emailConfiguration.EmailConfigurationID,
                MailoutGuid = Guid.NewGuid(),
            };
            await _emailService.SendEmail(toSend);
        }

        public string ResolveMacros(MacroResolver macroResolver, string content)
        {
            // Include support for anyone copying form placeholders from previous environments by changing $$label:FieldName$$ and $$value:FieldName$$ to {% label_FieldName %} and {% value_FieldName %}
            var oldFormatRegex = new Regex("\\$\\$(value|label):([a-zA-Z0-9]+)\\$\\$");
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

        public async Task<MailAddress> GetSenderMailAddress(EmailContentTypeSpecificFieldValues values)
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
