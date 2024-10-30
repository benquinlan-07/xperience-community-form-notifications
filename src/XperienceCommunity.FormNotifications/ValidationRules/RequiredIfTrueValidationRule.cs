using Kentico.Xperience.Admin.Base.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMS.Core;
using XperienceCommunity.FormNotifications.ValidationRules;
using System.Collections;

[assembly: RegisterFormValidationRule(RequiredIfTrueValidationRule.Identifier, typeof(RequiredIfTrueValidationRule), "Required if true", "Makes a field required based on another field value being set to true")]

namespace XperienceCommunity.FormNotifications.ValidationRules
{
    [ValidationRuleAttribute(typeof(RequiredIfTrueValidationRuleAttribute))]
    internal class RequiredIfTrueValidationRule : ValidationRule<RequiredIfTrueValidationRuleProperties, object>
    {
        public const string Identifier = "XperienceCommunity.FormNotifications.RequiredIfTrueValidationRule";

        private readonly ILocalizationService _localizationService;

        public RequiredIfTrueValidationRule(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public override Task<ValidationResult> Validate(object value, IFormFieldValueProvider formFieldValueProvider)
        {
            // Check conditional field value
            formFieldValueProvider.TryGet<bool>(this.Properties.ConditionalFieldName, out var conditionalFieldValue);
            
            if (!conditionalFieldValue)
                return ValidationResult.SuccessResult();

            if (value == null || value is string str && string.IsNullOrWhiteSpace(str))
                return ValidationResult.FailResult();

            return value is IEnumerable source && !source.Cast<object>().Any<object>() ? ValidationResult.FailResult() : ValidationResult.SuccessResult();
        }

        /// <inheritdoc />
        protected override Func<string, string> ErrorMessageFormatter
        {
            get
            {
                return errorMessage => string.Format(errorMessage, _localizationService.LocalizeString(Properties.FieldName));
            }
        }

        protected override string DefaultErrorMessage => _localizationService.GetString("base.forms.validationrule.requiredvalue.errormessage");
    }
}
