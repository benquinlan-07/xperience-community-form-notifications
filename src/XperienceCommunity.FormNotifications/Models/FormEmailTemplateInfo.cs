using System;
using System.Data;
using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.FormNotifications.Models;

[assembly: RegisterObjectType(typeof(FormEmailTemplateInfo), FormEmailTemplateInfo.OBJECT_TYPE)]

namespace XperienceCommunity.FormNotifications.Models;

/// <summary>
/// Data container class for <see cref="FormEmailTemplateInfo"/>.
/// </summary>
[Serializable]
public partial class FormEmailTemplateInfo : AbstractInfo<FormEmailTemplateInfo, IInfoProvider<FormEmailTemplateInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "xpcm.formemailtemplate";
    public const string OBJECT_CLASS_NAME = "XPCM.FormEmailTemplate";
    public const string OBJECT_CLASS_DISPLAYNAME = "Form Email Template";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<FormEmailTemplateInfo>), OBJECT_TYPE, OBJECT_CLASS_NAME, nameof(FormEmailTemplateId), null, nameof(FormEmailTemplateGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };


    /// <summary>
    /// Form Email Template id.
    /// </summary>
    [DatabaseField]
    public virtual int FormEmailTemplateId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(FormEmailTemplateId)), 0);
        set => SetValue(nameof(FormEmailTemplateId), value);
    }


    /// <summary>
    /// Form Email Template Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid FormEmailTemplateGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(FormEmailTemplateGuid)), default);
        set => SetValue(nameof(FormEmailTemplateGuid), value);
    }


    /// <summary>
    /// Form Email Template path.
    /// </summary>
    [DatabaseField]
    public virtual string FormEmailTemplateName
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormEmailTemplateName)), default);
        set => SetValue(nameof(FormEmailTemplateName), value);
    }


    /// <summary>
    /// Form Email Template path.
    /// </summary>
    [DatabaseField]
    public virtual string FormEmailTemplateDisplayName
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormEmailTemplateDisplayName)), default);
        set => SetValue(nameof(FormEmailTemplateDisplayName), value);
    }


    /// <summary>
    /// Form Email Template path.
    /// </summary>
    [DatabaseField]
    public virtual string FormEmailTemplateSourceCode
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormEmailTemplateSourceCode)), default);
        set => SetValue(nameof(FormEmailTemplateSourceCode), value);
    }


    /// <summary>
    /// Form Email Template path.
    /// </summary>
    [DatabaseField]
    public virtual string FormEmailTemplateSubject
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormEmailTemplateSubject)), default);
        set => SetValue(nameof(FormEmailTemplateSubject), value);
    }


    /// <summary>
    /// Form Email Template path.
    /// </summary>
    [DatabaseField]
    public virtual string FormEmailTemplateSender
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormEmailTemplateSender)), default);
        set => SetValue(nameof(FormEmailTemplateSender), value);
    }


    /// <summary>
    /// Deletes the object using appropriate provider.
    /// </summary>
    protected override void DeleteObject()
    {
        Provider.Delete(this);
    }


    /// <summary>
    /// Updates the object using appropriate provider.
    /// </summary>
    protected override void SetObject()
    {
        Provider.Set(this);
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="FormEmailTemplateInfo"/> class.
    /// </summary>
    public FormEmailTemplateInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="FormEmailTemplateInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public FormEmailTemplateInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}