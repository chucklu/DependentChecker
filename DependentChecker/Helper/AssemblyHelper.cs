using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog.Events;

namespace DependentChecker.Helper
{
    public class AssemblyHelper
    {
        public static List<AssemblyName> GetReferencedAssemblies(Assembly assembly)
        {
            List<AssemblyName> list = new List<AssemblyName>();
            try
            {
                list = assembly.GetReferencedAssemblies().ToList();
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog(LogEventLevel.Error, ex);
            }
            return list;
        }
        
        public static AssemblyName GetAssemblyNameByFullName(string fullName)
        {
            AssemblyName assemblyName = null;
            try
            {
                assemblyName = AssemblyName.GetAssemblyName(fullName);
            }
            catch (BadImageFormatException ex)
            {
                LogHelper.CreateLog(LogEventLevel.Error, ex);
            }

            return assemblyName;
        }

        public static Assembly LoadAssembly(AssemblyName assemblyName)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (BadImageFormatException ex)
            {
                LogHelper.CreateLog(LogEventLevel.Error, $"Load assembly [{assemblyName.FullName}] failed, {ex}");
            }

            if (assembly == null)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                assembly = assemblies.FirstOrDefault(a => a.FullName == assemblyName.FullName);
            }

            return assembly;
        }
    }
}
