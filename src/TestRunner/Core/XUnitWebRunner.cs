using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnitContrib.Web.TestRunner.Dtos;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runners;

namespace NUnitContrib.Web.TestRunner.Core
{
    public class XUnitWebRunner : IWebRunner
    {
        private readonly IReadOnlyCollection<string> assemblies;
        private readonly string testResultPath;


        private readonly XunitFrontController controller;
        private IList<ITestCase> testCases;
        private IList<object> results;

        public XUnitWebRunner(IReadOnlyCollection<string> assemblies, string testResultPath)
        {
            ManualResetEvent executionFinished = new ManualResetEvent(false);

            controller = new XunitFrontController(AppDomainSupport.Denied, assemblies.First());

            var discoverySink = new TestDiscoverySink();
            controller.Find(false, discoverySink, TestFrameworkOptions.ForDiscovery());

            discoverySink.Finished.WaitOne();

            testCases = discoverySink.TestCases;

            var executionSink = new TestMessageSink();

            controller.RunTests(testCases, executionSink, TestFrameworkOptions.ForExecution());

            executionSink.Execution.TestAssemblyFinishedEvent += args =>
            {
                executionFinished.Set();
            };

            executionSink.Execution.TestFinishedEvent += args => {

            };

            executionFinished.WaitOne();
        }

        public string SessionId { get; set; }

        public StatusMessage CancelRunner()
        {
            runner.Cancel();

            return new StatusMessage()
            {
                Text = "Runner cancelled",
                Status = "Warning"
            };
        }

        public RunnerStatus GetRunnerStatus()
        {
            switch (runner.Status)
            {
                case AssemblyRunnerStatus.Discovering:
                    return new RunnerStatus { Counter = 0, Active = true };
                case AssemblyRunnerStatus.Executing:
                    return new RunnerStatus { Counter = (completedTests * 100) / totalTestCount, Active = true };
                case AssemblyRunnerStatus.Idle:
                    return new RunnerStatus { Counter = 0, Active = false };
                default:
                    throw new NotImplementedException();
            }
        }

        public TestSuiteConfigInfo GetTestSuiteConfigInfo()
        {
            return new TestSuiteConfigInfo
            {
                TestResultPath = testResultPath,
                AssemblyList = assemblies,
            };
        }

        public TestSuiteInfo GetTestSuiteInfo()
        {
            throw new NotImplementedException();
        }

        public RunSummary RunAllTests()
        {
            throw new NotImplementedException();
        }

        public RunSummary RunCategories(IEnumerable<string> categories)
        {
            throw new NotImplementedException();
        }

        public RunSummary RunFixture(string name)
        {
            throw new NotImplementedException();
        }

        public RunSummary RunTest(string testId)
        {
            throw new NotImplementedException();
        }
    }
}
