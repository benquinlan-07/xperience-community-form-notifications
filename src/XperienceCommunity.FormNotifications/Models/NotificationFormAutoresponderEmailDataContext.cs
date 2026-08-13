using CMS.EmailMarketing.Internal;

namespace XperienceCommunity.FormNotifications.Models;

internal class NotificationFormAutoresponderEmailDataContext : FormAutoresponderEmailDataContext
{
    public FormEmailTemplateInfo FormEmailTemplate { get; set; }
}
