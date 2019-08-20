﻿using System;
using System.Collections.Generic;
using NUnit.Core;
using NUnitContrib.Web.TestRunner.Dtos;

namespace NUnitContrib.Web.TestRunner.Core
{
    public interface IWebRunner
    {
        string SessionId { get; set; }

        StatusMessage CancelRunner();

        RunnerStatus GetRunnerStatus();

        TestSuiteConfigInfo GetTestSuiteConfigInfo();

        TestSuiteInfo GetTestSuiteInfo();

        RunSummary RunAllTests();

        RunSummary RunCategories(IEnumerable<string> categories);

        RunSummary RunFixture(string name);

        RunSummary RunTest(string testId);
    }
}