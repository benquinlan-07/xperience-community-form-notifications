using CMS.Core;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XperienceCommunity.FormNotifications.Controls;

[assembly: RegisterFormComponent(DynamicOptionsDropDownComponent.IDENTIFIER, typeof(DynamicOptionsDropDownComponent), "DynamicOptionsDropDownComponent")]

namespace XperienceCommunity.FormNotifications.Controls
{
	/// <summary>Represents the drop-down component.</summary>
	[ComponentAttribute(typeof(DynamicOptionsDropDownComponentAttribute))]
	internal sealed class DynamicOptionsDropDownComponent : FormComponent<DropDownProperties, DropDownClientProperties, string>
	{
		private readonly ILocalizationService localizationService;
		private readonly IDropDownOptionsProviderActivator optionsProviderActivator;
		/// <summary>
		/// Represents the <see cref="T:Kentico.Xperience.Admin.Base.Forms.DropDownComponent" /> identifier.
		/// </summary>
		public const string IDENTIFIER = "XperienceCommunity.FormNotifications.DropDownSelector";

		/// <inheritdoc />
		public override string ClientComponentName => "@kentico/xperience-admin-base/DropDownSelector";

		/// <summary>
		/// Creates an instance of <see cref="T:Kentico.Xperience.Admin.Base.Forms.DropDownComponent" /> class.
		/// </summary>
		/// <param name="localizationService">The system localization service.</param>
		public DynamicOptionsDropDownComponent(ILocalizationService localizationService)
		  : this(localizationService, Service.Resolve<IDropDownOptionsProviderActivator>())
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="T:Kentico.Xperience.Admin.Base.Forms.DropDownComponent" /> class.
		/// </summary>
		/// <param name="localizationService">The system localization service.</param>
		/// <param name="optionsProviderActivator">The options provider activator.</param>
		internal DynamicOptionsDropDownComponent(
		  ILocalizationService localizationService,
		  IDropDownOptionsProviderActivator optionsProviderActivator)
		{
			this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
			this.optionsProviderActivator = optionsProviderActivator ?? throw new ArgumentNullException(nameof(optionsProviderActivator));
		}

		/// <inheritdoc />
		protected override async Task ConfigureClientProperties(DropDownClientProperties clientProperties)
		{
			clientProperties.Placeholder = !string.IsNullOrEmpty(Properties.Placeholder) ? localizationService.LocalizeString(Properties.Placeholder) : localizationService.GetString("base.forms.dropdown.placeholder");
			clientProperties.Options = await GetOptions();
			await base.ConfigureClientProperties(clientProperties);
		}

		private async Task<IEnumerable<DropDownOptionItem>> GetOptions()
		{
			return Array.Empty<DropDownOptionItem>();
		}

		/// <summary>
		/// Gets the value of the component.
		/// </summary>
		public override string GetValue()
		{
			string value = base.GetValue();
			return value;
		}

		/// <summary>
		/// Returns selected value no matter if the value is part of the options.
		/// </summary>
		internal string GetSelectedValue() => base.GetValue();
	}
}
