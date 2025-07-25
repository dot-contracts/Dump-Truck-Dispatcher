namespace DispatcherWeb.Fulcrum.Dto
{
    public class FulcrumSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CustomerNumber { get; set; }
        public bool IsEmpty() => string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(CustomerNumber);
    }
}
