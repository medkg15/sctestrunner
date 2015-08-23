namespace NUnitContrib.Web.TestRunner.Dtos
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class TestSuiteConfigInfo
    {
        [DataMember(Name = "assemblyList")]
        public IEnumerable<string> AssemblyList { get; set; }

        [DataMember(Name = "testResultPath")]
        public string TestResultPath { get; set; }
    }
}