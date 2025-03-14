using BenjaminHamon.DevelopmentToolkit.Toolkit.Processes;
using BenjaminHamon.DevelopmentToolkit.Toolkit.RevisionControl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace BenjaminHamon.DevelopmentToolkit.ToolkitTests.RevisionControl
{
    [TestClass]
    public class GitHelpersTest
    {
        public TestContext TestContext { get; set; }

        private string WorkingDirectory;

        [TestInitialize]
        public void Initialize()
        {
            WorkingDirectory = Path.Combine(TestContext.TestRunDirectory, "Working", TestContext.TestName);
        }

        [TestMethod]
        public void GitHelpers_IsRepository_WithRepository()
        {
            string repositoryPath = Path.Combine(WorkingDirectory, "Repository");
            GitClient gitClient = new GitClient(new ProcessRunnerImplementation(), "git", repositoryPath);

            Directory.CreateDirectory(repositoryPath);
            gitClient.RunCommand(new List<string>() { "init" });

            Assert.IsTrue(GitHelpers.IsRepository(repositoryPath));
        }

        [TestMethod]
        public void GitHelpers_IsRepository_WithoutRepository()
        {
            // Temporary directory since the workspace itself is a git repository
            Assert.IsFalse(GitHelpers.IsRepository(Path.GetTempPath()));
        }

        [TestMethod]
        public void GitHelpers_GetRepositoryPath_WithRepository()
        {
            string repositoryPath = Path.Combine(WorkingDirectory, "Repository");
            GitClient gitClient = new GitClient(new ProcessRunnerImplementation(), "git", repositoryPath);

            Directory.CreateDirectory(repositoryPath);
            gitClient.RunCommand(new List<string>() { "init" });

            Assert.IsTrue(GitHelpers.ResolveRepositoryPath(repositoryPath) == repositoryPath);
            Assert.IsTrue(GitHelpers.ResolveRepositoryPath(Path.Combine(repositoryPath, "Subdirectory")) == repositoryPath);
        }

        [TestMethod]
        public void GitHelpers_GetRepositoryPath_WithoutRepository()
        {
            // Temporary directory since the workspace itself is a git repository
            Assert.ThrowsExactly<DirectoryNotFoundException>(() => GitHelpers.ResolveRepositoryPath(Path.GetTempPath()));
        }
    }
}
