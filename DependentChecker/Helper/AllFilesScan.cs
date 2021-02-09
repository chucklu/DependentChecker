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
            LogHelper.CreateLog(LogEventLevel.Information,"All files scan started.");
        }
    }
}
