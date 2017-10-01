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
using System.Windows.Shapes;

namespace RubikTimer
{
    public partial class OptionPickerDialog : Window
    {
        public int SelectedIndex
        {
            get
            {
                if (radiosStackPanel != null)
                {
                    foreach (RadioButton rb in radiosStackPanel.Children) if ((bool)rb.IsChecked) return radiosStackPanel.Children.IndexOf(rb); 
                    return -1;
                }

                else return -1;
            }
        }

        private int defaultindex;

        public OptionPickerDialog(string title, string message, int defaultindex, params string[] options)
        {
            this.defaultindex = defaultindex;

            if (options.Length == 0) DialogResult = false;
            else if (options.Length == 1) DialogResult = true;
            else
            {
                InitializeComponent();
                Title = title;
                messageTextBlock.Text = message;
                foreach (string option in options)
                {
                    RadioButton r = new RadioButton();
                    r.Margin = new Thickness(10, 10, 10, 0);
                    r.Content = option;
                    radiosStackPanel.Children.Add(r);
                }
            }
        }

        private void Exit(object sender, RoutedEventArgs e) { DialogResult = false; }

        private void CanSelect(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = SelectedIndex >= 0; }
        private void Select(object sender, ExecutedRoutedEventArgs e) { DialogResult = true; }

        private void ContRendered(object sender, EventArgs e)
        {
            if (defaultindex >= 0 && defaultindex < radiosStackPanel.Children.Count) ((RadioButton)radiosStackPanel.Children[defaultindex]).IsChecked = true;

            Height = messageTextBlock.ActualHeight + (radiosStackPanel.Children.Count * 30) + 100;
            ResizeMode = ResizeMode.NoResize;
        }
    }
}
