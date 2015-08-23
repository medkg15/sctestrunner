namespace NUnitContrib.Web.TestRunner.Dtos
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class TestSuiteInfo
    {

        public TestSuiteInfo()
        {
            Fixtures = new List<TestFixtureInfo>();
            Categories = new HashSet<string>();
        }

        [DataMember(Name = "fixtures")]
        public List<TestFixtureInfo> Fixtures { get; set; }

        [DataMember(Name = "categories")]
        public HashSet<string> Categories { get; set; }
    }
}