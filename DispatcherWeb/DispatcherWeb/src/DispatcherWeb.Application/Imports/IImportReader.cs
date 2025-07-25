using System.Collections.Generic;
using DispatcherWeb.Imports.RowReaders;

namespace DispatcherWeb.Imports
{
    public interface IImportReader
    {
        string[] GetCsvHeaders();
        IEnumerable<T> AsEnumerable<T>() where T : IImportRow;
    }
}
