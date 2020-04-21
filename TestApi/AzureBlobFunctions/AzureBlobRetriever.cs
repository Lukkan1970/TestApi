using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using TestApi.Models;
using FSharpTestApi;

#nullable enable

namespace TestApi.AzureBlobFunctions
{

    public class AzureBlobRetriever
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private static CloudBlobClient _Client = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private static readonly string accountName = "sigmaiotexercisetest";
        private static readonly string SAS = "?sv=2017-11-09&ss=bfqt&srt=sco&sp=rl&se=2028-09-27T16:27:24Z&st=2018-09-27T08:27:24Z&spr=https&sig=eYVbQneRuiGn103jUuZvNa6RleEeoCFx1IftVin6wuA%3D";

        private static void InitClient()
        {
            var storageCredentials = new StorageCredentials(SAS);
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, accountName, endpointSuffix: null, useHttps: true);
            _Client = cloudStorageAccount.CreateCloudBlobClient();
        }

        internal static async Task<DeviceMeasuredValues> GetMeasuredValuesAsync(string deviceId, string date, string? sensorType)
        {
            InitClient();
            var device = new DeviceMeasuredValues
            {
                Name = deviceId
            };

            CloudBlobContainer container = await GetContainerAsync(_Client);
            BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(string.Empty, true, BlobListingDetails.Metadata, null, null, null, null);

            if (!CheckIfDeviceIdExist(resultSegment, deviceId))
            {
                device.Name = "DeviceId does not exist";
                return device;
            }

            List<string> sensorTypes = GetSensorTypesForDevice(resultSegment, deviceId);

            if (sensorType != null && !sensorTypes.Contains(sensorType))
            {
                device.Name = "SensorType does not exist";
                return device;
            }

            List<string> selectedSensorTypes = sensorType != null ? new List<string> { sensorType } : sensorTypes;

            List<MeasuredValue> allSensorValues = new List<MeasuredValue>();

            List<Task<List<MeasuredValue>>> tasks = new List<Task<List<MeasuredValue>>>();

            foreach (var sensor in selectedSensorTypes)
            {
                Task<List<MeasuredValue>> theTask = GetSensorFileAsync(container, deviceId, date, sensor);
                tasks.Add(theTask);

            }
            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                List<MeasuredValue> sensorFile = task.Result;
                if (task == tasks.First())
                {
                    allSensorValues = sensorFile;
                }
                else
                {
                    allSensorValues = allSensorValues.Zip(sensorFile, (first, second) =>
                    {
                        MeasuredValue mv = first;
                        mv.SensorValues.Add(second.SensorValues[0]);
                        return mv;
                    }).ToList();
                }
            }

            if (allSensorValues.Count == 0)
            {
                device.Name = "No data found that date";
            }

            device.MeasuredValues = allSensorValues;

            return device;
        }

        private static async Task<List<MeasuredValue>> GetSensorFileAsync(CloudBlobContainer container, string deviceId, string date, string sensor)
        {
            List<MeasuredValue> data = new List<MeasuredValue>();
            string csvText = await GetFileFromAzure(container, deviceId, date, sensor);

            var lines = csvText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var lineSplit = line.Split(';');
                var lineTime = lineSplit[0];
                var lineValue = lineSplit[1];
                data.Add(new MeasuredValue
                {
                    Date = lineTime,
                    SensorValues = new List<SensorTypeValue> { new SensorTypeValue
                    {
                        SensorType = sensor,
                        SensorValue = lineValue
                    }
                }
                });
            }
            return data;
        }

        private static async Task<string> GetFileFromAzure(CloudBlobContainer container, string deviceId, string date, string sensor)
        {
            string text = String.Empty;
            string blobPath = deviceId + "/" + sensor + "/" + date + ".csv";
            string zipPath = deviceId + "/" + sensor + "/historical.zip";

            if (await container.GetAppendBlobReference(blobPath).ExistsAsync())
            {
                text = await GetCsvFileFromAzure(container, blobPath);
            }
            else if (await container.GetBlockBlobReference(zipPath).ExistsAsync())
            {
                text = await GetZipFileFromAzure(container, date, zipPath);
            }

            return text;
        }

        private static async Task<string> GetZipFileFromAzure(CloudBlobContainer container, string date, string zipPath)
        {
            CloudBlockBlob zipBlob = container.GetBlockBlobReference(zipPath);

            using var blobStream = new MemoryStream();
            await zipBlob.DownloadToStreamAsync(blobStream);

            using ZipArchive zip = new ZipArchive(blobStream);
            var entryName = date + ".csv";
            var entry = zip.Entries.Where(e => e.Name == entryName).FirstOrDefault();

            if (entry != null)
            {
                using StreamReader sr = new StreamReader(entry.Open());
                return sr.ReadToEnd();
            }
            else
                return string.Empty;
        }

        private static async Task<string> GetCsvFileFromAzure(CloudBlobContainer container, string blobPath)
        {
            CloudAppendBlob blob = container.GetAppendBlobReference(blobPath);
            string csvText = await blob.DownloadTextAsync();
            return csvText;
        }

        private static List<string> GetSensorTypesForDevice(BlobResultSegment resultSegment, string deviceId)
        {
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
