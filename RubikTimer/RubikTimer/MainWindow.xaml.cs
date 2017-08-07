using System;
using System.IO;
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
using System.Windows.Navigation;
using Microsoft.Win32;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace RubikTimer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private enum SolvePhase { Scramble, Inspection, Solve, End }

        public event PropertyChangedEventHandler PropertyChanged;
        private void UpdateProperty(string name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        private Timer timer;
        private StatsManager statsmanager;
        private ScrambleGenerator gen;

        #region Properties
        private SolvePhase _phase;
        private SolvePhase Phase { get { return _phase; } set { _phase = value; UpdateProperty("Phase"); } }
        private DateTime _solvetime;
        public DateTime SolveTime { get { return _solvetime; } private set { _solvetime = value; UpdateProperty("SolveTime"); } }
        public byte InspectionSeconds { get; set; }
        private string _scramble;
        public string Scramble { get { return _scramble; } private set { _scramble = value; UpdateProperty("Scramble"); } }
        private byte _type;
        public byte Type { get { return _type; } private set { _type = value; UpdateProperty("Type"); } }
        private bool _autoscramble;
        public bool AutoScramble { get { return _autoscramble; } set { _autoscramble = value; UpdateProperty("AutoScramble"); } }
        #endregion

        public MainWindow(bool countdown, byte countdownsecs, bool count, bool automaticgeneration, byte gensteps, byte type, string userpath, string currentfile)
        {
            timer = new Timer();
            SolveTime = new DateTime(0);
            gen = new ScrambleGenerator();
            statsmanager = new StatsManager(userpath, currentfile);
            InitializeComponent();

            TimeBorder.DataContext = timer;
            PhaseStackPanel.DataContext = this;
            ScrambleGrid.DataContext = this;
            StatsStackPanel.DataContext = statsmanager;

            PropertyChanged += UpdatePhase;
            timer.PropertyChanged += TimerPropertyUpdate;

            InspectionCheckBox.IsChecked = countdown;
            SolveCheckBox.IsChecked = count;
            if (!countdown && !count) SolveCheckBox.IsChecked = true;

            AutoScramble = automaticgeneration;
            InspectionSeconds = countdownsecs;
            ScrambleLenghtTextBox.Text = gensteps.ToString();
            Type = type;
            Phase = SolvePhase.Scramble;
            foreach (string t in ScrambleGenerator.Type)
            {
                MenuItem m = new MenuItem();
                m.Header = t;
                m.IsCheckable = true;
                PuzzleSelectMenuItem.Items.Add(m);
            }

            ((MenuItem)PuzzleSelectMenuItem.Items[Type]).IsChecked = true;
        }

        private void WinContRendered(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                foreach(string arg in args)
                {
                    switch (arg)
                    {
                        case "/edit": CustomCommands.Edit.Execute(null,null); break;
                    }
                }
            }
        }

        private void KeyPress(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                #region Space
                if (e.Key == Key.Space)
                {
                    switch (Phase)
                    {
                        case SolvePhase.Scramble:
                            {
                                if ((bool)InspectionCheckBox.IsChecked)
                                {
                                    timer.ResetTime();
                                    timer.SetCountdown(InspectionSeconds * 10000000);
                                    Phase = SolvePhase.Inspection;
                                }
                                else
                                {
                                    timer.ResetTime();
                                    Phase = SolvePhase.Solve;
                                }
                                break;
                            }
                        case SolvePhase.Inspection:
                            {
                                timer.StopTime();

                                if ((bool)SolveCheckBox.IsChecked) Phase = SolvePhase.Solve;
                                else Phase = SolvePhase.Scramble;

                                timer.ResetTime();

                                break;
                            }
                        case SolvePhase.Solve:
                            {
                                timer.StopTime();
                                SolveTime = timer.Time;

                                Phase = SolvePhase.End;
                                break;
                            }
                        case SolvePhase.End:
                            {
                                Phase = SolvePhase.Scramble;
                                break;
                            }
                    }
                }
                #endregion

                if (e.Key == Key.R) Phase = SolvePhase.Scramble;
            }
        }

        private void KeyRelease(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                #region Space
                if (e.Key == Key.Space)
                {
                    switch (Phase)
                    {
                        case SolvePhase.Scramble:
                        case SolvePhase.End: break;
                        case SolvePhase.Inspection:
                            {
                                timer.Countdown();
                                break;
                            }
                        case SolvePhase.Solve:
                            {
                                timer.ResetTime();
                                timer.StartTime();
                                break;
                            }
                    }
                }
                #endregion
            }
        }

        private void TimerPropertyUpdate(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Time")
            {
                if (Phase == SolvePhase.Inspection)
                {
                    if (timer.Timeproperty.Ticks == 0)
                    {
                        Phase = SolvePhase.Solve;
                        timer.ResetTime();
                        timer.StartTime(0);
                    }
                }
            }
        }

        private void MouseClick(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) { Keyboard.ClearFocus(); FocusManager.SetFocusedElement(this, this); } }

        private void UpdatePhase(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Phase")
            {
                switch (Phase)
                {
                    case SolvePhase.Scramble:
                        {
                            ScrambleBorder.BorderThickness = new Thickness(1);
                            InspectionBorder.BorderThickness = new Thickness(0);
                            TimerBorder.BorderThickness = new Thickness(0);
                            SolvedBorder.BorderThickness = new Thickness(0);
                            if (AutoScramble) GenerateScramble(this, new RoutedEventArgs());
                            break;
                        }
                    case SolvePhase.Inspection:
                        {
                            ScrambleBorder.BorderThickness = new Thickness(0);
                            InspectionBorder.BorderThickness = new Thickness(1);
                            TimerBorder.BorderThickness = new Thickness(0);
                            SolvedBorder.BorderThickness = new Thickness(0);
                            break;
                        }
                    case SolvePhase.Solve:
                        {
                            ScrambleBorder.BorderThickness = new Thickness(0);
                            InspectionBorder.BorderThickness = new Thickness(0);
                            TimerBorder.BorderThickness = new Thickness(1);
                            SolvedBorder.BorderThickness = new Thickness(0);
                            break;
                        }
                    case SolvePhase.End:
                        {
                            ScrambleBorder.BorderThickness = new Thickness(0);
                            InspectionBorder.BorderThickness = new Thickness(0);
                            TimerBorder.BorderThickness = new Thickness(0);
                            SolvedBorder.BorderThickness = new Thickness(1);
                            break;
                        }
                }
            }
        }

        private void GenerateScramble(object sender, RoutedEventArgs e)
        {
            if (ScrambleLenghtTextBox.Text.Length > 0)
                Scramble = gen.Generate(Type, byte.Parse(ScrambleLenghtTextBox.Text));
        }

        private void ByteTextBoxCheck(object sender, TextChangedEventArgs e)
        {
            byte b = 0;
            int addedlenght = (((ReadOnlyCollection<TextChange>)(e.Changes))[e.Changes.Count - 1]).AddedLength;
            TextBox textbox = (TextBox)sender;

            if (textbox.Text.Length > 0 && !(byte.TryParse(textbox.Text, out b)))
            {
                int select = textbox.SelectionStart;
                textbox.Text = textbox.Text.Remove(textbox.Text.Length - addedlenght, addedlenght);
                textbox.SelectionStart = select;
            }

            else while (textbox.Text.StartsWith("00"))
                {
                    int select = textbox.SelectionStart;
                    textbox.Text = textbox.Text.Remove(0, 1);
                    textbox.SelectionStart = select;
                }
        }

        private void PhaseSelect(object sender, RoutedEventArgs e)
        {
            if (!((bool)SolveCheckBox.IsChecked) && !((bool)InspectionCheckBox.IsChecked))
            {
                if (sender.Equals(InspectionCheckBox)) SolveCheckBox.IsChecked = true;
                else if (sender.Equals(SolveCheckBox)) InspectionCheckBox.IsChecked = true;
            }
        }

        private void MainSize(object sender, SizeChangedEventArgs e) { AdjustMainGrid(); }
        private void ScrambleUpdated(object sender, DataTransferEventArgs e) { AdjustMainGrid(); }
        private void ScrambleGridSize(object sender, SizeChangedEventArgs e) { if (MainGrid.ActualHeight > Main.ActualHeight) AdjustMainGrid(); }

        private void AdjustMainGrid()
        {
            if (MainGrid.ActualHeight > Main.ActualHeight) MainGrid.RowDefinitions[3].Height = new GridLength(Main.ActualHeight - MainGrid.RowDefinitions[0].ActualHeight - MainGrid.RowDefinitions[1].ActualHeight - MainGrid.RowDefinitions[2].ActualHeight - 80);
            else MainGrid.RowDefinitions[3].Height = new GridLength();
        }

        #region Commands
        private void CanSelectPuzzle(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = (Phase == SolvePhase.End || Phase == SolvePhase.Scramble); }
        private void SelectPuzzle(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (MenuItem m in PuzzleSelectMenuItem.Items)
            {
                bool ischecked = false;
                if (m.IsChecked && PuzzleSelectMenuItem.Items.IndexOf(m) != Type)
                {
                    ((MenuItem)PuzzleSelectMenuItem.Items[Type]).IsChecked = false;
                    Type = byte.Parse((PuzzleSelectMenuItem.Items.IndexOf(m)).ToString());
                    ischecked = true;
                }

                if (!ischecked) ((MenuItem)PuzzleSelectMenuItem.Items[Type]).IsChecked = true;
            }
        }

        private void CanCreateFile(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = (Phase == SolvePhase.End || Phase == SolvePhase.Scramble); }
        private void CreateFile(object sender, ExecutedRoutedEventArgs e) { }

        private void CanOpenFile(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = (Phase == SolvePhase.End || Phase == SolvePhase.Scramble); }
        private void OpenFile(object sender, ExecutedRoutedEventArgs e) { }

        private void CanChangeFolder(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = (Phase == SolvePhase.End || Phase == SolvePhase.Scramble); }
        private void ChangeFolder(object sender, ExecutedRoutedEventArgs e) { }

        private void CanEdit(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = (Phase == SolvePhase.End || Phase == SolvePhase.Scramble); }
        private void EditStats(object sender, ExecutedRoutedEventArgs e)
        {
            List<string> files = statsmanager.GetStatisticFiles(false);
            files.Remove(statsmanager.CurrentFileName);
            FilePickerDialog d = new FilePickerDialog(files,"Pick a file for editing","Please pick a file from the list to be edited:");
            if (!((bool)d.ShowDialog())) return;
            else
            {
                try
                {
                    if (!statsmanager.LaunchEditor(files[d.SelectedIndex])) MessageBox.Show("RubikStatEditor.exe file seems to be missing. Try reinstalling the program.", "Editor failed to launch", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Editor failed to launch due to following exception: " + ex.Message, "Launch failure", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CanHelp(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void DisplayHelp(object sender, ExecutedRoutedEventArgs e) { }

        private void CanAbout(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void DisplayAbout(object sender, ExecutedRoutedEventArgs e) { }

        private void CanExit(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = (Phase == SolvePhase.End || Phase == SolvePhase.Scramble); }
        private void Exit(object sender, ExecutedRoutedEventArgs e) { Close(); }
        #endregion

        private void ClosingWindow(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to quit?", "Exit confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No) e.Cancel = true;
            else
            {
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(App.filepath, false))
                {
                    for (int i = 0; i < App.configitems.Length; i++)
                    {
                        switch (i)
                        {
                            case 0: sw.WriteLine(App.configitems[i] + InspectionCheckBox.IsChecked); break;
                            case 1: sw.WriteLine(App.configitems[i] + InspectionSeconds); break;
                            case 2: sw.WriteLine(App.configitems[i] + SolveCheckBox.IsChecked); break;
                            case 3: sw.WriteLine(App.configitems[i] + AutoScrambleCheckBox.IsChecked); break;
                            case 4: sw.WriteLine(App.configitems[i] + ScrambleLenghtTextBox.Text); break;
                            case 5: sw.WriteLine(App.configitems[i] + Type); break;
                            case 6: sw.WriteLine(App.configitems[i] + statsmanager.DirPath); break;
                            case 7: sw.WriteLine(App.configitems[i] + statsmanager.CurrentFileName); break;
                        }
                    }

                    sw.Flush();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Exception was thrown while saving the config file: " + ex.Message, "Config file exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}