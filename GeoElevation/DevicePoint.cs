using System.Xml.Serialization;

namespace FinalSurge.GeoElevation
{
    /// <summary>
    /// The device point.
    /// </summary>
    public class DevicePoint
    {
        [XmlElement(IsNullable = true)]
        public int? LapNumber { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public double? StartSeconds { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public decimal? Latitude { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public decimal? Longitude { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public decimal? AltitudeCalculated { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public decimal? Altitude { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public decimal? Distance { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public decimal? Speed { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public int? HR { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public int? RPM { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public int? CAD { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public int? Watts { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public int? Calories { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public int? Temp { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public decimal? GroundContact { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public decimal? VerticalOscillation { get; set; }
    }
}
