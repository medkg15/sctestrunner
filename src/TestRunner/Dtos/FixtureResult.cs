namespace NUnitContrib.Web.TestRunner.Dtos
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class FixtureResult
    {

        public FixtureResult()
        {
            Tests = new List<TestResult>();
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "tests")]
        public List<TestResult> Tests { get; set; }
    }
}