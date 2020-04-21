using System.Collections.Generic;

#nullable enable

namespace TestApi.Models
{
    public class Devices
    {
        public string? DeviceId { get; set; }
        public List<string>? SensorTypes { get; set; }
    }
}
