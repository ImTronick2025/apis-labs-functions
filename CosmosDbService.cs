using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace ApisLabs.Functions;

public class CosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(IConfiguration configuration)
    {
        var connectionString = configuration["CosmosDbConnectionString"];
        var databaseName = configuration["CosmosDbDatabaseName"];
        var containerName = configuration["CosmosDbContainerName"];

        var cosmosClient = new CosmosClient(connectionString);
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<IEnumerable<Book>> GetBooksAsync()
    {
        var query = _container.GetItemQueryIterator<Book>();
        var results = new List<Book>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task<Book?> GetBookByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Book>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Book> CreateBookAsync(Book book)
    {
        var response = await _container.CreateItemAsync(book, new PartitionKey(book.Id));
        return response.Resource;
    }

    public async Task<Book> UpdateBookAsync(Book book)
    {
        var response = await _container.UpsertItemAsync(book, new PartitionKey(book.Id));
        return response.Resource;
    }

    public async Task DeleteBookAsync(string id)
    {
        await _container.DeleteItemAsync<Book>(id, new PartitionKey(id));
    }
}
