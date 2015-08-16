using System.Configuration;

namespace NUnitContrib.Web.TestRunner.Configuration
{
    public class TestRunnerSection : ConfigurationSection
    {

        [ConfigurationProperty("enabled", DefaultValue = "false", IsRequired = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        [ConfigurationProperty("routePath", IsRequired = true)]
        public string RoutePath
        {
            get { return (string)this["routePath"]; }
            set { this["routePath"] = value; }
        }

        [ConfigurationProperty("resultPath", IsRequired = false)]
        public string ResultPath
        {
            get { return (string)this["resultPath"]; }
            set { this["resultPath"] = value; }
        }

        [ConfigurationProperty("assemblies", IsDefaultCollection = true)]
        public AssemblyElementCollection Assemblies
        {
            get { return (AssemblyElementCollection)this["assemblies"]; }
            set { this["assemblies"] = value; }
        }

    }
}