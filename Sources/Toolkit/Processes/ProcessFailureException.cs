namespace BenjaminHamon.DevelopmentToolkit.Toolkit.Processes
{
    /// <summary>Error for subprocess completing unsuccessfully.</summary>
    public class ProcessFailureException : ProcessException
    {
        public ProcessFailureException(string message, int exitCode)
            : base(message, exitCode)
        { }
    }
}
