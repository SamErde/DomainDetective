namespace DomainDetective;

using System;
using System.ComponentModel;
using System.Reflection;

/// <summary>
/// Provides helper methods for working with enum types.
/// </summary>
public static class EnumExtensions {
    /// <summary>
    /// Returns the <see cref="DescriptionAttribute"/> text for the enum value.
    /// </summary>
    /// <param name="value">Enum value.</param>
    /// <returns>Description text or the value name.</returns>
    public static string GetDescription(this Enum value) {
        var member = value.GetType().GetMember(value.ToString());
        if (member.Length > 0 &&
            Attribute.GetCustomAttribute(member[0], typeof(DescriptionAttribute)) is DescriptionAttribute attr) {
            return attr.Description;
        }

        return value.ToString();
    }
}
