using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace FeatureObjects.Implementation.Manager
{
    public class BlobConnectService
    {
        private readonly BlobClient _blobclient;
        private readonly ILogger<BlobConnectService> _logger;

        public BlobConnectService(IConfiguration _configurtaion, ILogger<BlobConnectService> logger)//fetch the connection string dynamically
        {
            _blobclient=new BlobServiceClient(connectionstring);

            _logger= logger;
        }

        public async Task<string> GetBlobDataAsync(string containername,string filename)
        {
            try
            {
                var containerclient = _blobclient.GetBlobContainerClient(containername);
                var blobclient = containerclient.GetBlobClient(filename);

                if(!await blobclient.ExistAsysnc())
                {
                    throw new FileNotFoundException($"Blob {filename} not found in the container {containername}");

                }

                BlobDownloadResult downloadresult = await blobclient.DownloadContentAsync();
                return downloadresult.Content.Tostring();
            }

            catch(Exception ex)
            {
                _logger.LogError($"Error fetching blob {filename}: {ex.Message}");
                return string.Empty;
            }
        }





    }
}
