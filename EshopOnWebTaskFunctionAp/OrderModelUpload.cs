using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Text.Json;
using System.Xml.Linq;

namespace EshopOnWebTaskFunctionAp
{
    public class OrderModelUpload
    {
        private readonly ILogger<OrderModelUpload> _logger;

        public OrderModelUpload(ILogger<OrderModelUpload> logger)
        {
            _logger = logger;
        }

        [Function("OrderModelUpload")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("Container-name");

            try
            {
                BlobContainerClient containerClient = GetContainerClient(Connection);

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                string localPath = "data";
                Directory.CreateDirectory(localPath);
                string fileName = "OrderModelPayload" + Guid.NewGuid().ToString() + ".json";
                string localFilePath = Path.Combine(localPath, fileName);
                await File.WriteAllTextAsync(localFilePath, requestBody);

                BlobClient blobClient = containerClient.GetBlobClient(fileName);

                await blobClient.UploadAsync(localFilePath, true);
            }
            catch (Exception ex)
            {
                throw;
            }

            return new OkObjectResult("Order Data uploaded to Blob Successfully");
        }

        private static BlobContainerClient GetContainerClient(string Connection)
        {
            var blobServiceClient = new BlobServiceClient(Connection);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("order-item-reserver");
            containerClient.CreateIfNotExistsAsync().Wait();

            containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
            return containerClient;
        }
    }
}
