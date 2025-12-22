using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ApisLabs.Functions;

public class PetFunctions
{
    private readonly ILogger<PetFunctions> _logger;
    private readonly CosmosDbService _cosmosService;

    public PetFunctions(ILogger<PetFunctions> logger, CosmosDbService cosmosService)
    {
        _logger = logger;
        _cosmosService = cosmosService;
    }

    [Function("GetPets")]
    public async Task<HttpResponseData> GetPets(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "pets")] HttpRequestData req)
    {
        _logger.LogInformation("Getting all pets");

        try
        {
            var pets = await _cosmosService.GetPetsAsync();
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(pets);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pets");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("GetPetById")]
    public async Task<HttpResponseData> GetPetById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "pets/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation($"Getting pet with ID: {id}");

        try
        {
            var pet = await _cosmosService.GetPetByIdAsync(id);
            
            if (pet == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Pet with ID {id} not found");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(pet);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting pet {id}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("CreatePet")]
    public async Task<HttpResponseData> CreatePet(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "pets")] HttpRequestData req)
    {
        _logger.LogInformation("Creating new pet");

        try
        {
            var petInput = await JsonSerializer.DeserializeAsync<PetInput>(req.Body);
            
            if (petInput == null || string.IsNullOrEmpty(petInput.Name))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid pet data");
                return badRequestResponse;
            }

            var pet = new Pet
            {
                Id = Guid.NewGuid().ToString(),
                Name = petInput.Name,
                Species = petInput.Species,
                Breed = petInput.Breed,
                Age = petInput.Age,
                Color = petInput.Color,
                Weight = petInput.Weight,
                Status = petInput.Status ?? "available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _cosmosService.CreatePetAsync(pet);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(pet);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pet");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("UpdatePet")]
    public async Task<HttpResponseData> UpdatePet(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "pets/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation($"Updating pet with ID: {id}");

        try
        {
            var existingPet = await _cosmosService.GetPetByIdAsync(id);
            if (existingPet == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Pet with ID {id} not found");
                return notFoundResponse;
            }

            var petInput = await JsonSerializer.DeserializeAsync<PetInput>(req.Body);
            
            existingPet.Name = petInput?.Name ?? existingPet.Name;
            existingPet.Species = petInput?.Species ?? existingPet.Species;
            existingPet.Breed = petInput?.Breed;
            existingPet.Age = petInput?.Age;
            existingPet.Color = petInput?.Color;
            existingPet.Weight = petInput?.Weight;
            existingPet.Status = petInput?.Status ?? existingPet.Status;
            existingPet.UpdatedAt = DateTime.UtcNow;

            await _cosmosService.UpdatePetAsync(existingPet);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(existingPet);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating pet {id}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("DeletePet")]
    public async Task<HttpResponseData> DeletePet(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "pets/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation($"Deleting pet with ID: {id}");

        try
        {
            var pet = await _cosmosService.GetPetByIdAsync(id);
            if (pet == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Pet with ID {id} not found");
                return notFoundResponse;
            }

            await _cosmosService.DeletePetAsync(id);

            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting pet {id}");
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
}
