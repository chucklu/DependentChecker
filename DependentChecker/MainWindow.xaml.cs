using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using DependentChecker.Helper;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog.Events;

namespace DependentChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _dependencyPath;

        private string _configFilePath;

        private readonly List<string> libraryList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            RegisterUncaughtExceptionsHandler(AppDomain.CurrentDomain);
            LogHelper.StartProgram();
        }

        private void DependencyChoose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dependencyExtensions = ".exe;*.dll";
                _dependencyPath = ChooseFileDialog("library", dependencyExtensions);
                bool needBindingRedirect = FindDependent(_dependencyPath);
                SetInfoText(_dependencyPath, needBindingRedirect);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void RecordScanResults(IEnumerable<SingleFileScanResult> results)
        {
            var needBindingRedirectResults = results.Where(x => x.NeedBindingRedirect).ToList();
            int count = needBindingRedirectResults.Count;
            LogHelper.CreateLog(LogEventLevel.Debug,$"We should have bindingRedirect for {count} assemblies as following:");

            int index = 0;
            foreach (var needBindingRedirectResult in needBindingRedirectResults)
            {
                index++;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(
                    $"Assembly{index:000}  {needBindingRedirectResult.DependencyName} need have binding redirect.");
                var str = string.Join(Environment.NewLine,
                    needBindingRedirectResult.DependentLibraries.Select(x => x.ToString()));
                stringBuilder.AppendLine(str);
                LogHelper.CreateLog(LogEventLevel.Debug, stringBuilder.ToString());
            }
        }

        private void SetInfoText(string dependencyPath, bool needBindingRedirect)
        {
            var dependency = Assembly.ReflectionOnlyLoadFrom(dependencyPath);
            var dependencyAssemblyName = dependency.GetName();
            InfoText.Text = $"The dependents of {dependencyAssemblyName.Name}({dependencyAssemblyName.Version}) are as following:(NeedBindingRedirect:{needBindingRedirect})";
        }

        internal static string ChooseFileDialog(string rawDisplayName, string extensions)
        {
            using (CommonOpenFileDialog fileDialog = new CommonOpenFileDialog())
            {
                fileDialog.Filters.Add(new CommonFileDialogFilter(rawDisplayName, extensions));
                fileDialog.EnsurePathExists = true;
                fileDialog.Multiselect = false;

                CommonFileDialogResult result = fileDialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(fileDialog.FileName))
                {
                    return fileDialog.FileName;
                }

                return string.Empty;
            }
        }

        private bool FindDependent(string dependencyPath)
        {
            LogHelper.CreateLog(LogEventLevel.Information, $"Dependency file chose: {dependencyPath}");
            FilesList.Items.Clear();
            bool needBindingRedirect = false;
            var folder = Path.GetDirectoryName(dependencyPath);
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new Exception($"Can not get the folder of path {dependencyPath}");
            }

            var assemblyFiles = FileHelper.GetExeAndDllFileInfos(folder).ToList();//all .exe and .dll files under folder

            string libraryName = Path.GetFileNameWithoutExtension(dependencyPath);
            var libraries = new[] {
                libraryName,
            };
            string version = string.Empty;
            bool find = false;
            foreach (var assemblyFile in assemblyFiles)
            {
                AssemblyName assemblyName = AssemblyHelper.GetAssemblyNameByFullName(assemblyFile.FullName);
                if (assemblyName == null)
                {
                    continue;
                }

                if (libraries.Contains(assemblyName.Name))
                {
                    Console.WriteLine(assemblyName.FullName);
                    version = assemblyName.Version.ToString();
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                throw new Exception($"Can not find {libraryName} under {folder}");
            }

            int i = 0;
            foreach (var assemblyFile in assemblyFiles)
            {
                AssemblyName assemblyName = AssemblyHelper.GetAssemblyNameByFullName(assemblyFile.FullName);
                if (assemblyName == null)
                {
                    continue;
                }

                var assembly = AssemblyHelper.LoadAssembly(assemblyName);
                if (assembly == null)
                {
                    assembly = AssemblyHelper.GetAssemblyByFile(assemblyFile.FullName);
                }

                var allDependencies = AssemblyHelper.GetReferencedAssemblies(assembly);
                var dependencies = allDependencies.Where(x => libraries.Contains(x.Name)).ToList();
                if (dependencies.Count > 0)
                {
                    i++;
                    Console.WriteLine(i);
                    Console.WriteLine(assemblyName);
                    var tempName = assemblyName.Name;
                    if (!libraryList.Contains(tempName))
                    {
                        libraryList.Add(tempName);
                    }

                    foreach (var dependency in dependencies)
                    {
                        Console.WriteLine($@"{dependency.FullName}");
                        if (!dependency.Version.ToString().Equals(version))
                        {
                            Console.WriteLine($@"{dependency.Version}!={version}");
                            needBindingRedirect = true;
                        }
                        Console.WriteLine();
                    }

                    var tempDependency = dependencies[0];
                    var dependentLibrary = new DependentLibrary
                    {
                        DependentName = assemblyName.Name,
                        DependentVersion = assemblyName.Version.ToString(),
                        DependencyName = tempDependency.Name,
                        DependencyVersion = tempDependency.Version.ToString()
                    };
                    FilesList.Items.Add(dependentLibrary);
                    LogHelper.CreateLog(LogEventLevel.Information, $"{dependentLibrary.DependentName} depends on [{dependentLibrary.DependencyName}]({dependentLibrary.DependencyVersion})");
                }
            }

            return needBindingRedirect;
        }

        private void RegisterUncaughtExceptionsHandler(AppDomain domain)
        {
            domain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                HandleException(ex);
            };
        }

        private void HandleException(Exception ex)
        {
            LogHelper.CreateLog(LogEventLevel.Error, ex);
            MessageBox.Show(ex.ToString(), "Error");
        }

        private void ConfigFileChoose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var configFileExtensions = ".config";
                var rawDisplayName = "config";
                _configFilePath = ChooseFileDialog(rawDisplayName, configFileExtensions);
                LogHelper.CreateLog(LogEventLevel.Information, $"Config file chose: \"{_configFilePath}\"");
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void FolderChoose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folder = PickFolderDialog();
                Thread thread = new Thread(new ParameterizedThreadStart(ScanAllLibraries));
                thread.IsBackground = true;
                thread.Start(folder);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        internal static string PickFolderDialog()
        {
            using (CommonOpenFileDialog fileDialog = new CommonOpenFileDialog())
            {
                fileDialog.IsFolderPicker = true;
                fileDialog.EnsurePathExists = true;
                fileDialog.Multiselect = false;

                CommonFileDialogResult result = fileDialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(fileDialog.FileName))
                {
                    return fileDialog.FileName;
                }

                return string.Empty;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ScanAllLibraries(object parameter)
        {
            var folder = parameter as string;
            var scanResults = AllFilesScanner.ScanFolder(folder);
            RecordScanResults(scanResults);
            if (string.IsNullOrWhiteSpace(_configFilePath))
            {
                return;
            }

            ConfigHelper.ConfigFilePathValue = _configFilePath;
            var allBindingRedirect = ConfigHelper.GetAllBindingRedirect().ToList();
            LogHelper.CreateLog(LogEventLevel.Debug, $"Currently we have bindingRedirect for {allBindingRedirect.Count} assemblies in config file as following:");
            foreach (var bindingRedirect in allBindingRedirect)
            {
                LogHelper.CreateLog(LogEventLevel.Debug, bindingRedirect);
            }

            var needBindingRedirectResults =
                scanResults.Where(x => x.NeedBindingRedirect).Select(x => x.DependencyName).ToList();
            var uselessBindingRedirect = allBindingRedirect.Except(needBindingRedirectResults).ToList();
            var lackingBindingRedirect = needBindingRedirectResults.Except(allBindingRedirect).ToList();

            LogHelper.CreateLog(LogEventLevel.Information, $"Please remove the following {uselessBindingRedirect.Count} useless bindingRedirect:");
            foreach (var item in uselessBindingRedirect)
            {
                LogHelper.CreateLog(LogEventLevel.Information, item);
            }

            LogHelper.CreateLog(LogEventLevel.Information, $"Please complement the following {lackingBindingRedirect.Count} required bindingRedirect:");
            foreach (var item in lackingBindingRedirect)
            {
                LogHelper.CreateLog(LogEventLevel.Information, item);
            }
        }
    }
}
