using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System;

public class UploadFileToBlobStorage
{
    private readonly BlobServiceClient _blobClient;

    public UploadFileToBlobStorage(BlobServiceClient blobServiceClient)
    {
        _blobClient = blobServiceClient;
    }

    [Function("UploadFileToBlobStorage")]
    public async Task<IActionResult> ProcessUpload(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
        ILogger logger)
    {
        logger.LogInformation("Iniciando upload de arquivo para o Blob Storage.");

        var arquivoEnviado = request.Form.Files["file"];
        if (arquivoEnviado == null)
        {
            logger.LogWarning("Nenhum arquivo encontrado na requisição.");
            return new BadRequestObjectResult("Arquivo não foi fornecido.");
        }

        var container = _blobClient.GetBlobContainerClient("uploads");
        await container.CreateIfNotExistsAsync();

        var blob = container.GetBlobClient(arquivoEnviado.FileName);

        using (var streamArquivo = arquivoEnviado.OpenReadStream())
        {
            await blob.UploadAsync(streamArquivo, overwrite: true);
        }

        logger.LogInformation("Upload concluído: {NomeArquivo}", arquivoEnviado.FileName);
        return new OkObjectResult($"Arquivo '{arquivoEnviado.FileName}' carregado com sucesso!");
    }
}
