using System.Data;
using DataStore.Abstraction.IRepositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using FeatureObjects.Abstraction.IManager;
using DataStore.Abstraction.IDTO;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using DataStore.Abstraction.Exceptions;
using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace FeatureObjects.Implementation.Manager
{
    public class BulkinsertionService : IBulkinsertion
    {
        private readonly ILogger<BulkinsertionService> _logger;
        private readonly IcolumnFetchReop _columnFetchRepo;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly int _batchSize;
        private readonly INotificationService _notificationService;
        private readonly ICosmosDBDataFetchingRepository _cosmosDBDataFetchingRepository;
        public BulkinsertionService(IcolumnFetchReop columnFetchRepo, ILogger<BulkinsertionService> logger, ICosmosDBDataFetchingRepository cosmosDBDataFetchingRepository, IConfiguration configuration, INotificationService notificationService)
        {
            _columnFetchRepo = columnFetchRepo;

            _logger = logger;

            _configuration = configuration;

            _batchSize = _configuration.GetValue<int>("BatchSize:Default");
            _notificationService = notificationService;
            _cosmosDBDataFetchingRepository = cosmosDBDataFetchingRepository;
        }
        public async Task DataTransferServiceAsync(IQueryParameterDTO queryParameter, CancellationToken cancellationToken)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            _logger.LogInformation("Starting parallel data transfer with batch size {BatchSize}", _batchSize);
            var stopwatch = Stopwatch.StartNew();
            int batchCount = 0;

            var channel = Channel.CreateBounded<List<IDictionary<string, object>>>(new BoundedChannelOptions(10)
            {
                SingleWriter = false,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait
            });

            using var connection = new SqlConnection(queryParameter.SQLServerConnectionString);

            try
            {
                await _notificationService.SendProgressAsync("Connecting to SQL Server");
                await connection.OpenAsync(cancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL-specific failure");
                await _notificationService.SendStatusUpdateasync("Could not connect to SQL server.Recheck the connection string", "error");
                throw new DatabaseExceptions("SQL error occurred while opening connection", ex);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during SQL connection open");
                await _notificationService.SendStatusUpdateasync("Unexpected error while connecting to SQL", "error");

                throw new DatabaseExceptions("Unexpected error while trying to open SQL connection", ex);
            }
            _logger.LogInformation("Starting data fetch process with batch size {BatchSize}", _batchSize);

            var dbColumns = await _columnFetchRepo.FetchCoulmnNameFromDB(connection, queryParameter);

            if (dbColumns == null || !dbColumns.Any())
            {
                _logger.LogWarning("No columns found in the database for table {TableName}", queryParameter.SQLTableName);
                _notificationService.SendStatusUpdateasync("SQL Table not found", "error");
                throw new NullExceptions("No columns were fetched from the database ");
            }

            connection.CloseAsync();

            await _notificationService.SendProgressAsync("Connecting to Cosmos DB");

            // Start producer
            var producer = Task.Run(async () =>
            {
                try
                {
                    await foreach (var batch in _cosmosDBDataFetchingRepository.FetchDataAsync(queryParameter, _batchSize, cancellationToken))
                    {
                        if (batch == null || !batch.Any())
                        {
                            _logger.LogWarning("Received empty batch");
                            _notificationService.SendStatusUpdateasync("No Data found in the container", "error");
                            continue;
                        }

                        int currentBatch = Interlocked.Increment(ref batchCount);
                        _logger.LogInformation("Producer fetched batch {BatchCount} with {RecordCount} records", currentBatch, batch.Count);

                        await channel.Writer.WriteAsync(batch, cancellationToken);
                    }
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Unauthorized Cosmos DB access");
                    await _notificationService.SendStatusUpdateasync("Unauthorized access to Cosmos DB", "error");
                    throw new CosmosDBExceptions("The User does not have access to cosmos db resource", ex);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, $"Container {queryParameter.CosmosDBDatabaseName} does not exists");
                    await _notificationService.SendStatusUpdateasync($"Container {queryParameter.CosmosDBDatabaseName} does not exists", "error");
                    throw new CosmosDBExceptions($"Container {queryParameter.CosmosDBDatabaseName} does not exists", ex);
                }
                catch (CosmosException ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Query is incorrect");
                    await _notificationService.SendStatusUpdateasync("Query is incorrect", "error");
                    throw new CosmosDBExceptions("The User does not have access to cosmos db resource", ex);
                }
                catch (ArgumentException ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Invalid Cosmos DB connection string");
                    await _notificationService.SendStatusUpdateasync("Invalid Cosmos DB connection string", "error");
                    throw new CosmosDBExceptions("Invalid  fromat of cosmos DB connection string", ex);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Error in producer while fetching data");
                    throw;
                }
                finally
                {
                    channel.Writer.Complete();
                }
            });

            var consumerCount = Math.Max(1, Environment.ProcessorCount / 2);
            var consumers = Enumerable.Range(0, consumerCount).Select(_ => Task.Run(async () =>
            {
                try
                {
                    using var connection = new SqlConnection(queryParameter.SQLServerConnectionString);
                    await connection.OpenAsync(cancellationToken);

                    await foreach (var batch in channel.Reader.ReadAllAsync(cancellationToken))
                    {
                        await BulkInsertToSqlAsync(queryParameter, batch, connection, dbColumns, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in consumer while processing data");
                }
            })).ToArray();

            await producer; 
            await Task.WhenAll(consumers); 

            stopwatch.Stop();
            _logger.LogInformation("Parallel Data transfer completed in {ElapsedTime} ms", stopwatch.ElapsedMilliseconds);
        }

        private async Task BulkInsertToSqlAsync(IQueryParameterDTO queryparameter, List<IDictionary<string, object>> records, SqlConnection connection, List<string> dbColumns, CancellationToken cancellationToken)
        {

            var stopwatch = Stopwatch.StartNew();

            var _destinationTableName = queryparameter.SQLTableName;
            //var dbColumns = await _columnFetchRepo.FetchCoulmnNameFromDB(connection, queryparameter);

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                BatchSize = _batchSize,
                DestinationTableName = _destinationTableName,
            };

            try
            {
                var dataTable = CreateDataTable(dbColumns);

                foreach (var columnName in dbColumns)
                {
                    bulkCopy.ColumnMappings.Add(columnName, columnName);
                }

                PopulateDataTableParallelSafe(dataTable, records, dbColumns);

                await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
                _logger.LogInformation("Successfully inserted {RecordCount} records into {TableName}",
                    records.Count, _destinationTableName);

                stopwatch.Stop();

                _logger.LogInformation("Bulk inserting to SQL Server each batch time taking {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk insert {RecordCount} records to {TableName}",
                    records.Count, _destinationTableName);
                throw new OperationFailureException("The Bulk insert opeartion was failed to complete", ex);
            }
        }

        private static DataTable CreateDataTable(List<string> columns)
        {
            var dataTable = new DataTable();


            var dataColumns = columns.Select(columnName => new DataColumn(columnName, typeof(object))).ToArray();


            dataTable.Columns.AddRange(dataColumns);

            return dataTable;
        }
        private void PopulateDataTableParallelSafe(DataTable dataTable, List<IDictionary<string, object>> records, List<string> dbColumns)
        {
            var rowsToAdd = new ConcurrentBag<DataRow>();
            var dataTableLock = new object();

            Parallel.ForEach(records, record =>
            {
                DataRow row;


                lock (dataTableLock)
                {
                    row = dataTable.NewRow();
                }

                foreach (var column in dbColumns)
                {
                    if (!dataTable.Columns.Contains(column)) continue;

                    object value = GetColumnValue(record, column);
                    row[column] = ConvertToAppropriateType(value, column);
                }

                rowsToAdd.Add(row);
            });


            dataTable.BeginLoadData();
            foreach (var row in rowsToAdd)
            {
                dataTable.Rows.Add(row);
            }
            dataTable.EndLoadData();

        }
        private static object GetColumnValue(IDictionary<string, object> records, string columnName)
        {
            var columnVariations = new[]
            {
          columnName,
          columnName.ToLower(),
          char.ToLower(columnName[0]) + columnName.Substring(1)
      };

            foreach (var column in columnVariations)
            {
                if (records.TryGetValue(column, out var value))
                {
                    return value;
                }
            }

            return null;
        }

        private static object ConvertToAppropriateType(object value, string columnName)
        {
            if (value == null)
            {
                return DBNull.Value;
            }
            if (columnName != null && columnName.Equals("operation", StringComparison.OrdinalIgnoreCase))
            {
                if (value is string operationStr)
                {
                    return string.Join("", operationStr.Select(c => $"\\u{(int)c:X4}"));
                }
            }
            return value switch
            {
                JObject jObj => jObj.ToString(Formatting.None),
                JValue jValue => jValue.Value ?? DBNull.Value,
                JToken jToken => jToken.ToString(Formatting.None),
                _ => value
            };

        }
    }
}
