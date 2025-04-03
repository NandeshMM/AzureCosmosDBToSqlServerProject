using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DataStore.Abstraction.IDTO;

namespace DataStore.Implementation.DTO
{
    public class QueryParameterDTO : IQueryParameterDTO
    {
        [Required]
        public string ContainerName { get; set; }
        [Required]
        public string TableName { get; set; }
         
        public string? CompanyId { get; set; }
        public string ?UserId { get; set; }

    }
}
