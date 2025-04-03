using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStore.Abstraction.IDTO;
using DataStore.Abstraction.IRepositories;
using DataStore.Implementation.DTO;
using Microsoft.Azure.Cosmos;

namespace DataStore.Implementation.Repositories
{
    public class CosmosDBDataFetchingRepository : ICosmosDBDataFetchingRepository
    {
        private readonly Container _container;

        public CosmosDBDataFetchingRepository(string connectionString, string databaseName, string containerName)
        {
            var cosmosClient = new CosmosClient(connectionString);
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<List<dynamic>> GetDataFromCosmosDBAsync(IQueryParameterDTO queryparameters, int batchSize = 1000)
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
            var query = $"SELECT * FROM c WHERE {whereClause}";
            var iterator = _container.GetItemQueryIterator<dynamic>(new QueryDefinition(query), requestOptions: new QueryRequestOptions { MaxItemCount = batchSize });

            var results = new List<dynamic>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
    }
}
