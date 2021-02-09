using DependentChecker.Log;
using Serilog.Events;

namespace DependentChecker.Helper
{
    public class LogHelper
    {
        private static readonly ChuckSerilog logHelper = new ChuckSerilog();

        public static void CreateLog(LogEventLevel level, object obj)
        {
            logHelper.CreateLog(level, obj.ToString());
        }

        public static void StartProgram()
        {
            logHelper.StartProgram();
        }
    }
}
