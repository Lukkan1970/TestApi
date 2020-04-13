using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TestApi.Models;

namespace TestApi.AzureBlobFunctions
{

    public class AzureBlobRetriever
    {
        private static CloudBlobClient _Client = null;
        private static string accountName = "sigmaiotexercisetest";
        private static string SAS = "?sv=2017-11-09&ss=bfqt&srt=sco&sp=rl&se=2028-09-27T16:27:24Z&st=2018-09-27T08:27:24Z&spr=https&sig=eYVbQneRuiGn103jUuZvNa6RleEeoCFx1IftVin6wuA%3D";

        private static void InitClient()
        {
            var storageCredentials = new StorageCredentials(SAS);
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, accountName, endpointSuffix: null, useHttps: true);
            _Client = cloudStorageAccount.CreateCloudBlobClient();
        }

        internal static async Task<DeviceMeasuredValues> GetMeasuredValuesAsync(string deviceId, string date, string? sensorType)
        {
            InitClient();
            var dev = new DeviceMeasuredValues();
            dev.Name = deviceId;

            CloudBlobContainer container = await GetContainerAsync(_Client);

            BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(string.Empty,
                       true, BlobListingDetails.Metadata, null, null, null, null);

            if (!CheckIfDeviceIdExist(resultSegment, deviceId))
            {
                dev.Name = "DeviceId does not exist";
                return dev;
            }

            List<string> sensorTypes = GetSensorTypesForDevice(resultSegment, deviceId);

            if (sensorType != null && !sensorTypes.Contains(sensorType))
            {
                dev.Name = "SensorType does not exist";
                return dev;
            }

            List<string> selectedSensorTypes = sensorType != null ? new List<string> { sensorType } : sensorTypes;

            List<List<SensorTypeValue>> listOfSensorValues = new List<List<SensorTypeValue>>();

            foreach (var sensor in selectedSensorTypes)
            {
                List<SensorTypeValue> sensorValues = new List<SensorTypeValue>();
                List<SensorTypeValue> mySensorFile = await GetSensorFileAsync(container, deviceId, date, sensor);
                listOfSensorValues.Add(mySensorFile);

            }
            List<MeasuredValue> firstSensorValues = await GetFirstSensorFileAsync(container, deviceId, date, selectedSensorTypes.First());
          
            
            foreach (var sensor in selectedSensorTypes)
            {
                List<SensorTypeValue> mySensorFile = await GetSensorFileAsync(container, deviceId, date, sensor);
                listOfSensorValues.Add(mySensorFile);

            }

            dev.MeasuredValues = firstSensorValues;
            return dev;
        }

        private static async Task<List<MeasuredValue>> GetFirstSensorFileAsync(CloudBlobContainer container, string deviceId, string date, string sensor)
        {
            List<MeasuredValue> data = new List<MeasuredValue>();
            string blobPath = deviceId + "/" + sensor + "/" + date + ".csv";
            CloudAppendBlob blob = container.GetAppendBlobReference(blobPath);

            string csvText = await blob.DownloadTextAsync();
            var lines = csvText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var lineSplit = line.Split(',');
               
                var lineTime = lineSplit[0].Trim(new char[] { ',', '-', ';' });
                var value = lineSplit[1];
                data.Add(new MeasuredValue
                {
                    Date = lineTime,
                    SensorValues = new List<SensorTypeValue> { new SensorTypeValue
                    {
                        SensorType = sensor,
                        SensorValue = value
                    }
                }
                });
            }


           
            return data;

        }

        private static async Task<List<SensorTypeValue>> GetSensorFileAsync(CloudBlobContainer container, string deviceId, string date, string sensor)
        {
            List<SensorTypeValue> data = new List<SensorTypeValue>();
            string blobPath = deviceId + "/" + sensor + "/" + date + ".csv";
            CloudAppendBlob blob = container.GetAppendBlobReference(blobPath);
            string text;
            string myText = await blob.DownloadTextAsync();
            var test = myText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in test)
            {
                var temp = line.Split(',');
                if (temp.Length < 2)
                {
                    var tpp = temp;
                }
                var ff = temp[1];
                data.Add(new SensorTypeValue {SensorType=sensor, SensorValue=ff});
            }

            return data;
        }

        private static List<string> GetSensorTypesForDevice(BlobResultSegment resultSegment, string deviceId)
        {
            List<string> ret = new List<string>();
            var numberOfBlobs = resultSegment.Results.Cast<CloudBlob>().Where(t => t.Name.StartsWith(deviceId)).ToList();
            return numberOfBlobs.Select(n => n.Name.Split('/')[1]).Distinct().ToList();
        }

        private static bool CheckIfDeviceIdExist(BlobResultSegment resultSegment, string deviceId)
        {
            var numberOfBlobs = resultSegment.Results.Cast<CloudBlob>().Where(t => t.Name.StartsWith(deviceId)).ToList();
            return numberOfBlobs.Count > 0;
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
