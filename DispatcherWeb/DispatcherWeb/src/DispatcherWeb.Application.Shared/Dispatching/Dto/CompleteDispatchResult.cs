namespace DispatcherWeb.Dispatching.Dto
{
    public class CompleteDispatchResult
    {
        public bool NextDispatch { get; set; }
        public int NextDispatchId { get; set; }
        public bool IsCanceled { get; set; }
        public bool NotFound { get; set; }
        public bool IsCompleted { get; set; }
    }
}
