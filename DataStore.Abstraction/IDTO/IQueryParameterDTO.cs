

using System.ComponentModel.DataAnnotations;

namespace DataStore.Abstraction.IDTO
{
    public interface IQueryParameterDTO
    {
        string ContainerName { get; set; }
        string CosmosDBDatabaseName { get; set; }
        string Query { get; set; }
        //string CosmosDBAccountName { get; set; }
        string CosmosDBConnectionString { get; set; }
        string SQLServerConnectionString { get; set; }
        string SQLDatabaseName { get; set; }
        string SQLTableName { get; set; }
    }
}
