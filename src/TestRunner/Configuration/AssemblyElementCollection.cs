using System.Configuration;

namespace TestRunner.Configuration
{
    [ConfigurationCollection(typeof(AssemblyElement), AddItemName = "assembly")]
    public class AssemblyElementCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new AssemblyElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AssemblyElement)element).Name;
        }

    }
}