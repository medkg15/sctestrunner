namespace NUnitContrib.Web.TestRunner.Dtos
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class RunSummary
    {
        [DataMember(Name = "message")]
        public StatusMessage Message { get; set; }

        [DataMember(Name = "fixtures")]
        public IEnumerable<FixtureResult> Fixtures { get; set; } 

        [DataMember(Name = "errorList")]
        public IEnumerable<FixtureResult> ErrorList { get; set; } 

        [DataMember(Name = "ignoredList")]
        public IEnumerable<FixtureResult> IgnoredList { get; set; }

        [DataMember(Name = "textOutput")]
        public string TextOutput { get; set; }
    }
}