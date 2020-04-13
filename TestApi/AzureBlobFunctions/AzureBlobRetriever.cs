using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApi.AzureBlobFunctions
{
   
    public class AzureBlobRetriever
    {
        private static CloudBlobClient _Client = null;

        public static void Class_Init()
        {

            //var saName = _Context.Properties["AzureStorageAccountName"] as string;
            //var saKey = _Context.Properties["AzureStorageAccountAccessKey1"] as string;


            var storageCredentials = new StorageCredentials("?sv=2017-11-09&ss=bfqt&srt=sco&sp=rl&se=2028-09-27T16:27:24Z&st=2018-09-27T08:27:24Z&spr=https&sig=eYVbQneRuiGn103jUuZvNa6RleEeoCFx1IftVin6wuA%3D");
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, "sigmaiotexercisetest", endpointSuffix: null, useHttps: true);
            _Client = cloudStorageAccount.CreateCloudBlobClient();



        }


    }
}
