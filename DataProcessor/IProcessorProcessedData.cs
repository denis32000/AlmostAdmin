namespace DataProcessor
{
    public interface IProcessorProcessedData
    {
        bool Success { get; set; }
        string JsonResult { get; set; }
    }
}
