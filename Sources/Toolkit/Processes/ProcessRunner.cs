using System;
using System.Collections.Generic;

namespace BenjaminHamon.DevelopmentToolkit.Toolkit.Processes
{
    /// <summary>Interface for running processes.</summary>
    public interface ProcessRunner
    {
        event Action<ProcessRunnerImplementation, string> ErrorDataReceived;
        event Action<ProcessRunnerImplementation, string> OutputDataReceived;

        /// <summary>Run a process for general usage, with its output accessible through events and not included in the result.</summary>
        /// <exception cref="ProcessStartException">Thrown when the executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown when checkExitCode is set and the process exit code is not zero.</exception>
        ProcessResult Run(
            IEnumerable<string> command,
            string workingDirectory = null,
            bool checkExitCode = true,
            IDictionary<string, string> environment = null);

        /// <summary>Run a simple process and return its result with the output included.</summary>
        /// <exception cref="ProcessStartException">Thrown when the executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown when checkExitCode is set and the process exit code is not zero.</exception>
        ProcessResult RunSimple(
            IEnumerable<string> command,
            string workingDirectory = null,
            bool checkExitCode = true,
            IDictionary<string, string> environment = null);
    }
}
