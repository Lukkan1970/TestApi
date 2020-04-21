using System.Threading.Tasks;
using TestApi.AzureBlobFunctions;
using TestApi.Models;

#nullable enable

namespace TestApi.APIs
{
    public static class V1
    {
        public static async Task<string> GetValuesAsync(string deviceId, string date, string? sensorType)
        {
            DeviceMeasuredValues measuredValues = await AzureBlobRetriever.GetMeasuredValuesAsync(deviceId, date, sensorType);
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(measuredValues);
            return result;
        }
    }
}
