using System.Threading.Tasks;
using TestApi.AzureBlobFunctions;
using TestApi.Models;

namespace TestApi.APIs
{
    public static class V1
    {
        public static async Task<DeviceMeasuredValues> GetValuesAsync(string deviceId, string date, string? sensorType)
        {
            DeviceMeasuredValues measuredValues = await AzureBlobRetriever.GetMeasuredValuesAsync(deviceId, date, sensorType);

            return measuredValues;
        }
    }
}
