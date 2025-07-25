using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DispatcherWeb.Imports.Dto
{
    public class ImportResultDto
    {
        [JsonProperty]
        public bool IsImported { get; set; }

        [JsonIgnore]
        public bool HasErrors => ParseErrors.Count > 0
                                || StringExceedErrors.Count > 0
                                || EmptyRows.Count > 0
                                || ResourceErrors.Count > 0
                                || NotFoundTrucks.Count > 0
                                || NotFoundOffices.Count > 0
                                || TruckCodeInOffices.Count > 0
            ;

        [JsonProperty]
        public int ImportedNumber { get; set; }

        [JsonProperty]
        public int SkippedNumber { get; set; }

        [JsonProperty]
        public Dictionary<int, Dictionary<string, (string value, Type type)>> ParseErrors { get; set; } = new Dictionary<int, Dictionary<string, (string value, Type type)>>();

        [JsonProperty]
        public Dictionary<int, Dictionary<string, Tuple<string, int>>> StringExceedErrors { get; } = new Dictionary<int, Dictionary<string, Tuple<string, int>>>();

        [JsonProperty]
        public List<int> EmptyRows { get; set; } = new List<int>();

        [JsonProperty]
        public List<string> ResourceErrors { get; set; } = new List<string>();


        [JsonProperty]
        public List<string> NotFoundTrucks { get; set; } = new List<string>();

        [JsonProperty]
        public List<string> NotFoundOffices { get; set; } = new List<string>();

        [JsonProperty]
        public List<(string truckCode, List<string> offices)> TruckCodeInOffices { get; set; } = new List<(string truckCode, List<string> offices)>();

    }
}
