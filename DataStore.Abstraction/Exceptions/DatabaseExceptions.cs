using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Abstraction.Exceptions
{
    public class DatabaseExceptions :Exception
    {

        public DatabaseExceptions(string message) : base(message)
        {

        }

        public DatabaseExceptions(string message, Exception innerException) : base(message, innerException)
        {

        }


    }
}
