namespace DispatcherWeb.Authorization.Users.Dto
{
    public interface IGetUserListFilter
    {
        string Filter { get; set; }
        string Permission { get; set; }
        int? Role { get; set; }
        bool OnlyLockedUsers { get; set; }
        int? OfficeId { get; set; }
    }
}
