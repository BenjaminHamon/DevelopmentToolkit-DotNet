using BenjaminHamon.DevelopmentToolkit.Toolkit.SystemExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BenjaminHamon.DevelopmentToolkit.Toolkit.Processes
{
    /// <summary>Wrapper around <see cref="Process"/> to handle initializing, watching output and handling termination.</summary>
    public class ProcessRunnerImplementation : ProcessRunner
    {
        public event Action<ProcessRunnerImplementation, string> OutputDataReceived;
        public event Action<ProcessRunnerImplementation, string> ErrorDataReceived;

        /// <summary>Run a process for general usage, with its output accessible through events and not included in the result.</summary>
        /// <exception cref="ProcessStartException">Thrown when the executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown when checkExitCode is set and the process exit code is not zero.</exception>
        public ProcessResult Run(
            IEnumerable<string> command,
            string workingDirectory = null,
            bool checkExitCode = true,
            IDictionary<string, string> environment = null)
        {
            ProcessStartInfo processInformation = new ProcessStartInfo()
            {
                FileName = command.First(),
                Arguments = ProcessHelpers.ConvertArgumentsToString(command.Skip(1)),
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (environment != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in environment)
                {
                    processInformation.Environment[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            string executable = Path.GetFileName(processInformation.FileName);

            using (Process process = new Process())
            {
                process.StartInfo = processInformation;

                DataReceivedEventHandler outputDataHandler = (s, e) => OutputDataReceived?.Invoke(this, e.Data);
                DataReceivedEventHandler errorDataHandler = (s, e) => ErrorDataReceived?.Invoke(this, e.Data);

                process.OutputDataReceived += outputDataHandler;
                process.ErrorDataReceived += errorDataHandler;

                try
                {
                    try
                    {
                        process.Start();
                    }
                    catch (Win32Exception exception) when (exception.NativeErrorCode == SystemErrorCodes.ERROR_FILE_NOT_FOUND)
                    {
                        string exceptionMessage = String.Format("Executable not found: '{0}'.", process.StartInfo.FileName);
                        throw new ProcessStartException(exceptionMessage, exception.ErrorCode);
                    }

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();
                }
                finally
                {
                    process.OutputDataReceived -= outputDataHandler;
                    process.ErrorDataReceived -= errorDataHandler;
                }

                if (checkExitCode && (process.ExitCode != 0))
                {
                    string exceptionMessage = String.Format("Subprocess failed (Executable: '{0}', ExitCode: {1}", executable, process.ExitCode);
                    throw new ProcessFailureException(exceptionMessage, process.ExitCode);
                }

                return new ProcessResult(process.ExitCode, outputData: null, errorData: null);
            }
        }

        /// <summary>Run a simple process and return its result with the output included.</summary>
        /// <exception cref="ProcessStartException">Thrown when the executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown when checkExitCode is set and the process exit code is not zero.</exception>
        public ProcessResult RunSimple(
            IEnumerable<string> command,
            string workingDirectory = null,
            bool checkExitCode = true,
            IDictionary<string, string> environment = null)
        {
            string outputData = "";
            string errorData = "";

            Action<ProcessRunnerImplementation, string> outputDataHandler = (s, e) => outputData += e + Environment.NewLine;
            Action<ProcessRunnerImplementation, string> errorDataHandler = (s, e) => errorData += e + Environment.NewLine;

            OutputDataReceived += outputDataHandler;
            ErrorDataReceived += errorDataHandler;

            ProcessResult runResult;

            try
            {
                runResult = Run(command, workingDirectory, checkExitCode, environment);
            }
            finally
            {
                OutputDataReceived -= outputDataHandler;
                ErrorDataReceived -= errorDataHandler;
            }

            return new ProcessResult(runResult.ExitCode, outputData.Trim(), errorData.Trim());
        }
    }
}
