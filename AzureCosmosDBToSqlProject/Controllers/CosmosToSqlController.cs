using System.Diagnostics;
using DataStore.Implementation.DTO;
using FeatureObjects.Abstraction.IManager;
using Microsoft.AspNetCore.Mvc;

namespace AzureCosmosDBToSqlServerProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CosmosToSqlController:ControllerBase
    {
        private readonly ILogger<CosmosToSqlController> _logger;
        private readonly IBulkinsertion _bulkinsertionservice;
        public CosmosToSqlController(ILogger<CosmosToSqlController> logger, IBulkinsertion bulkinsertionservice)
        {
            _logger = logger;
            _bulkinsertionservice = bulkinsertionservice;
        }
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferData([FromBody] QueryParameterDTO dto, CancellationToken cancellationToken)
        {
            if (dto == null)
            {
                return BadRequest("Invalid request");
            }
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await _bulkinsertionservice.DataTransferServiceAsync(dto,cancellationToken);
                stopwatch.Stop();
                _logger.LogInformation("Total time {ElapsedMillisecond} ms", stopwatch.ElapsedMilliseconds);
                return Ok("Data transfer initiated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
                                    