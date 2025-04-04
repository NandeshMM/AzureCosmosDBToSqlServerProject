using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text;
using DataStore.Implementation;
using Microsoft.Extensions.Logging;
using FeatureObjects.Abstraction.IManager;
using DataStore.Implementation.DTO;
using DataStore.Implementation.Repositories;

namespace FeatureObjects.Implementation.Manager
{
    public class Datatransferservice:IDatatransfer
    {
        private readonly ILogger<Datatransferservice> _logger;
        private readonly CosmosDBDataFetchingRepository _cosmosrepo;
        private readonly SqlServerRepository _sqlRepo;
        private readonly BlobConnectService _blobService;

        public Datatransferservice(CosmosDBDataFetchingRepository cosmosrepo, SqlServerRepository sqlRepo, BlobConnectService blobService, ILogger<Datatransferservice> logger)
        {
            _blobService = blobService;
            _sqlRepo = sqlRepo;
            _cosmosrepo = cosmosrepo;
            _logger = logger;
        }

        public async Task TransferDataAsync(QueryParameterDTO dto)
        {
            var records = await _cosmosrepo.GetAllRecordsAsync(dto);

            foreach (var record in records)
            {
                string tablename = record.tableName?.Tostring();
                DynamicDto dto;


                if (record.ContainsKey("isLarge") && record.isLarge == true)
                {
                    string containerName = record.storageAccountContainer?.ToString();
                    string fileName = record.fileName?.Tostring();



                    if (!string.IsNullOrEmpty(containerName) && !string.IsNullOrEmpty(fileName))
                    {


                        var blobdata = await _blobService.GetBlobDataAsync(containerName, fileName);
                        dto = ConvertBlobDataToDto(blobdata);

                    }
                    else
                    {
                        _logger.LogError("Invalid blob storage reference");
                        continue;
                    }
                }
                else
                {
                    dto = new DynamicDto(record);
                }

                await _sqlRepo.InsertAsync(dto);


            }
        }
    }
}