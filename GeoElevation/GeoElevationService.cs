using System;
using System.Collections.Generic;
using System.Device.Location;

namespace FinalSurge.GeoElevation
{
    using System.IO;

    /// <summary>
    /// The geo elevation service.
    /// </summary>
    public sealed class GeoElevationService
    {
        /// <summary>
        /// The NED1 data directory.
        /// </summary>
        private readonly string ned1DataDirectory;

        /// <summary>
        /// The NED2 data directory.
        /// </summary>
        private readonly string ned2DataDirectory;

        /// <summary>
        /// The SRTM1 data directory.
        /// </summary>
        private readonly string srtm1DataDirectory;

        /// <summary>
        /// The SRTM3 data directory.
        /// </summary>
        private readonly string srtm3DataDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoElevationService"/> class.
        /// </summary>
        /// <param name="ned1DataDirectory">
        /// The NED1 data directory.
        /// </param>
        /// <param name="ned2DataDirectory">
        /// The NED2 data directory.
        /// </param>
        /// <param name="srtm1DataDirectory">
        /// The SRTM1 data directory.
        /// </param>
        /// <param name="srtm3DataDirectory">
        /// The SRTM3 data directory.
        /// </param>
        public GeoElevationService(
            string ned1DataDirectory,
            string ned2DataDirectory,
            string srtm1DataDirectory,
            string srtm3DataDirectory)
        {
            if (!Directory.Exists(ned1DataDirectory))
            {
                throw new DirectoryNotFoundException(
                    $"Specified {nameof(ned1DataDirectory)} was not found: {ned1DataDirectory}");
            }

            if (!Directory.Exists(ned2DataDirectory))
            {
                throw new DirectoryNotFoundException(
                    $"Specified {nameof(ned2DataDirectory)} was not found: {ned2DataDirectory}");
            }

            if (!Directory.Exists(srtm1DataDirectory))
            {
                throw new DirectoryNotFoundException(
                    $"Specified {nameof(srtm1DataDirectory)} was not found: {srtm1DataDirectory}");
            }

            if (!Directory.Exists(srtm3DataDirectory))
            {
                throw new DirectoryNotFoundException(
                    $"Specified {nameof(srtm3DataDirectory)} was not found: {srtm3DataDirectory}");
            }

            this.ned1DataDirectory = ned1DataDirectory;
            this.ned2DataDirectory = ned2DataDirectory;
            this.srtm1DataDirectory = srtm1DataDirectory;
            this.srtm3DataDirectory = srtm3DataDirectory;
        }

        /// <summary>
        /// Gets the elevation value for the specified geo coordinate
        /// </summary>
        /// <param name="coordindate">
        /// The coordinate.
        /// </param>
        /// <returns>
        /// The <see cref="Nullable{Double}"/>.
        /// </returns>
        public double? GetElevation(GeoCoordinate coordindate)
        {
            var elevation = new Ned1DataStore(this.ned1DataDirectory).GetElevation(coordindate);
            if (elevation != null)
            {
                return elevation;
            }

            elevation = new Ned2DataStore(this.ned2DataDirectory).GetElevation(coordindate);
            if (elevation != null)
            {
                return elevation;
            }

            elevation = new Srtm1DataStore(this.srtm1DataDirectory).GetElevation(coordindate);
            if (elevation != null)
            {
                return elevation;
            }

            elevation = new Srtm3DataStore(this.srtm3DataDirectory).GetElevation(coordindate);
            return elevation;
        }

        /// <summary>
        /// Updates the elevation values for a series of device points.
        /// </summary>
        /// <param name="devicePoints">
        /// The device points.
        /// </param>
        /// <returns>
        /// The updated <see cref="IEnumerable{DevicePoint}"/>.
        /// </returns>
        public IEnumerable<DevicePoint> UpdateElevations(List<DevicePoint> devicePoints)
        {
            devicePoints.ForEach(
                x =>
                    {
                        if (!x.Latitude.HasValue || !x.Longitude.HasValue)
                        {
                            return;
                        }

                        var latitude = (double)x.Latitude.Value;
                        var longitude = (double)x.Longitude.Value;
                        var coord = new GeoCoordinate(latitude, longitude);
                        var elevation = this.GetElevation(coord);
                        if (!elevation.HasValue)
                        {
                            return;
                        }

                        x.AltitudeCalculated = (decimal)elevation.Value;
                    });

            return devicePoints;
        }
    }
}
