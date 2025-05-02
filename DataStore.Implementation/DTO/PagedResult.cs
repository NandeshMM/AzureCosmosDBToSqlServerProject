using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStore.Abstraction.IDTO;
using Microsoft.Azure.Cosmos;

namespace DataStore.Implementation.DTO
{
    public class PagedResult<T> : IPagedResult<T>
    {
        public IEnumerable<T> Results { get; set; }
        public string? ContinuationToken { get; set; }
    }
}
