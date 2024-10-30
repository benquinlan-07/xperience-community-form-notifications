using CMS.Core;
using Kentico.Xperience.Admin.Base.Forms;

namespace XperienceCommunity.FormNotifications.ValidationRules;

internal class RequiredIfTrueValidationRuleProperties : ValidationRuleProperties
{
    public string ConditionalFieldName { get; set; }

    public string FieldName { get; set; }

    public override string GetDescriptionText(ILocalizationService localizationService)
    {
        return localizationService.GetString("base.forms.validationrule.requiredvalue.label");
    }
}