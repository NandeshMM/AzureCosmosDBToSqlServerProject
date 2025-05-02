using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Abstraction.Exceptions
{
    public class CosmosDBExceptions : Exception
    {
        public CosmosDBExceptions(string message) : base(message)
        {

        }

        public CosmosDBExceptions(string message, Exception innerException) : base(message, innerException)
        {

        }

    }
}
