// IngilizceProje/MAUIClient/Converters/EqualValueConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace IngilizceProjeMAUI.Converters
{
    public class EqualValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null && parameter == null) return true;
            if (value == null || parameter == null) return false;

            string strValue = value.ToString() ?? string.Empty;
            string strParam = parameter.ToString() ?? string.Empty;

            // Check if parameter has pipe syntax: "MatchValue:TrueReturn|FalseReturn"
            // Example: "Words:#4F46E5|Transparent"
            if (strParam.Contains(':') && strParam.Contains('|'))
            {
                var colonParts = strParam.Split(':', 2);
                string targetMatch = colonParts[0];
                var pipeParts = colonParts[1].Split('|', 2);
                string trueVal = pipeParts[0];
                string falseVal = pipeParts[1];

                bool isMatch = string.Equals(strValue, targetMatch, StringComparison.OrdinalIgnoreCase);
                string chosenStr = isMatch ? trueVal : falseVal;

                if (targetType == typeof(Color))
                {
                    if (chosenStr.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                        return Colors.Transparent;
                    return Color.FromArgb(chosenStr);
                }

                if (targetType == typeof(bool))
                {
                    return bool.TryParse(chosenStr, out bool b) && b;
                }

                return chosenStr;
            }

            return string.Equals(strValue, strParam, StringComparison.OrdinalIgnoreCase);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
