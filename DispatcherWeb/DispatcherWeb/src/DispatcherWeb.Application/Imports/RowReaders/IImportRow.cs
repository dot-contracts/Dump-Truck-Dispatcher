using System;
using System.Collections.Generic;

namespace DispatcherWeb.Imports.RowReaders
{
    public interface IImportRow
    {
        Dictionary<string, (string value, Type type)> ParseErrors { get; }
        Dictionary<string, Tuple<string, int>> StringExceedErrors { get; }
    }
}
