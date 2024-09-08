using System;
using Azure.Messaging.ServiceBus;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace ServiceBusFunctionApp
{
    public class ServiceBusFunction
    {
        [FunctionName("ServiceBusFunction")]
        public async Task Run(
            [ServiceBusTrigger("orderqueue", Connection = "ServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            BlobContainerClient containerClient = GetContainerClient("DefaultEndpointsProtocol=https;AccountName=eshoponwebblobstorage;AccountKey=v6z9N82l0VHTo7u/bv0IHICyTA1k/pvxgQP3uWEWCoeprlQEygo3xOVP00qlayeUPOfDSLY8yHNw+ASt+MTGcA==;EndpointSuffix=core.windows.net");
            string localPath = "data";
            string fileName = "OrderModelPayload" + Guid.NewGuid().ToString() + ".json";
            string localFilePath = Path.Combine(localPath, fileName);

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(message.Body.ToStream(), true);
            await messageActions.CompleteMessageAsync(message);
        }

        private static BlobContainerClient GetContainerClient(string Connection)
        {
            var blobServiceClient = new BlobServiceClient(Connection);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("eshoponwebblob");
            containerClient.CreateIfNotExistsAsync().Wait();

            containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
            return containerClient;
        }
    }
}
