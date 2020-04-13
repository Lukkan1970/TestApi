using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using TestApi.Models;

namespace TestApi.AzureBlobFunctions
{
   
    public class AzureBlobRetriever
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private static CloudBlobClient _Client = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        private static string accountName = "sigmaiotexercisetest";
        private static string SAS = "?sv=2017-11-09&ss=bfqt&srt=sco&sp=rl&se=2028-09-27T16:27:24Z&st=2018-09-27T08:27:24Z&spr=https&sig=eYVbQneRuiGn103jUuZvNa6RleEeoCFx1IftVin6wuA%3D";



        public static void InitClient()
        {
            //var saName = _Context.Properties["AzureStorageAccountName"] as string;
            //var saKey = _Context.Properties["AzureStorageAccountAccessKey1"] as string;
            var storageCredentials = new StorageCredentials(SAS);
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, accountName, endpointSuffix: null, useHttps: true);
            _Client = cloudStorageAccount.CreateCloudBlobClient();
        }

        internal static async Task<DeviceMeasuredValues> GetMeasuredValuesAsync(string deviceId, string date, string? sensorType)
        {
            InitClient();
            var dev = new DeviceMeasuredValues();
            
            CloudBlobContainer container = await GetContainerAsync(_Client);
            dev.Name = container.Name;

            return dev;
        }

        private static async Task<CloudBlobContainer> GetContainerAsync(CloudBlobClient cloudBlobClient)
        {
            var containers = new List<CloudBlobContainer>();

            ContainerResultSegment response = await cloudBlobClient.ListContainersSegmentedAsync(null);
            containers.AddRange(response.Results);

            return containers.FirstOrDefault();

        }
    }
}
