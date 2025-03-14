using BenjaminHamon.DevelopmentToolkit.Toolkit.Processes;
using System;
using System.Collections.Generic;
using System.IO;

namespace BenjaminHamon.DevelopmentToolkit.Toolkit.RevisionControl
{
    /// <summary>Helpers for operations related to git and which are outside the scope of <see cref="GitClient"/></summary>
    public static class GitHelpers
    {
        /// <summary>Check if Git is installed and available for use by GitClient.</summary>
        /// <exception cref="ProcessException">Thrown if the Git executable was not found.</exception>
        public static void CheckGitIsAvailable(string gitExecutablePath = null)
        {
            CheckGitIsAvailable(new ProcessRunnerImplementation(), gitExecutablePath);
        }

        /// <summary>Check if Git is installed and available for use by GitClient.</summary>
        /// <exception cref="ProcessException">Thrown if the git executable was not found.</exception>
        public static void CheckGitIsAvailable(ProcessRunner processRunner, string gitExecutablePath = null)
        {
            List<string> command = new List<string>() { gitExecutablePath ?? "git", "--version" };

            try
            {
                processRunner.Run(command);
            }
            catch (ProcessStartException exception)
            {
                string exceptionMessage = String.Format("Git executable was not found (Path: {0})", gitExecutablePath);
                throw new ProcessException(exceptionMessage, exception.ExitCode, exception);
            }
        }

        /// <summary>Determine if the provided path is inside a git repository.</summary>
        public static bool IsRepository(string path)
        {
            try
            {
                ResolveRepositoryPath(path);

                return true;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
        }

        /// <summary>Resolve the exact path to a git repository.</summary>
        /// <exception cref="DirectoryNotFoundException">Thrown if the provided path is not inside a git repository.</exception>
        public static string ResolveRepositoryPath(string path)
        {
            string currentDirectory = path;

            while (currentDirectory != null)
            {
                if (Directory.Exists(Path.Combine(currentDirectory, ".git")))
                    return currentDirectory;

                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }

            throw new DirectoryNotFoundException(String.Format("Path '{0}' is not a git repository", path));
        }
    }
}
