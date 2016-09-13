using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Xml.Serialization;

using FinalSurge.GeoElevation;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var ned1DataFileDirectory = @"C:\DEV\TempData\Elevation\NED1\";
            var ned2DataFileDirectory = @"C:\DEV\TempData\Elevation\NED2\";
            var srtm1DataFileDirectory = @"C:\DEV\TempData\Elevation\SRTM1\";
            var srtm3DataFileDirectory = @"C:\DEV\TempData\Elevation\SRTM3\";

            var coords = new GeoCoordinate(35.9297645, -78.5790372); // SRTM1 File = N35W079.hgt; Altitude = 79.4 m
            //var coords = new GeoCoordinate(61.218056, -149.900278); // SRTM1 File = N61W150.hgt; Altitude = 35.9664 m

            var ned1Elevation = new Ned1DataStore(ned1DataFileDirectory).GetElevation(coords);
            var ned2Elevation = new Ned2DataStore(ned2DataFileDirectory).GetElevation(coords);
            var srtm1Elevation = new Srtm1DataStore(srtm1DataFileDirectory).GetElevation(coords);
            var srtm3Elevation = new Srtm3DataStore(srtm3DataFileDirectory).GetElevation(coords);

            Console.WriteLine($"{coords.Latitude},{coords.Longitude}");
            Console.WriteLine($"NED1 = {ned1Elevation} m");
            Console.WriteLine($"NED2 = {ned2Elevation} m");
            Console.WriteLine($"SRTM1 = {srtm1Elevation} m");
            Console.WriteLine($"SRTM3 = {srtm3Elevation} m");

            /*
            var service = new GeoElevationService(
                              ned1DataFileDirectory,
                              ned2DataFileDirectory,
                              srtm1DataFileDirectory,
                              srtm3DataFileDirectory);

            var dataFile = @"C:\DEV\TempData\Elevation\RawGarminDevicePoints.xml";
            var resultsFile = @"C:\DEV\TempData\Elevation\UpdatedGarminDevicePoints.xml";
            List<DevicePoint> devicePoints;
            var serializer = new XmlSerializer(typeof(List<DevicePoint>));
            using (var stream = File.OpenRead(dataFile))
            {
                devicePoints = (List<DevicePoint>)serializer.Deserialize(stream);
            }

            var results = service.UpdateElevations(devicePoints);
            using (var writer = File.CreateText(resultsFile))
            {
                serializer.Serialize(writer, results);
            }
            */

            Console.WriteLine("\n\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
