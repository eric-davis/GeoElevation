using System;
using System.Collections.Concurrent;
using System.Device.Location;
using System.IO;

namespace FinalSurge.GeoElevation
{
    /// <summary>
    /// The NED1 data store.
    /// </summary>
    public sealed class Ned1DataStore : GeoElevationDataStore
    {
        /// <summary>
        /// The data tile file stream cache
        /// </summary>
        private static readonly ConcurrentDictionary<string, FileStream> FileStreams;

        /// <summary>
        /// Initializes static members of the <see cref="Ned1DataStore"/> class.
        /// </summary>
        static Ned1DataStore()
        {
            FileStreams = new ConcurrentDictionary<string, FileStream>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ned1DataStore"/> class.
        /// </summary>
        /// <param name="dataFileDirectory">
        /// The data file directory.
        /// </param>
        public Ned1DataStore(string dataFileDirectory)
            : base(dataFileDirectory)
        {
            // Nothing to do...
        }

        /// <summary>
        /// The data file extension.
        /// </summary>
        protected override string DataFileExtension => "ele";

        /// <summary>
        /// The row count.
        /// </summary>
        protected override int RowCount => 3612;

        /// <summary>
        /// The column count.
        /// </summary>
        protected override int ColumnCount => 3612;

        /// <summary>
        /// The bytes per record.
        /// </summary>
        protected override int BytesPerRecord => 2;

        /// <summary>
        /// The latitude interval.
        /// </summary>
        protected override double LatitudeInterval => -1.0 / 3600.0; // 1 arc-second

        /// <summary>
        /// The longitude interval.
        /// </summary>
        protected override double LongitudeInterval => 1.0 / 3600.0; // 1 arc-second

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
            var filename = $"{GetNorthwestCornerFilename(targetCoordinates)}.{this.DataFileExtension}";
            FileStream stream;
            if (!FileStreams.TryGetValue(filename, out stream))
            {
                try
                {
                    stream = File.OpenRead(this.DataFileDirectory + filename);
                }
                catch (FileNotFoundException)
                {
                    FileStreams.TryAdd(filename, null);
                    return null;
                }

                FileStreams.TryAdd(filename, stream);
            }

            return this.GetElevation(stream, targetCoordinates);
        }
    }
}
