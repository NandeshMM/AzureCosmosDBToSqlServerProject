using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureObjects.Abstraction.IManager
{
    public interface  IBlobservice
    {
        Task<string> GetBlobDataAsync(string containername, string filename);


    }
}
