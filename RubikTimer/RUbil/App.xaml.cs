using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
                // opening RubikStatEditor.exe not a .stxt file
                MessageBox.Show("If you want to edit statistics open Rubik time and...."); // NEDODELANO
                Shutdown();
            }
            else
            {
                Window win = new MainWindow();
                win.Show();
            }
        }
    }
}
