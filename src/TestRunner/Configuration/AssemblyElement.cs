using System.Configuration;

namespace TestRunner.Configuration
{
    public class AssemblyElement : ConfigurationElement
    {

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("runner", IsRequired = true)]
        public string Runner
        {
            get { return (string)this["runner"]; }
            set { this["runner"] = value; }
        }

    }
}