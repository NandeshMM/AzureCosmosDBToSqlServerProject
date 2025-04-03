using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStore.Implementation;

namespace FeatureObjects.Abstraction.IManager
{
    public interface IDatatransfer
    {
        Task TransferDataAsync(QueryParameterDTO dto);

    }
}
