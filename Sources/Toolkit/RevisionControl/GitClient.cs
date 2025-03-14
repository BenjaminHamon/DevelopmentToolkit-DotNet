using BenjaminHamon.DevelopmentToolkit.Toolkit.Processes;
using BenjaminHamon.DevelopmentToolkit.Toolkit.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BenjaminHamon.DevelopmentToolkit.Toolkit.RevisionControl
{
    /// <summary>Client to interact with a git repository.</summary>
    public class GitClient : RevisionControlClient
    {
        public const int ShortRevisionLength = 10;

        public GitClient()
            : this(new ProcessRunnerImplementation(), null, null)
        { }

        public GitClient(ProcessRunner processRunner, string gitExecutable = null, string workingDirectory = null)
        {
            this.ProcessRunner = processRunner;
            this.GitExecutable = gitExecutable ?? "git";
            this.WorkingDirectory = workingDirectory ?? GitHelpers.ResolveRepositoryPath(Directory.GetCurrentDirectory());
        }

        private readonly ProcessRunner ProcessRunner;
        private readonly string GitExecutable;

        public string WorkingDirectory { get; }

        /// <summary>Run a git command.</summary>
        /// <exception cref="ProcessStartException">Thrown if the git executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown if the command exited with an error code.</exception>
        public ProcessResult RunCommand(List<string> arguments, IDictionary<string,string> environment = null)
        {
            List<string> command = new List<string>() { GitExecutable };
            command.AddRange(arguments);

            return ProcessRunner.RunSimple(command, workingDirectory: WorkingDirectory, environment: environment);
        }

        /// <summary>Get the revision currently checked out in the repository.</summary>
        /// <exception cref="ProcessStartException">Thrown if the git executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown if the directory is not a git repository.</exception>
        public string GetCurrentRevision()
        {
            return ResolveRevision("HEAD");
        }

        /// <summary>Generate a revision identifier for the current state, including local changes.</summary>
        /// <exception cref="ProcessStartException">Thrown if the git executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown if the directory is not a git repository.</exception>
        public string GenerateRevisionWithLocalChanges()
        {
            // This method generates a revision as if we were committing all local changes.
            // The operations can be summed up as: switch to a temporary git index, add all local changes, create a new commit object.

            // The git index is preserved through using a temporary index.
            // The git working tree is preserved because the new commit object is not added to it nor is it referenced by anything.

            string gitRepositoryRootPath = GitHelpers.ResolveRepositoryPath(WorkingDirectory);
            string temporaryIndexPath = Path.Combine(gitRepositoryRootPath, ".git", "index.DevelopmentToolkit.tmp");
            string commitMessage = "Fake commit for GenerateRevisionWithLocalChanges";

            DateTime headRevisionDateTime = GetRevisionDate("HEAD");

            Dictionary<string, string> environment = new Dictionary<string, string>()
            {
                // This will make git use a different, temporary index
                { "GIT_INDEX_FILE", temporaryIndexPath },

                // This will make the commit use a fixed datetime so that its hash stays consistent through several calls
                { "GIT_AUTHOR_DATE", headRevisionDateTime.ToString("o", CultureInfo.InvariantCulture) },
                { "GIT_COMMITTER_DATE", headRevisionDateTime.ToString("o", CultureInfo.InvariantCulture) },
            };

            string commitIdentifier;

            try
            {
                ProcessResult result;

                // Resets the index, which is not needed but returns the index from nothing to the head status rather.
                result = RunCommand(new List<string>() { "reset" }, environment: environment);

                // Adds all local changes, including new files.
                result = RunCommand(new List<string>() { "add", "--all" }, environment: environment);

                // Creates a git tree object which is needed by the commit-tree operation.
                // The process result contains the name of the new tree object.
                result = RunCommand(new List<string>() { "write-tree" }, environment: environment);
                string gitTreeIdentifier = result.OutputData;

                // Creates a unreferenced commit with the local changes and fixed metadata.
                // The process result contains the identifier of the new commit object.
                result = RunCommand(new List<string>() { "commit-tree", gitTreeIdentifier, "-m", commitMessage }, environment: environment);
                commitIdentifier = result.OutputData;
            }
            finally
            {
                if (File.Exists(temporaryIndexPath))
                {
                    File.Delete(temporaryIndexPath);
                }
            }

            // Returns the hash of the new commit object.
            return commitIdentifier;
        }

        /// <summary>
        /// Get the branch currently checked out in the repository.
        /// Return null if no branch is checked out.
        /// </summary>
        /// <exception cref="ProcessStartException">Thrown if the git executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown if the directory is not a git repository.</exception>
        public string GetCurrentBranch()
        {
            ProcessResult result = RunCommand(new List<string>() { "branch", "--show-current" });
            return String.IsNullOrEmpty(result.OutputData) ? null : result.OutputData;
        }

        /// <summary>
        /// Resolve a git reference into its revision identifier.
        /// Return null if the reference cannot be resolved or if the directory is not a git repository.
        /// </summary>
        /// <exception cref="ProcessStartException">Thrown if the git executable was not found.</exception>
        public string TryResolveRevision(string reference)
        {
            try
            {
                return ResolveRevision(reference);
            }
            catch (ProcessFailureException)
            {
                return null;
            }
        }

        /// <summary>Resolve a git reference into its revision identifier.</summary>
        /// <exception cref="ProcessStartException">Thrown if the git executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown if the reference cannot be resolved or the directory is not a git repository.</exception>
        public string ResolveRevision(string reference)
        {
            return RunCommand(new List<string>() { "rev-list", "--max-count", "1", reference }).OutputData;
        }

        /// <summary>Resolve a git reference into a best-effort numeric revision identifier.</summary>
        /// <exception cref="ProcessStartException">Thrown if the git executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown if the reference cannot be resolved or the directory is not a git repository.</exception>
        public int ResolveRevisionAsNumeric(string reference)
        {
            return Int32.Parse(RunCommand(new List<string>() { "rev-list", "--merges", "--count", reference }).OutputData);
        }

        /// <summary>Convert a revision to its shorter form if possible.</summary>
		public string ConvertRevisionToRevisionShort(string revision)
        {
            return revision.Substring(0, ShortRevisionLength);
        }

        /// <summary>Get the UTC datetime for a revision.</summary>
        /// <exception cref="ProcessStartException">Thrown if the git executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown if the reference cannot be resolved or the directory is not a git repository.</exception>
        public DateTime GetRevisionDate(string revision)
        {
            ProcessResult result = RunCommand(new List<string>() { "show", "--no-patch", "--format=%ct", revision });
            return DateTimeOffset.FromUnixTimeSeconds(Int64.Parse(result.OutputData)).UtcDateTime.TruncateToSeconds();
        }

        /// <summary>Resolve a git reference into a best-effort numeric revision identifier.</summary>
        /// <exception cref="ProcessStartException">Thrown if the git executable was not found.</exception>
        /// <exception cref="ProcessFailureException">Thrown if the reference cannot be resolved or the directory is not a git repository.</exception>
        public List<string> ListRevisions(string start = null, int limit = 100)
        {
            ProcessResult result = RunCommand(new List<string>() { "rev-list", "--max-count", limit.ToString(), start ?? "HEAD" });
            return result.OutputData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        /// <summary>Compare two git revision identifiers and determine if they reference the same revision.</summary>
        /// <remarks>This is useful to compare truncated revision identifiers, however short identifiers are more likely to have collisions.</remarks>
        public bool AreRevisionsEqual(string first, string second)
        {
            return first.StartsWith(second) || second.StartsWith(first);
        }
    }
}
