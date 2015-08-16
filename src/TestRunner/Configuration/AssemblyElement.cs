using System.Configuration;

namespace NUnitContrib.Web.TestRunner.Configuration
{
    public class AssemblyElement : ConfigurationElement
    {

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
        
    }
}