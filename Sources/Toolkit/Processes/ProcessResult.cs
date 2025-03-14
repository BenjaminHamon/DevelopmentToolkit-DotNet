namespace BenjaminHamon.DevelopmentToolkit.Toolkit.Processes
{
    /// <summary>Data class with information about the execution result of a subprocess.</summary>
    public class ProcessResult
    {
        public ProcessResult(int exitCode, string outputData, string errorData)
        {
            ExitCode = exitCode;
            OutputData = outputData;
            ErrorData = errorData;
        }

        public int ExitCode { get; }
        public string OutputData { get; }
        public string ErrorData { get; }
    }
}
