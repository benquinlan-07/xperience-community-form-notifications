using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using System.Linq;
using XperienceCommunity.FormNotifications.Models;
using XperienceCommunity.FormNotifications.UI;

[assembly: UIPage(typeof(FormEmailTemplateListingPage), PageParameterConstants.PARAMETERIZED_SLUG, typeof(FormEmailTemplateEdit), "Edit email template", TemplateNames.EDIT, 100, Icon = "xp-message")]

namespace XperienceCommunity.FormNotifications.UI;

public class FormEmailTemplateEdit : FormEmailTemplateCreate
{
    private readonly IInfoProvider<FormEmailTemplateInfo> _formEmailTemplateInfoProvider;

    public FormEmailTemplateEdit(IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IInfoProvider<FormEmailTemplateInfo> formEmailTemplateInfoProvider)
        : base(formItemCollectionProvider, formDataBinder, formEmailTemplateInfoProvider)
    {
        _formEmailTemplateInfoProvider = formEmailTemplateInfoProvider;
    }

    [PageParameter(typeof(IntPageModelBinder))]
    public int ObjectId { get; set; }

    protected override FormEmailTemplateInfo GetFormEmailTemplateInfo()
    {
        return _formEmailTemplateInfoProvider.Get()
            .WhereEquals(nameof(FormEmailTemplateInfo.FormEmailTemplateId), ObjectId)
            .First();
    }
}