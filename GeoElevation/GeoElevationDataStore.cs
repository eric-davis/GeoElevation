using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Linq;

namespace FinalSurge.GeoElevation
{
    using System.Collections.Concurrent;

    /// <summary>
    /// The geo elevation data store base class.
    /// </summary>
    public abstract class GeoElevationDataStore
    {
        /// <summary>
        /// The data file directory.
        /// </summary>
        private readonly string dataFileDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoElevationDataStore"/> class.
        /// </summary>
        /// <param name="dataFileDirectory">
        /// The data file directory.
        /// </param>
        protected GeoElevationDataStore(string dataFileDirectory)
        {
            if (!Directory.Exists(dataFileDirectory))
            {
                throw new DirectoryNotFoundException($"Data file directory not found: {dataFileDirectory}");
            }

            this.dataFileDirectory = dataFileDirectory;
        }

        /// <summary>
        /// The data file naming type.
        /// </summary>
        protected enum DataFileNamingType
        {
            /// <summary>
            /// Based upon southwest corner.
            /// </summary>
            SouthwestCorner,

            /// <summary>
            /// Based upon northwest corner.
            /// </summary>
            NorthwestCorner
        }

        /// <summary>
        /// Gets the data file extension.
        /// </summary>
        protected abstract string DataFileExtension { get; }

        /// <summary>
        /// Gets the data file naming.
        /// </summary>
        protected abstract DataFileNamingType DataFileNaming { get; }

        /// <summary>
        /// Gets the row count.
        /// </summary>
        protected abstract int RowCount { get; }

        /// <summary>
        /// Gets the column count.
        /// </summary>
        protected abstract int ColumnCount { get; }

        /// <summary>
        /// Gets the bytes per record.
        /// </summary>
        protected abstract int BytesPerRecord { get; }

        /// <summary>
        /// Gets the latitude interval.
        /// </summary>
        protected abstract double LatitudeInterval { get; }

        /// <summary>
        /// Gets the longitude interval.
        /// </summary>
        protected abstract double LongitudeInterval { get; }

        /// <summary>
        /// Gets a value indicating whether the data is stored as big endian.
        /// </summary>
        protected abstract bool IsBigEndian { get; }

        /// <summary>
        /// Gets the no elevation data value.
        /// </summary>
        protected abstract int NoElevationDataValue { get; }

        /// <summary>
        /// Gets the add to elevation result.
        /// </summary>
        protected abstract int AddToElevationResult { get; }

        /// <summary>
        /// Gets the elevation result multiplier.
        /// </summary>
        protected abstract double ElevationResultMultiplier { get; }

        /// <summary>
        /// Gets the southwest corner filename.
        /// </summary>
        /// <param name="targetCoordinates">
        /// The target coordinates.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> filename.
        /// </returns>
        protected static string GetSouthwestCornerFilename(GeoCoordinate targetCoordinates)
        {
            var latPart = targetCoordinates.Latitude >= 0
                               ? $"N{Math.Floor(targetCoordinates.Latitude):00}"
                               : $"S{Math.Ceiling(Math.Abs(targetCoordinates.Latitude)):00}";

            var lonPart = targetCoordinates.Longitude >= 0
                              ? $"E{Math.Floor(targetCoordinates.Longitude):000}"
                              : $"W{Math.Ceiling(Math.Abs(targetCoordinates.Longitude)):000}";

            return string.Concat(latPart, lonPart);
        }

        /// <summary>
        /// Gets the northwest corner filename.
        /// </summary>
        /// <param name="targetCoordinates">
        /// The target coordinates.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> filename.
        /// </returns>
        protected static string GetNorthwestCornerFilename(GeoCoordinate targetCoordinates)
        {
            var latPart = targetCoordinates.Latitude >= 0
                               ? $"N{Math.Ceiling(targetCoordinates.Latitude):00}"
                               : $"S{Math.Floor(Math.Abs(targetCoordinates.Latitude)):00}";

            var lonPart = targetCoordinates.Longitude >= 0
                              ? $"E{Math.Floor(targetCoordinates.Longitude):000}"
                              : $"W{Math.Ceiling(Math.Abs(targetCoordinates.Longitude)):000}";

            return string.Concat(latPart, lonPart);
        }

        /// <summary>
        /// Gets the elevation value from the provided data file stream for the specified coordinates.
        /// </summary>
        /// <param name="fileStreamCache">
        /// The data file stream cache.
        /// </param>
        /// <param name="targetCoordinates">
        /// The target coordinates.
        /// </param>
        /// <returns>
        /// The <see cref="Nullable{Double}"/> elevation value; null if not found or invalid.
        /// </returns>
        protected virtual double? GetElevation(ConcurrentDictionary<string, FileStream> fileStreamCache, GeoCoordinate targetCoordinates)
        {
            var filename = this.DataFileNaming == DataFileNamingType.SouthwestCorner
                               ? $"{GetSouthwestCornerFilename(targetCoordinates)}"
                               : $"{GetNorthwestCornerFilename(targetCoordinates)}";

            filename = $"{filename}.{this.DataFileExtension}";

            FileStream stream;
            if (!fileStreamCache.TryGetValue(filename, out stream))
            {
                try
                {
                    stream = File.OpenRead(this.dataFileDirectory + filename);
                    fileStreamCache.TryAdd(filename, stream);
                }
                catch (FileNotFoundException)
                {
                    fileStreamCache.TryAdd(filename, null);
                }
            }

            if (stream == null)
            {
                return null;
            }

            var extraRows = this.RowCount - (1.0 / Math.Abs(this.LatitudeInterval));
            var extraCols = this.ColumnCount - (1.0 / Math.Abs(this.LongitudeInterval));

            // ((int)extraRows / 2) yields 0 when extraRows is 1, because a single row of overlap does not need an offset
            var startOffsetLat = ((int)extraRows / 2) * (-1 * this.LatitudeInterval);
            var startOffsetLon = ((int)extraCols / 2) * (-1 * this.LongitudeInterval);

            // If there are an even number of rows, that implies that the whole numbers (e.g., 45.000000 degrees) 
            // fall BETWEEN the records, not on them, so adjust things back half a row...
            if (this.RowCount % 2 == 0)
            {
                startOffsetLat += this.LatitudeInterval / 2;
            }

            if (this.ColumnCount % 2 == 0)
            {
                startOffsetLon += this.LongitudeInterval / 2;
            }

            // Fortunately, all file formats start in the NW corner and proceed 
            // eastward first, then southward after each row.
            var nwCornerCoords = GetNorthWestTileCoordinates(targetCoordinates);
            var startingLat = nwCornerCoords.Latitude + startOffsetLat;
            var startingLon = nwCornerCoords.Longitude + startOffsetLon;

            var requestedRow = (targetCoordinates.Latitude - startingLat) / this.LatitudeInterval;
            requestedRow = Math.Truncate(requestedRow * 1000000) / 1000000;

            var requestedCol = (targetCoordinates.Longitude - startingLon) / this.LongitudeInterval;
            requestedCol = Math.Truncate(requestedCol * 1000000) / 1000000;

            var lowerRow = (int)requestedRow;
            var higherRow = lowerRow + 1;
            var lowerCol = (int)requestedCol;
            var higherCol = lowerCol + 1;

            var rowProportion = requestedRow - lowerRow;
            var colProportion = requestedCol - lowerCol;

            var nwByteOffset = (lowerRow * this.ColumnCount * this.BytesPerRecord) + (lowerCol * this.BytesPerRecord);
            var neByteOffset = (lowerRow * this.ColumnCount * this.BytesPerRecord) + (higherCol * this.BytesPerRecord);
            var swByteOffset = (higherRow * this.ColumnCount * this.BytesPerRecord) + (lowerCol * this.BytesPerRecord);
            var seByteOffset = (higherRow * this.ColumnCount * this.BytesPerRecord) + (higherCol * this.BytesPerRecord);

            var buffer = new byte[this.BytesPerRecord];
            stream.Seek(nwByteOffset, SeekOrigin.Begin);
            stream.Read(buffer, 0, this.BytesPerRecord);
            var nwElevation = GetElevationValueFromByteArray(buffer, this.IsBigEndian);

            stream.Seek(neByteOffset, SeekOrigin.Begin);
            stream.Read(buffer, 0, this.BytesPerRecord);
            var neElevation = GetElevationValueFromByteArray(buffer, this.IsBigEndian);

            stream.Seek(swByteOffset, SeekOrigin.Begin);
            stream.Read(buffer, 0, this.BytesPerRecord);
            var swElevation = GetElevationValueFromByteArray(buffer, this.IsBigEndian);

            stream.Seek(seByteOffset, SeekOrigin.Begin);
            stream.Read(buffer, 0, this.BytesPerRecord);
            var seElevation = GetElevationValueFromByteArray(buffer, this.IsBigEndian);

            // Most formats will indicate "no data" with -9999 or -32768, but some may use a specific value
            nwElevation = nwElevation == this.NoElevationDataValue ? -32768 : nwElevation;
            neElevation = neElevation == this.NoElevationDataValue ? -32768 : neElevation;
            swElevation = swElevation == this.NoElevationDataValue ? -32768 : swElevation;
            seElevation = seElevation == this.NoElevationDataValue ? -32768 : seElevation;

            // This is probably a highly suspect way of accounting for missing corners...
            if (nwElevation <= -999 && neElevation > -999 && swElevation > -999)
            {
                nwElevation = (neElevation + swElevation) / 2;
            }

            if (neElevation <= -999 && nwElevation > -999 && seElevation > -999)
            {
                neElevation = (nwElevation + seElevation) / 2;
            }

            if (swElevation <= -999 && nwElevation > -999 && seElevation > -999)
            {
                swElevation = (nwElevation + seElevation) / 2;
            }

            if (seElevation <= -999 && neElevation > -999 && swElevation > -999)
            {
                seElevation = (neElevation + swElevation) / 2;
            }

            // If any corners are missing, bail out; the interpolation calculations won't work unless there are four numbers
            var cornerElevations = new List<int> { nwElevation, neElevation, swElevation, seElevation };
            if (cornerElevations.Any(e => e <= -999) || cornerElevations.Any(e => e == -1))
            {
                return null;
            }

            // Proportionally average the four corners by drawing a N-S line through the rectangle
            var northElevation = (colProportion * neElevation) + ((1 - colProportion) * nwElevation); // elevation on north edge; directly N of requested point
            var southElevation = (colProportion * seElevation) + ((1 - colProportion) * swElevation); // elevation on south edge; directly S of requested point
            var elevation = (rowProportion * southElevation) + ((1 - rowProportion) * northElevation);
            elevation += this.AddToElevationResult;
            elevation = elevation * this.ElevationResultMultiplier;
            elevation = Math.Round(elevation, 3);

            return elevation;
        }

        /// <summary>
        /// Gets the north west coordinates of the associated data tile.
        /// </summary>
        /// <param name="targetCoords">
        /// The target coordinates.
        /// </param>
        /// <returns>
        /// The <see cref="GeoCoordinate"/>.
        /// </returns>
        private static GeoCoordinate GetNorthWestTileCoordinates(GeoCoordinate targetCoords)
        {
            return new GeoCoordinate(
                Math.Ceiling(targetCoords.Latitude),
                Math.Floor(targetCoords.Longitude));
        }

        /// <summary>
        /// Gets the elevation value from a 2 byte array.
        /// </summary>
        /// <param name="array">
        /// The byte array.
        /// </param>
        /// <param name="isBigEndian">
        /// Indicates whether the data is stored in big endian format or not.
        /// </param>
        /// <returns>
        /// The <see cref="int"/> elevation value.
        /// </returns>
        private static int GetElevationValueFromByteArray(IReadOnlyList<byte> array, bool isBigEndian)
        {
            if (isBigEndian)
            {
                return array[0] << 8 | array[1];
            }

            return array[1] << 8 | array[0];
        }
    }
}
