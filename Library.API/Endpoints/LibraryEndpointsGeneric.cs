using Library.API.Endpoints.Internal;
using Library.API.Services;

namespace Library.API.Endpoints;

public class LibraryEndpointsGeneric : IEndpoints
{
    public static void DefineEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("hello", () => "Hello world");
    }

    public static void AddServices(IServiceCollection services)
    {
        services.AddSingleton<IBookService, BookService>();
    }
}