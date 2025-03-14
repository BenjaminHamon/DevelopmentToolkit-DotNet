using System;

namespace BenjaminHamon.DevelopmentToolkit.Toolkit.Processes
{
    /// <summary>Error related to a subprocess.</summary>
    public class ProcessException : Exception
    {
        public ProcessException(string message, int exitCode)
            : base(message)
        {
            ExitCode = exitCode;
        }

        public ProcessException(string message, int exitCode, Exception innerException)
            : base(message, innerException)
        {
            ExitCode = exitCode;
        }

        public int ExitCode { get; }
    }
}
