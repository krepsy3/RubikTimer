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
        private FileManager fileManager;

        private ObservableCollection<FileItem> fileItems;

        public MainWindow()
        {
            InitializeComponent();

            fileManager = new FileManager();

            try
            {
                fileItems = new ObservableCollection<FileItem>(fileManager.LoadFileItemsFromFile(App.args[0]));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is ArgumentNullException || ex is DirectoryNotFoundException || ex is FileNotFoundException || ex is PathTooLongException)
            {
                MessageBox.Show("Selected Statistic file path is invalid", "Loading error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("File opening failed due to following exception: " + ex.Message,"Loading error",MessageBoxButton.OK,MessageBoxImage.Error);
                Close();
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
            if (MessageBox.Show("Are you sure you want to remove this line?", "Remove line confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                fileItems.Remove((FileItem)dataGrid.SelectedItem);
        }

        private void btn_AddLine_Click(object sender, RoutedEventArgs e)
        {
            fileItems.Add(new FileItem(null, ""));
        }

        private void SortOriginally(object sender, RoutedEventArgs e)
        {

        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        #region File Menu
        private void SaveFile(object sender, RoutedEventArgs e)
        {
            fileManager.SaveFileItems(new List<FileItem>(fileItems));
        }

        private void SaveFileAs(object sender, RoutedEventArgs e)
        {
            /*Open

            fileManager.SaveFileItemsToFile(new List<FileItem>(fileItems), path);*/
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            
        }
        #endregion
    }
}
