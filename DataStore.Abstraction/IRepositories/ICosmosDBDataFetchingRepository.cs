
using System.Runtime.CompilerServices;
using DataStore.Abstraction.IDTO;
using Microsoft.Azure.Cosmos;

namespace DataStore.Abstraction.IRepositories
{
    public interface ICosmosDBDataFetchingRepository
    {
        IAsyncEnumerable<List<IDictionary<string, object>>> FetchDataAsync(
    IQueryParameterDTO queryparameters,
    int pageSize,
    //Container container,
    //string partitionKey, // NEW PARAM
    CancellationToken cancellationToken);
    }
}
