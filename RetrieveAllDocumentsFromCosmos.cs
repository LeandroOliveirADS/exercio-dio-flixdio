using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

public class RetrieveAllDocumentsFromCosmos
{
    private readonly CosmosClient _client;

    public RetrieveAllDocumentsFromCosmos(CosmosClient cosmosClient)
    {
        _client = cosmosClient;
    }

    [Function("RetrieveAllDocumentsFromCosmos")]
    public async Task<IActionResult> ProcessRequest(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request,
        ILogger logger)
    {
        logger.LogInformation("Iniciando listagem de registros no Cosmos DB.");

        var containerRef = _client.GetContainer("database_name", "container_name");

        var definition = new QueryDefinition("SELECT * FROM c");
        var iterator = containerRef.GetItemQueryIterator<dynamic>(definition);

        var documentList = new List<dynamic>();

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();
            documentList.AddRange(page);
        }

        var serializedResponse = JsonSerializer.Serialize(documentList);
        logger.LogInformation("Listagem concluída. Retornando dados.");

        return new OkObjectResult(serializedResponse);
    }
}
