using System;
using System.Collections.Concurrent;
using System.Device.Location;
using System.IO;

namespace FinalSurge.GeoElevation
{
    /// <summary>
    /// The NED2 data store.
    /// </summary>
    public sealed class Ned2DataStore : GeoElevationDataStore
    {
        /// <summary>
        /// The data tile file stream cache
        /// </summary>
        private static readonly ConcurrentDictionary<string, FileStream> FileStreams;

        /// <summary>
        /// Initializes static members of the <see cref="Ned2DataStore"/> class.
        /// </summary>
        static Ned2DataStore()
        {
            FileStreams = new ConcurrentDictionary<string, FileStream>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ned2DataStore"/> class.
        /// </summary>
        /// <param name="dataFileDirectory">
        /// The data file directory.
        /// </param>
        public Ned2DataStore(string dataFileDirectory)
            : base(dataFileDirectory)
        {
            // Nothing to do...
        }

        /// <summary>
        /// The data file extension.
        /// </summary>
        protected override string DataFileExtension => "ele";

        /// <summary>
        /// The data file naming convention type.
        /// </summary>
        protected override DataFileNamingType DataFileNaming => DataFileNamingType.NorthwestCorner;

        /// <summary>
        /// The row count.
        /// </summary>
        protected override int RowCount => 1812;

        /// <summary>
        /// The column count.
        /// </summary>
        protected override int ColumnCount => 1812;

        /// <summary>
        /// The bytes per record.
        /// </summary>
        protected override int BytesPerRecord => 2;

        /// <summary>
        /// The latitude interval.
        /// </summary>
        protected override double LatitudeInterval => -1.0 / 1800.0; // 2 arc-seconds

        /// <summary>
        /// The longitude interval.
        /// </summary>
        protected override double LongitudeInterval => 1.0 / 1800.0; // 2 arc-seconds

        /// <summary>
        /// The is big endian.
        /// </summary>
        protected override bool IsBigEndian => false;

        /// <summary>
        /// The no elevation data value.
        /// </summary>
        protected override int NoElevationDataValue => 65535;

        /// <summary>
        /// The add to elevation result.
        /// </summary>
        protected override int AddToElevationResult => -1000;

        /// <summary>
        /// The elevation result multiplier.
        /// </summary>
        protected override double ElevationResultMultiplier => 0.1;

        /// <summary>
        /// Gets the elevation value for a specific set of coordinates.
        /// </summary>
        /// <param name="targetCoordinates">
        /// The target coordinates.
        /// </param>
        /// <returns>
        /// The <see cref="Nullable{Double}"/> elevation value in meters.
        /// </returns>
        public double? GetElevation(GeoCoordinate targetCoordinates)
        {
            return this.GetElevation(FileStreams, targetCoordinates);
        }
    }
}
