namespace TestRunner.Dtos
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class TestFixtureInfo
    {

        public TestFixtureInfo()
        {
            Tests = new List<TestInfo>();
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "tests")]
        public List<TestInfo> Tests { get; set; }
    }
}