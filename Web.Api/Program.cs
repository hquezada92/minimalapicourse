using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Web.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<PeopleService>();
builder.Services.AddSingleton<GuidGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

//Object result sync
app.MapGet("ok-object", () => Results.Ok(new { Name = "Alex Quezada" }))
    .WithName("GetOkObject")
    .WithOpenApi();

//Object result async
app.MapGet("slow-request", async () =>
    {
        await Task.Delay(1000);
        return Results.Ok(new { Name = "Alex Quezada" });
    })
    .WithName("GetSlowRequest")
    .WithOpenApi();

//Request mapping
app.MapGet("get", () => "This is a get");
app.MapGet("post", () => "This is a post");
app.MapGet("put", () => "This is a put");
app.MapGet("delete", () => "This is a delete");

//Not natively implemented methods
app.MapMethods("options-or-head", new[] { "HEAD", "OPTIONS" },
    () => "Hello from either options or head");

//Route parameters
app.MapGet("get-params/{age:int}", (int age) =>
    {
        return $"Age provided was {age}";
    })
    .WithName("GetRouteParams")
    .WithOpenApi();

app.MapGet("cars/{carId:regex(^[a-z0-9]+$)}", (string carId) =>
    {
        return $"Car id provided was {carId}";
    })
    .WithName("GetCars")
    .WithOpenApi();

app.MapGet("books/{isbn:length(13)}", (string isbn) =>
    {
        return $"ISBN provided was {isbn}";
    })
    .WithName("GetBooks")
    .WithOpenApi();

//parameter binding query params
app.MapGet("people/search", (string? searchTerm, PeopleService peopleService) =>
    {
        if (searchTerm is null)
            return Results.NotFound();
        var result = peopleService.Search(searchTerm);
        return Results.Ok(result);
    })
    .WithName("GetPeople")
    .WithOpenApi();

app.MapGet("mix/{routeParam}", (
            //Can also specify where it comes from [FromRoute]
            string routeParam, 
            //Can also specify where it comes from [FromQuery]
            //usefull to define different param name [FromQuery(Name="Name")]
            int queryParam, 
            //Can also specify where it comes from [FromServices]
            GuidGenerator guidGenerator,
            //If you want to pull anything in the headers of the request
            [FromHeader(Name = "Accept-Encoding")] string encoding) => 
        Results.Ok($"{routeParam} {queryParam} {guidGenerator.NewGuid} {encoding}"))
    .WithName("GetMix")
    .WithOpenApi();

//Request body
app.MapPost("people", (
        //This can also be specified to come [FromBody] if u want to
        Person person) =>
{
    return Results.Ok(person);
})
.WithName("GetPeopleBody")
.WithOpenApi();

//Special params
app.MapGet("httpcontext-1", async context =>
    {
        await context.Response.WriteAsync("Hello world from HttpContext 1");
    })
    .WithName("GetContext")
    .WithOpenApi();

app.MapGet("http", async (HttpRequest request, HttpResponse response) =>
    {
        var queries = request.QueryString.Value;
        await response.WriteAsync($"Hello world from HttpResponse. Queries were: {queries}");
    })
    .WithName("GetRequestResponse")
    .WithOpenApi();

app.MapGet("claims", async (ClaimsPrincipal user) =>
    {
        
    })
    .WithName("GetUserClaims")
    .WithOpenApi();

app.MapGet("cancel", (CancellationToken token) =>
{
    return Results.Ok();
})
.WithName("GetCancel")
.WithOpenApi();

//Custom parameter binding
app.MapGet("map-point", (MapPoint point) =>
    {
        return Results.Ok(point);
    })
    .WithName("GetMapPoint")
    .WithOpenApi();

//Available response types
app.MapGet("simple-string", () => "Hello world");
app.MapGet("json-raw-obj", () => new { message = "Hello json raw object" });
app.MapGet("ok-obj", () => Results.Ok(new { message = "Hello ok object" }));
app.MapGet("json-obj", () => Results.Json(new { message = "Hello from json result object" }));
app.MapGet("text-string", () => Results.Text("Hello from text result object" ));
app.MapGet("stream-result", () =>
{
    var memoryStream = new MemoryStream();
    var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
    streamWriter.Write("Hello world from stream");
    streamWriter.Flush();
    memoryStream.Seek(0, SeekOrigin.Begin);
    return Results.Stream(memoryStream);
});
app.MapGet("redirect", () => Results.Redirect("https://google.com"));
app.MapGet("download", () => Results.File("/myFile.txt"));

//Logging
app.MapGet("logging", (ILogger<Program> logger) =>
{
    logger.LogInformation("Hello from endpoint");
    return Results.Ok();
});
app.Run();
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}