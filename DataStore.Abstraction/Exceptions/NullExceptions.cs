using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Abstraction.Exceptions
{
    public class NullExceptions:Exception
    {

        public NullExceptions(string message):base(message) {
        
        }

public NullExceptions(string message, Exception innerException) : base(message, innerException) { 
        
        }

    }
}
