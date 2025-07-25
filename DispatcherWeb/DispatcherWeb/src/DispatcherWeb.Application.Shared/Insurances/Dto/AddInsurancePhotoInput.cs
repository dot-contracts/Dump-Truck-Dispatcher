using Abp.Auditing;

namespace DispatcherWeb.Insurances.Dto
{
    public class AddInsurancePhotoInput
    {
        public int InsuranceId { get; set; }

        [DisableAuditing]
        public string FileBytesString { get; set; }

        public string Filename { get; set; }
    }
}
