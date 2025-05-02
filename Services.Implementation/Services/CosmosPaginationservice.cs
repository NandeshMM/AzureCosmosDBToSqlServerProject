using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Services.Implementation.Services
{
    public class CosmosPaginationService
    {
        private readonly Container _container;

        public CosmosPaginationService(CosmosClient cosmosClient, string databaseId, string containerId)
        {
            _container = cosmosClient.GetContainer(databaseId, containerId);
        }

        public async Task<PagedResult<dynamic>> QueryWithPaginationAsync<T>(
            string query,
            IDictionary<string, object>? parameters = null,
            string? continuationToken = null,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
            {
                var queryDefinition = new QueryDefinition(query);

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        queryDefinition.WithParameter(param.Key, param.Value);
                    }
                }

                var queryRequestOptions = new QueryRequestOptions
                {
                    MaxItemCount = pageSize
                };

                var iterator = _container.GetItemQueryIterator<T>(
                    queryDefinition,
                    continuationToken,
                    queryRequestOptions);

                var results = new List<dynamic>();
                string? newToken = null;

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);
                    results.AddRange(response);
                    newToken = response.ContinuationToken;
                }

                return new PagedResult<dynamic>
                {
                    Results = results,
                    ContinuationToken = newToken
                };
            }
    }
}
