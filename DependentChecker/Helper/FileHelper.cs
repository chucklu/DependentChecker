using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependentChecker.Helper
{
    public class FileHelper
    {
        public static IEnumerable<FileInfo> GetFileInfos(string folder, IEnumerable<string> extensions)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            var totalFiles = directoryInfo.GetFiles();
            var targetFiles = totalFiles.Where(x => extensions.Contains(x.Extension));
            return targetFiles;
        }

        public static IEnumerable<FileInfo> GetExeAndDllFileInfos(string folder)
        {
            var extensions = new[] {".dll", ".exe"};
            return GetFileInfos(folder, extensions);
        }
    }
}
