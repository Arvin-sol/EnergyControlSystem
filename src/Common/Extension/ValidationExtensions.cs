

namespace Common.Extension;

public static class ValidationExtensions
{
    public static bool IsValidEnum<TEnum>(this string value, out TEnum result) where TEnum : struct => Enum.TryParse(value, out result);

    public static bool IsValidDateTime(this string value, out DateTime result) => DateTime.TryParse(value, out result);

    public static bool IsValidDecimal(this string value, out decimal result) => decimal.TryParse(value, out result);

}
