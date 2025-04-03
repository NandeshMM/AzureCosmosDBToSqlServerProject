using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureIObjects.Implementation
{
    public class Datatransferservice
    {
        private readonly CosmosDbRepository _cosmosrepo;
        private readonly SqlServerRepository _sqlRepo;
            private readonly BlobstorageService _blobService;

        public Datatransferservice(CosmosDbRepository cosmosrepo, SqlServerRepository sqlRepo, BlobstorageService blobService)
        {
            _blobService = blobService;
            _sqlRepo = sqlRepo;
             _cosmosrepo = cosmosrepo;
        }

        public async Task TransferDataAsync()
        {
            var records= await _cosmosrepo.GetAllRecordsAsync();


        }


    }
}
