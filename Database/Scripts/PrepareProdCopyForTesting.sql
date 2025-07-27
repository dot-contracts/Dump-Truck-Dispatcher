--this script should be suitable for temporary prod2 copies that are not supposed to contact real customers during our own testing

--disallow the prod2copy app service to communicate with the outside world
delete from AbpSettings where Name like 'App.GpsIntegration%'
delete from AbpSettings where Name like 'App.Heartland%'
delete from AbpSettings where Name like 'App.Invoice.Quickbooks%'
delete from AbpSettings where Name like 'Abp.Net.Mail.Smtp.Password'
delete from AbpSettings where Name like 'App.Sms%'
Update Office set HeartlandSecretKey = null, HeartlandPublicKey = null where HeartlandPublicKey is not null or HeartlandSecretKey is not null

--we want to avoid sending push messages from prod2copy to existing prod2 driver apps
delete from FcmPushMessage
delete from [FcmRegistrationToken]
delete from DriverPushSubscription
delete from PushSubscriptions
--delete from DriverApplicationDevice

--existing jobs might fail
delete from [HangFire].[Job]
delete from [HangFire].[Set]
