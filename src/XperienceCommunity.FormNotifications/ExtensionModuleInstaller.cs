using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;
using XperienceCommunity.FormNotifications.Models;

namespace XperienceCommunity.FormNotifications;

internal class ExtensionModuleInstaller
{
    private readonly IInfoProvider<ResourceInfo> _resourceProvider;

    public ExtensionModuleInstaller(IInfoProvider<ResourceInfo> resourceProvider)
    {
        _resourceProvider = resourceProvider;
    }

    public void Install()
    {
        var resource = _resourceProvider.Get(Constants.ResourceName)
                       ?? new ResourceInfo();

        InitializeResource(resource);
        InstallFormNotificationInfo(resource);
        InstallFormEmailTemplateInfo(resource);
    }

    public ResourceInfo InitializeResource(ResourceInfo resource)
    {
        resource.ResourceDisplayName = Constants.ResourceDisplayName;
        resource.ResourceName = Constants.ResourceName;
        resource.ResourceDescription = Constants.ResourceDescription;
        resource.ResourceIsInDevelopment = false;

        if (resource.HasChanged)
            _resourceProvider.Set(resource);

        return resource;
    }

    public void InstallFormNotificationInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(FormNotificationInfo.OBJECT_TYPE) ?? DataClassInfo.New(FormNotificationInfo.OBJECT_TYPE);

        info.ClassName = FormNotificationInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = FormNotificationInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = FormNotificationInfo.OBJECT_CLASS_DISPLAYNAME;
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(FormNotificationInfo.FormNotificationId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationFormId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = DataClassInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationSendEmailNotification),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "boolean"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailAutoresponderRecipientEmailField),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailNotificationRecipient),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailNotificationSubject),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailNotificationTemplate),
            AllowEmpty = true,
            Visible = true,
            Precision = 0,
            DataType = "guid",
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailNotificationEmailTemplate),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailNotificationEmailMessage),
            AllowEmpty = true,
            Visible = true,
            DataType = "longtext"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailNotificationIncludeAttachments),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "boolean"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationSendEmailAutoresponder),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "boolean"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailAutoresponderSubject),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailAutoresponderTemplate),
            AllowEmpty = true,
            Visible = true,
            Precision = 0,
            DataType = "guid",
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailAutoresponderEmailTemplate),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailAutoresponderEmailMessage),
            AllowEmpty = true,
            Visible = true,
            DataType = "longtext"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormNotificationInfo.FormNotificationEmailAutoresponderIncludeAttachments),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "boolean"
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallFormEmailTemplateInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(FormEmailTemplateInfo.OBJECT_TYPE) ?? DataClassInfo.New(FormEmailTemplateInfo.OBJECT_TYPE);

        info.ClassName = FormEmailTemplateInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = FormEmailTemplateInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = FormEmailTemplateInfo.OBJECT_CLASS_DISPLAYNAME;
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(FormEmailTemplateInfo.FormEmailTemplateId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(FormEmailTemplateInfo.FormEmailTemplateGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormEmailTemplateInfo.FormEmailTemplateName),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormEmailTemplateInfo.FormEmailTemplateDisplayName),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormEmailTemplateInfo.FormEmailTemplateSender),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormEmailTemplateInfo.FormEmailTemplateSubject),
            AllowEmpty = true,
            Visible = true,
            Size = 500,
            DataType = "text"
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(FormEmailTemplateInfo.FormEmailTemplateSourceCode),
            AllowEmpty = true,
            Visible = true,
            DataType = "longtext"
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}