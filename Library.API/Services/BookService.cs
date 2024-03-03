using Dapper;
using Library.API.Data;
using Library.API.Models;

namespace Library.API.Services;

public class BookService : IBookService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BookService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(Book book)
    {
        var existingBook = await GetByIsbnAsync(book.Isbn);
        if (existingBook is not null)
            return false;
        using var connection = await _connectionFactory.IDbCreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"INSERT INTO Books (Isbn, Title, Author, ShortDescription, PageCount,ReleaseDate)
                VALUES (@Isbn, @Title, @Author, @ShortDescription, @PageCount,@ReleaseDate)",
            book);
        return result > 0;
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        using var connection = await _connectionFactory.IDbCreateConnectionAsync();
        var result = await connection.QuerySingleOrDefaultAsync<Book>(
            @"SELECT Isbn,Title,Author,ShortDescription,PageCount,ReleaseDate
                FROM Books WHERE Isbn = @Isbn LIMIT 1",new {Isbn=isbn});
        return result;
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        using var connection = await _connectionFactory.IDbCreateConnectionAsync();
        return await connection.QueryAsync<Book>("SELECT * FROM Books");
    }

    public async Task<IEnumerable<Book>> SearchByTitleAsync(string searchString)
    {
        using var connection = await _connectionFactory.IDbCreateConnectionAsync();
        return await connection.QueryAsync<Book>(
            @"SELECT * FROM Books WHERE Title LIKE '%' || @SearchTerm || '%'",
            new { SearchTerm = searchString });
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        var existingBook = await GetByIsbnAsync(book.Isbn);
        if (existingBook is null)
            return false;
        
        using var connection = await _connectionFactory.IDbCreateConnectionAsync();
        var updated = await connection.ExecuteAsync(
            @"UPDATE Books SET 
                 Title = @Title, 
                 Author = @Author, 
                 ShortDescription = @ShortDescription,
                 PageCount = @PageCount,
                 ReleaseDate = @ReleaseDate 
                 WHERE Isbn = @Isbn", book);
        return updated > 0;
    }

    public async Task<bool> DeleteAsync(string isbn)
    {
        using var connection = await _connectionFactory.IDbCreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"DELETE FROM Books WHERE Isbn = @Isbn", new { Isbn = isbn });
        return result > 0;
    }
}