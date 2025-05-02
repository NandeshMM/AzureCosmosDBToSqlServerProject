using System.Diagnostics;
using System.Runtime.CompilerServices;
using Azure.Identity;
using DataStore.Abstraction.IDTO;
using DataStore.Abstraction.IRepositories;
using FeatureObjects.Abstraction.IManager;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataStore.Implementation.Repositories
{
    public class CosmosDBDataFetchingRepository : ICosmosDBDataFetchingRepository
    {
        private readonly ILogger<CosmosDBDataFetchingRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;

        public CosmosDBDataFetchingRepository(
            ILogger<CosmosDBDataFetchingRepository> logger,
            IConfiguration configuration,
            INotificationService notificationService)
        {
            _logger = logger;
            _configuration = configuration;
            _notificationService = notificationService;
        }

        public async IAsyncEnumerable<List<IDictionary<string, object>>> FetchDataAsync(
    IQueryParameterDTO queryparameters,
    int pageSize,
    [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var totalDocumentTransferred = 0;

            //string cosmosEndpoint = "https://cp-csdb-eus-perseus-dev-audit-logs.documents.azure.com:443/";
            //string userAssignedClientId = "25eefb94-2bfb-4656-a05c-454675d00304"; // <- This is the Client ID of the user-assigned managed identity

            //var credential = new ManagedIdentityCredential(clientId: userAssignedClientId);
            //            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions()
            //            {
            //                AuthorityHost =
            //new Uri("https://login.microsoftonline.com/185e7ed4-24c7-4904-a70c-4d1b7fa32214"),
            //                ManagedIdentityClientId = "",
            //            });
            //            var cosmosClient = new CosmosClient(cosmosEndpoint, credential);

            var cosmosClient = new CosmosClient(queryparameters.CosmosDBConnectionString);

            var container = cosmosClient.GetContainer(queryparameters.CosmosDBDatabaseName, queryparameters.ContainerName);

            string continuationToken = null;
            double totalRequestCharge = 0;

            var queryDef = new QueryDefinition(queryparameters.Query);
            var requestOptions = new QueryRequestOptions
            {
                MaxItemCount = pageSize
            };

            do
            {
                var stopwatch = Stopwatch.StartNew();

                var iterator = container.GetItemQueryIterator<dynamic>(
                    queryDef,
                    continuationToken,
                    requestOptions);

                if (!iterator.HasMoreResults)
                    break;

                FeedResponse<dynamic> response = await iterator.ReadNextAsync(cancellationToken);

                if (response == null || !response.Any())
                {
                    _logger.LogWarning("Received empty batch");
                    _notificationService.SendStatusUpdateasync("No Data found in the container", "error");
                    continue;
                }

                double batchCharge = response.RequestCharge;
                totalRequestCharge += batchCharge;
                Console.WriteLine($"[RU] Batch charge: {batchCharge:0.##} — Total so far: {totalRequestCharge:0.##}");

                List<IDictionary<string, object>> dictList = response
                    .Select(item =>
                    {
                        var d = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                    JsonConvert.SerializeObject(item));
                        return (IDictionary<string, object>)d;
                    })
                    .ToList();

                yield return dictList;

                continuationToken = response.ContinuationToken;
                totalDocumentTransferred += response.Count;

                stopwatch.Stop();
                _logger.LogInformation("CosmosDB fetching time per batch: {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
                await _notificationService.SendProgressAsync($"Transfering  {totalDocumentTransferred} documents");
            }
            while (continuationToken != null);

            await _notificationService.SendStatusUpdateasync($"Transfered {totalDocumentTransferred} documents","success");
            Console.WriteLine($"[RU] Total RU consumed for this fetch: {totalRequestCharge:0.##}");
            _logger.LogInformation("Finished fetching all data in batches.");
        }
    }
}
