// Ignore Spelling: Api

namespace BestWeatherForecast.Api;

using System.Diagnostics.Metrics;

/// <summary>
/// Defines the metrics meter for the API.
/// </summary>
public class ApiMeters
{
    /// <summary>
    /// The name of the meter.
    /// </summary>
    public const string MeterName = "ApiMeter";

    /// <summary>
    /// Gets the meter.
    /// </summary>
    public Meter Meter { get; } = new Meter(MeterName);
}
