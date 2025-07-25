using Abp.Auditing;

namespace DispatcherWeb.DriverApp.BinaryObjects.Dto
{
    public class BinaryObjectDto
    {
        [DisableAuditing]
        public string Base64String { get; set; }
    }
}
