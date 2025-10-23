using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using System.Threading.Tasks;
using XperienceCommunity.FormNotifications.Models;
using XperienceCommunity.FormNotifications.UI;

[assembly: UIPage(typeof(FormsApplication), "email-templates", typeof(FormEmailTemplateListingPage), "Email templates", TemplateNames.LISTING, 500, Icon = "xp-message")]

namespace XperienceCommunity.FormNotifications.UI;

public class FormEmailTemplateListingPage : ListingPage
{
    protected override string ObjectType => FormEmailTemplateInfo.OBJECT_TYPE;

    public override Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(FormEmailTemplateInfo.FormEmailTemplateDisplayName), "Name")
            .AddColumn(nameof(FormEmailTemplateInfo.FormEmailTemplateSubject), "Default subject")
            .AddColumn(nameof(FormEmailTemplateInfo.FormEmailTemplateSender), "Sender");

        PageConfiguration.HeaderActions.AddLink<FormEmailTemplateCreate>("New form email template");
        PageConfiguration.AddEditRowAction<FormEmailTemplateEdit>();

        return base.ConfigurePage();
    }
}