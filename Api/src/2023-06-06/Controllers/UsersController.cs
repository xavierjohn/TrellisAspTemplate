namespace BestWeatherForecast.Api._2023_06_06.Controllers;

using Asp.Versioning;
using BestWeatherForecast.Api._2023_06_06.Models;
using BestWeatherForecast.Domain;
using Microsoft.AspNetCore.Mvc;
using Trellis.Asp;
using Trellis.Primitives;


/// <summary>
/// User management controller.
/// </summary>
[ApiController]
[ApiVersion("2023-06-06")]
[Consumes("application/json")]
[Produces("application/json")]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult<User> RegisterUser([FromBody] RegisterUserRequest request) =>
        FirstName.TryCreate(request.FirstName)
        .Combine(LastName.TryCreate(request.LastName))
        .Combine(EmailAddress.TryCreate(request.Email))
        .Bind((firstName, lastName, email) => Domain.User.TryCreate(firstName, lastName, email, request.Password))
        .ToActionResult(this);

    /// <summary>
    /// Get a greeting for the specified user.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpGet("{name}")]
    public ActionResult<string> Get(string name) => Ok($"Hello {name}!");

    /// <summary>
    /// Delete a user by ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public ActionResult<Unit> Delete(string id) =>
        UserId.TryCreate(id).Match(
            ok => NoContent(),
            err => err.ToActionResult<Unit>(this));
}
