
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DataStore.Abstraction.Exceptions;
using DataStore.Abstraction.IDTO;
using DataStore.Abstraction.IRepositories;
using FeatureObjects.Abstraction.IManager;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DataStore.Implementation.Repositories
{
    public class CoulmnFetchRepo : IcolumnFetchReop
    {
        private readonly ILogger<CoulmnFetchRepo> _logger;
        private readonly INotificationService _notificationService;
        public CoulmnFetchRepo(ILogger<CoulmnFetchRepo> logger,INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }
        public async Task<List<string>> FetchCoulmnNameFromDB(SqlConnection connection, IQueryParameterDTO queryParameter, CancellationToken cancellationToken=default)
        {
            var columns = new List<string>();
            try
            {
                string Query = $@"
        SELECT COLUMN_NAME 
        FROM [{queryParameter.SQLDatabaseName}].INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = @tableName";

                using var command = new SqlCommand(Query, connection);
                command.Parameters.AddWithValue("@tableName", queryParameter.SQLTableName);


                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync())
                {
                    columns.Add(reader["COLUMN_NAME"].ToString());
                }

                return columns;
            }
            catch(SqlException ex)
            {
                _logger.LogError(ex, $"Database {queryParameter.SQLDatabaseName} not found");
                await _notificationService.SendStatusUpdateasync($"Database {queryParameter.SQLDatabaseName} not found", "error");
                throw new DatabaseExceptions($"Database {queryParameter.SQLDatabaseName} not found", ex);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during fetching the columns");
                await _notificationService.SendStatusUpdateasync("Unexpected error while fetching the columns", "error");

                throw new DatabaseExceptions("Unexpected error while trying to fetch the columns", ex);
            }
        }

    }

}

