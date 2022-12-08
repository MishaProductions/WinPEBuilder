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
using MahApps.Metro.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using WinPEBuilder.Core;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WinPEBuilder.WPF
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            UWPBox.IsEnabled = false;
            LogonUIBox.IsEnabled = false;
            ExplorerBox.IsEnabled = false;

            if (Debugger.IsAttached && Environment.UserName.ToLower() == "misha")
            {
                //Debug code
                ISOSourceBox.Text = @"D:\1Misha\Downloads\25252.1010_amd64_en-us_professional_0ec350c5_convert\25252.1010.221122-1933.RS_PRERELEASE_FLT_CLIENTPRO_OEMRET_X64FRE_EN-US.ISO";
                OutputVHDBox.Text = @"D:\winpegen.vhd";
            }
        }

        private void GitButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", "https://github.com/MishaTY/WinPEBuilder/");
        }

        private void DWMBox_Click(object sender, RoutedEventArgs e)
        {
            if (DWMBox.IsChecked == true)
            {
                UWPBox.IsEnabled = true;
                LogonUIBox.IsEnabled = true;
                ExplorerBox.IsEnabled = true;
                // Port backend event
            }
            else
            {
                UWPBox.IsEnabled = false;
                LogonUIBox.IsEnabled = false;
                ExplorerBox.IsEnabled = false;
                UWPBox.IsChecked = false;
                LogonUIBox.IsChecked = false;
                ExplorerBox.IsChecked = false;
                // Port backend event
            }
        }

        private void UWPBox_Click(object sender, RoutedEventArgs e)
        {
            if (UWPBox.IsChecked == true)
            {
                // Port backend event
            }
            else
            {
                // Port opposite backend event
            }
        }

        private void LogonUIBox_Click(object sender, RoutedEventArgs e)
        {
            if (LogonUIBox.IsChecked == true)
            {
                // Port backend event
            }
            else
            {
                // Port opposite backend event
            }
        }

        private void ExplorerBox_Click(object sender, RoutedEventArgs e)
        {
            if (ExplorerBox.IsChecked == true)
            {
                // Port backend event
            }
            else
            {
                // Port opposite backend event
            }
        }

        private async void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ISOSourceBox.Text))
            {
                await this.ShowMessageAsync("Error", "You must select an ISO in the sources tab");
                return;
            }
            if (string.IsNullOrEmpty(OutputVHDBox.Text))
            {
                await this.ShowMessageAsync("Error", "You must select an output type/file in the output tab");
                return;
            }

            TabControlHeader.Visibility = Visibility.Collapsed;
            //create options
            var options = new BuilderOptions();
            options.UseDWM = DWMBox.IsChecked == true;
            options.EnableFullUWPSupport = UWPBox.IsChecked == true;
            options.UseLogonUI = LogonUIBox.IsChecked == true;
            options.UseExplorer = ExplorerBox.IsChecked == true;
            options.UseModernTaskmgr = TaskmgrBox.IsChecked == true;
            if (VHDOutput.IsChecked == true)
            {
                options.OutputType = BuilderOptionsOutputType.VHD;
                options.Output = OutputVHDBox.Text;
            }
            else
            {
                throw new NotImplementedException("ISO file output not implemented");
            }

            var builder = new Builder(options, ISOSourceBox.Text, System.AppDomain.CurrentDomain.BaseDirectory + @"work\");


            builder.OnComplete += Builder_OnComplete;
            builder.OnProgress += Builder_OnProgress;
            builder.Start();
        }

        private async void Builder_OnProgress(bool error, int progress, string message)
        {
            await Application.Current.Dispatcher.BeginInvoke(
  DispatcherPriority.Background,
  new Action(() => HandleProgress(error, progress, message)));
        }

        private async void Builder_OnComplete(object? sender, EventArgs e)
        {
            await Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
  new Action(HandleComplete));
        }

        private void HandleProgress(bool error, int progress, string message)
        {
            if (error)
            {
                ProgressText.Text = "";
            }
            else
            {
                BuildProgress.Value = progress;
                ProgressText.Text = message;
            }
        }
        private void HandleComplete()
        {
            ProgressText.Text = "";
            TabControlHeader.Visibility = Visibility.Visible;
        }
        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Pdawg: I've got the settings, you just work on backend :)
        }

        private void ISOOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "ISO Files (*.iso)|*.iso";
            dlg.Title = "Select Windows 10/11 ISO";
            if (dlg.ShowDialog(this) == true)
            {
                ISOSourceBox.Text = dlg.FileName;
            }
        }

        private void OutputVHDOpen_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "VHD Files (*.vhd)|*.vhd";
            dlg.Title = "Select location for output";
            if (dlg.ShowDialog(this) == true)
            {
                OutputVHDBox.Text = dlg.FileName;
            }
        }

        // FOR MISHA: Use the "await this.ShowMessageAsync("This is the title", "Some message");" code to show errors. It fits well with the metro theme.
    }
}
