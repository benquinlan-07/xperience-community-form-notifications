using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace XperienceCommunity.FormNotifications.Controls;

/// <summary>
/// Indicates that the DropDown form component will be used for editing of this property value in the administration interface.
/// </summary>
/// <remarks>
/// The underlying property must be of the type '<see cref="T:System.String" />'.
/// </remarks>
public class DynamicOptionsDropDownComponentAttribute : FormComponentAttribute
{
	/// <summary>Text for the placeholder.</summary>
	public string Placeholder { get; set; }
}