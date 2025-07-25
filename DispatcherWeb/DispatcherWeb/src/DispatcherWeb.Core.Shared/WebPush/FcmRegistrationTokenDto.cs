namespace DispatcherWeb.WebPush
{
    public class FcmRegistrationTokenDto
    {
        public long UserId { get; set; }

        public string Token { get; set; }

        public MobilePlatform MobilePlatform { get; set; }

        public int? Version { get; set; }

        public int Id { get; set; }
    }
}
