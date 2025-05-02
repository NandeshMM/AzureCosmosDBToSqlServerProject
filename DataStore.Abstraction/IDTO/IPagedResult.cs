using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Abstraction.IDTO
{
    public interface IPagedResult<T>
    {
        IEnumerable<T> Results { get; set; }
        string? ContinuationToken { get; set; }
    }
}
