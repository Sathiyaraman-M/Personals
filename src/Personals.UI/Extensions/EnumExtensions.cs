using System.ComponentModel.DataAnnotations;

namespace Personals.UI.Extensions;

public static class EnumExtensions
{
    public static string ToDescriptionString(this Enum val)
    {
        var fieldInfo = val.GetType().GetField(val.ToString());
        if (fieldInfo == null) return "";
        var attributes = (DisplayAttribute[])fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false);

        return attributes.Length > 0
            ? attributes[0].Name ?? val.ToString()
            : val.ToString();
    }
}