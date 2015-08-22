using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NUnitContrib.Web.TestRunner.Dtos
{
    [DataContract]
    public class NUnitTestFixtureInfo
    {

        public NUnitTestFixtureInfo()
        {
            Tests = new List<NUnitTestInfo>();
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "tests")]
        public List<NUnitTestInfo> Tests { get; set; }
    }
}