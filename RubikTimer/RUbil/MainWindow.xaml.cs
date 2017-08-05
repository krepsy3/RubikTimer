﻿using System;
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

using RUbil;

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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //fileItems.Add(new FileItem(new RubikTimer.Statistic(TimeSpan.FromSeconds(10), "info"), "Comment", "Line text"));
            fileItems.ElementAt(1).SolveTime = "";
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
