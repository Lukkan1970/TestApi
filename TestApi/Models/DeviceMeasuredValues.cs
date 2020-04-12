using System.Collections.Generic;

namespace TestApi.Models
{
    public class DeviceMeasuredValues
    {
        public string Name { get; set; }
        public IEnumerable<MeasuredValue> MeasuredValues { get; set; }
    }
}
