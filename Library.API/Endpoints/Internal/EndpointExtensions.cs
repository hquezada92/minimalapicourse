namespace Library.API.Endpoints.Internal;

public static class EndpointExtensions
{
    public static void AddEndPoints<TMarker>(this IServiceCollection services,
        IConfiguration configuration)
    {
        AddEndPoints(services,typeof(TMarker),configuration);
    }
    public static void AddEndPoints(this IServiceCollection services,
        Type typeMarker, IConfiguration configuration)
    {
        
    }
}