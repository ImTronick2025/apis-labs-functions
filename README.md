# APIs Labs - Azure Functions (Book Catalog)

This repo contains a .NET 8 isolated Azure Functions API for a book catalog.

## Functions

- GET `/api/books` - List all books
- GET `/api/books/{id}` - Get book by id
- POST `/api/books` - Create book
- PUT `/api/books/{id}` - Update book
- DELETE `/api/books/{id}` - Delete book
- GET `/api/health` - Health check

## Project Layout

```
ApisLabs.Functions/
├── BookFunctions.cs
├── CosmosDbService.cs
├── Models.cs
├── Program.cs
├── host.json
├── local.settings.json
└── ApisLabs.Functions.csproj
```

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
curl http://localhost:7071/api/health

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

curl http://localhost:7071/api/books
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
