using System.Collections.Generic;

namespace DependentChecker
{
    public class DependentLibrary
    {
        public string DependentName { get; set; }

        public string DependencyName { get; set; }

        public string DependencyVersion { get; set; }

        public override string ToString()
        {
            return $"{DependentName} depends on [{DependencyName}]({DependencyVersion})";
        }
    }

    public class SingleFileScanResult
    {
        public string DependencyName { get; set; }

        public List<DependentLibrary> DependentLibraries { get; set; } = new List<DependentLibrary>();

        public bool NeedBindingRedirect { get; set; }
    }
}
