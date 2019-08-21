using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace TestRunner.Core
{
    public class XUnitTestResult
    {
        public XUnitTestResultType Result { get; set; }
        public ITestCase TestCase { get; set; }
        public string Message { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
        public decimal ExecutionTime { get; internal set; }
    }

    public enum XUnitTestResultType
    {
        Passed,
        Skipped,
        Failed,
        Error,
    }
}
