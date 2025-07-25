using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace DispatcherWeb.Distance.Dto
{
    public class GoogleDistanceMatrixApi
    {
        public List<ILocation> SourceLocations { get; set; } = new List<ILocation>();

        public List<ILocation> DestinationLocations { get; set; } = new List<ILocation>();

        public string Mode = "driving";

        public class Response
        {
            public string Status { get; set; }

            [JsonProperty(PropertyName = "origin_addresses")]
            public string[] OriginAddresses { get; set; }

            [JsonProperty(PropertyName = "destination_addresses")]
            public string[] DestinationAddresses { get; set; }

            public Row[] Rows { get; set; }

            public class Data
            {
                public int Value { get; set; }
                public string Text { get; set; }
            }

            public class Element
            {
                public string Status { get; set; }
                public Data Duration { get; set; }
                public Data Distance { get; set; }
            }

            public class Row
            {
                public Element[] Elements { get; set; }
            }
        }

        public async Task<Response> GetResponse(HttpClient httpClient, string key)
        {
            var uri = new Uri(GetRequestUrl(key));

            var response = await httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("GoogleDistanceMatrixApi failed with status code: " + response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response>(content);
        }

        private string GetRequestUrl(string key)
        {
            if (SourceLocations.Count == 0 || DestinationLocations.Count == 0)
            {
                throw new Exception("Source and Destination locations must be set.");
            }
            if (SourceLocations.Any(x => !IsLocationValid(x)))
            {
                throw new Exception("Source locations are invalid.");
            }
            if (DestinationLocations.Any(x => !IsLocationValid(x)))
            {
                throw new Exception("Destination locations are invalid.");
            }

            var origins = GetLocationArrayString(SourceLocations);
            var destinations = GetLocationArrayString(DestinationLocations);
            var mode = HttpUtility.UrlEncode(Mode);
            const string baseUrl = "https://maps.googleapis.com/maps/api/distancematrix/json";
            return $"{baseUrl}?origins={origins}&destinations={destinations}&mode={mode}&key={key}";
        }

        private static string GetLocationArrayString(List<ILocation> locations)
        {
            return string.Join("|", locations.Select(GetLocationString).Select(HttpUtility.UrlEncode));
        }

        private static string GetLocationString(ILocation location)
        {
            if (!string.IsNullOrEmpty(location.PlaceId))
            {
                return $"place_id:{location.PlaceId}";
            }

            if (location.Latitude != null && location.Longitude != null)
            {
                return $"{location.Latitude},{location.Longitude}";
            }

            return $"{location.StreetAddress}, {location.City}, {location.State} {location.ZipCode}, {location.CountryCode}";
        }

        public static bool IsLocationValid(ILocation location)
        {
            if (!string.IsNullOrEmpty(location.PlaceId))
            {
                return true;
            }

            if (location.Latitude != null && location.Longitude != null)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(location.StreetAddress)
                || !string.IsNullOrEmpty(location.City)
                || !string.IsNullOrEmpty(location.State)
                || !string.IsNullOrEmpty(location.ZipCode)
                || !string.IsNullOrEmpty(location.CountryCode))
            {
                return true;
            }

            return false;
        }
    }
}
