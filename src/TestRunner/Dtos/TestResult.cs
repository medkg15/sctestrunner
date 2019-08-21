namespace TestRunner.Dtos
{
    using System.Runtime.Serialization;

    [DataContract]
    public class TestResult : TestInfo
    {
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}