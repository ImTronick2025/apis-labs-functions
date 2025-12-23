# APIs Labs - Azure Functions (Book Catalog)

This repo contains a .NET 8 isolated Azure Functions API for a book catalog.

## Functions

- GET `/api/books` - List all books
- GET `/api/books/{id}` - Get book by id
- POST `/api/books` - Create book
- PUT `/api/books/{id}` - Update book
- DELETE `/api/books/{id}` - Delete book
- GET `/api/health` - Health check

## Authentication

All endpoints except `/api/health` require a function key. Use `x-functions-key` header or the `?code=` query string.

## Project Layout

```
ApisLabs.Functions/
|-- BookFunctions.cs
|-- CosmosDbService.cs
|-- Models.cs
|-- Program.cs
|-- host.json
|-- local.settings.json
`-- ApisLabs.Functions.csproj
```

## Configuration

Required settings:
- AzureWebJobsStorage
- CosmosDbConnectionString
- CosmosDbDatabaseName=apis-labs-db
- CosmosDbContainerName=books

Optional settings:
- KeyVaultUri (if set, the app loads secrets from Key Vault using DefaultAzureCredential)
- Key Vault secret names expected (mapped by AppKeyVaultSecretManager): cosmosdb-connection-string -> CosmosDbConnectionString; cosmosdb-primary-key -> CosmosDbPrimaryKey; appinsights-connection-string -> APPLICATIONINSIGHTS_CONNECTION_STRING; functions-storage-connection-string -> FunctionsStorageConnectionString.

## Local Configuration

Update `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDbConnectionString": "<from-terraform-output>",
    "CosmosDbDatabaseName": "apis-labs-db",
    "CosmosDbContainerName": "books"
  }
}
```

## Run Locally

```bash
dotnet restore
dotnet build
func start
```

## Example Requests

```bash
# Local health check
curl http://localhost:7071/api/health

# Local create
curl -X POST http://localhost:7071/api/books \
  -H "Content-Type: application/json" \
  -d '{
    "isbn": "978-1-234567-89-0",
    "title": "Clean Architecture",
    "author": { "id": "author-001", "name": "Robert C. Martin" },
    "categories": ["Technology"],
    "publicationYear": 2017,
    "language": "en",
    "pages": 432
  }'

# Azure Function App (requires key)
$funcUrl = "https://<function-app-name>.azurewebsites.net"
$funcKey = "<function-key>"

curl "$funcUrl/api/books" \
  -H "x-functions-key: $funcKey"
```

## Cosmos DB Data

Seed data and schema live in `D:\Proyectos\DemoBook\apis-labs-db`:
- `sample-data/books-seed.json`
- `schema/books.json`
- scripts: `scripts\init-database.ps1` and `scripts\seed-data.ps1`

## Deploy

```bash
func azure functionapp publish <function-app-name>
```

## Postman Collection

The Postman collection and environment files are located in `D:\Proyectos\DemoBook\postman\`:
- `ApisLabs-BookCatalog.postman_collection.json` - API endpoints
- `ApisLabs-BookCatalog.postman_environment.json` - Environment variables (baseUrl, functionKey, bookId)

### Using Postman

1. Import both files into Postman
2. Select the "ApisLabs Book Catalog" environment from the top-right dropdown
3. The environment is pre-configured with:
   - **baseUrl**: `https://apislabsdev-func-lshkrw.azurewebsites.net`
   - **functionKey**: Default function key (already set)
   - **bookId**: Auto-populated after creating a book

### Get Function Key via Azure CLI

If you need to retrieve or rotate the function key:

```bash
az functionapp keys list --name apislabsdev-func-lshkrw --resource-group apis-labs-dev-rg
```

## Troubleshooting

If `/api/health` returns 502:
- For Flex Consumption, avoid setting `FUNCTIONS_WORKER_RUNTIME` in app settings.
- Confirm the app has a deployment package (Function list should include `GetBooks`, not only `WarmUp`).
- If `KeyVaultUri` is set, grant the Function App identity Get/List secrets on the Key Vault.
- Ensure `CosmosDbConnectionString` resolves to a real connection string (Key Vault reference or direct value).
- Validate `AzureWebJobsStorage` is present and correct.
