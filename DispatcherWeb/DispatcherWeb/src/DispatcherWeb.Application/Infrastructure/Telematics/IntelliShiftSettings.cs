namespace DispatcherWeb.Infrastructure.Telematics
{
    public class IntelliShiftSettings
    {
        public string User { get; set; }
        public string Password { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Password);
        }
    }
}
