using DispatcherWeb.DriverApp.Messages.Dto;
using DispatcherWeb.DriverApp.Users.Dto;

namespace DispatcherWeb.DriverApp.Friends.Dto
{
    public class FriendDto
    {
        public MessageDto LastMessage { get; set; }
        public UserDto User { get; set; }
        public int UnreadMessageCount { get; set; }
        public bool IsOnline { get; set; }
    }
}
