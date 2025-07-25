namespace DispatcherWeb.Offices.Dto
{
    public interface IOfficeIdNameDto
    {
        int OfficeId { get; set; }
        string OfficeName { get; set; }
        bool IsSingleOffice { get; set; }
    }
}
