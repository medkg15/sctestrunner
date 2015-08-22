namespace NUnitContrib.Web.TestRunner.Dtos
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class NUnitTestSuiteConfigInfo
    {
        [DataMember(Name = "assemblyList")]
        public IEnumerable<string> AssemblyList { get; set; }

        [DataMember(Name = "testResultPath")]
        public string TestResultPath { get; set; }
    }
}