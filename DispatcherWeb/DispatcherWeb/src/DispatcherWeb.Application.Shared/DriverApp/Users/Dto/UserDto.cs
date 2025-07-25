using System;

namespace DispatcherWeb.DriverApp.Users.Dto
{
    public class UserDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? ProfilePictureId { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
    }
}
