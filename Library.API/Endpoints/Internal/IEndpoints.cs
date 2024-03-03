namespace Library.API.Endpoints.Internal;

public interface IEndpoints
{
    public static abstract void DefineEndpoint(IEndpointRouteBuilder app);
    public static abstract void AddServices(IServiceCollection services);
}