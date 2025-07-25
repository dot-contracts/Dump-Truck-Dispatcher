using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Abp.UI;
using CsvHelper;
using CsvHelper.Configuration;
using DispatcherWeb.Imports.RowReaders;

namespace DispatcherWeb.Imports
{
    public class ImportReader : IImportReader, IDisposable
    {
        private readonly CsvReader _csv;
        private readonly ILookup<string, string> _fieldMap;

        public ImportReader(TextReader textReader) : this(textReader, null) { }
        public ImportReader(TextReader textReader, FieldMapItem[] fieldMap)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                TrimOptions = TrimOptions.Trim,
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToLower().Trim(),
                MissingFieldFound = null,
            };

            _csv = new CsvReader(textReader, config);

            if (fieldMap != null)
            {
                _fieldMap = fieldMap.ToLookup(f => f.StandardField, f => f.UserField, StringComparer.OrdinalIgnoreCase);
            }
        }

        public string[] GetCsvHeaders()
        {
            _csv.Read();
            try
            {
                _csv.ReadHeader();
            }
            catch
            {
                throw new UserFriendlyException("The header row cannot be read");
            }

            return _csv.HeaderRecord.Select(h => h.Trim()).ToArray();
        }

        public IEnumerable<T> AsEnumerable<T>() where T : IImportRow
        {
            _csv.Read();
            _csv.ReadHeader();
            while (_csv.Read())
            {
                yield return (T)Activator.CreateInstance(typeof(T), _csv, _fieldMap);
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _csv.Dispose();
            }
        }
    }
}
