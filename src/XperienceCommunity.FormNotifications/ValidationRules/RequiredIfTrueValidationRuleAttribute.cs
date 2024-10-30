using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace XperienceCommunity.FormNotifications.ValidationRules;

internal class RequiredIfTrueValidationRuleAttribute : ValidationRuleAttribute
{
    public string ConditionalFieldName { get; }

    public string FieldName { get; set; }

    public RequiredIfTrueValidationRuleAttribute(string conditionalFieldName)
    {
        ConditionalFieldName = conditionalFieldName;
    }
}