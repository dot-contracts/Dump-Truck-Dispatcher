using System;

namespace DispatcherWeb.Offices
{
    public interface ILogoStorageObject
    {
        Guid? LogoId { get; set; }
        string LogoFileType { get; set; }
        Guid? ReportsLogoId { get; set; }
        string ReportsLogoFileType { get; set; }
    }
}
