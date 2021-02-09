using System;
using System.IO;
using System.Linq;
using System.Text;
using DependentChecker.Log.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;

namespace DependentChecker.Log
{
    public class ChuckSerilog
    {
        public ChuckSerilog()
        {
            Init();
            SelfLog.Enable(SelfLogHandler);
        }

        private string GetConfigPath()
        {
            const string componentName = "appsettings";
            const string fileExtension = "json";
            var fileName = $"{componentName}.{fileExtension}";
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            var configFile =
                new FileInfo(Path.Combine(baseDirectory,
                    AppDomain.CurrentDomain.FriendlyName + $".{fileName}"));
            if (!configFile.Exists)
            {
                stringBuilder.AppendLine(configFile.FullName);
                configFile =
                    new FileInfo(Path.Combine(baseDirectory, $"{fileName}"));
            }
            if (!configFile.Exists)
            {
                stringBuilder.AppendLine(configFile.FullName);
                configFile =
                    new FileInfo(Path.Combine(baseDirectory, "bin",
                        $"{fileName}"));
            }
            if (!configFile.Exists)
            {
                throw new Exception($"Can not find the configuration of log in following path {stringBuilder}");
            }
            else
            {
                return configFile.FullName;
            }
        }

        public void StartProgram()
        {
            Type type1 = typeof(FileLoggerConfigurationExtensions);
            Type type2 = typeof(LoggerSinkConfigurationExtensions);
            Console.WriteLine($@"{type1},{type2}");
            CreateLog(LogEventLevel.Information, $"Program {AppDomain.CurrentDomain.FriendlyName} started.");
        }

        private void Init()
        {
            Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);
            var path = GetConfigPath();
            if (!string.IsNullOrWhiteSpace(path))
            {
                _configurationFilePath = path;

                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(path)
                    .Build();

                var loggerConfiguration = new LoggerConfiguration();
                loggerConfiguration = loggerConfiguration.ReadFrom.Configuration(configuration);
                var logger = loggerConfiguration.CreateLogger();
                Serilog.Log.Logger = logger;
            }
            else
            {
                throw new Exception($"Can not find the configuration file for Serilog");
            }
        }

        public void CreateLog(LogEventLevel level, Exception ex)
        {
            switch (level)
            {
                case LogEventLevel.Debug:
                    Serilog.Log.Debug(ex, string.Empty);
                    break;
                case LogEventLevel.Information:
                    Serilog.Log.Information(ex, string.Empty);
                    break;
                case LogEventLevel.Warning:
                    Serilog.Log.Warning(ex, string.Empty);
                    break;
                case LogEventLevel.Error:
                    Serilog.Log.Error(ex, string.Empty);
                    break;
                case LogEventLevel.Fatal:
                    Serilog.Log.Fatal(ex, string.Empty);
                    break;
            }
        }

        public void CreateLog(LogEventLevel level, string message)
        {
            switch (level)
            {
                case LogEventLevel.Debug:
                    Serilog.Log.Debug(message);
                    break;
                case LogEventLevel.Information:
                    Serilog.Log.Information(message);
                    break;
                case LogEventLevel.Warning:
                    Serilog.Log.Warning(message);
                    break;
                case LogEventLevel.Error:
                    Serilog.Log.Error(message);
                    break;
                case LogEventLevel.Fatal:
                    Serilog.Log.Fatal(message);
                    break;
            }
        }

        private void SelfLogHandler(string log)
        {
            throw new Exception(log);
        }

        public string GetLogFolderByName(string loggerName, string writeToName)
        {
            string logFolder = string.Empty;
            var filePath = _configurationFilePath;
            var text = File.ReadAllText(filePath);
            var root = JsonConvert.DeserializeObject<Root>(text);
            var writeTo = root.Serilog.WriteTo.FirstOrDefault(x => x.Name.Equals(loggerName));
            if (writeTo != null)
            {
                var writeTo2 = writeTo.Args.configureLogger.WriteTo.FirstOrDefault(x => x.Name.Equals(writeToName));
                string path = string.Empty;
                if (writeTo2 != null)
                {
                    path = writeTo2.Args.path;
                }

                var query = Path.GetDirectoryName(path);
                if (query != null)
                {
                    logFolder = Environment.ExpandEnvironmentVariables(query);
                }
            }

            return logFolder;
        }

        private string _configurationFilePath = string.Empty;
    }
}
