namespace DispatcherWeb.Dto
{
    public class OperationResultDto
    {
        public bool IsFailed { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class OperationResultDto<T> : OperationResultDto
    {
        public T Item { get; set; }
    }
}
