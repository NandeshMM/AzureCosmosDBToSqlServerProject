using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataStore.Abstraction.IDTO;
using DataStore.Abstraction.IRepositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging; // Ensure this is included for ILogger

namespace DataStore.Implementation.Repositories
{
    public class CosmosDBDataFetchingRepository : ICosmosDBDataFetchingRepository
    {
        private readonly Container _container;
        private readonly IDataTransferService _dataTransferService;
        private readonly ILogger<CosmosDBDataFetchingRepository> _logger;

        public CosmosDBDataFetchingRepository(
            string connectionString,
            string databaseName,
            string containerName,
            IDataTransferService dataTransferService,
            ILogger<CosmosDBDataFetchingRepository> logger)
        {
            var cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions { AllowBulkExecution = true });
            _container = cosmosClient.GetContainer(databaseName, containerName);
            _dataTransferService = dataTransferService;
            _logger = logger;
        }

        public async Task FetchDataAsync(IQueryParameterDTO queryparameters, int batchSize = 1000)
        {
            string continuationToken = null;
            double totalRequestCharge = 0; // Track total RU consumption

            do
            {
                // Fetch data from Cosmos DB
                var (batch, newContinuationToken, requestCharge) = await GetDataFromCosmosDBAsync(queryparameters, continuationToken, batchSize);

                // Track RU charge
                totalRequestCharge += requestCharge;
                _logger.LogInformation($"Batch retrieved. RU consumed: {requestCharge}");

                if (batch.Count == 0)
                {
                    _logger.LogInformation("No more data to process.");
                    break;
                }

                // Send batch to SQL transfer service asynchronously
                await _dataTransferService.TransferDataToSqlAsync(batch);

                // Update continuation token for next batch
                continuationToken = newContinuationToken;

            } while (!string.IsNullOrEmpty(continuationToken)); // Continue until no more data

            _logger.LogInformation($"Total RU consumed for query: {totalRequestCharge}");
        }

        private async Task<(List<dynamic> results, string continuationToken, double requestCharge)> GetDataFromCosmosDBAsync(
            IQueryParameterDTO queryparameters, string continuationToken, int batchSize)
        {
            List<string> conditions = new() { "c.TableName = @tableName" };

            if (!string.IsNullOrEmpty(queryparameters.CompanyId))
            {
                conditions.Add("c.CompanyId = @companyId");
            }

            if (!string.IsNullOrEmpty(queryparameters.UserId))
            {
                conditions.Add("c.UserId = @userId");
            }

            string whereClause = string.Join(" AND ", conditions);
            var query = $@"SELECT c.Id, c.CompanyId, c.UserId, c.DataField1, c.DataField2 FROM c WHERE {whereClause}";

            var queryDefinition = new QueryDefinition(query)
                .WithParameter("@tableName", queryparameters.TableName)
                .WithParameter("@companyId", queryparameters.CompanyId ?? "")
                .WithParameter("@userId", queryparameters.UserId ?? "");

            var queryRequestOptions = new QueryRequestOptions
            {
                MaxItemCount = batchSize,
                PartitionKey = new PartitionKey(queryparameters.TableName)
            };

            var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition, continuationToken, queryRequestOptions);

            var results = new List<dynamic>();
            string newContinuationToken = null;
            double requestCharge = 0; // Store RU consumption for this batch

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
                newContinuationToken = response.ContinuationToken;
                requestCharge = response.RequestCharge; // Capture RU charge
            }

            return (results, newContinuationToken, requestCharge);
        }
    }
}
