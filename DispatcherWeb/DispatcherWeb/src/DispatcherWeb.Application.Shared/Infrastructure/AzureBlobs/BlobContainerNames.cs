namespace DispatcherWeb.Infrastructure.AzureBlobs
{
    public static class BlobContainerNames
    {
        public const string Default = "attachments";
        public const string BinaryObjects = "binaryobjects"; //these were never double-encrypted and will continue to use v1 container name
        public const string ReportFiles = "reportfiles";
        public const string SecureFiles = "securefiles";
        public const string TempFiles = "tempfiles";
        public const string TruckFiles = "truckfiles";
        public const string VehicleServiceDocuments = "vehicleservicedocuments";
        public const string WorkOrderPictures = "workorderpictures";
    }
}
