using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStore.Abstraction.IDTO;

namespace FeatureObjects.Abstraction.IManager
{
    public interface ICosmosFetchService
    {
        Task ProcessDataTransferAsync(IQueryParameterDTO queryParameter, CancellationToken cancellationToken);
    }
}
