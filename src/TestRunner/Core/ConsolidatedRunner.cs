using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnitContrib.Web.TestRunner.Dtos;

namespace NUnitContrib.Web.TestRunner.Core
{
    public class ConsolidatedRunner
    {
        private readonly List<ITestRunner> _runners;

        public ConsolidatedRunner(List<string> assemblies, List<ITestRunner> runners)
        {
            _runners = runners;
        }
        
        public StatusMessage CancelRunner()
        {
            foreach(var runner in _runners)
            {
                runner.CancelRunner();
            }
        }

        public RunnerStatus GetRunnerStatus()
        {
            throw new NotImplementedException();
        }

        public TestSuiteInfo GetTestSuiteInfo()
        {
            _runners.Select(r => r.GetTestSuiteInfo()).Aggregate()
        }

        public RunSummary RunAllTests()
        {
            foreach(var runner in _runners)
            {
                runner.RunAllTests();
            }
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

        private RunSummary CombineRunSummaries()
        {

        }

        public TestSuiteConfigInfo GetTestSuiteConfigInfo()
        {
            throw new NotImplementedException();
        }
    }
}
