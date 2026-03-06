namespace BestWeatherForecast.Api;

using Microsoft.Extensions.Options;
using ServiceLevelIndicators;

internal sealed class ConfigureServiceLevelIndicatorOptions : IConfigureOptions<ServiceLevelIndicatorOptions>
{
    private readonly ApiMeters meters;

    public ConfigureServiceLevelIndicatorOptions(ApiMeters meters) => this.meters = meters;

    public void Configure(ServiceLevelIndicatorOptions options) => options.Meter = meters.Meter;
}
