using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

public class FilterDocumentsFromCosmos
{
    private readonly CosmosClient _client;

    public FilterDocumentsFromCosmos(CosmosClient cosmosClient)
    {
        _client = cosmosClient;
    }

    [Function("FilterDocumentsFromCosmos")]
    public async Task<IActionResult> Execute(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request,
        ILogger logger)
    {
        logger.LogInformation("Iniciando o processo de filtragem no Cosmos DB.");

        string campo = request.Query["field"];
        string valor = request.Query["value"];

        if (string.IsNullOrEmpty(campo) || string.IsNullOrEmpty(valor))
        {
            logger.LogWarning("Parâmetros de filtro ausentes ou inválidos.");
            return new BadRequestObjectResult("Informe parâmetros de consulta válidos.");
        }

        var container = _client.GetContainer("database_name", "container_name");

        var definicaoConsulta = new QueryDefinition($"SELECT * FROM c WHERE c.{campo} = @valor")
            .WithParameter("@valor", valor);

        var iterador = container.GetItemQueryIterator<dynamic>(definicaoConsulta);
        var registrosFiltrados = new List<dynamic>();

        while (iterador.HasMoreResults)
        {
            var resultado = await iterador.ReadNextAsync();
            registrosFiltrados.AddRange(resultado);
        }

        return new OkObjectResult(JsonSerializer.Serialize(registrosFiltrados));
    }
}
