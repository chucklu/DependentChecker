﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var dependencyExtensions = ".exe;*.dll";
            _dependencyPath = ChooseFileDialog("library", dependencyExtensions);
            PathTextBox.Text = _dependencyPath;
            bool needBindingRedirect = FindDependent(_dependencyPath);
            SetInfoText(_dependencyPath, needBindingRedirect);

            var folder = Path.GetDirectoryName(_dependencyPath);
            AllFilesScanner.ScanFolder(folder);
        }


        private void SetInfoText(string dependencyPath,bool needBindingRedirect)
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
                        Console.WriteLine($@"{dependency.FullName}");
                        if (!dependency.Version.ToString().Equals(version))
                        {
                            Console.WriteLine($@"{dependency.Version}!={version}");
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
                    LogHelper.CreateLog(LogEventLevel.Error, e);
                    MessageBox.Show(e.ToString(), "Error");
                    Close();
                });
        }

        private void ConfigFileChoose_Click(object sender, RoutedEventArgs e)
        {
            var configFileExtensions = ".config";
            var rawDisplayName = "config";
            _configFilePath = ChooseFileDialog(rawDisplayName, configFileExtensions);
            LogHelper.CreateLog(LogEventLevel.Information, $"Config file chose: \"{_configFilePath}\"");
        }
    }
}
