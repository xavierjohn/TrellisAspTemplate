namespace BestWeatherForecast.Domain;

using FluentValidation;
using Trellis.FluentValidation;

public class ZipCode : ScalarValueObject<ZipCode, string>,
    IScalarValue<ZipCode, string>
{
    private ZipCode(string value) : base(value)
    {
    }

    public static Result<ZipCode> TryCreate(string value, string? fieldName = null)
    {
        var zipCode = new ZipCode(value);
        return s_validation.ValidateToResult(zipCode);
    }

    static readonly InlineValidator<ZipCode> s_validation = new()
    {
        v => v.RuleFor(x => x.Value)
        .NotEmpty().OverridePropertyName("zipCode")
        .Matches(@"^\d{5}(?:[-\s]\d{4})?$").OverridePropertyName("zipCode") //US Zip codes.
    };
}
