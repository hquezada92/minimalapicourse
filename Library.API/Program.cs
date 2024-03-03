using FluentValidation;
using FluentValidation.Results;
using Library.API;
using Library.API.Auth;
using Library.API.Data;
using Library.API.Endpoints;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AnyOrigin", x => x.AllowAnyOrigin());
});
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddAuthentication(ApiKeySchemeConstant.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstant.SchemeName, _ => { });
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")
        ));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddLibraryEndpoints();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseLibraryEndpoints();
app.MapGet("status", 
    //[EnableCors("AnyOrigin")]//Just a decoration alternative
    () =>
{
    return Results.Extensions.Html(@"<!doctype html>
<html>
    <head><title>Status page</title></head>
    <body>
        <h1>Status</h1>
        <p>The server is working fine.</p>
    </body>
</html>");
})
    .ExcludeFromDescription()//just to exclude this from showing up in swagger
    .RequireCors("AnyOrigin");

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();
app.UseHttpsRedirection();
//DB init

app.Run();