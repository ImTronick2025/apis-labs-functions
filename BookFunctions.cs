using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ApisLabs.Functions;

public class BookFunctions
{
    private readonly ILogger<BookFunctions> _logger;
    private readonly CosmosDbService _cosmosService;

    public BookFunctions(ILogger<BookFunctions> logger, CosmosDbService cosmosService)
    {
        _logger = logger;
        _cosmosService = cosmosService;
    }

    [Function("GetBooks")]
    public async Task<HttpResponseData> GetBooks(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "books")] HttpRequestData req)
    {
        _logger.LogInformation("Getting all books");

        try
        {
            var books = await _cosmosService.GetBooksAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(books);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting books");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("GetBookById")]
    public async Task<HttpResponseData> GetBookById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "books/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Getting book with ID: {BookId}", id);

        try
        {
            var book = await _cosmosService.GetBookByIdAsync(id);

            if (book == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Book with ID {id} not found");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(book);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book {BookId}", id);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("CreateBook")]
    public async Task<HttpResponseData> CreateBook(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "books")] HttpRequestData req)
    {
        _logger.LogInformation("Creating new book");

        try
        {
            var bookInput = await JsonSerializer.DeserializeAsync<BookInput>(req.Body);

            if (!ValidateBookInput(bookInput, out var validationError))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync(validationError);
                return badRequestResponse;
            }

            var book = new Book
            {
                Id = GenerateBookId(),
                Isbn = bookInput!.Isbn!,
                Title = bookInput.Title!,
                Author = bookInput.Author!,
                Categories = bookInput.Categories!,
                PublicationYear = bookInput.PublicationYear!.Value,
                Language = bookInput.Language!,
                Pages = bookInput.Pages,
                Publisher = bookInput.Publisher,
                Description = bookInput.Description,
                CoverImage = bookInput.CoverImage,
                Available = bookInput.Available ?? true,
                Rating = bookInput.Rating,
                ReviewCount = bookInput.ReviewCount ?? 0,
                Price = bookInput.Price,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _cosmosService.CreateBookAsync(book);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(book);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("UpdateBook")]
    public async Task<HttpResponseData> UpdateBook(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "books/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Updating book with ID: {BookId}", id);

        try
        {
            var existingBook = await _cosmosService.GetBookByIdAsync(id);
            if (existingBook == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Book with ID {id} not found");
                return notFoundResponse;
            }

            var bookInput = await JsonSerializer.DeserializeAsync<BookInput>(req.Body);

            if (bookInput == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid book data");
                return badRequestResponse;
            }

            if (!string.IsNullOrWhiteSpace(bookInput.Isbn))
            {
                existingBook.Isbn = bookInput.Isbn;
            }

            if (!string.IsNullOrWhiteSpace(bookInput.Title))
            {
                existingBook.Title = bookInput.Title;
            }

            if (bookInput.Author != null)
            {
                existingBook.Author = bookInput.Author;
            }

            if (bookInput.Categories != null && bookInput.Categories.Count > 0)
            {
                existingBook.Categories = bookInput.Categories;
            }

            if (bookInput.PublicationYear.HasValue)
            {
                existingBook.PublicationYear = bookInput.PublicationYear.Value;
            }

            if (!string.IsNullOrWhiteSpace(bookInput.Language))
            {
                existingBook.Language = bookInput.Language;
            }

            existingBook.Pages = bookInput.Pages ?? existingBook.Pages;
            existingBook.Publisher = bookInput.Publisher ?? existingBook.Publisher;
            existingBook.Description = bookInput.Description ?? existingBook.Description;
            existingBook.CoverImage = bookInput.CoverImage ?? existingBook.CoverImage;
            existingBook.Available = bookInput.Available ?? existingBook.Available;
            existingBook.Rating = bookInput.Rating ?? existingBook.Rating;
            existingBook.ReviewCount = bookInput.ReviewCount ?? existingBook.ReviewCount;
            existingBook.Price = bookInput.Price ?? existingBook.Price;
            existingBook.UpdatedAt = DateTime.UtcNow;

            await _cosmosService.UpdateBookAsync(existingBook);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(existingBook);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book {BookId}", id);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("DeleteBook")]
    public async Task<HttpResponseData> DeleteBook(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "books/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Deleting book with ID: {BookId}", id);

        try
        {
            var book = await _cosmosService.GetBookByIdAsync(id);
            if (book == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Book with ID {id} not found");
                return notFoundResponse;
            }

            await _cosmosService.DeleteBookAsync(id);

            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book {BookId}", id);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("HealthCheck")]
    public HttpResponseData HealthCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        _logger.LogInformation("Health check requested");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteString(JsonSerializer.Serialize(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        }));
        return response;
    }

    private static string GenerateBookId()
    {
        var suffix = Random.Shared.Next(100, 1000000);
        return $"book-{suffix}";
    }

    private static bool ValidateBookInput(BookInput? input, out string message)
    {
        if (input == null)
        {
            message = "Invalid book data";
            return false;
        }

        if (string.IsNullOrWhiteSpace(input.Isbn))
        {
            message = "ISBN is required";
            return false;
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            message = "Title is required";
            return false;
        }

        if (input.Author == null || string.IsNullOrWhiteSpace(input.Author.Id) || string.IsNullOrWhiteSpace(input.Author.Name))
        {
            message = "Author is required";
            return false;
        }

        if (input.Categories == null || input.Categories.Count == 0)
        {
            message = "At least one category is required";
            return false;
        }

        if (!input.PublicationYear.HasValue)
        {
            message = "Publication year is required";
            return false;
        }

        if (string.IsNullOrWhiteSpace(input.Language))
        {
            message = "Language is required";
            return false;
        }

        message = string.Empty;
        return true;
    }
}
