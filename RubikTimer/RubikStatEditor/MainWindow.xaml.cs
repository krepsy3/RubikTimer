using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RubikStatEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<FileItem> fileItems;

        public MainWindow()
        {
            InitializeComponent();

            FileReader fileReader = new FileReader();

            try
            {
                fileItems = new ObservableCollection<FileItem>(fileReader.LoadFileItemsFromFile(App.args[0]));
            }
            catch (Exception e) when (e is ArgumentException || e is ArgumentNullException || e is DirectoryNotFoundException || e is FileNotFoundException || e is PathTooLongException)
            {
                MessageBox.Show("Invalid file path.");
            }
            catch
            {
                MessageBox.Show("Could not open file.");
            }

            dataGrid.ItemsSource = fileItems;
            //dataGrid.ColumnFromDisplayIndex(4).SortMemberPath
        }

        private void ConvertToTextComment(object sender, RoutedEventArgs e)
        {
            FileItem item = (FileItem)dataGrid.SelectedItem;
            item.ConvertToTextComment();
        }

        private void RemoveFileItem(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove this line?", "Remove line", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                fileItems.Remove((FileItem)dataGrid.SelectedItem);
        }

        private void btn_AddLine_Click(object sender, RoutedEventArgs e)
        {
            fileItems.Add(new FileItem(null, ""));
            dataGrid.ScrollIntoView(fileItems[fileItems.Count - 1]);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*ICollectionView dataView = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            //clear the existing sort order
            dataView.SortDescriptions.Clear();
            dataView.Refresh();*/
        }
    }
}
