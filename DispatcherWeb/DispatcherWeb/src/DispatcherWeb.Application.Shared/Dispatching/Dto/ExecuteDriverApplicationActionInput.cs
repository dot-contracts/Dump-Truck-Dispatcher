using System;
using System.Collections.Generic;
using DispatcherWeb.DriverApplication.Dto;

namespace DispatcherWeb.Dispatching.Dto
{
    public class ExecuteDriverApplicationActionInput
    {
        public DateTime ActionTimeInUtc { get; set; }
        public Guid DriverGuid { get; set; }
        public int? DeviceId { get; set; }
        public Guid? DeviceGuid { get; set; }
        public string AppVersion { get; set; }
        public DriverApplicationAction Action { get; set; }
        public DriverClockInInput ClockInData { get; set; }
        public AcknowledgeDispatchInput AcknowledgeDispatchData { get; set; }
        public LoadDispatchInput LoadDispatchData { get; set; }
        public CompleteDispatchDto CompleteDispatchData { get; set; }
        public CancelDispatchForDriverInput CancelDispatchData { get; set; }
        public MarkDispatchCompleteInput MarkDispatchCompleteData { get; set; }
        public AddSignatureInput AddSignatureData { get; set; }
        public ModifyDriverPushSubscriptionInput PushSubscriptionData { get; set; }
        public UploadDeferredBinaryObjectInput UploadDeferredData { get; set; }
        public List<UploadLogsInput> UploadLogsData { get; set; }
        public EmployeeTimeSlimEditDto ModifyEmployeeTimeData { get; set; }
        public RemoveEmployeeTimeInput RemoveEmployeeTimeData { get; set; }
        public AddDriverNoteInput AddDriverNoteData { get; set; }

    }
}
