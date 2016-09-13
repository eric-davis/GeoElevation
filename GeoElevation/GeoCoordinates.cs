using System;

namespace FinalSurge.GeoElevation
{
    /// <summary>
    /// The geo coordinates.
    /// </summary>
    public sealed class GeoCoordinates : IEquatable<GeoCoordinates>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoCoordinates"/> class.
        /// </summary>
        /// <param name="latitude">
        /// The latitude.
        /// </param>
        /// <param name="longitude">
        /// The longitude.
        /// </param>
        public GeoCoordinates(double latitude, double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        public double Latitude { get; private set; }

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        public double Longitude { get; private set; }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Equals(GeoCoordinates other)
        {
            return other != null && other.Latitude == this.Latitude && other.Longitude == this.Longitude;
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeoCoordinates);
        }

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }
    }
}
