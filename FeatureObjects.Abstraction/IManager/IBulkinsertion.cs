
using System.Threading.Channels;
using DataStore.Abstraction.IDTO;
using Microsoft.Data.SqlClient;

namespace FeatureObjects.Abstraction.IManager
{
    public interface IBulkinsertion
    {
        Task DataTransferServiceAsync(IQueryParameterDTO queryParameter, CancellationToken cancellationToken);
    }
}
