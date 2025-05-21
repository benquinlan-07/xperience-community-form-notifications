using System.Threading;
using System.Threading.Tasks;
using CMS.DataEngine;
using CMS.OnlineForms;
using XperienceCommunity.FormNotifications.Services;

namespace XperienceCommunity.FormNotifications.EventHandlers
{
    public sealed class BizFormItemAfterInsertHandler : IInfoObjectEventHandler<InfoObjectAfterInsertEvent<BizFormItem>>
    {
        private readonly IFormNotificationEmailService _formNotificationEmailService;

        public BizFormItemAfterInsertHandler(IFormNotificationEmailService formNotificationEmailService)
        {
            _formNotificationEmailService = formNotificationEmailService;
        }

        public void Handle(InfoObjectAfterInsertEvent<BizFormItem> infoObjectEvent)
        {
            var emailTask = Task.Run(async () =>
            {
                await HandleAsync(infoObjectEvent, CancellationToken.None);
            });
            emailTask.Wait();
        }

        public async Task HandleAsync(InfoObjectAfterInsertEvent<BizFormItem> infoObjectEvent, CancellationToken cancellationToken)
        {
            await _formNotificationEmailService.SendFormEmails(infoObjectEvent.InfoObject);
        }
    }
}
