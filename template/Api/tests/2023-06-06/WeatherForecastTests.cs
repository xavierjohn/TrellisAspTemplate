namespace Api.Tests._2023_06_06;

using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[Collection(TestWebApplicationFactoryCollectionFixture.Id)]
public class WeatherForecastTests
{
    private readonly TestWebApplicationFactoryFixture _factory;

    public WeatherForecastTests(TestWebApplicationFactoryFixture factory, ITestOutputHelper output)
    {
        _factory = factory;
        _factory.OutputHelper = output;
    }

    [Fact]
    public async Task Get_forecast_for_Redmond()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/WeatherForecast/?api-version=2023-06-06", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var forecast = await response.Content.ReadAsAsyncWithAssertion<WeatherForecast>();
        forecast.Should().NotBeNull();
        forecast.zipCode.Should().Be("98052");
        var dailyTemperatures = forecast.dailyTemperatures;
        Assert.NotNull(dailyTemperatures);

        dailyTemperatures.Length.Should().Be(3);
        var day = dailyTemperatures[0];
        day.date.Should().Be(new DateOnly(2023, 6, 6));
        day.temperatureC.Should().Be(10);
        day.temperatureF.Should().BeApproximately(50, 0.001);
        day.summary.Should().Be("Sunny");

        day = dailyTemperatures[1];
        day.date.Should().Be(new DateOnly(2023, 6, 7));
        day.temperatureC.Should().Be(20);
        day.temperatureF.Should().BeApproximately(68.0, 0.001);
        day.summary.Should().Be("Cloudy");

        day = dailyTemperatures[2];
        day.date.Should().Be(new DateOnly(2023, 6, 8));
        day.temperatureC.Should().Be(30);
        day.temperatureF.Should().BeApproximately(86.0, 0.001);
        day.summary.Should().Be("Rainy");
    }

    [Fact]
    public async Task Get_forecast_for_Irvine()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/WeatherForecast/75014?api-version=2023-06-06", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var forecast = await response.Content.ReadAsAsyncWithAssertion<WeatherForecast>();
        forecast.Should().NotBeNull();
        forecast.zipCode.Should().Be("75014");
        var dailyTemperatures = forecast.dailyTemperatures;
        Assert.NotNull(dailyTemperatures);

        dailyTemperatures.Length.Should().Be(3);
        var day = dailyTemperatures[0];
        day.date.Should().Be(new DateOnly(2023, 6, 6));
        day.temperatureC.Should().Be(10);
        day.temperatureF.Should().BeApproximately(50, 0.001);
        day.summary.Should().Be("Sunny");

        day = dailyTemperatures[1];
        day.date.Should().Be(new DateOnly(2023, 6, 7));
        day.temperatureC.Should().Be(20);
        day.temperatureF.Should().BeApproximately(68.0, 0.001);
        day.summary.Should().Be("Cloudy");

        day = dailyTemperatures[2];
        day.date.Should().Be(new DateOnly(2023, 6, 8));
        day.temperatureC.Should().Be(30);
        day.temperatureF.Should().BeApproximately(86.0, 0.001);
        day.summary.Should().Be("Rainy");
    }

    [Fact]
    public async Task Unknown_zip_code_will_return_NotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/WeatherForecast/12345?api-version=2023-06-06", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        problemDetails.Should().NotBeNull();
        Assert.NotNull(problemDetails);
        problemDetails.Status.Should().Be(404);
        problemDetails.Title.Should().Be("Not Found");
        problemDetails.Detail.Should().Be("No weather forecast found for the zip code.");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.5");
        problemDetails.Instance.Should().Be("12345");
    }
}
