namespace DispatcherWeb.EmployeeTime.Dto
{
    public class EmployeeTimeIndexDto
    {
        public bool LockToCurrentUser { get; set; }
        public string UserFullName { get; set; }
        public long? UserId { get; set; }
    }
}
