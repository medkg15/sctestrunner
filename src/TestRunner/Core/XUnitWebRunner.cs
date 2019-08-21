using System;
using System.Collections.Concurrent;
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
        private Lazy<IList<ITestCase>> testCases;
        private volatile bool cancel;
        private bool running;
        private int completedTests;

        public XUnitWebRunner(IReadOnlyCollection<string> assemblies, string testResultPath)
        {
            this.assemblies = assemblies;

            controller = new XunitFrontController(AppDomainSupport.Denied, assemblies.First());

            testCases = new Lazy<IList<ITestCase>>(() => {
                var discoverySink = new TestDiscoverySink();

                controller.Find(false, discoverySink, TestFrameworkOptions.ForDiscovery());

                discoverySink.Finished.WaitOne();

                return discoverySink.TestCases;
            });
        }

        public string SessionId { get; set; }

        public StatusMessage CancelRunner()
        {
            cancel = true;

            return new StatusMessage()
            {
                Text = "Runner cancelled",
                Status = "Warning"
            };
        }

        public RunnerStatus GetRunnerStatus()
        {
            if (running)
            {
                return new RunnerStatus { Counter = (completedTests * 100) / testCases.Value.Count, Active = true };
            }
            else
            {
                return new RunnerStatus { Counter = 0, Active = false };
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
            return new TestSuiteInfo
            {
                Categories = new HashSet<string>(testCases.Value.GroupBy(t => t.TestMethod.TestClass.TestCollection.DisplayName).Select(g => g.Key)),
                Fixtures = testCases.Value.GroupBy(t => t.TestMethod.TestClass.Class.Name)
                    .Select(g => new TestFixtureInfo
                    {
                        Name = g.Key,
                        Tests = g.Select(c => new Dtos.TestInfo
                        {
                            Id = c.UniqueID,
                            Name = c.DisplayName,
                            Description = null,
                            Fixture = c.TestMethod.TestClass.Class.Name,
                            Status = null,
                        }).ToList(),
                    }).ToList(),
            };
        }

        public RunSummary RunAllTests()
        {
            return RunTests(testCases.Value);
        }

        public RunSummary RunCategories(IEnumerable<string> categories)
        {
            return RunTests(testCases.Value.Where(c => categories.Contains(c.TestMethod.TestClass.TestCollection.DisplayName)));
        }

        public RunSummary RunFixture(string name)
        {
            return RunTests(testCases.Value.Where(c => c.TestMethod.TestClass.Class.Name == name));
        }

        public RunSummary RunTest(string testId)
        {
            return RunTests(testCases.Value.Where(c => c.UniqueID == testId));
        }

        private RunSummary RunTests(IEnumerable<ITestCase> cases)
        {
            if (running)
            {
                throw new Exception("Already running tests.");
            }

            running = true;
            cancel = false;

            ManualResetEvent executionFinished = new ManualResetEvent(false);

            var results = new Dictionary<string, XUnitTestResult>();
            var output = new StringBuilder();

            var executionSink = new TestMessageSink();

            executionSink.Execution.TestAssemblyFinishedEvent += args =>
            {
                executionFinished.Set();
            };

            executionSink.Execution.TestSkippedEvent += args =>
            {
                results.Add(args.Message.TestCase.UniqueID, new XUnitTestResult
                {
                    Result = XUnitTestResultType.Skipped,
                    TestCase = args.Message.TestCase,
                    ExecutionTime = args.Message.ExecutionTime,
                    Message = args.Message.Reason,
                });

                if (cancel)
                {
                    args.Stop();
                }
            };

            executionSink.Execution.TestPassedEvent += args =>
            {
                results.Add(args.Message.TestCase.UniqueID, new XUnitTestResult
                {
                    Result = XUnitTestResultType.Passed,
                    TestCase = args.Message.TestCase,
                    ExecutionTime = args.Message.ExecutionTime,
                    Message = args.Message.Output,
                });

                if (cancel)
                {
                    args.Stop();
                }
            };

            executionSink.Execution.TestFailedEvent += args =>
            {
                results.Add(args.Message.TestCase.UniqueID, new XUnitTestResult
                {
                    Result = XUnitTestResultType.Failed,
                    TestCase = args.Message.TestCase,
                    ExecutionTime = args.Message.ExecutionTime,
                    Message = args.Message.Messages.FirstOrDefault(),
                });

                if (cancel)
                {
                    args.Stop();
                }
            };

            executionSink.Execution.TestOutputEvent += args =>
            {
                output.Append(args.Message.Output);

                if (cancel)
                {
                    args.Stop();
                }
            };

            executionSink.Execution.TestFinishedEvent += args =>
            {
                Interlocked.Increment(ref this.completedTests);

                if (cancel)
                {
                    args.Stop();
                }
            };

            executionSink.Diagnostics.ErrorMessageEvent += (args) => OnError(args, results);
            executionSink.Execution.TestAssemblyCleanupFailureEvent += (args) => OnError(args, results);
            executionSink.Execution.TestCaseCleanupFailureEvent += (args) => OnError(args, results);
            executionSink.Execution.TestClassCleanupFailureEvent += (args) => OnError(args, results);
            executionSink.Execution.TestCleanupFailureEvent += (args) => OnError(args, results);
            executionSink.Execution.TestCollectionCleanupFailureEvent += (args) => OnError(args, results);
            executionSink.Execution.TestMethodCleanupFailureEvent += (args) => OnError(args, results);

            executionSink.Diagnostics.DiagnosticMessageEvent += args =>
            {

            };

            var options = TestFrameworkOptions.ForExecution();
            options.SetDiagnosticMessages(true);
            options.SetDisableParallelization(true);
            options.SetMaxParallelThreads(1);

            controller.RunTests(cases, executionSink, options);

            executionFinished.WaitOne();

            // cleanup
            completedTests = 0;
            running = false;

            return CreateRunSummary(results, output);
        }

        private RunSummary CreateRunSummary(Dictionary<string, XUnitTestResult> results, StringBuilder output)
        {
            var passed = results.Values.Count(t => t.Result == XUnitTestResultType.Passed);
            var failed = results.Values.Count(t => t.Result == XUnitTestResultType.Failed);
            var errors = results.Values.Count(t => t.Result == XUnitTestResultType.Error);
            var skipped = results.Values.Count(t => t.Result == XUnitTestResultType.Skipped);
            var time = results.Sum(t => t.Value.ExecutionTime);

            string status;
            if (passed == results.Count) status = "success";
            else if (failed + errors > 0) status = "danger";
            else status = "warning";

            var text = $"Passed {passed}, Failed {failed}, Errors {errors}, Skipped {skipped}, Time {time}";

            Func<XUnitTestResult, string> getStatus = r =>
            {
                switch (r.Result)
                {
                    case XUnitTestResultType.Passed:
                        return "success";
                    case XUnitTestResultType.Error:
                    case XUnitTestResultType.Failed:
                        return "danger";
                    default:
                        return "warning";
                }
            };

            var testResults = testCases.Value.Where(c => results.ContainsKey(c.UniqueID)).Select(c =>
            {
                var result = results[c.UniqueID];
                return new TestResult
                {
                    Id = c.UniqueID,
                    Name = c.DisplayName,
                    Description = null,
                    Fixture = c.TestMethod.TestClass.Class.Name,
                    Status = getStatus(result),
                    Message = result.Message
                };
            }).ToList();

            return new RunSummary
            {
                Message = new StatusMessage { Text = text, Status = status },
                TextOutput = output.ToString(),
                Fixtures = testResults
                    .GroupBy(x => x.Fixture)
                    .Select(g => new FixtureResult { Name = g.Key, Tests = g.ToList() }),
                ErrorList = testResults
                    .Where(x => x.Status == "danger")
                    .GroupBy(x => x.Fixture)
                    .Select(g => new FixtureResult { Name = g.Key, Tests = g.ToList() }),
                IgnoredList = testResults
                    .Where(x => x.Status == "warning")
                    .GroupBy(x => x.Fixture)
                    .Select(g => new FixtureResult { Name = g.Key, Tests = g.ToList() }),
            };
        }

        private void OnError<T>(MessageHandlerArgs<T> args, Dictionary<string, XUnitTestResult> results) where T : class, IMessageSinkMessage, IFailureInformation, IExecutionMessage
        {
            results.Add(args.Message.TestCases.First().UniqueID, new XUnitTestResult
            {
                Result = XUnitTestResultType.Error,
                Message = args.Message.Messages.FirstOrDefault(),
                ExceptionType = args.Message.ExceptionTypes.FirstOrDefault(),
                StackTrace = args.Message.StackTraces.FirstOrDefault(),
            });

            if (cancel)
            {
                args.Stop();
            }
        }
    }
}
