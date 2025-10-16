using System.Threading.Tasks;
using CMS.EmailEngine;
using CMS.EmailLibrary;
using CMS.OnlineForms;
using XperienceCommunity.FormNotifications.Models;

namespace XperienceCommunity.FormNotifications.Services
{
    /// <summary>
    /// Interface for handlers that can transform the email message before it is sent.
    /// Register implementations in the DI container to customize email messages.
    /// </summary>
    public interface IFormNotificationEmailMessageHandler
    {
        /// <summary>
        /// Transforms the email message before it is sent.
        /// </summary>
        /// <param name="emailMessage">The email message to transform</param>
        /// <param name="formNotification">The form notification configuration</param>
        /// <param name="emailConfiguration">The email configuration used for sending the email</param>
        /// <param name="bizFormItem">The form item that triggered the notification</param>
        /// <param name="isAutoresponder">True if this is an autoresponder email, false if it's a notification email</param>
        /// <returns>The transformed email message</returns>
        Task<EmailMessage> TransformEmailMessageAsync(EmailMessage emailMessage, FormNotificationInfo formNotification, EmailConfigurationInfo emailConfiguration, BizFormItem bizFormItem, bool isAutoresponder);
    }
}
