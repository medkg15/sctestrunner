using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NUnitContrib.Web.TestRunner.Dtos
{
    [DataContract]
    public class NUnitTestSuiteInfo
    {

        public NUnitTestSuiteInfo()
        {
            Fixtures = new List<NUnitTestFixtureInfo>();
            Categories = new HashSet<string>();
        }

        [DataMember(Name = "fixtures")]
        public List<NUnitTestFixtureInfo> Fixtures { get; set; }

        [DataMember(Name = "categories")]
        public HashSet<string> Categories { get; set; }
    }
}