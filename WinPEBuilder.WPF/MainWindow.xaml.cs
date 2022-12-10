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
using WinPEBuilder.WPF.Configuration;
using System.Media;

namespace WinPEBuilder.WPF
{
    public partial class MainWindow : MetroWindow
    {
        public static string Themes = "";
        public static string ColorTheme = "";
        public MainWindow()
        {
            InitializeComponent();
            VersionText.Text = "Version: " + Builder.Version;
            OutputVHDBox.Text = Settings.Data.VHDPath;
            ISOSourceBox.Text = Settings.Data.ISOSourcePath;

            LoadPlugins();
        }

        private void LoadPlugins()
        {
            var plg = PluginLoader.GetPlugins();
            foreach (var item in plg)
            {
                var chk = new CheckBox();
                chk.Content = item.DisplayName;
                chk.IsChecked = true;
                chk.Tag = item;
                chk.Checked += Chk_Checked;
                CheckedPlugins.Add(item.PluginGuid);
                PluginsList.Children.Add(chk);
            }
        }
        public List<Guid> CheckedPlugins = new List<Guid>();
        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk)
            {
                if (chk.IsChecked == true)
                {
                    CheckedPlugins.Add(((IPlugin)chk.Tag).PluginGuid);
                }
                else
                {
                    CheckedPlugins.Remove(((IPlugin)chk.Tag).PluginGuid);
                }
            }
        }

        private void GitButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", "https://github.com/MishaTY/WinPEBuilder/");
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
            options.Plugins = CheckedPlugins;
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
            builder.OnLog += Builder_OnLog;
            builder.Start();
        }

        private async void Builder_OnLog(string message)
        {
            await Application.Current.Dispatcher.BeginInvoke(
  DispatcherPriority.Background,
  new Action(() => RchLog.AppendText(message)));
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
            this.ShowMessageAsync("Success", "Project built successfully");
            SystemSounds.Exclamation.Play();
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
                Settings.Data.ISOSourcePath = dlg.FileName;
                Settings.Save();
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
                Settings.Data.VHDPath = dlg.FileName;
                Settings.Save();
            }
        }

        // FOR MISHA: Use the "await this.ShowMessageAsync("This is the title", "Some message");" code to show errors. It fits well with the metro theme.
    }
}
