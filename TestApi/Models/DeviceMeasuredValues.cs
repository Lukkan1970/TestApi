using System.Collections.Generic;

namespace TestApi.Models
{
    public class DeviceMeasuredValues
    {
        public string Name { get; set; }
        public List<MeasuredValue> MeasuredValues { get; set; }
    }
}
