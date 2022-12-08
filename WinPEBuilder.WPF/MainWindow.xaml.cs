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
using ControlzEx.Theming;

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
            VersionText.Text = "Version: " + Builder.Version;
            if (Debugger.IsAttached && Environment.UserName.ToLower() == "misha")
            {
                //Debug code for Misha
                ISOSourceBox.Text = @"D:\1Misha\Downloads\25252.1010_amd64_en-us_professional_0ec350c5_convert\25252.1010.221122-1933.RS_PRERELEASE_FLT_CLIENTPRO_OEMRET_X64FRE_EN-US.ISO";
                OutputVHDBox.Text = @"C:\winpegen.vhd";
            }
            if (Debugger.IsAttached && Environment.UserName.ToLower() == "pdawg")
            {
                // Debug code for Pdawg
                ISOSourceBox.Text = @"D:\Other\win11pebuilder\Win11_22H2_English_x64v1.iso";
                OutputVHDBox.Text = @"D:\Other\win11pebuilder\winpegen.vhd";
            }
        }

        public static string Themes;
        public static string ColorTheme;

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
            }
            else
            {
                UWPBox.IsEnabled = false;
                LogonUIBox.IsEnabled = false;
                ExplorerBox.IsEnabled = false;
                UWPBox.IsChecked = false;
                LogonUIBox.IsChecked = false;
                ExplorerBox.IsChecked = false;
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

            BuildButton.IsEnabled = false;

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
                TabControlHeader.Visibility = Visibility.Visible;
                this.ShowMessageAsync("Build Error", message);
                BuildButton.IsEnabled = true;
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
            BuildButton.IsEnabled = true;
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog settingDialog = new SettingsDialog();
            if (settingDialog.ShowDialog() == true)
            {
                if (Themes == "Dark" && ColorTheme != null)
                {
                    ThemeManager.Current.ChangeTheme(this, "Dark." + ColorTheme);
                    SettingsDialog.SettingLocalTheme = "Dark";
                }
                else if (Themes == "Light" && ColorTheme != null)
                {
                    ThemeManager.Current.ChangeTheme(this, "Light." + ColorTheme);
                    SettingsDialog.SettingLocalTheme = "Light";
                }
                else if (Themes == "Dark" && ColorTheme == null)
                {
                    ThemeManager.Current.ChangeTheme(this, "Dark.Purple");
                    SettingsDialog.SettingLocalTheme = "Dark";
                }
                else if (Themes == "Light" && ColorTheme == null)
                {
                    ThemeManager.Current.ChangeTheme(this, "Light.Purple");
                    SettingsDialog.SettingLocalTheme = "Light";
                }
            };

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
