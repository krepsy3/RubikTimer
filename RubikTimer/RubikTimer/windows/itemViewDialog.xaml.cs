using System;
using System.Collections.Generic;
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
    public partial class itemViewDialog : Window
    {
        private StatsManager statsmanager;
        private bool removingperformed;

        public itemViewDialog(StatsManager statsmanager)
        {
            this.statsmanager = statsmanager;
            InitializeComponent();
            mainItemControl.DataContext = statsmanager;
            statsmanager.PropertyChanged += StatPropertyChange;
            removingperformed = false;
        }

        private void StatPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Stats") UpdateRemoveButton();
        }

        private void SizeChange(object sender, SizeChangedEventArgs e) { UpdateRemoveButton(); }

        private void UpdateRemoveButton()
        {
            if (mainItemControl.Items.Count >= 1)
            {
                DependencyObject dp = mainItemControl.ItemContainerGenerator.ContainerFromIndex(statsmanager.Stats.Count - 1);
                TextBlock lasttb = (TextBlock)VisualTreeHelper.GetChild(dp, 0);
                removeButton.Margin = new Thickness(0, 0, 10, 8 + lasttb.ActualHeight);
            }

            else removeButton.Visibility = Visibility.Hidden;
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void Delete(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateRemoveButton();
            if (MessageBox.Show("Are you sure you want to delete last statistic permanently?", "Statistic deletion confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                statsmanager.RemoveLastStatistic();
                removingperformed = true;
            }

            UpdateRemoveButton();
        }

        private void WinClosing(object sender, CancelEventArgs e)
        {
            if (removingperformed) statsmanager.ReloadStats();
        }
    }
}
