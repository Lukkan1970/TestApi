using System.Collections.Generic;

namespace TestApi.Models
{
    public class MeasuredValue
    {
        public string Date { get; set; }
        public List<SensorTypeValue> SensorValues { get; set; }
    }
}
