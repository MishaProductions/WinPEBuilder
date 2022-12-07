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

        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            // Port backend event
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Pdawg: I've got the settings, you just work on backend :)
        }

        // FOR MISHA: Use the "await this.ShowMessageAsync("This is the title", "Some message");" code to show errors. It fits well with the metro theme.
    }
}
