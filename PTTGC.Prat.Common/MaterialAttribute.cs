using Newtonsoft.Json;

namespace PTTGC.Prat.Core
{
    public class MaterialAttribute
    {
        public string AttributeName { get; set; }

        public float? LowerBound { get; set; }

        public float? UpperBound { get; set; }

        public string MeasurementUnit { get; set; }
    }
}
