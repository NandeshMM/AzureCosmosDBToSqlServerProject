using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Abstraction.IDTO
{
    public interface IQueryParameterDTO
    {

      
        string ContainerName { get; set; }
     
        string TableName { get; set; }
        
        string CompanyId { get; set; }
        string UserId { get; set; }
    }
}
