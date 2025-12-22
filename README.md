# APIs Labs - Azure Functions

Este repositorio contiene el código de Azure Functions en .NET 8.0 para el laboratorio DevOps.

## 🎯 Funciones Implementadas

### Pet Functions (CRUD Completo)

- **GET** `/api/pets` - Obtener todas las mascotas
- **GET** `/api/pets/{id}` - Obtener mascota por ID
- **POST** `/api/pets` - Crear nueva mascota
- **PUT** `/api/pets/{id}` - Actualizar mascota
- **DELETE** `/api/pets/{id}` - Eliminar mascota
- **GET** `/api/health` - Health check

## 🏗️ Arquitectura

```
ApisLabs.Functions/
├── PetFunctions.cs        # HTTP Triggers (endpoints)
├── CosmosDbService.cs     # Servicio para acceso a Cosmos DB
├── Models.cs              # DTOs y entidades
├── Program.cs             # Configuración y startup
├── host.json              # Configuración de Functions runtime
├── local.settings.json    # Settings locales (no subir a Git)
└── ApisLabs.Functions.csproj
```

## 🔧 Tecnologías

- **.NET 8.0**: Framework principal
- **Azure Functions v4**: Runtime
- **Isolated Worker Process**: Modelo de ejecución
- **Azure Cosmos DB SDK**: Cliente para Cosmos DB
- **Application Insights**: Telemetría y logging

## 🚀 Desarrollo Local

### Prerequisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/) con extensión Azure Functions
- [Azure Cosmos DB Emulator](https://docs.microsoft.com/azure/cosmos-db/local-emulator) (opcional, para desarrollo local)

### Instalación

```bash
# Restaurar paquetes NuGet
dotnet restore

# Compilar el proyecto
dotnet build
```

### Configuración Local

Edita `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDbConnectionString": "TU_CONNECTION_STRING_AQUI",
    "CosmosDbDatabaseName": "apis-labs-db",
    "CosmosDbContainerName": "items"
  }
}
```

**Para Cosmos DB Emulator local:**
```
AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==
```

### Ejecutar Localmente

```bash
# Con Azure Functions Core Tools
func start

# O con dotnet
dotnet run
```

La función estará disponible en: `http://localhost:7071`

### Probar los Endpoints

```bash
# Health check
curl http://localhost:7071/api/health

# Crear mascota
curl -X POST http://localhost:7071/api/pets \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Firulais",
    "species": "dog",
    "breed": "Labrador",
    "age": 3,
    "color": "Golden",
    "weight": 25.5
  }'

# Obtener todas las mascotas
curl http://localhost:7071/api/pets

# Obtener mascota por ID
curl http://localhost:7071/api/pets/{id}

# Actualizar mascota
curl -X PUT http://localhost:7071/api/pets/{id} \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Firulais Updated",
    "age": 4
  }'

# Eliminar mascota
curl -X DELETE http://localhost:7071/api/pets/{id}
```

## 📦 Despliegue a Azure

### Opción 1: GitHub Actions (Recomendado)

Crea `.github/workflows/deploy-functions.yml`:

```yaml
name: Deploy Azure Functions

on:
  push:
    branches:
      - main
  workflow_dispatch:

env:
  AZURE_FUNCTIONAPP_NAME: 'apislabs-func-xxxxx'
  DOTNET_VERSION: '8.0.x'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Publish
      run: dotnet publish --configuration Release --output ./output
    
    - name: Deploy to Azure Functions
      uses: Azure/functions-action@v1
      with:
        app-name: ${{ env.AZURE_FUNCTIONAPP_NAME }}
        package: './output'
        publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}
```

### Opción 2: Azure CLI

```bash
# Obtener el nombre de la Function App desde Terraform output
func azure functionapp publish <function-app-name>
```

### Opción 3: VS Code

1. Instala la extensión "Azure Functions"
2. Click derecho en el proyecto → "Deploy to Function App"
3. Selecciona tu suscripción y Function App

## 🔐 Configuración en Azure

Después del despliegue, configura las App Settings en Azure:

```bash
az functionapp config appsettings set \
  --name <function-app-name> \
  --resource-group apis-labs-rg \
  --settings \
    CosmosDbConnectionString="<from-terraform-output>" \
    CosmosDbDatabaseName="apis-labs-db" \
    CosmosDbContainerName="items"
```

O usa el portal de Azure:
1. Function App → Configuration → Application Settings
2. Agrega las variables necesarias

## 🔗 Integración con API Management

Una vez desplegadas las Functions, configura el backend en APIM:

```hcl
# En apis-labs-infra/main.tf
resource "azurerm_api_management_backend" "functions" {
  name                = "petstore-backend"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  protocol            = "http"
  url                 = "https://${azurerm_windows_function_app.main.default_hostname}/api"
}

resource "azurerm_api_management_api_policy" "petstore" {
  api_name            = azurerm_api_management_api.petstore.name
  api_management_name = azurerm_api_management.main.name
  resource_group_name = azurerm_resource_group.main.name

  xml_content = <<XML
<policies>
  <inbound>
    <base />
    <set-backend-service backend-id="petstore-backend" />
  </inbound>
</policies>
XML
}
```

## 🧪 Testing

### Unit Tests (Próximamente)

```bash
dotnet test
```

### Integration Tests con Postman

Importa la colección desde `apis-labs-api/petstore-api.yaml`

## 📊 Monitoring

### Application Insights

Las Functions están configuradas con Application Insights para:
- Trazas de ejecución
- Métricas de rendimiento
- Logs personalizados
- Detección de errores

### Ver logs en tiempo real

```bash
func azure functionapp logstream <function-app-name>
```

## 🐛 Troubleshooting

### Error: "Cannot find CosmosDbConnectionString"
Verifica que las App Settings estén configuradas correctamente en Azure.

### Error: "The specified partition key was not found"
Asegúrate de que el container de Cosmos DB tenga `/id` como partition key.

### Error 500 en producción
Revisa los logs en Application Insights o con `func azure functionapp logstream`.

## 📚 Recursos

- [Azure Functions .NET Isolated Worker](https://docs.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide)
- [Azure Cosmos DB .NET SDK](https://docs.microsoft.com/azure/cosmos-db/sql/sql-api-sdk-dotnet-standard)
- [Azure Functions Best Practices](https://docs.microsoft.com/azure/azure-functions/functions-best-practices)

## 🔄 Roadmap

- [ ] Agregar autenticación JWT
- [ ] Implementar Durable Functions para workflows
- [ ] Agregar validación de entrada con FluentValidation
- [ ] Unit tests con xUnit
- [ ] Implementar retry policies con Polly
- [ ] Agregar swagger/OpenAPI para documentación
