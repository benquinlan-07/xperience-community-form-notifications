using CMS.FormEngine;
using Kentico.Forms.Web.Mvc;

namespace XperienceCommunity.FormNotifications.Extensions
{
    internal static class FormFieldExtensions
    {
        public static bool IsFileUpload(this FormFieldInfo formField)
        {
            return GetComponentIdentifier(formField) == FileUploaderComponent.IDENTIFIER;
        }

        private static string GetComponentIdentifier(FormFieldInfo formField)
        {
            return formField?.Settings != null && formField.Settings.ContainsKey("componentidentifier")
                ? formField.Settings["componentidentifier"] as string
                : null;
        }
    }
}
