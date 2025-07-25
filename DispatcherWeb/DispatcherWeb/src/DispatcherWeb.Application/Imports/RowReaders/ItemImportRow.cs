using System.Linq;
using CsvHelper;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Imports.RowReaders
{
    public class ItemImportRow : ImportRow
    {
        public ItemImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public bool IsActive => GetString(ServiceColumn.IsActive, 30) == "Active" || !HasField(ServiceColumn.IsActive);
        public string Type => GetString(ServiceColumn.Type, 100);
        public string Name => GetString(ServiceColumn.Name, EntityStringFieldLengths.Item.Name);
        public string Description => GetString(ServiceColumn.Description, EntityStringFieldLengths.Item.Description);
        public string Uom => GetString(ServiceColumn.Uom, 100);
        public decimal? Price => GetDecimal(ServiceColumn.Price);

        public bool IsTaxable => GetBoolean(ServiceColumn.IsTaxable, "tax");

        public string IncomeAccount => GetString(ServiceColumn.IncomeAccount, EntityStringFieldLengths.Item.IncomeAccount);
    }
}
