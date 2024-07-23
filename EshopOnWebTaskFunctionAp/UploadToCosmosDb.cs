using Azure.Identity;
using Azure.Storage.Blobs;
using EshopOnWebTaskFunctionAp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Xml.Linq;

namespace EshopOnWebTaskFunctionAp
{
    public class UploadToCosmosDb
    {
        private readonly ILogger<UploadToCosmosDb> _logger;
        private CosmosClient cosmosClient;
        private Database database;
        private Microsoft.Azure.Cosmos.Container container;

        public UploadToCosmosDb(ILogger<UploadToCosmosDb> logger)
        {
            _logger = logger;
        }

        [Function("UploadToCosmosDb")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            

            try
            {
                _logger.LogInformation("New Order details posting to Cosmos is started");
                await CosmosAsync();

                var orderDetails = await GerOrderDetails(req);

                var createdItem = await container.CreateItemAsync<OrderDetails>(orderDetails);
                _logger.LogInformation(createdItem.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            return new OkObjectResult("Order Details uploaded to CosmosDb Successfully");
        }

        private async Task<OrderDetails> GerOrderDetails(HttpRequest httpRequest)
        {
            using StreamReader reader = new(httpRequest.Body);
            var bodyAsString = await reader.ReadToEndAsync();
            var orderDetailsResult = JsonConvert.DeserializeObject<OrderDetails>(bodyAsString);

            orderDetailsResult.id += "_" + Guid.NewGuid().ToString();
            orderDetailsResult.Type = "ShoppingOrder";

            return orderDetailsResult;
        }

        public async Task CosmosAsync()
        {
            string EndpointUrl = Environment.GetEnvironmentVariable("CosmosDbEndPointUrl");
            string PrimaryKey = Environment.GetEnvironmentVariable("CosmosPrimayKey");
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUrl, PrimaryKey);

            // Runs the CreateDatabaseAsync method
            await this.CreateDatabaseAsync();

            // Run the CreateContainerAsync method
            await this.CreateContainerAsync();
        }

        private async Task CreateDatabaseAsync()
        {
            string databaseId = Environment.GetEnvironmentVariable("DatabaseId");
            // Create a new database using the cosmosClient
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            _logger.LogInformation("Created Database: {0}\n", this.database.Id);
        }

        private async Task CreateContainerAsync()
        {
            string containerId = Environment.GetEnvironmentVariable("ContainerId");
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/Type");
            _logger.LogInformation("Created Container: {0}\n", this.container.Id);
        }
    }
}
