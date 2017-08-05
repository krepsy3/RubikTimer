using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        }

        private void ConvertToStatistic(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedIndex == -1) // nothing is selected
                return;

            FileItem item = (FileItem)dataGrid.SelectedItem;

            // DODELAT; tady musi byt nejaka podminka na convertovani do Statistic; pokud bude Statistic null, musi se vytvorit NOVA INSTANCE,
            //   a taky pokud nebude zadna vlastnost z FileItem urcena, musi se urcit.
            // Na toto by asi bylo nejlepsi male okenko, kde by si to uzivatel zvolil; nebo taky ne => vsechen text by se ulozil do Comment a Info
            //   s SolveTime by zustalo prazdne

            item.LineContent = "Statistic";
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
