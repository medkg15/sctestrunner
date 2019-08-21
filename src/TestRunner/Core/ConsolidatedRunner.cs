using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestRunner.Dtos;

namespace TestRunner.Core
{
    public class ConsolidatedRunner
    {
        private readonly List<string> assemblies;
        private readonly string testResultPath;
        private readonly List<ITestRunner> runners;

        public ConsolidatedRunner(List<string> assemblies, string testResultPath, List<ITestRunner> runners)
        {
            this.assemblies = assemblies;
            this.testResultPath = testResultPath;
            this.runners = runners;
        }

        public StatusMessage CancelRunner()
        {
            foreach (var runner in runners)
            {
                runner.CancelRunner();
            }

            return new StatusMessage()
            {
                Text = "Runner cancelled",
                Status = "Warning"
            };
        }

        public Dtos.RunnerStatus GetRunnerStatus()
        {
            var statuses = runners.Select(r => r.GetRunnerStatus()).ToList();

            return new Dtos.RunnerStatus
            {
                Active = statuses.Any(s => s.IsActive),
                Counter = (statuses.Sum(s => s.CompletedTests) * 100) / statuses.Sum(s => s.TotalTests),
            };
        }

        public TestSuiteInfo GetTestSuiteInfo()
        {
            var results = runners.Select(r => r.GetTestSuiteInfo()).ToList();

            return new TestSuiteInfo
            {

                Categories = new HashSet<string>(results.SelectMany(r => r.Categories)),
                Fixtures = results.SelectMany(r => r.Fixtures).ToList(),
            };
        }

        public RunSummary RunAllTests()
        {
            return CombineResults(runners.Select(r => r.RunAllTests()).ToList());
        }

        public RunSummary RunCategories(IEnumerable<string> categories)
        {
            return CombineResults(runners.Select(r => r.RunCategories(categories)).ToList());
        }

        public RunSummary RunFixture(string name)
        {
            return CombineResults(runners.Select(r => r.RunFixture(name)).ToList());
        }

        public RunSummary RunTest(string testId)
        {
            return CombineResults(runners.Select(r => r.RunTest(testId)).ToList());
        }

        private RunSummary CombineResults(List<RunResult> results)
        {
            string status;
            if (results.All(r => r.Passed == r.Total))
            {
                status = "success";
            }
            else if (results.Sum(r => r.Failed) + results.Sum(r => r.Errors) > 0)
            {
                status = "danger";
            }
            else
            {
                status = "warning";
            }

            var text = String.Format("Passed {0}, Failed {1}, Errors {2}, Inconclusive {3}, Invalid {4}, Ignored {5}, Skipped {6}, Time {7}",
                results.Sum(r => r.Passed),
                 results.Sum(r => r.Failed),
                 results.Sum(r => r.Errors),
                 results.Sum(r => r.Inconclusive),
                 results.Sum(r => r.Invalid),
                 results.Sum(r => r.Ignored),
                 results.Sum(r => r.Skipped),
                 results.Sum(r => r.ExecutionTime));
            var message = new StatusMessage { Text = text, Status = status };

            return new RunSummary
            {
                Message = message,
                Fixtures = results.SelectMany(r => r.Fixtures),
                ErrorList = results.SelectMany(r => r.ErrorList),
                IgnoredList = results.SelectMany(r => r.IgnoredList),
                TextOutput = results.Aggregate(new StringBuilder(), (sb, r) => sb.AppendLine(r.TextOutput), sb => sb.ToString()),
            };
        }

        public TestSuiteConfigInfo GetTestSuiteConfigInfo()
        {
            return new TestSuiteConfigInfo
            {
                AssemblyList = assemblies,
                TestResultPath = testResultPath,
            };
        }

        public void SetSessionID(string sessionID)
        {
            foreach (var nunitRunner in runners.OfType<NUnitWebRunner>())
            {
                nunitRunner.SessionId = sessionID;
            }
        }
    }
}
