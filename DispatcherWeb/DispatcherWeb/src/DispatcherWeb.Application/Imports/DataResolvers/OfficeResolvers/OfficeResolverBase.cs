using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Extensions;

namespace DispatcherWeb.Imports.DataResolvers.OfficeResolvers
{
    public abstract class OfficeResolverBase
    {
        protected Dictionary<string, int> _officeStringValueIdDictionary;

        public async Task<int?> GetOfficeIdAsync(string officeStringValue)
        {
            if (officeStringValue.IsNullOrEmpty())
            {
                throw new ArgumentException($"The {nameof(officeStringValue)} is null or empty!");
            }

            _officeStringValueIdDictionary ??= await GetOfficeStringValueIdDictionaryAsync();

            if (_officeStringValueIdDictionary.ContainsKey(officeStringValue.ToLowerInvariant()))
            {
                return _officeStringValueIdDictionary[officeStringValue.ToLowerInvariant()];
            }

            return null;

        }

        protected abstract Task<Dictionary<string, int>> GetOfficeStringValueIdDictionaryAsync();
    }
}
