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

    public async Task<IEnumerable<Pet>> GetPetsAsync()
    {
        var query = _container.GetItemQueryIterator<Pet>();
        var results = new List<Pet>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task<Pet?> GetPetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Pet>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Pet> CreatePetAsync(Pet pet)
    {
        var response = await _container.CreateItemAsync(pet, new PartitionKey(pet.Id));
        return response.Resource;
    }

    public async Task<Pet> UpdatePetAsync(Pet pet)
    {
        var response = await _container.UpsertItemAsync(pet, new PartitionKey(pet.Id));
        return response.Resource;
    }

    public async Task DeletePetAsync(string id)
    {
        await _container.DeleteItemAsync<Pet>(id, new PartitionKey(id));
    }
}
