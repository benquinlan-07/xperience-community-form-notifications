using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using System.Linq;
using System.Threading.Tasks;
using CMS.DataEngine.Query;
using XperienceCommunity.FormNotifications.Models;
using XperienceCommunity.FormNotifications.UI;

[assembly: UIPage(typeof(FormsApplication), "email-templates", typeof(FormEmailTemplateListingPage), "Email templates", TemplateNames.LISTING, 500, Icon = "xp-message")]

namespace XperienceCommunity.FormNotifications.UI;

public class FormEmailTemplateListingPage : ListingPage
{
    private readonly IInfoProvider<FormEmailTemplateInfo> _formEmailTemplateInfoProvider;
    private readonly IInfoProvider<FormNotificationInfo> _formNotificationInfoProvider;
    private readonly IPageLinkGenerator _pageLinkGenerator;

    public FormEmailTemplateListingPage(IInfoProvider<FormEmailTemplateInfo> formEmailTemplateInfoProvider,
        IInfoProvider<FormNotificationInfo> formNotificationInfoProvider,
        IPageLinkGenerator pageLinkGenerator)
    {
        _formEmailTemplateInfoProvider = formEmailTemplateInfoProvider;
        _formNotificationInfoProvider = formNotificationInfoProvider;
        _pageLinkGenerator = pageLinkGenerator;
    }

    protected override string ObjectType => FormEmailTemplateInfo.OBJECT_TYPE;

    public override Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(FormEmailTemplateInfo.FormEmailTemplateDisplayName), "Name")
            .AddColumn(nameof(FormEmailTemplateInfo.FormEmailTemplateSubject), "Default subject")
            .AddColumn(nameof(FormEmailTemplateInfo.FormEmailTemplateSender), "Sender");

        PageConfiguration.HeaderActions.AddLink<FormEmailTemplateCreate>("New form email template");
        PageConfiguration.AddEditRowAction<FormEmailTemplateEdit>();

        PageConfiguration.TableActions.AddDeleteAction("Delete");

        return base.ConfigurePage();
    }

    [PageCommand]
    public async Task<ICommandResponse> Delete(int id)
    {
        var formEmailTemplate = _formEmailTemplateInfoProvider.Get()
            .WhereEquals(nameof(FormEmailTemplateInfo.FormEmailTemplateId), id)
            .TypedResult
            .FirstOrDefault();

        var listingPage = _pageLinkGenerator.GetPath(typeof(FormEmailTemplateListingPage));
        var response = NavigateTo(listingPage);

        if (formEmailTemplate != null)
        {
            var countFormsInUse = await _formNotificationInfoProvider.Get()
                .Where(where => where
                    .WhereEquals(nameof(FormNotificationInfo.FormNotificationEmailAutoresponderEmailTemplate), formEmailTemplate.FormEmailTemplateName)
                    .Or()
                    .WhereEquals(nameof(FormNotificationInfo.FormNotificationEmailNotificationEmailTemplate), formEmailTemplate.FormEmailTemplateName))
                .GetCountAsync();

            if (countFormsInUse > 0)
            {
                return response.AddWarningMessage($"Could not delete as template is in use on {countFormsInUse} forms");
            }

            await _formEmailTemplateInfoProvider.DeleteAsync(formEmailTemplate);
        }

        return response.AddSuccessMessage("Form email template was deleted");
    }
}