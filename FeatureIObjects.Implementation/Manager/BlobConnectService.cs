using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using FeatureObjects.Abstraction.IManager;
using Microsoft.Extensions.Configuration;

namespace FeatureObjects.Implementation.Manager
{
    public class BlobConnectService:IBlobservice
    {
        private readonly BlobServiceClient _blobclient;
        private readonly ILogger<BlobConnectService> _logger;
        private readonly IConfiguration _configurtaion;

        public BlobConnectService(IConfiguration configurtaions, ILogger<BlobConnectService> logger)
        {
            var connectionstring = configurtaions.GetConnectionString("Blobstorage");
            _blobclient=new BlobServiceClient(connectionstring);
            _logger= logger;
        }

        public async Task<string> GetBlobDataAsync(string containername,string filename)
        {
            try
            {
                var containerclient = _blobclient.GetBlobContainerClient(containername);
                var blobclient = containerclient.GetBlobClient(filename);

                if(!await blobclient.ExistsAsync())
                {
                    throw new FileNotFoundException($"Blob {filename} not found in the container {containername}");

                }

                BlobDownloadResult downloadresult = await blobclient.DownloadContentAsync();
                return downloadresult.Content.ToString();
            }

            catch(Exception ex)
            {
                _logger.LogError($"Error fetching blob {filename}: {ex.Message}");
                return string.Empty;
            }
        }





    }
}
