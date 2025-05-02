using System.ComponentModel.DataAnnotations;
using DataStore.Abstraction.IDTO;
using Newtonsoft.Json;

namespace DataStore.Implementation.DTO
{
    public class QueryParameterDTO : IQueryParameterDTO
    {
        [Required]
        public string ContainerName { get; set; }
        [Required]
        public string CosmosDBDatabaseName { get; set; }
        [Required]
        public string Query { get; set; }
        //[Required]
        //public string CosmosDBAccountName{ get; set; }
        [Required]
        public string CosmosDBConnectionString { get; set; }
        [Required]
        public string SQLServerConnectionString { get; set; }
        [Required]
        public string SQLDatabaseName { get; set; }
        [Required]
        public string SQLTableName { get; set; }
    }
}
