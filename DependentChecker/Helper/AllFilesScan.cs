﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Serilog.Events;

namespace DependentChecker.Helper
{
    public class AllFilesScanner
    {
        public static void ScanFolder(string folder)
        {
            LogHelper.CreateLog(LogEventLevel.Information,"Start to scan all files.");
            var files = FileHelper.GetExeAndDllFileInfos(folder).ToList();
            var results=new List<SingleFileScanResult>();
            foreach (var file in files)
            {
                LogHelper.CreateLog(LogEventLevel.Information, $"Start to analysis {file.FullName}");
                var result = ScanFile(file.FullName, files);
                results.Add(result);
            }
            LogHelper.CreateLog(LogEventLevel.Information, "Scan all files completed.");
            var needBindingRedirectResults = results.Where(x => x.NeedBindingRedirect);

            int count = 0;
            foreach (var needBindingRedirectResult in needBindingRedirectResults)
            {
                count++;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(
                    $"Assembly{count:000}  {needBindingRedirectResult.DependencyName} need have binding redirect.");
                var str = string.Join(Environment.NewLine,
                    needBindingRedirectResult.DependentLibraries.Select(x => x.ToString()));
                stringBuilder.AppendLine(str);
                LogHelper.CreateLog(LogEventLevel.Information, stringBuilder.ToString());
            }
        }

        public static SingleFileScanResult ScanFile(string filePath,IEnumerable<FileInfo> files)
        {
            AssemblyName fileToName = AssemblyName.GetAssemblyName(filePath); 
            var version = fileToName.Version.ToString();
            bool needBindingRedirect = false;
            List<DependentLibrary> dependentLibraries = new List<DependentLibrary>();

            foreach (var assemblyFile in files)
            {
                string libraryName = Path.GetFileNameWithoutExtension(filePath);
                var libraries = new[] {
                    libraryName,
                }; 
                List<string> libraryList = new List<string>();
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFile.FullName);
                var assembly = Assembly.Load(assemblyName);
                var allDependencies = assembly.GetReferencedAssemblies().ToList();
                var dependencies = allDependencies.Where(x => libraries.Contains(x.Name)).ToList();
                if (dependencies.Count > 0)
                {
                    var tempName = assemblyName.Name;
                    if (!libraryList.Contains(tempName))
                    {
                        libraryList.Add(tempName);
                    }

                    foreach (var dependency in dependencies)
                    {
                        Console.WriteLine($"{dependency.FullName}");
                        if (!dependency.Version.ToString().Equals(version))
                        {
                            Console.WriteLine($"{dependency.Version}!={version}");
                            needBindingRedirect = true;
                        }
                        Console.WriteLine();
                    }

                    var tempDependency = dependencies[0];
                    dependentLibraries.Add(new DependentLibrary
                    {
                        DependentName = assemblyName.Name,
                        DependencyName = tempDependency.Name,
                        DependencyVersion = tempDependency.Version.ToString()
                    });
                }
            }

            return new SingleFileScanResult()
            {
                DependencyName = fileToName.Name,
                DependentLibraries = dependentLibraries,
                NeedBindingRedirect = needBindingRedirect
            };
        }
    }
}
