using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.FormAnnotations.Internal;

namespace XperienceCommunity.FormNotifications.UI;

[FormCategory(Label = "General", Order = 0)]
[FormCategory(Label = "Template", Order = 3)]
public class FormEmailTemplateModel
{
    [TextInputComponent(Label = "Name", Order = 1)]
    [RequiredValidationRule]
    public string DisplayName { get; set; }

    [CodeNameComponent(Label = "Identifier", Order = 2, HasAutomaticCodeNameGenerationOption = true)]
    [CodeNameValidationRule]
    public string Name { get; set; }

    [TextInputComponent(Label = "Subject", Order = 4)]
    [RequiredValidationRule]
    public string Subject { get; set; }

    [TextInputComponent(Label = "Sender", Order = 5)]
    [RequiredValidationRule]
    public string Sender { get; set; }

    [CodeEditorComponent(Label = "Source", Order = 6, ExplanationText = "Include the macro syntax {% message %} as a placeholder to output the message content.")]
    [RequiredValidationRule]
    public string Template { get; set; }
}