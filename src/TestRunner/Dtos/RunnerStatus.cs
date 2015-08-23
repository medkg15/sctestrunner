namespace NUnitContrib.Web.TestRunner.Dtos
{
    using System.Runtime.Serialization;

    [DataContract]
    public class RunnerStatus
    {
        [DataMember(Name = "counter")]
        public int Counter { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }
    }
}