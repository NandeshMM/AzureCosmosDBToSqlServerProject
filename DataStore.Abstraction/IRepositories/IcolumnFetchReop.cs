
using DataStore.Abstraction.IDTO;
using Microsoft.Data.SqlClient;

namespace DataStore.Abstraction.IRepositories
{
    public interface IcolumnFetchReop
    {
        Task<List<string>> FetchCoulmnNameFromDB(SqlConnection connection, IQueryParameterDTO queryparameter, CancellationToken cancellationToken=default);
    }
}
