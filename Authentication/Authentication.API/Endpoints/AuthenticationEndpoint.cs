using Carter;

namespace Authentication.API.Endpoints;

public class AuthenticationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("init", HelloWorld);
    }

    private IResult HelloWorld(HttpContext context)
    {
        return  Results.Ok("Hello World!");
    }
}