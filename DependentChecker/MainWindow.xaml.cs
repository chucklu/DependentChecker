using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DependentChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DependencyChoose_Click(object sender, RoutedEventArgs e)
        {
            var fileName = PickDependencyDialog();
            PathTextBox.Text = fileName;
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
    }
}
