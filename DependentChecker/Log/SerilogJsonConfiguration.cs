using System.Collections.Generic;

namespace DependentChecker.Log.Configuration
{

    // https://json2csharp.com/
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Override
    {
        public string Default { get; set; }
        public string Microsoft { get; set; }
        //public string Microsoft.Hosting.Lifetime { get; set; }
    }

    public class Args2
    {
        public string expression { get; set; }
    }

    public class Filter
    {
        public string Name { get; set; }
        public Args2 Args { get; set; }
    }

    public class Args3
    {
        public string path { get; set; }
        public string rollOnFileSizeLimit { get; set; }
        public string fileSizeLimitBytes { get; set; }
        public string shared { get; set; }
        public string retainedFileCountLimit { get; set; }
        public string rollingInterval { get; set; }
    }

    public class WriteTo2
    {
        public string Name { get; set; }
        public Args3 Args { get; set; }
    }

    public class ConfigureLogger
    {
        public List<Filter> Filter { get; set; }
        public List<WriteTo2> WriteTo { get; set; }
    }

    public class Args
    {
        public ConfigureLogger configureLogger { get; set; }
    }

    public class WriteTo
    {
        public string Name { get; set; }
        public Args Args { get; set; }
    }

    public class Serilog
    {
        public string MinimumLevel { get; set; }
        public List<WriteTo> WriteTo { get; set; }
    }

    public class Root
    {
        public Serilog Serilog { get; set; }
    }


}
