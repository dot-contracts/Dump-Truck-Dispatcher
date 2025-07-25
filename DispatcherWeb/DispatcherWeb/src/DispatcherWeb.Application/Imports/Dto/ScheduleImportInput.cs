namespace DispatcherWeb.Imports.Dto
{
    public class ScheduleImportInput
    {
        public string BlobName { get; set; }
        public FieldMapItem[] FieldMap { get; set; }
        public ImportType ImportType { get; set; }
        public bool JacobusEnergy { get; set; }
    }
}
