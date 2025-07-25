namespace DispatcherWeb.DriverMessages.Dto
{
    public class TestSmsNumberInput
    {
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }

        public string FullPhoneNumber => $"{CountryCode}{PhoneNumber}";
    }
}
