using System.ComponentModel;

namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

[TypeConverter(typeof(TokenisableDateTimeConverter))]
public class TokenisableDateTime
{
    public TokenisableDateTime(DateTime? value)
    {
        DateTime = value;
    }

    public DateTime? DateTime { get; }

    public static TokenisableDateTime FromString(string value)
    {
        if (System.DateTime.TryParse(value, out var parseResult))
        {
            return new TokenisableDateTime(parseResult);
        }

        if (value.ToLower() == TokenisableYearConstants.Null.ToLower())
        {
            return new TokenisableDateTime(null);
        }

        if (value.ToLower() == TokenisableYearConstants.CurrentDate.ToLower())
        {
            return new TokenisableDateTime(System.DateTime.Now);
        }

        if (value.ToLower() == TokenisableYearConstants.NextMonthFirstDay.ToLower())
        {
            var nextMonth = System.DateTime.Now.AddMonths(1);
            var firstDayOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);
            return new TokenisableDateTime(firstDayOfNextMonth);
        }

        if (value.ToLower() == TokenisableYearConstants.LastDayOfCurrentMonth.ToLower())
        {
            var firstDayOfNextMonth = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1).AddMonths(1);
            var lastDayOfCurrentMonth = firstDayOfNextMonth.AddDays(-1);
            return new TokenisableDateTime(lastDayOfCurrentMonth);
        }

        var dateComponents = value.Split('-');

        if (dateComponents[0].ToLower() == TokenisableYearConstants.CurrentAyToken.ToLower())
        {
            return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents));
        }

        if (dateComponents[0].ToLower() == TokenisableYearConstants.PreviousAyToken.ToLower())
        {
            return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents).AddYears(-1));
        }

        if (dateComponents[0].ToLower() == TokenisableYearConstants.NextAyToken.ToLower())
        {
            return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents).AddYears(1));
        }

        if (dateComponents[0].ToLower() == TokenisableYearConstants.CurrentAyPlusTwoToken.ToLower())
        {
            return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents).AddYears(2));
        }

        if (dateComponents[0].ToLower() == TokenisableYearConstants.TwoYearsAgoAYToken.ToLower())
        {
            return new TokenisableDateTime(GetDateForCurrentAcademicYear(dateComponents).AddYears(-2));
        }

        throw new ArgumentException("Invalid date string format for TokenisableDateTime.");
    }

    private static DateTime GetDateForCurrentAcademicYear(string[] dateComponents)
    {
        if (!int.TryParse(dateComponents[1], out var month)) throw new ArgumentException("Invalid date string format for TokenisableDateTime: Invalid month.");
        if (!int.TryParse(dateComponents[2], out var day)) throw new ArgumentException("Invalid date string format for TokenisableDateTime: Invalid day.");

        int startYearOfCurrentAcademicYear = System.DateTime.Now.Month > 7 ? System.DateTime.Now.Year : System.DateTime.Now.Year - 1;
        int yearToUse = month > 7 ? startYearOfCurrentAcademicYear : startYearOfCurrentAcademicYear + 1;
        return new DateTime(yearToUse, month, day);
    }
}

#pragma warning disable CS8765, CS8603
public class TokenisableDateTimeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value is string str)
        {
            return TokenisableDateTime.FromString(str);
        }
        return base.ConvertFrom(context, culture, value);
    }
}
#pragma warning restore CS8765, CS8603

public static class TokenisableYearConstants
{
    public const string Null = "null";
    public const string CurrentAyToken = "currentAY";
    public const string PreviousAyToken = "previousAY";
    public const string TwoYearsAgoAYToken = "TwoYearsAgoAY";
    public const string NextAyToken = "nextAY";
    public const string CurrentAyPlusTwoToken = "currentAYPlusTwo";
    public const string CurrentDate = "currentDate";
    public const string NextMonthFirstDay = "nextMonthFirstDay";
    public const string LastDayOfCurrentMonth = "lastDayOfCurrentMonth";
}