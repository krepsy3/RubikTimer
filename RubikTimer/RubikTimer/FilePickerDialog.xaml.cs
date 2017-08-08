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
    public partial class FilePickerDialog : Window
    {
        public ObservableCollection<string> Files { get; private set; }
        public int SelectedIndex { get { return filesView != null ? filesView.SelectedIndex : -1; } }

        public FilePickerDialog(List<string> files, string title = "File Pick", string message = "Please pick a file from the list:")
        {
            Files = new ObservableCollection<string>(files);
            InitializeComponent();
            DataContext = this;
            Title = title;
            messageTextBlock.Text = message;
        }

        private void CanSelect(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = filesView != null ? filesView.SelectedIndex >= 0 : false; }
        private void Select(object sender, ExecutedRoutedEventArgs e) { DialogResult = true; }

        private void Exit(object sender, RoutedEventArgs e) { DialogResult = false; }

        private void WinMouseDown(object sender, MouseButtonEventArgs e) { filesView.SelectedIndex = -1; }

        private void ListMouseDown(object sender, MouseButtonEventArgs e) { e.Handled = true; }

        private void ListMouseDouble(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DependencyObject dpo = (DependencyObject)e.OriginalSource;
                while (dpo != null && dpo != filesView)
                {
                    if (dpo is ListViewItem) MediaCommands.Select.Execute(null, null);
                    dpo = VisualTreeHelper.GetParent(dpo);
                }
            }
        }
    }
}
