namespace NUnitContrib.Web.TestRunner.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Runtime.Caching;
    using NUnit.Core;
    using NUnit.Util;
    using Dtos;
    using NUnit.Core.Filters;

    /// <summary>
    /// Handle NUnit tests inside an ASP.NET environment.
    /// It uses memory cache to save state and avoid losing data between requests.
    /// </summary>
    public class NUnitWebRunner : EventListener, IWebRunner
    {

        private readonly IReadOnlyCollection<string> assemblies;
        private readonly string testResultPath;
        private readonly MemoryCache runnerCache = MemoryCache.Default;

        public NUnitWebRunner(IReadOnlyCollection<string> assemblies, string testResultPath)
        {
            this.assemblies = assemblies;
            this.testResultPath = testResultPath;
        }

        #region Properties

        public string SessionId { get; set; }

        private TestPackage Package
        {
            get { return GetRunnerCacheItem<TestPackage>("TestPackage"); }
            set { SetRunnerCacheItem("TestPackage", value); }
        }

        private TestSuite TestSuite
        {
            get { return GetRunnerCacheItem<TestSuite>("TestSuite"); }
            set { SetRunnerCacheItem("TestSuite", value); }
        }

        private SimpleTestRunner Runner
        {
            get { return GetRunnerCacheItem<SimpleTestRunner>("TestRunner"); }
            set { SetRunnerCacheItem("TestRunner", value); }
        }

        private List<NUnit.Core.TestResult> TestResults
        {
            get
            {
                var testResults = GetRunnerCacheItem<List<NUnit.Core.TestResult>>("TestResults") ??
                                  SetRunnerCacheItem("TestResults", new List<NUnit.Core.TestResult>());
                return testResults;
            }
        }

        private StringBuilder TextOutputBuilder
        {
            get
            {
                var textOutputBuilder = GetRunnerCacheItem<StringBuilder>("TextOutputBuilder") ??
                                        SetRunnerCacheItem("TextOutputBuilder", new StringBuilder());
                return textOutputBuilder;
            }
        }

        private StringBuilder ConsoleBuilder
        {
            get
            {
                var consoleBuilder = GetRunnerCacheItem<StringBuilder>("ConsoleBuilder") ??
                                     SetRunnerCacheItem("ConsoleBuilder", new StringBuilder());
                return consoleBuilder;
            }
        }

        private StringWriter ConsoleStringWriter
        {
            get { return GetRunnerCacheItem<StringWriter>("ConsoleStringWriter"); }
            set { SetRunnerCacheItem("ConsoleStringWriter", value); }
        }

        private int TotalTests
        {
            get { return GetRunnerCacheItem<int>("TotalTests"); }
            set { SetRunnerCacheItem("TotalTests", value); }
        }

        #endregion

        public TestSuiteConfigInfo GetTestSuiteConfigInfo()
        {
            return new TestSuiteConfigInfo
            {
                AssemblyList = assemblies,
                TestResultPath = testResultPath ?? "(none)"
            };
        }

        public TestSuiteInfo GetTestSuiteInfo()
        {
            CheckState();

            var testsInfo = GetRunnerCacheItem<TestSuiteInfo>("TestSuiteInfo");
            if (testsInfo != null)
                return testsInfo;

            testsInfo = GetTestSuiteInfo(TestSuite);
            SetRunnerCacheItem("TestSuiteInfo", testsInfo);

            return testsInfo;
        }

        public RunSummary RunAllTests()
        {
            var testsResult = RunTests(TestFilter.Empty);
            return testsResult;
        }

        public RunSummary RunTest(string testId)
        {
            if (string.IsNullOrEmpty(testId))
                throw new ArgumentNullException("testId");

            var suiteInfo = GetTestSuiteInfo();
            var existsTest = suiteInfo.Fixtures.Any(x => x.Tests.Any(t => t.Id == testId));

            if (!existsTest)
                throw new Exception("Test not found");

            var simpleNameFilter = new SimpleNameFilter(testId);
            return RunTests(simpleNameFilter);
        }

        public RunSummary RunCategories(IEnumerable<string> categories)
        {
            var suiteInfo = GetTestSuiteInfo();
            var validCategories = suiteInfo.Categories.Intersect(categories).ToArray();

            if (!validCategories.Any())
                throw new Exception("Invalid Categories");

            var categoryFilter = new CategoryFilter(validCategories);
            var testResults = RunTests(categoryFilter);
            return testResults;
        }

        public RunSummary RunFixture(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var suiteInfo = GetTestSuiteInfo();
            var fixture = suiteInfo.Fixtures.FirstOrDefault(x => x.Name == name);
            
            if (fixture == null)
                throw new Exception("Fixture not found");

            var namesToAdd = fixture.Tests
                .OrderBy(t => t.Id)
                .Select(t => t.Id)
                .ToArray();

            var simpleNameFilter = new SimpleNameFilter(namesToAdd);
            return RunTests(simpleNameFilter);
        }

        public RunnerStatus GetRunnerStatus()
        {
            var active = Runner != null && Runner.Running;
            if (TotalTests.Equals(0)) return new RunnerStatus { Counter = 0, Active = active };
            var copy = new List<NUnit.Core.TestResult>(TestResults);
            int counter = (copy.Count * 100) / TotalTests;
            return new RunnerStatus { Counter = counter, Active = active };
        }

        public StatusMessage CancelRunner()
        {
            if (Runner != null && Runner.Running)
            {
                Runner.CancelRun();
            }

            return new StatusMessage
            {
                Text = "Runner cancelled",
                Status = "Warning"
            };
        }

        private static TestSuiteInfo GetTestSuiteInfo(TestSuite suite)
        {
            var infoResults = new TestSuiteInfo();

            foreach (ITest testItem in suite.Tests)
            {

                foreach (string category in testItem.Categories)
                {
                    infoResults.Categories.Add(category);
                }

                var testMethod = testItem as NUnitTestMethod;
                if (testMethod != null)
                {
                    var test = new Dtos.TestInfo
                    {
                        Id = testMethod.TestName.FullName,
                        Name = testMethod.MethodName,
                        Fixture = testMethod.ClassName,
                        Description = testMethod.Description,
                        Status = testMethod.RunState == RunState.Runnable ? "info" : "warning"
                    };

                    var testFixture = infoResults.Fixtures
                        .FirstOrDefault(x => x.Name.Equals(testMethod.ClassName));
                    if (testFixture == null)
                    {
                        testFixture = new TestFixtureInfo();
                        testFixture.Name = testMethod.ClassName;
                        testFixture.Tests.Add(test);
                        infoResults.Fixtures.Add(testFixture);
                    }
                    else
                    {
                        testFixture.Tests.Add(test);
                    }
                }

                if (testItem.IsSuite)
                {
                    var suiteInfo = GetTestSuiteInfo(testItem as TestSuite);
                    infoResults.Fixtures.AddRange(suiteInfo.Fixtures);

                    foreach (var category in suiteInfo.Categories)
                    {
                        infoResults.Categories.Add(category);
                    }
                }
            }

            return infoResults;
        }

        /// <summary>
        /// Ensures that we've got all the items we need to run any operation.
        /// </summary>
        private void CheckState()
        {
            if (string.IsNullOrEmpty(SessionId))
                throw new InvalidOperationException("You must set the SessionId property before calling any other method.");

            if (!CoreExtensions.Host.Initialized)
                CoreExtensions.Host.InitializeService();

            if (Package == null || TestSuite == null)
            {
                Package = new TestPackage("TestRunner", assemblies.ToList());
                TestSuite = new TestSuiteBuilder().Build(Package);
            }
        }

        private RunSummary RunTests(ITestFilter filter)
        {
            CheckState();

            if (Runner != null && Runner.Running)
            {
                while (Runner.Running) { /* do nothing */ }
            }

            using (Runner = new SimpleTestRunner())
            {
                Runner.Load(Package);
                if (Runner.Test == null)
                {
                    Runner.Unload();
                    //return new { Text = "Unable to load the tests", Status = "Warning" };
                }

                NUnit.Core.TestResult result = null;
                try
                {
                    result = Runner.Run(this, filter, true, LoggingThreshold.All);
                }
                catch (Exception e)
                {
                    //return new { Text = e.Message, Status = "Error" };
                }

                return result == null
                    ? null
                    : GetTestResult();
            }
        }

        private RunSummary GetTestResult()
        {
            Func<NUnit.Core.TestResult, string> getStatus = r =>
            {
                switch (r.ResultState)
                {
                    case ResultState.Success:
                        return "success";
                    case ResultState.Error:
                    case ResultState.Failure:
                        return "danger";
                    default:
                        return "warning";
                }
            };

            var passed = TestResults.Count(t => t.ResultState == ResultState.Success);
            var failed = TestResults.Count(t => t.ResultState == ResultState.Failure);
            var errors = TestResults.Count(t => t.ResultState == ResultState.Error);
            var inconclusive = TestResults.Count(t => t.ResultState == ResultState.Inconclusive);
            var invalid = TestResults.Count(t => t.ResultState == ResultState.NotRunnable);
            var ignored = TestResults.Count(t => t.ResultState == ResultState.Ignored);
            var skipped = TestResults.Count(t => t.ResultState == ResultState.Skipped);
            var time = TestResults.Sum(t => t.Time);

            string status;
            if (passed == TestResults.Count) status = "success";
            else if (failed + errors > 0) status = "danger";
            else status = "warning";

            var text = String.Format("Passed {0}, Failed {1}, Errors {2}, Inconclusive {3}, Invalid {4}, Ignored {5}, Skipped {6}, Time {7}",
                passed, failed, errors, inconclusive, invalid, ignored, skipped, time);
            var message = new StatusMessage { Text = text, Status = status };

            var testResults = TestResults.Select(r => new Dtos.TestResult 
            {
                Id = r.Test.TestName.TestID.ToString(),
                Name = r.Test.MethodName,
                Fixture = r.Test.ClassName,
                Description = r.Test.Description,
                Status = getStatus(r),
                Message = string.Concat(r.Message, r.IsError ? "\r\n" + r.StackTrace : string.Empty)
            }).ToList();

            var fixtures = testResults
                .GroupBy(x => x.Fixture)
                .Select(g => new FixtureResult{ Name = g.Key, Tests = g.ToList() });

            var errorList = testResults
                .Where(x=> x.Status == "danger")
                .GroupBy(x => x.Fixture)
                .Select(g => new FixtureResult{ Name = g.Key, Tests = g.ToList() });

            var ignoredList = testResults
                .Where(x => x.Status == "warning")
                .GroupBy(x => x.Fixture)
                .Select(g => new FixtureResult { Name = g.Key, Tests = g.ToList() });

            var textoutput = TextOutputBuilder.ToString().TrimEnd();

            return new RunSummary
            {
                Message = message,
                Fixtures = fixtures,
                ErrorList = errorList,
                IgnoredList = ignoredList,
                TextOutput = textoutput
            };
        }

        private T SetRunnerCacheItem<T>(string key, T item)
        {
            runnerCache[SessionId + ":" + key] = item;
            return item;
        }

        private T GetRunnerCacheItem<T>(string key)
        {
            return (T)runnerCache[SessionId + ":" + key];
        }

        #region EventHandlers

        public void RunStarted(string name, int testCount)
        {
            // Clear the results. The list will be populated when TestFinished is called
            TestResults.Clear();

            // Intercept the console write
            TextOutputBuilder.Clear();
            ConsoleBuilder.Clear();
            ConsoleStringWriter = new StringWriter(ConsoleBuilder);
            Console.SetOut(ConsoleStringWriter);

            // Set the test count
            TotalTests = testCount;
        }

        public void RunFinished(NUnit.Core.TestResult result)
        {
            // Reset console output
            ConsoleStringWriter.Dispose();
            var consoleOut = Console.Out;
            Console.SetOut(consoleOut);

            // Write the TestResult.xml
            if (!string.IsNullOrEmpty(testResultPath))
            {
                var testResultBuilder = new StringBuilder();
                using (var writer = new StringWriter(testResultBuilder))
                {
                    var xmlWriter = new XmlResultWriter(writer);
                    xmlWriter.SaveTestResult(result);
                }

                var xmlOutput = testResultBuilder.ToString();
                using (var writer = new StreamWriter(testResultPath))
                {
                    writer.Write(xmlOutput);
                }
            }
        }

        public void TestFinished(NUnit.Core.TestResult result)
        {
            // Add the test
            TestResults.Add(result);

            // Append the info to the text output
            var output = ConsoleBuilder.ToString();
            if (string.IsNullOrWhiteSpace(output)) return;
            TextOutputBuilder.AppendLine(result.Test.TestName.FullName).Append(output).AppendLine();
            ConsoleBuilder.Clear();
        }

        #endregion

        #region NotImplemented

        public void TestStarted(TestName testName)
        {
        }

        public void SuiteStarted(TestName testName)
        {
        }

        public void SuiteFinished(NUnit.Core.TestResult result)
        {
        }

        public void UnhandledException(Exception exception)
        {
        }

        public void TestOutput(TestOutput testOutput)
        {
        }

        public void RunFinished(Exception exception)
        {
        }

        #endregion

    }
}