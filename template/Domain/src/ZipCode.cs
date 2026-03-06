namespace BestWeatherForecast.Domain;

using System.Text.RegularExpressions;

public partial class ZipCode : ScalarValueObject<ZipCode, string>,
    IScalarValue<ZipCode, string>
{
    private ZipCode(string value) : base(value)
    {
    }

    public static Result<ZipCode> TryCreate(string value, string? fieldName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationError.For(fieldName ?? "zipCode", "Zip code is required.");

        if (!ZipCodePattern().IsMatch(value))
            return ValidationError.For(fieldName ?? "zipCode", "Invalid US zip code format.");

        return new ZipCode(value);
    }

    [GeneratedRegex(@"^\d{5}(?:[-\s]\d{4})?$")]
    private static partial Regex ZipCodePattern();
}
