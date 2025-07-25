namespace DispatcherWeb.Infrastructure
{
    public static class EntityStringFieldLengths
    {
        public static class GeneralAddress
        {
            public const int MaxStreetAddressLength = 200;
            public const int MaxCityLength = 100;
            public const int MaxStateLength = 100;
            public const int MaxZipCodeLength = 15;
            public const int MaxCountryCodeLength = 50;
            public const int FullAddress = 400;
        }


        public static class General
        {
            public const int PhoneNumber = 15;
            public const int Email = 200;

            public const int ExtraShort = 10;
            public const int Short = 20;
            public const int Medium = 50;
            public const int Long = 100;
            public const int ExtraLong = 200;
            public const int Length2000 = 2000;
        }

        public static class Truck
        {
            public const int SteerTires = General.Medium;
            public const int DriveAxleTires = General.Medium;
            public const int DropAxleTires = General.Medium;
            public const int TrailerTires = General.Medium;
            public const int Transmission = General.Medium;
            public const int Engine = General.Medium;
            public const int RearEnd = General.Medium;
            public const int Make = General.Medium;
            public const int Model = General.Medium;
            public const int Vin = General.Medium;
            public const int Plate = General.Short;
            public const int InsurancePolicyNumber = General.Medium;
            public const int TruxTruckId = General.Short;
            public const int DtdTrackerUniqueId = 100; //Wialon limit
            public const int DtdTrackerDeviceTypeName = 50; //43 is the longest so far in their data
            public const int DtdTrackerServerAddress = 50;
            public const int DtdTrackerPassword = 100; //the password field doesn't seem to have a limit in Wialon
            public const int Notes = 1000;
            public const int Imei = General.Medium;
            public const int SimId = General.Medium;
        }

        public static class TruckPosition
        {
            public const int GeofenceIdentifier = General.Medium;
            public const int GeofenceActionRaw = General.Medium;
            public const int ActivityTypeRaw = General.Medium;
        }

        public static class WialonDeviceType
        {
            public const int DeviceCategory = 50;
            public const int ServerAddress = 259; //domain:port
            public const int Name = 127;
        }

        public static class ScheduledReport
        {
            public const int SendTo = General.Length2000;
        }
        public static class Item
        {
            public const int Name = 50;
            public const int ServiceInQuickBooks = 31;
            public const int Description = 150;
            public const int IncomeAccount = General.Long;
            public const int ExpenseAccount = General.Long;
        }



        public static class DriverMessage
        {
            public const int Subject = General.Long;
            public const int Body = General.ExtraLong;
        }

        public static class HostEmail
        {
            public const int Subject = General.Long;
            public const int Body = 4000;
        }

        public static class CustomerNotification
        {
            public const int Title = General.Long;
            public const int Body = 4000;
        }

        public static class TrackableEmail
        {
            public const int Subject = 200;
        }

        public static class ChatMessage
        {
            public const int Message = General.Length2000;
        }

        public static class TrackableSms
        {
            public const int Sid = General.Long;
        }

        public static class Customer
        {
            public const int Name = 100;
            public const int AccountNumber = 30;
            public const int CreditCardToken = 500;
            public const int CreditCardStreetAddress = 500;
            public const int CreditCardZipCode = 50;
            public const int TaxIdNumber = 20;
            public const int Notes = 1000;
        }

        public static class CustomerContact
        {
            public const int Title = 40;
            public const int Name = 129;
        }

        public static class Order
        {
            public const int PoNumber = 20;
            public const int Directions = 1000;
            public const int SpectrumNumber = 20;
            public const int ChargeTo = 500;
            public const int InternalNotes = 700;
            public const int EncryptedInternalNotes = 1000;
        }

        public static class OrderLine
        {
            public const int JobNumber = 20;
            public const int Note = 1000;
            public const int DriverNote = 500;
            public const int CustomerNotificationContactName = 100;
            public const int CustomerNotificationPhoneNumber = General.PhoneNumber;
        }

        public static class OrderLineTruck
        {
            public const int DriverNote = 255;
        }

        public static class Invoice
        {
            public const int JobNumber = 100; //different from OrderLine.JobNumber limit to support multiple job numbers displayed
            public const int PoNumber = 50;
            public const int Message = 400;
            public const int Description = 250;
            public const int QuickbooksInvoiceId = 30;
        }

        public static class Dispatch
        {
            public const int PhoneNumber = General.PhoneNumber;
            public const int Email = General.Email;
            public const int Message = 4000;
            public const int SmsMessageLimit = 550;
            public const int Note = 1000;
        }

        public static class Location
        {
            public const int Name = General.Long;
            public const int Abbreviation = General.ExtraShort;
            public const int Notes = 1000;
            public const int PlaceId = 255;
            public const int DisplayName = Name
               + GeneralAddress.MaxStreetAddressLength
               + GeneralAddress.MaxCityLength
               + GeneralAddress.MaxStateLength
               + 2 * 3; //The separator is ", " in all cases
        }

        public static class WorkOrder
        {
            public const int Note = 500;
        }

        public static class TimeOff
        {
            public const int Reason = General.Long;
        }

        public static class EmployeeTime
        {
            public const int Description = General.ExtraLong;
        }

        public static class TruxEarnings
        {
            public const int JobName = General.ExtraLong;
            public const int TruckType = General.Long;
            public const int Status = General.Medium;
            public const int TruxTruckId = Truck.TruxTruckId;
            public const int DriverName = General.ExtraLong;
            public const int HaulerName = General.ExtraLong;
            public const int Unit = General.Medium;
        }

        public static class ImportedEarnings
        {
            public const int TicketNumber = Ticket.TicketNumber;
            public const int Site = General.Long;
            public const int CustomerName = Location.Name;
            public const int LicensePlate = Truck.Plate;
            public const int ProductDescription = Item.Name;
            public const int Uom = General.Medium;
            public const int HaulerRef = General.Short;
        }

        public static class FuelSurchargeCalculation
        {
            public const int Name = General.Medium;
        }

        public static class FcmRegistrationToken
        {
            public const int Token = 512;
        }

        public static class FcmPushMessage
        {
            public const int JsonPayload = 4000;
            public const int MaxAllowedJsonPayloadLength = JsonPayload - 1000; //the whole FCM message will have additional required fields around our specific payload, so we need to account for that extra length.
            public const int Error = 500;
        }

        public static class Ticket
        {
            public const int TicketNumber = 20;
            public const int TicketPhotoFilename = 255;
        }

        public static class VehicleCategory
        {
            public const int Name = 50;
        }

        public static class Driver
        {
            public const int FirstName = 50;
            public const int LastName = 50;
            public const int EmailAddress = 256;
            public const int LicenseNumber = 20;
            public const int TypeOfLicense = 50;
            public const int EmployeeId = 10;
        }

        public static class Tenant
        {
            public const int MaxLogoMimeTypeLength = 64;
        }

        public static class TaxRate
        {
            public const int Name = 20;
        }

        public static class LeaseHaulerRequest
        {
            public const int Comments = General.Length2000;
            public const int Message = 550;
        }
        public static class User
        {
            public const int SignInToken = 255;
            public const int GoogleAuthenticatorKey = 255;
        }

        public static class BackgroundJobHistory
        {
            public const int Details = 500;
        }

        public static class CannedText
        {
            public const int Text = 1000;
        }

        public static class CustomerNotificationRole
        {
            public const int RoleName = 500;
        }

        public static class Load
        {
            public const int SignatureName = 100;
        }

        public static class DriverApplicationDevice
        {
            public const int Useragent = 500;
            public const int AppVersion = 50;
        }

        public static class Friendship
        {
            public const int FriendTenancyName = 255;
        }

        public static class HostEmailRole
        {
            public const int RoleName = 255;
        }

        public static class LuckStoneLocation
        {
            public const int Site = 100;
        }

        public static class AppInvoices
        {
            public const int InvoiceNo = 30;
            public const int TenantLegalName = 255;
            public const int TenantAddress = 500;
            public const int TenantTaxNo = 50;
        }

        public static class SubscriptionPayment
        {
            public const int Description = 2000;
            public const int ExternalPaymentId = 450;
            public const int InvoiceNo = 30;
            public const int SuccessUrl = 500;
            public const int ErrorUrl = 500;
        }

        public static class SubscriptionPaymentExtensionData
        {
            public const int Key = 450;
            public const int Value = 500;
        }

        public static class Payment
        {
            public const int TransactionType = 255;
            public const int BatchSummaryId = 1000;
        }

        public static class Quote
        {
            public const int Directions = 1000;
            public const int Notes = 4000;
        }

        public static class BinaryObject
        {
            public const int Description = 255;
        }

        public static class WorkOrderPicture
        {
            public const int FileName = 100;
        }

        public static class PricingTier
        {
            public const int Name = 50;
        }

        public static class Insurance
        {
            public const int IssuedBy = 63;
            public const int BrokerName = 63;
            public const int Comments = 511;
            public const int FileName = 255;
            public const int InsuranceTypeName = 50;
        }

        public static class LeaseHauler
        {
            public const int Name = 100;
            public const int AccountNumber = 30;
            public const int PhoneNumber = 20;
            public const int MotorCarrierNumber = 15;
            public const int DeptOfTransportationNumber = 15;
            public const int EinOrTin = 15;
        }

        public static class UnitOfMeasure
        {
            public const int Name = 20;
        }

        public static class HaulZone
        {
            public const int Name = 50;
        }

        public static class Charge
        {
            public const int Description = 100;
        }

        public static class TempFile
        {
            public const int FileName = 255;
            public const int MimeType = 64;
        }
    }
}
