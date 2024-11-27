using System;
using System.Data;
using System.Runtime.Serialization;
using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.FormNotifications.Models;

[assembly: RegisterObjectType(typeof(FormNotificationInfo), FormNotificationInfo.OBJECT_TYPE)]

namespace XperienceCommunity.FormNotifications.Models;

/// <summary>
/// Data container class for <see cref="FormNotificationInfo"/>.
/// </summary>
[Serializable]
public partial class FormNotificationInfo : AbstractInfo<FormNotificationInfo, IInfoProvider<FormNotificationInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "xpcm.formnotification";
    public const string OBJECT_CLASS_NAME = "XPCM.FormNotification";
    public const string OBJECT_CLASS_DISPLAYNAME = "Form Notification";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<FormNotificationInfo>), OBJECT_TYPE, OBJECT_CLASS_NAME, nameof(FormNotificationId), null, nameof(FormNotificationGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };


    /// <summary>
    /// Form notification id.
    /// </summary>
    [DatabaseField]
    public virtual int FormNotificationId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(FormNotificationId)), 0);
        set => SetValue(nameof(FormNotificationId), value);
    }


    /// <summary>
    /// Form notification Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid FormNotificationGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(FormNotificationGuid)), default);
        set => SetValue(nameof(FormNotificationGuid), value);
    }


    /// <summary>
    /// Form notification redirect method.
    /// </summary>
    [DatabaseField]
    public virtual int FormNotificationFormId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(FormNotificationFormId)), default);
        set => SetValue(nameof(FormNotificationFormId), value);
    }


    /// <summary>
    /// Form notification redirect method.
    /// </summary>
    [DatabaseField]
    public virtual bool FormNotificationSendEmailNotification
    {
        get => ValidationHelper.GetBoolean(GetValue(nameof(FormNotificationSendEmailNotification)), default);
        set => SetValue(nameof(FormNotificationSendEmailNotification), value);
    }


    /// <summary>
    /// Form notification path.
    /// </summary>
    [DatabaseField]
    public virtual string FormNotificationEmailNotificationRecipient
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormNotificationEmailNotificationRecipient)), default);
        set => SetValue(nameof(FormNotificationEmailNotificationRecipient), value);
    }


    /// <summary>
    /// Form notification path.
    /// </summary>
    [DatabaseField]
    public virtual string FormNotificationEmailNotificationSubject
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormNotificationEmailNotificationSubject)), default);
        set => SetValue(nameof(FormNotificationEmailNotificationSubject), value);
    }


    /// <summary>
    /// Form notification path.
    /// </summary>
    [DatabaseField]
    public virtual Guid FormNotificationEmailNotificationTemplate
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(FormNotificationEmailNotificationTemplate)), default);
        set => SetValue(nameof(FormNotificationEmailNotificationTemplate), value);
    }


    /// <summary>
    /// Form notification redirect method.
    /// </summary>
    [DatabaseField]
    public virtual bool FormNotificationSendEmailAutoresponder
    {
        get => ValidationHelper.GetBoolean(GetValue(nameof(FormNotificationSendEmailAutoresponder)), default);
        set => SetValue(nameof(FormNotificationSendEmailAutoresponder), value);
    }


    /// <summary>
    /// Form notification path.
    /// </summary>
    [DatabaseField]
    public virtual string FormNotificationEmailAutoresponderRecipientEmailField
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormNotificationEmailAutoresponderRecipientEmailField)), default);
        set => SetValue(nameof(FormNotificationEmailAutoresponderRecipientEmailField), value);
    }


    /// <summary>
    /// Form notification path.
    /// </summary>
    [DatabaseField]
    public virtual string FormNotificationEmailAutoresponderSubject
    {
        get => ValidationHelper.GetString(GetValue(nameof(FormNotificationEmailAutoresponderSubject)), default);
        set => SetValue(nameof(FormNotificationEmailAutoresponderSubject), value);
    }


    /// <summary>
    /// Form notification path.
    /// </summary>
    [DatabaseField]
    public virtual Guid FormNotificationEmailAutoresponderTemplate
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(FormNotificationEmailAutoresponderTemplate)), default);
        set => SetValue(nameof(FormNotificationEmailAutoresponderTemplate), value);
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
    /// Constructor for de-serialization.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    protected FormNotificationInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="FormNotificationInfo"/> class.
    /// </summary>
    public FormNotificationInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="FormNotificationInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public FormNotificationInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}