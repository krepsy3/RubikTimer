﻿using System;
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

using Microsoft.Win32;
using RubikTimer;

namespace RubikStatEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool saved = true;
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
            Title = (new FileInfo(App.args[0])).Name + " - RubikTimer Satistic files Editor";
        }

        private void ContRendered(object sender, EventArgs e)
        {
            if (App.args.Length > 1 && App.args[1] == "TIMER")
                return;
            else
                MessageBox.Show("Editor launched succesfully. However, you should allways start from the main application RubikTimer.exe to prevent data loss due to using undedicated directories.", "Data loss warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ConvertToTextComment(object sender, RoutedEventArgs e)
        {
            FileItem item = (FileItem)dataGrid.SelectedItem;
            item.ConvertToTextComment();
            saved = false;
        }

        private void RemoveFileItem(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove this line?", "Remove line confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                fileItems.Remove((FileItem)dataGrid.SelectedItem);
                saved = false;
            }
        }

        private void AddLine(object sender, RoutedEventArgs e)
        {
            fileItems.Add(new FileItem(null, ""));
            saved = false;
        }

        private void SortOriginally(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            if (view != null && view.SortDescriptions != null)
            {
                view.SortDescriptions.Clear();
                foreach (DataGridColumn c in dataGrid.Columns) { c.SortDirection = null; }
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

            if (!saved)
            {
                MessageBoxResult mbr = MessageBox.Show("Your changes have not been saved. Do you wish to save?", "Save and exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (mbr == MessageBoxResult.Cancel) e.Cancel = true;
                else if (mbr == MessageBoxResult.Yes) ApplicationCommands.Save.Execute(null, null);
            }
        }

        #region Commands
        private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            fileManager.SaveFileItems(new List<FileItem>(fileItems));
            saved = true;
        }

        private void SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            FileNameDialog dialog = new FileNameDialog("", "Save As", "Please enter the name for the new file:");

            if (dialog.ShowDialog() == true)
            {
                fileManager.SaveFileItemsToFile(new List<FileItem>(fileItems), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"RubikTimer\" + dialog.FileName));
                saved = true;
            }
        }

        private void Exit(object sender, ExecutedRoutedEventArgs e) { Close(); }

        private void DisplayHelp(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                new HelpWindow(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/help/editorhelp")).Show();
            }
            catch { }
        }

        private void DisplayAbout(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                new HelpWindow(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/help/editorabout")).Show();
            }
            catch { }
        }
        #endregion
    }
}
