//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Channels;
//using System.Threading.Tasks;
//using DataStore.Abstraction.Exceptions;
//using DataStore.Abstraction.IDTO;
//using DataStore.Abstraction.IRepositories;
//using FeatureObjects.Abstraction.IManager;
//using Microsoft.Azure.Cosmos;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using static FeatureObjects.Implementation.Manager.CosmosFetchService;

//namespace FeatureObjects.Implementation.Manager
//{
//    public class CosmosFetchService:ICosmosFetchService
//    {
//        private readonly ILogger<CosmosFetchService> _logger;
//        private readonly ICosmosDBDataFetchingRepository _cosmosDBDataFetchingRepository;
//        private readonly IBulkinsertion _bulkInsertionService;
//        private readonly IConfiguration _configuration;
//        private readonly int _batchSize;
//        private readonly INotificationService _notificationService;
//        public CosmosFetchService(
//            ILogger<CosmosFetchService> logger,
//            ICosmosDBDataFetchingRepository cosmosDBDataFetchingRepository,
//            IBulkinsertion bulkInsertionService, INotificationService notificationService,

//            IConfiguration configuration)

//        {
//            _logger = logger;
//            _cosmosDBDataFetchingRepository = cosmosDBDataFetchingRepository;
//            _bulkInsertionService = bulkInsertionService;
//            _configuration = configuration;
//            _batchSize = _configuration.GetValue<int>("BatchSize:Default");
//            _notificationService = notificationService;
//        }

//        public async Task ProcessDataTransferAsync(IQueryParameterDTO queryParameter, CancellationToken cancellationToken)
//        {

//            using var connection = new SqlConnection(queryParameter.SQLServerConnectionString);

//            try
//            {
//                await _notificationService.SendProgressAsync("Connecting to SQL Server");
//                await connection.OpenAsync(cancellationToken);
//                //Kibis
//            }
//            catch (SqlException ex)
//            {
//                _logger.LogError(ex, "SQL-specific failure");
//                await _notificationService.SendStatusUpdateasync("Could not connect to SQL server.Recheck the connection string", "error");
//                throw new DatabaseExceptions("SQL error occurred while opening connection", ex);

//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Unexpected error during SQL connection open");
//                await _notificationService.SendStatusUpdateasync("Unexpected error while connecting to SQL", "error");

//                throw new DatabaseExceptions("Unexpected error while trying to open SQL connection", ex);
//            }
//            _logger.LogInformation("Starting data fetch process with batch size {BatchSize}", _batchSize);

//            _logger.LogInformation("🚀 Starting parallel data transfer with batch size {BatchSize}", _batchSize);

//            var stopwatch = new Stopwatch();
//            stopwatch.Start();



//            await _notificationService.SendProgressAsync("Connecting to Cosmos DB");
//            try
//            {
//                    int batchCount = 0;
//                    int totaldocumentstransfered = 0;

//                    var channel = Channel.CreateBounded<List<IDictionary<string, object>>>(new BoundedChannelOptions(10)
//                    {
//                        SingleWriter = false,
//                        SingleReader = false,
//                        FullMode = BoundedChannelFullMode.Wait
//                    });

//                    // Start producer
//                    var producer = Task.Run(async () =>
//                    {
//                        try
//                        {
//                            await foreach (var batch in _cosmosDBDataFetchingRepository.FetchDataAsync(queryParameter, _batchSize, cancellationToken))
//                            {
//                                if (batch == null || batch.Count == 0) continue;

//                                int currentBatch = Interlocked.Increment(ref batchCount);
//                                _logger.LogInformation("📥 Producer fetched batch {BatchCount} with {RecordCount} records", currentBatch, batch.Count);

//                                await channel.Writer.WriteAsync(batch, cancellationToken);

//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogError(ex, "❌ Error in producer while fetching data");
//                        }
//                        finally
//                        {
//                            channel.Writer.Complete();
//                        }
//                    });

//                    _bulkInsertionService.DataTransferServiceAsync(queryParameter, connection, channel, producer, cancellationToken);

//                    stopwatch.Stop();
//                    await _notificationService.SendStatusUpdateasync($"Successfully transfered {totaldocumentstransfered} Documents", "success");
//                    _logger.LogInformation("Data transfer process completed successfully in {ElapsedTime} ms", stopwatch.ElapsedMilliseconds);
//            }
//            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
//            {
//                stopwatch.Stop();
//                _logger.LogError(ex, "Unauthorized Cosmos DB access");
//                await _notificationService.SendStatusUpdateasync("Unauthorized access to Cosmos DB", "error");
//                throw new CosmosDBExceptions("The User does not have access to cosmos db resource", ex);
//            }
//            catch (ArgumentException ex)
//            {
//                stopwatch.Stop();
//                _logger.LogError(ex, "Invalid Cosmos DB connection string");
//                await _notificationService.SendStatusUpdateasync("Invalid Cosmos DB connection string", "error");
//                throw new CosmosDBExceptions("Invalid  fromat of cosmos DB connection string", ex);
//            }
//            catch (Exception ex)
//            {
//                stopwatch.Stop();
//                _logger.LogError(ex, "Error during data transfer process");
//                throw;
//            }
//        }
//    }
//}
