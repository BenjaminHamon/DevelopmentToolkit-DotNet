using BenjaminHamon.DevelopmentToolkit.Toolkit.Processes;
using BenjaminHamon.DevelopmentToolkit.Toolkit.RevisionControl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BenjaminHamon.DevelopmentToolkit.ToolkitTests.RevisionControl
{
    [TestClass]
    public class GitClientTest
    {
        public TestContext TestContext { get; set; }

        private string WorkingDirectory;

        [TestInitialize]
        public void Initialize()
        {
            WorkingDirectory = Path.Combine(TestContext.TestRunDirectory, "Working", TestContext.TestName);
        }

        [TestMethod]
        public void GitClient_GetCurrentBranch()
        {
            Mock<ProcessRunner> processRunner = new Mock<ProcessRunner>();
            GitClient gitClient = new GitClient(processRunner.Object, "git", WorkingDirectory);

            List<string> command = new List<string>() { "git", "branch", "--show-current" };
            ProcessResult result = new ProcessResult(0, "my-branch", String.Empty);
            SetupRun(processRunner, command).Returns(result);

            Assert.AreEqual("my-branch", gitClient.GetCurrentBranch());
        }

        [TestMethod]
        public void GitClient_GetCurrentBranch_NoBranch()
        {
            Mock<ProcessRunner> processRunner = new Mock<ProcessRunner>();
            GitClient gitClient = new GitClient(processRunner.Object, "git", WorkingDirectory);

            List<string> command = new List<string>() { "git", "branch", "--show-current" };
            ProcessResult result = new ProcessResult(0, String.Empty, String.Empty);
            SetupRun(processRunner, command).Returns(result);

            Assert.IsNull(gitClient.GetCurrentBranch());
        }

        [TestMethod]
        public void GitClient_GetRevisionDate()
        {
            Mock<ProcessRunner> processRunner = new Mock<ProcessRunner>();
            GitClient gitClient = new GitClient(processRunner.Object, "git", WorkingDirectory);

            string revision = "abcde";
            string dateAsTimestamp = "1651656926";
            DateTime dateAsObject = DateTime.Parse("2022-05-04T09:35:26Z").ToUniversalTime();

            List<string> command = new List<string>() { "git", "show", "--no-patch", "--format=%ct", revision };
            ProcessResult result = new ProcessResult(0, dateAsTimestamp, String.Empty);
            SetupRun(processRunner, command).Returns(result);

            Assert.AreEqual(dateAsObject, gitClient.GetRevisionDate(revision));
        }

        [TestMethod]
        public void GitClient_AreRevisionsEqual()
        {
            GitClient gitClient = new GitClient(new ProcessRunnerImplementation());

            Assert.IsTrue(gitClient.AreRevisionsEqual("aaa", "aaa"));
            Assert.IsFalse(gitClient.AreRevisionsEqual("aaa", "bbb"));
            Assert.IsTrue(gitClient.AreRevisionsEqual("aaa", "aaaaa"));
            Assert.IsTrue(gitClient.AreRevisionsEqual("aaa", "aaabb"));
            Assert.IsFalse(gitClient.AreRevisionsEqual("aaa", "bbaaa"));
            Assert.IsFalse(gitClient.AreRevisionsEqual("abc1", "abc2"));
        }

        private Moq.Language.Flow.ISetup<ProcessRunner,ProcessResult> SetupRun(
            Mock<ProcessRunner> processRunner, IEnumerable<string> command)
        {
            return processRunner.Setup(x => x.RunSimple(
                It.Is<List<string>>(arg => arg.SequenceEqual(command)), WorkingDirectory, true, null));
        }
    }
}
