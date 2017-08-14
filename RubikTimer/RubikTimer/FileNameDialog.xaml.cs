using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace RubikTimer
{
    public partial class FileNameDialog : Window
    {
        public string FileName { get { return inputTextBox.Text; } }
        public FileNameDialog(string defaultname, string title, string message)
        {
            InitializeComponent();
            Title = title;
            messageTextBlock.Text = message;
            inputTextBox.Text = defaultname;
        }

        private void ExitOK(object sender, RoutedEventArgs e) { DialogResult = true; }

        private void Exit(object sender, RoutedEventArgs e) { DialogResult = false; }

        private void ListMouseDown(object sender, MouseButtonEventArgs e) { e.Handled = true; }
    }
}
