using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace JobApplicationHelper.Extensions;

public static class EnumExtensions
{
    public static string ToDisplayValue<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        var member = typeof(TEnum)
            .GetMember(value.ToString())
            .FirstOrDefault();

        var displayAttribute = member?.GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.GetName()
            ?? value.ToString();
    }
}