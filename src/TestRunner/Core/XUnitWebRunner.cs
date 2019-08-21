using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestRunner.Dtos;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runners;

namespace TestRunner.Core
{
    public class XUnitWebRunner : ITestRunner
    {
        private readonly XunitFrontController controller;
        private Lazy<IList<ITestCase>> testCases;
        private volatile bool cancel;
        private bool running;
        private int completedTests;

        public XUnitWebRunner(string assembly)
        {
            controller = new XunitFrontController(AppDomainSupport.Denied, assembly);

            testCases = new Lazy<IList<ITestCase>>(() => {
                var discoverySink = new TestDiscoverySink();

                controller.Find(false, discoverySink, TestFrameworkOptions.ForDiscovery());

                discoverySink.Finished.WaitOne();

                return discoverySink.TestCases;
            });
        }

        public string SessionId { get; set; }

        public void CancelRunner()
        {
            cancel = true;
        }

        public RunnerStatus GetRunnerStatus()
        {
            if (running)
            {
                return new RunnerStatus { CompletedTests = completedTests, TotalTests = testCases.Value.Count, IsActive = true };
            }
            else
            {
                return new RunnerStatus { CompletedTests = 0, TotalTests = testCases.Value.Count, IsActive = false };
            }
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

        public RunResult RunAllTests()
        {
            return RunTests(testCases.Value);
        }

        public RunResult RunCategories(IEnumerable<string> categories)
        {
            return RunTests(testCases.Value.Where(c => categories.Contains(c.TestMethod.TestClass.TestCollection.DisplayName)));
        }

        public RunResult RunFixture(string name)
        {
            return RunTests(testCases.Value.Where(c => c.TestMethod.TestClass.Class.Name == name));
        }

        public RunResult RunTest(string testId)
        {
            return RunTests(testCases.Value.Where(c => c.UniqueID == testId));
        }

        private RunResult RunTests(IEnumerable<ITestCase> cases)
        {
            if (running)
            {
                throw new Exception("Already running tests.");
            }

            running = true;
            cancel = false;

            ManualResetEvent executionFinished = new ManualResetEvent(false);

            var results = new Dictionary<string, XUnitTestResult>();

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

            return CreateRunSummary(results);
        }

        private RunResult CreateRunSummary(Dictionary<string, XUnitTestResult> results)
        {
            var passed = results.Values.Count(t => t.Result == XUnitTestResultType.Passed);
            var failed = results.Values.Count(t => t.Result == XUnitTestResultType.Failed);
            var errors = results.Values.Count(t => t.Result == XUnitTestResultType.Error);
            var skipped = results.Values.Count(t => t.Result == XUnitTestResultType.Skipped);
            var time = results.Sum(t => t.Value.ExecutionTime);
            
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

            return new RunResult
            {
                Total = testCases.Value.Count,
                Passed = passed,
                Failed = failed,
                Errors = errors,
                Skipped = skipped,
                ExecutionTime = time,
                TextOutput = null,
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
