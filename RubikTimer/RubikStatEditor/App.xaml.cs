using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RubikStatEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] args;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            args = e.Args;

            if (e.Args.Length == 0)
            {
                // opening RubikStatEditor.exe not .stxt file
                if (MessageBox.Show("To edit statistic files, please open the main app (RubikTimer.exe) and select Edit Statistic Files. Do you wish to launch it now?","Fatal error",MessageBoxButton.YesNo,MessageBoxImage.Error,MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo("RubikTimer.exe","/edit /skip");
                    p.Start();
                }
                Shutdown();
            }
            else
            {
                MainWindow win = new MainWindow();
                win.Show();
            }
        }
    }
}
