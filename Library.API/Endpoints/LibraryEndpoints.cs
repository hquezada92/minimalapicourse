using FluentValidation;
using FluentValidation.Results;
using Library.API.Models;
using Library.API.Services;

namespace Library.API.Endpoints;

public static class LibraryEndpoints
{
    public static void AddLibraryEndpoints(
        this IServiceCollection services)
    {
        services.AddSingleton<IBookService, BookService>();
    }

    public static void UseLibraryEndpoints(
        this IEndpointRouteBuilder app)
    {
        //Create book
        app.MapPost("books",
                //[Authorize(AuthenticationSchemes = ApiKeySchemeConstant.SchemeName)]
                //[AllowAnonymous]// If wanted to exclude an endpoint from authorization
                CreateBookAsync)
            .WithName("PostBookCreate")
            .Accepts<Book>("application/json")
            .Produces<Book>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithOpenApi()
            .WithTags("Books");
        //.AllowAnonymous();//same as the decorator

        //Get all books by search term or simply get all
        app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
            {
                if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
                {
                    var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
                    return Results.Ok(matchedBooks);
                }

                var books = await bookService.GetAllAsync();
                return Results.Ok(books);
            })
            .WithName("GetBooks")
            .WithOpenApi()
            .WithTags("Books");

        //Get a specific book by ISBN
        app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) =>
            {
                var book = await bookService.GetByIsbnAsync(isbn);
                return book is not null ? Results.Ok(book) : Results.NotFound();
            })
            .WithName("GetBook")
            .WithOpenApi()
            .WithTags("Books");

        //Edit a existing book
        app.MapPut("books/{isbn}", async (string isbn, Book book, IBookService bookService,
                IValidator<Book> validator) =>
            {
                book.Isbn = isbn;
                var validationResult = await validator.ValidateAsync(book);
                if (!validationResult.IsValid)
                    return Results.BadRequest(validationResult.Errors);

                var updated = await bookService.UpdateAsync(book);
                return updated ? Results.Ok(book) : Results.NotFound();
            })
            .WithName("PutBookUpdate")
            .WithOpenApi()
            .WithTags("Books");

        //Delete book
        app.MapDelete("books/{isbn}", async (string isbn, IBookService bookService) =>
            {
                var deleted = await bookService.DeleteAsync(isbn);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteBook")
            .WithOpenApi()
            .WithTags("Books");
    }

    private static async Task<IResult> CreateBookAsync(Book book, IBookService bookService,
        IValidator<Book> validator, LinkGenerator linker)
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);

        var created = await bookService.CreateAsync(book);
        if (!created)
            return Results.BadRequest(new List<ValidationFailure>
            {
                new("Isbn", "A book with this ISBN-13 already exists")
            });

        var path = linker.GetPathByName("GetBook", new { isbn = book.Isbn })!;
        return Results.Created(path, book);
        //return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book); //using CreateAtRoute
        //return Results.Created($"/books/{book.Isbn}", book); //Normal way to hardcode the created path
    }
}