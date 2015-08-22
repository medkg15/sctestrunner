using System.Runtime.Serialization;

namespace NUnitContrib.Web.TestRunner.Dtos
{
    [DataContract]
    public class NUnitTestInfo
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "fixture")]
        public string Fixture { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }
    }
}