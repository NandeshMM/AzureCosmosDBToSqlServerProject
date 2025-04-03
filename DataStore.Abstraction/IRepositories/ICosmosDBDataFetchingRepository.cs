using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Abstraction.IRepositories
{
    public interface ICosmosDBDataFetchingRepository
    {
        Task<List<dynamic>> GetDataFromCosmosDBAsync(int batchsize = 1000, QueryParameterDTO queryparameters);
    }
}
