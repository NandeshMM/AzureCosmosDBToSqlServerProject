using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStore.Abstraction.IDTO;

namespace DataStore.Abstraction.IRepositories
{
    public interface ICosmosDBDataFetchingRepository
    {
        Task FetchDataAsync(IQueryParameterDTO queryparameters, int batchSize = 1000);
        
    }
}
