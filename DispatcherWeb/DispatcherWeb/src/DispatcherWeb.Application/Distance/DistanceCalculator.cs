using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.Dependency;
using Castle.Core.Logging;
using DispatcherWeb.Configuration;
using DispatcherWeb.Distance.Dto;

namespace DispatcherWeb.Distance
{
    public class DistanceCalculator : IDistanceCalculator, ISingletonDependency
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _googleMapsApiKey;

        public DistanceCalculator(
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _googleMapsApiKey = configurationAccessor.Configuration["GoogleMaps:ApiKeyPrivate"];
        }

        public ILogger Logger { protected get; set; } = NullLogger.Instance;

        public async Task PopulateDistancesAsync(PopulateDistancesInput input)
        {
            switch (input.UomBaseId)
            {
                case UnitOfMeasureBaseEnum.AirKMs:
                case UnitOfMeasureBaseEnum.AirMiles:
                    PopulateAirDistances(input);
                    break;

                case UnitOfMeasureBaseEnum.DriveKMs:
                case UnitOfMeasureBaseEnum.DriveMiles:
                    await PopulateDriveDistances(input);
                    break;

                default:
                    throw new ArgumentException($"UomBaseId value of {(int)input.UomBaseId} is not supported", nameof(input.UomBaseId));
            }
        }

        private void PopulateAirDistances(PopulateDistancesInput input)
        {
            if (input.Destination.Latitude == null || input.Destination.Longitude == null)
            {
                return;
            }

            var destinationLatitude = (double)input.Destination.Latitude.Value;
            var destinationLongitude = (double)input.Destination.Longitude.Value;

            var r = GetEarthRadius(input.UomBaseId);
            const double toRadians = Math.PI / 180;

            foreach (var source in input.Sources)
            {
                if (source.Latitude == null || source.Longitude == null)
                {
                    continue;
                }

                var sourceLatitude = (double)source.Latitude.Value;
                var sourceLongitude = (double)source.Longitude.Value;

                var lat1 = sourceLatitude * toRadians;
                var lat2 = destinationLatitude * toRadians;
                var diffLat = lat2 - lat1;
                var diffLon = (destinationLongitude - sourceLongitude) * toRadians;
                source.Distance = (decimal)(2 * r * Math.Asin(
                    Math.Sqrt(
                        Math.Pow(Math.Sin(diffLat / 2), 2)
                        + Math.Cos(lat1) * Math.Cos(lat2)
                        * Math.Pow(Math.Sin(diffLon / 2), 2)
                    )
                ));
            }
        }

        private static double GetEarthRadius(UnitOfMeasureBaseEnum uomBaseId)
        {
            return uomBaseId switch
            {
                UnitOfMeasureBaseEnum.AirKMs => 6371.07,
                UnitOfMeasureBaseEnum.AirMiles => 3958.8,
                _ => throw new ArgumentException($"UomBaseId value of {(int)uomBaseId} is not supported", nameof(uomBaseId)),
            };
        }

        private async Task PopulateDriveDistances(PopulateDistancesInput input)
        {
            if (string.IsNullOrEmpty(_googleMapsApiKey))
            {
                return;
            }

            var googleDistanceMatrixApi = new GoogleDistanceMatrixApi();

            if (!GoogleDistanceMatrixApi.IsLocationValid(input.Destination))
            {
                return;
            }
            googleDistanceMatrixApi.DestinationLocations.Add(input.Destination);

            var validSources = input.Sources.Where(GoogleDistanceMatrixApi.IsLocationValid).ToList();
            if (!validSources.Any())
            {
                return;
            }
            foreach (var source in validSources)
            {
                googleDistanceMatrixApi.SourceLocations.Add(source);
            }

            googleDistanceMatrixApi.Mode = "driving";

            GoogleDistanceMatrixApi.Response response;
            try
            {
                response = await googleDistanceMatrixApi.GetResponse(_httpClient, _googleMapsApiKey);
            }
            catch (Exception e)
            {
                Logger.Error("Error during GoogleDistanceMatrixApi call, " + e, e);
                return;
            }

            if (response.Status != "OK")
            {
                return;
            }

            for (var i = 0; i < response.Rows.Length; i++)
            {
                var source = validSources[i];
                var row = response.Rows[i];

                if (row.Elements.Length == 0)
                {
                    continue;
                }

                var element = row.Elements[0];

                if (element.Status != "OK")
                {
                    continue;
                }

                source.Distance = element.Distance.Value / 1000m;
                if (input.UomBaseId == UnitOfMeasureBaseEnum.DriveMiles)
                {
                    source.Distance *= 0.621371m;
                }
            }
        }

        public static string FormatDistanceWithUnits(decimal distance, UnitOfMeasureBaseEnum uomBaseId)
        {
            var truncatedDistance = Math.Truncate(distance * 10) / 10; //2.99 -> 2.9
            var uom = FormatDistanceUnits(uomBaseId);
            return $"{truncatedDistance:0.#}{uom}";
        }

        private static string FormatDistanceUnits(UnitOfMeasureBaseEnum uomBaseId)
        {
            switch (uomBaseId)
            {
                case UnitOfMeasureBaseEnum.AirKMs:
                case UnitOfMeasureBaseEnum.DriveKMs:
                    return "km";
                case UnitOfMeasureBaseEnum.AirMiles:
                case UnitOfMeasureBaseEnum.DriveMiles:
                    return "mi";
                default:
                    throw new ArgumentException($"UomBaseId value of {(int)uomBaseId} is not supported", nameof(uomBaseId));
            }
        }
    }
}
