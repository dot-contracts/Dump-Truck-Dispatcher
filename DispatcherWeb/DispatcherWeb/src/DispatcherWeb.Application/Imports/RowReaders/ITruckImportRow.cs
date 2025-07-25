namespace DispatcherWeb.Imports.RowReaders
{
    public interface ITruckImportRow : IImportRow
    {
        string Office { get; }
        string TruckNumber { get; }
    }
}
