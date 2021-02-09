using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Events;

namespace DependentChecker.Helper
{
    public class AllFilesScan
    {
        public static void Scan(string folder)
        {
            LogHelper.CreateLog(LogEventLevel.Information,"Start to scan all files.");
            var files = FileHelper.GetExeAndDllFileInfos(folder).ToList();
            foreach (var file in files)
            {
                LogHelper.CreateLog(LogEventLevel.Information, $"Start to analysis {file.FullName}");
            }
            LogHelper.CreateLog(LogEventLevel.Information, "Scan all files completed.");
        }
    }
}
