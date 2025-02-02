using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;

public class InsertDocumentIntoCosmos
{
    private readonly CosmosClient _client;

    public InsertDocumentIntoCosmos(CosmosClient cosmosClient)
    {
        _client = cosmosClient;
    }

    [Function("InsertDocumentIntoCosmos")]
    public async Task<IActionResult> SaveItem(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
        ILogger logger)
    {
        logger.LogInformation("Iniciando processo de inserção no Cosmos DB.");

        var jsonBody = await new StreamReader(request.Body).ReadToEndAsync();
        var contentObject = JsonSerializer.Deserialize<dynamic>(jsonBody);

        if (contentObject == null)
        {
            logger.LogWarning("Conteúdo nulo ou inválido.");
            return new BadRequestObjectResult("Dados fornecidos são inválidos.");
        }

        var container = _client.GetContainer("database_name", "container_name");

        await container.CreateItemAsync(contentObject);

        logger.LogInformation("Inserção bem-sucedida de documento no Cosmos DB.");
        return new OkObjectResult("Documento inserido com sucesso no Cosmos DB!");
    }
}
