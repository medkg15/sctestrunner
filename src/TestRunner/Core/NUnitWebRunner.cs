namespace NUnitContrib.Web.TestRunner.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using NUnit.Core;
    using Dtos;

    /// <summary>
    /// Handle NUnit tests inside an ASP.NET environment.
    /// It uses the session to save state and avoid losing data between requests.
    /// </summary>
    public class NUnitWebRunner : EventListener
    {

        private readonly IReadOnlyCollection<string> assemblies;
        private TestPackage package;
        private TestSuite testSuite;
        private SimpleTestRunner runner;

        public NUnitWebRunner(IReadOnlyCollection<string> assemblies)
        {
            this.assemblies = assemblies;
        }

        public HttpSessionStateBase Session { get; set; }

        public NUnitTestSuiteInfo GetTestSuiteInfo()
        {
            CheckState();
            return GetTestSuiteInfo(testSuite);
        }

        /// <summary>
        /// Ensures that we've got all the items we need to run any operation.
        /// </summary>
        private void CheckState()
        {
            if (Session == null)
                throw new InvalidOperationException("You must set the Session property before calling any other method.");
            
            if (!CoreExtensions.Host.Initialized)
                CoreExtensions.Host.InitializeService();

            package = Session["TestPackage"] as TestPackage;
            testSuite = Session["TestSuite"] as TestSuite;

            if (package == null || testSuite == null)
            {
                package = new TestPackage("TestRunner", assemblies.ToList());
                testSuite = new TestSuiteBuilder().Build(package);
                Session["TestPackage"] = package;
                Session["TestSuite"] = testSuite;
            }
        }

        private static NUnitTestSuiteInfo GetTestSuiteInfo(TestSuite suite)
        {
            var infoResults = new NUnitTestSuiteInfo();

            foreach (ITest testItem in suite.Tests)
            {

                foreach (string category in testItem.Categories)
                {
                    infoResults.Categories.Add(category);
                }

                var testMethod = testItem as NUnitTestMethod;
                if (testMethod != null)
                {
                    var test = new NUnitTestInfo
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
                        testFixture = new NUnitTestFixtureInfo();
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

        public void RunStarted(string name, int testCount)
        {
            throw new NotImplementedException();
        }

        public void RunFinished(TestResult result)
        {
            throw new NotImplementedException();
        }

        public void RunFinished(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void TestStarted(TestName testName)
        {
            throw new NotImplementedException();
        }

        public void TestFinished(TestResult result)
        {
            throw new NotImplementedException();
        }

        public void SuiteStarted(TestName testName)
        {
            throw new NotImplementedException();
        }

        public void SuiteFinished(TestResult result)
        {
            throw new NotImplementedException();
        }

        public void UnhandledException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void TestOutput(TestOutput testOutput)
        {
            throw new NotImplementedException();
        }

    }
}