namespace BenjaminHamon.DevelopmentToolkit.Toolkit.Processes
{
    /// <summary>Error for failing to start a subprocess.</summary>
    public class ProcessStartException : ProcessException
    {
        public ProcessStartException(string message, int exitCode)
            : base(message, exitCode)
        { }
    }
}
