using System;
using System.Collections.Generic;
using NUnit.Core;
using TestRunner.Dtos;

namespace TestRunner.Core
{
    public interface ITestRunner
    {
        void CancelRunner();

        RunnerStatus GetRunnerStatus();

        TestSuiteInfo GetTestSuiteInfo();

        RunResult RunAllTests();

        RunResult RunCategories(IEnumerable<string> categories);

        RunResult RunFixture(string name);

        RunResult RunTest(string testId);
    }
}