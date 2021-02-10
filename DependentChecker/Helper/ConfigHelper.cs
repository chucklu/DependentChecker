using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DependentChecker.Helper
{
    internal static class ConfigHelper
    {
        private static string _configFilePathValue;

        internal static string ConfigFilePathValue
        {
            get
            {
                return _configFilePathValue;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception($"ConfigFilePathValue should not be null or empty or whitespace");
                }

                if (!File.Exists(value))
                {
                    throw new FileNotFoundException($"Can not find file {value}");
                }
                _configFilePathValue = value;
            }
        }

        internal static void ResetPath()
        {
            _configFilePathValue = string.Empty;
        }

        /// <summary>
        /// https://stackoverflow.com/a/988325/13338936
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(RemoveAllNamespaces));
        }

        internal static IEnumerable<string> GetAllBindingRedirect()
        {
            var filePath = _configFilePathValue;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new List<string>();
            }
            var root = XElement.Load(filePath);
            var root2 = RemoveAllNamespaces(root);
            var runtime = root2.Element("runtime");
            var assemblyBinding = runtime?.Element("assemblyBinding");
            var assemblyIdentities = assemblyBinding?.Elements("dependentAssembly")
                .Elements("assemblyIdentity").Select(x => x.Attribute("name")?.Value);
            return assemblyIdentities;
        }
    }
}
