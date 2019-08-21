namespace TestRunner.Dtos
{
    using System.Runtime.Serialization;

    [DataContract]
    public class StatusMessage
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }
    }
}