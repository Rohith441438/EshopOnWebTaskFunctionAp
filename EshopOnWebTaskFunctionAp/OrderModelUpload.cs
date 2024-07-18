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
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("Container-name");

            //var blobClient = new BlobContainerClient(Connection, containerName);

            //var orderModel = JsonSerializer.Deserialize<OrderModel>(requestBody);

            //Stream orderModelToUpload = req.Body;
            //string orderModelBlobName = orderModel.UserId + "-" + orderModel.OrderId + "-" + new Guid();

            //var blob = blobClient.GetBlobClient(orderModelBlobName);
            //await blob.UploadAsync(req.Body
            //
            try
            {
                // TODO: Replace <storage-account-name> with your actual storage account name
                var blobServiceClient = new BlobServiceClient(Connection);

                //Create a unique name for the container
                //string containerName = "quickstartblobs" + Guid.NewGuid().ToString();

                // Create the container and return a container client object
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("order-item-reserver");
                containerClient.CreateIfNotExistsAsync().Wait();

                containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

                // Create a local file in the ./data/ directory for uploading and downloading
                string localPath = "data";
                Directory.CreateDirectory(localPath);
                string fileName = "OrderModelPayload" + Guid.NewGuid().ToString() + ".json";
                string localFilePath = Path.Combine(localPath, fileName);

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                // Write text to the file
                await File.WriteAllTextAsync(localFilePath, requestBody);

                // Get a reference to a blob
                BlobClient blobClient = containerClient.GetBlobClient(fileName);

                Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

                // Upload data from the local file
                await blobClient.UploadAsync(localFilePath, true);
            }
            catch (Exception ex)
            {
                throw;
            }

            return new OkObjectResult("Order Data uploaded to Blob Successfully");
        }
    }

    public class OrderModel
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public IEnumerable<Item> Items { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }
}
