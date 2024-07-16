using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EshopOnWebTaskFunctionAp
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("Container-name");

            var blobClient = new BlobContainerClient(Connection, containerName);

            var orderModel = JsonSerializer.Deserialize<OrderModel>(requestBody);

            Stream orderModelToUpload = req.Body;
            string orderModelBlobName = orderModel.UserId + "-" + orderModel.OrderId + "-" + new Guid();
            
            var blob = blobClient.GetBlobClient(orderModelBlobName);
            await blob.UploadAsync(req.Body);
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
