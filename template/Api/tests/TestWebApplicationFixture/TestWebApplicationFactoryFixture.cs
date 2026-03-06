namespace Api.Tests;

using System;
using Application.Tests;
using BestWeatherForecast.Application.Abstractions;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit.Sdk;
using Xunit.v3;

public class TestWebApplicationFactoryFixture : WebApplicationFactory<Program>, ITestOutputHelperAccessor
{
    private readonly bool _useRealServices;

    public TestWebApplicationFactoryFixture()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        _useRealServices = Environment.GetEnvironmentVariable("USE_REAL_SERVICES") == "true";
        if (_useRealServices)
            TestContext.Current.SendDiagnosticMessage("Using real services");
        else
            TestContext.Current.SendDiagnosticMessage("Using mock services");
    }

    public ITestOutputHelper? OutputHelper { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging((p) => p.AddXUnit(this));

        builder.ConfigureServices(services =>
            {
                if (_useRealServices)
                    return;

                services.RemoveAll<IWeatherForecastService>();
                services.AddMockAntiCorruptionLayer();
            });
    }
}

[CollectionDefinition(Id)]
public class TestWebApplicationFactoryCollectionFixture : ICollectionFixture<TestWebApplicationFactoryFixture>
{
    public const string Id = "Test web application factory fixture collection";
}
