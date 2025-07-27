--this script should be suitable for cleaning up sensitive or unimportant data so that developers could run qa4 backup locally

--these settings contain private keys
delete from AbpSettings where Name like 'App.GpsIntegration%'
delete from AbpSettings where Name like 'App.Heartland%'
delete from AbpSettings where Name like 'App.Invoice.Quickbooks%'
delete from AbpSettings where Name like 'Abp.Net.Mail.Smtp.Password'
delete from AbpSettings where Name like 'App.Sms%'
Update Office set HeartlandSecretKey = null, HeartlandPublicKey = null where HeartlandPublicKey is not null or HeartlandSecretKey is not null

--these tables might contain private information
delete from AbpPersistedGrants
delete from AbpUserLoginAttempts
delete from [OneTimeLogin]

--these tables are overly big and not very useful for local debug
delete from AbpTenantNotifications where CreationTime < '2024'
delete from AbpUserNotifications where CreationTime < '2024'
delete from AbpAuditLogs
delete from DriverApplicationLog
delete from [TenantDailyHistory] where Date < '2023'
delete from [TransactionDailyHistory] where Date < '2024'
delete from UserDailyHistory where Date < '2023'
delete from [VehicleUsage] where CreationTime < '2024'

--these records are dependent on Azure Blob service which will be empty locally
delete from AppBinaryObjects
delete from [DeferredBinaryObject]
delete from [SecureFileDefinitions]
delete from TruckFile
delete from [VehicleServiceDocument]
delete from [WorkOrderPicture]
update Ticket set TicketPhotoId = null, TicketPhotoFilename = null, DeferredTicketPhotoId = null
update AbpUsers set ProfilePictureId = null
update AbpTenants set LogoId = null, ReportsLogoId = null

--we want to avoid sending push messages from locally running instances to existing QA driver apps
delete from FcmPushMessage
delete from [FcmRegistrationToken]
delete from DriverPushSubscription
delete from PushSubscriptions
delete from DriverApplicationDevice

--they won't be able to decrypt qa notes with a local key
update [Order]
  set HasInternalNotes = 0, EncryptedInternalNotes = null
  where HasInternalNotes = 1

--these might not contain private information, but we don't use payments right now so these are not useful anyway
delete from [OrderPayment]
delete from [Payment]
delete from [PaymentHeartlandKey]

--existing jobs will fail because local version is older than QA version
delete from [HangFire].[Job]
delete from [HangFire].[Set]

--this will let them log in as any existing user
update AbpUsers set Password = 'AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==' --123qwe
