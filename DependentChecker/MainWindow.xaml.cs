using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using DependentChecker.Log;
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

        private List<string> libraryList = new List<string>();

        private ChuckSerilog logHelper = new ChuckSerilog();

        public MainWindow()
        {
            InitializeComponent();
            RegisterUncaughtExceptionsHandler(AppDomain.CurrentDomain);
            logHelper.StartProgram();
        }

        private void DependencyChoose_Click(object sender, RoutedEventArgs e)
        {
            _dependencyPath = PickDependencyDialog();
            PathTextBox.Text = _dependencyPath;
            bool needBindingRedirect = FindDependent(_dependencyPath);
            SetInfoText(_dependencyPath, needBindingRedirect);
        }


        private void SetInfoText(string dependencyPath,bool needBindingRedirect)
        {
            var dependency = Assembly.ReflectionOnlyLoadFrom(dependencyPath);
            var dependencyAssemblyName = dependency.GetName();
            InfoText.Text = $"The dependents of {dependencyAssemblyName.Name}({dependencyAssemblyName.Version}) are as following:(NeedBindingRedirect:{needBindingRedirect})";
        }

        internal static string PickDependencyDialog()
        {
            using (CommonOpenFileDialog fileDialog = new CommonOpenFileDialog())
            {
                fileDialog.Filters.Add(new CommonFileDialogFilter("library", ".exe;*.dll"));
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
            FilesList.Items.Clear();
            bool needBindingRedirect = false;
            var folder = Path.GetDirectoryName(dependencyPath);
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new Exception($"Can not get the folder of path {dependencyPath}");
            }

            string libraryName = Path.GetFileNameWithoutExtension(dependencyPath);
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            var extensions = new[] { ".dll", ".exe" };
            var totalFiles = directoryInfo.GetFiles();
            var assemblyFiles = totalFiles.Where(x => extensions.Contains(x.Extension)).ToList();//all .exe and .dll files under folder

            var libraries = new[] {
                libraryName,
            };
            string version = string.Empty;
            bool find = false;
            foreach (var assemblyFile in assemblyFiles)
            {
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFile.FullName);
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
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFile.FullName);
                var assembly = Assembly.Load(assemblyName);
                var allDependencies = assembly.GetReferencedAssemblies().ToList();
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
                        Console.WriteLine($"{dependency.FullName}");
                        if (!dependency.Version.ToString().Equals(version))
                        {
                            Console.WriteLine($"{dependency.Version}!={version}");
                            needBindingRedirect = true;
                        }
                        Console.WriteLine();
                    }

                    var tempDependency = dependencies[0];
                    FilesList.Items.Add(new DependentLibrary
                    {
                        DependentName = assemblyName.Name,
                        DependencyName = tempDependency.Name,
                        DependencyVersion = tempDependency.Version.ToString()
                    });
                }
            }

            return needBindingRedirect;
        }

        private void RegisterUncaughtExceptionsHandler(AppDomain domain)
        {
            domain.UnhandledException += new UnhandledExceptionEventHandler(
                (sender, args) =>
                {
                    Exception e = (Exception)args.ExceptionObject;
                    logHelper.CreateLog(LogEventLevel.Error, e);
                    MessageBox.Show(e.ToString(), "Error");
                    Close();
                });
        }
    }
}
