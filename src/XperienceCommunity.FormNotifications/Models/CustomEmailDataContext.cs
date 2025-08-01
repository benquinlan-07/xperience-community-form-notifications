using CMS.ContactManagement;
using CMS.EmailLibrary.Internal;
using System;
using System.Collections.Generic;

namespace XperienceCommunity.FormNotifications.Models;

internal class CustomEmailDataContext : IEmailDataContext
{
    /// <summary>
    /// Creates a new instance of <see cref="T:CMS.EmailLibrary.Internal.AbstractEmailDataContext" />.
    /// </summary>
    public CustomEmailDataContext() => this.MailoutGuid = Guid.NewGuid();

    /// <inheritdoc />
    public Guid MailoutGuid { get; }

    public Guid ContactGuid { get; }

    /// <summary>
    /// Email recipient, represents a contact that is not connected to a recipient list.
    /// </summary>
    public Recipient Recipient { get; set; }

    /// <inheritdoc />
    public string Resolve(string parameterName) => this.Recipient?.Resolve(parameterName);

    public bool ShouldBeHtmlEncoded(string parameterName) => false;

    /// <inheritdoc />
    public List<EmailDynamicTextPattern> GetAvailableMacros()
    {
        return Recipient.GetAvailableMacros();
    }

    /// <inheritdoc />
    public void InitWithFakeData()
    {
        this.Recipient = new Recipient();
        this.Recipient.InitWithFakeData();
    }
}
