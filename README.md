# Xperience Community: Form Notifications

## Description

This package provides Xperience by Kentico administrators with an interface to manage form email notifications. While Kentico has developed autoresponder management into the Xperience by Kentico solution, this package was developed as an alternative to the current implementation specifically for developers upgrading clients from KX13 and older instances where users were able to manage email content and configuration more easily within the administration interface.

One particular feature of note within this extension is that it has been extended to support the use of adding form data into email templates by way of macro expressions. Similar to the behaviour of KX13 and prior versions, macro expressions defined in the recipients, subject or the content of the email will be resolved where they match a field name set within the form structure.

![Xperience by Kentico Form Notifications](https://raw.githubusercontent.com/benquinlan-07/xperience-community-form-notifications/refs/heads/main/images/form-notifications.jpeg)

## Requirements

### Dependencies

- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.kentico.com)

## Package Installation

Add the package to your application using the .NET CLI

```
dotnet add package XperienceCommunity.FormNotifications
```

or via the Package Manager

```
Install-Package XperienceCommunity.FormNotifications
```

## Quick Start

1. Install the NuGet package.

1. Update your Program.cs to register the necessary services.

```csharp
    using XperienceCommunity.FormNotifications;

    ...

    builder.Services.AddFormNotificationsExtensionServices();
```

## Full Instructions

1. Start your XbyK website.

1. Log in to the administration site.

1. Create or edit a form.

1. Select the Emails option from the menu on the right side of the screen.
![Xperience by Kentico Form Notifications](https://raw.githubusercontent.com/benquinlan-07/xperience-community-form-notifications/refs/heads/main/images/form-notifications.jpeg)

1. Configure your preferred notification settings.

1. Submit the form.

1. Watch the magic happen.