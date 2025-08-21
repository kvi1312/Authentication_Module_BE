using Authentication.Domain.Entities;
using Authentication.Domain.Interfaces;
using Carter;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Authentication.API.Endpoints;

public class UserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/hello", HelloWorld);
        app.MapGet("api/user/{userName}", GetUserByName).Produces((int)HttpStatusCode.OK, typeof(User));
    }

    private async Task<IResult> GetUserByName([Required] string userName, IUserServices userServices)
    {
        var user = await userServices.GetUserByName(userName);
        return Results.Ok(user);
    }

    private IResult HelloWorld(HttpContext context)
    {
        return  Results.Ok("Hello World!");
    }
}