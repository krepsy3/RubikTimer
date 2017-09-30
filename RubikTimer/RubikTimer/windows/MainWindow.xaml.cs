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
using System.Diagnostics;
using System.Media;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using WFDialogResult = System.Windows.Forms.DialogResult;

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
        private SoundPlayer player;

        private bool skipquitconfirmation;
        private bool[] soundplay;

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
        private bool _dontsavestats;
        public bool DontSaveStats { get { return _dontsavestats; } set { _dontsavestats = value; UpdateProperty("DontSaveStats"); } }
        private string _info;
        public string Info { get { return _info; } private set { _info = value; UpdateProperty("Info"); UpdateProperty("InfoBrush"); } }
        public SolidColorBrush InfoBrush { get { if (Info.StartsWith("Warning")) return new SolidColorBrush(Colors.Red); else return new SolidColorBrush(Colors.Black); } }
        #endregion

        public MainWindow(bool countdown, byte countdownsecs, bool count, bool automaticgeneration, byte gensteps, byte type, bool dontsavestats, string userpath, string currentfile)
        {
            timer = new Timer();
            SolveTime = new DateTime(0);
            gen = new ScrambleGenerator();
            statsmanager = new StatsManager(userpath, currentfile, dontsavestats);
            player = new SoundPlayer();
            skipquitconfirmation = false;
            soundplay = new bool[] { false, false, false, false, false };
            InitializeComponent();

            TimeBorder.DataContext = timer;
            PhaseStackPanel.DataContext = this;
            ScrambleGrid.DataContext = this;
            infoTextBlock.DataContext = this;
            statsGrid.DataContext = statsmanager;

            PropertyChanged += UpdatePhase;
            timer.PropertyChanged += TimerPropertyUpdate;
            statsmanager.PropertyChanged += StatsManagerPropertyUpdate;

            InspectionCheckBox.IsChecked = countdown;
            SolveCheckBox.IsChecked = count;
            if (!countdown && !count) SolveCheckBox.IsChecked = true;
            ScrambleLenghtTextBox.Text = gensteps.ToString();
            dontSaveMenuItem.IsChecked = dontsavestats;

            AutoScramble = automaticgeneration;
            InspectionSeconds = countdownsecs;
            DontSaveStats = dontsavestats;
            Type = type;
            Title = "RubikTimer - Professional offline speedcubing timer";
            Phase = SolvePhase.Scramble;
            Scramble = "";

            foreach (string t in ScrambleGenerator.Type)
            {
                MenuItem m = new MenuItem();
                m.Header = t;
                m.IsCheckable = true;
                PuzzleSelectMenuItem.Items.Add(m);
            }
        }

        private void WinContRendered(object sender, EventArgs e)
        {
            UpdateInfo();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                bool pathhandled = false;

                foreach(string arg in args)
                {
                    switch (arg)
                    {
                        case "/edit": CustomCommands.Edit.Execute(null,null); break;
                        case "/skip": skipquitconfirmation = true; break;
                        default:
                            {
                                #region openfile
                                if (File.Exists(arg) && new FileInfo(arg).Extension == statsmanager.extension && !pathhandled)
                                {
                                    if (arg.StartsWith(statsmanager.DirPath))
                                    {
                                        pathhandled = true;
                                        string filename = (new FileInfo(arg).Name);
                                        filename = filename.Remove(filename.Length - statsmanager.extension.Length);

                                        OptionPickerDialog opd = new OptionPickerDialog("File opened", "You have opened a statistic file " + filename + statsmanager.extension + ". Please Choose what to do:", 1, "Select the it as a current statistic file", "Edit it", "Nothing");
                                        if ((bool)opd.ShowDialog())
                                        {
                                            switch (opd.SelectedIndex)
                                            {
                                                case 0:
                                                    {
                                                        try
                                                        {
                                                            statsmanager.ChangeCurrentFile(filename);
                                                            MessageBox.Show("Succesfully changed current statistic file to " + filename + statsmanager.extension, "Statistic File swap succesful", MessageBoxButton.OK, MessageBoxImage.Information);
                                                        }

                                                        catch (Exception ex)
                                                        {
                                                            MessageBox.Show("Changing the Statistic file failed due to following Exception: " + ex.Message, "Statistic File swap Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                        }

                                                        break;
                                                    }

                                                case 1:
                                                    {
                                                        try
                                                        {
                                                            if (!statsmanager.LaunchEditor(filename)) MessageBox.Show("RubikStatEditor.exe file seems to be missing. Try reinstalling the program.", "Editor failed to launch", MessageBoxButton.OK, MessageBoxImage.Error);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            MessageBox.Show("Editor failed to launch due to following exception: " + ex.Message, "Launch failure", MessageBoxButton.OK, MessageBoxImage.Error);
                                                        }

                                                        break;
                                                    }

                                                case 2: break;
                                            }
                                        }
                                    }

                                    else MessageBox.Show("You attempted to open statistic file " + (new FileInfo(arg).Name) + " from other directory than the current one. Please move the file into the current directory (" + statsmanager.DirPath + ") to be able to use it in the application.", "File selection invalid", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                #endregion

                                break;
                            }
                    }
                }
            }

            MediaCommands.Select.Execute(null, null);
            CustomCommands.Generate.Execute(null, null);
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

        private void MouseClick(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) { Keyboard.ClearFocus(); FocusManager.SetFocusedElement(this, this); Focus(); } }

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
                            InspectionCheckBox.IsEnabled = true;
                            inspectionSecondsTextBox.IsEnabled = true;
                            SolveCheckBox.IsEnabled = true;

                            if (AutoScramble) CustomCommands.Generate.Execute(null, null);
                            break;
                        }
                    case SolvePhase.Inspection:
                        {
                            ScrambleBorder.BorderThickness = new Thickness(0);
                            InspectionBorder.BorderThickness = new Thickness(1);
                            TimerBorder.BorderThickness = new Thickness(0);
                            SolvedBorder.BorderThickness = new Thickness(0);
                            InspectionCheckBox.IsEnabled = false;
                            inspectionSecondsTextBox.IsEnabled = false;
                            SolveCheckBox.IsEnabled = false;

                            if (InspectionSeconds > 8) { for (int i = 0; i < soundplay.Length; i++) soundplay[i] = false; }
                            else if (InspectionSeconds > 3) { soundplay[0] = true; for (int i = 1; i < soundplay.Length; i++) soundplay[i] = false; }
                            else { for (int i = 0; i < soundplay.Length; i++) soundplay[i] = true; }
                            player.SoundLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/sound", (!soundplay[0]) ? "eightsecs.wav" : "countdown.wav");
                            try { player.LoadAsync(); } catch { }
                            break;
                        }
                    case SolvePhase.Solve:
                        {
                            ScrambleBorder.BorderThickness = new Thickness(0);
                            InspectionBorder.BorderThickness = new Thickness(0);
                            TimerBorder.BorderThickness = new Thickness(1);
                            SolvedBorder.BorderThickness = new Thickness(0);
                            InspectionCheckBox.IsEnabled = false;
                            inspectionSecondsTextBox.IsEnabled = false;
                            SolveCheckBox.IsEnabled = false;
                            break;
                        }
                    case SolvePhase.End:
                        {
                            ScrambleBorder.BorderThickness = new Thickness(0);
                            InspectionBorder.BorderThickness = new Thickness(0);
                            TimerBorder.BorderThickness = new Thickness(0);
                            SolvedBorder.BorderThickness = new Thickness(1);
                            InspectionCheckBox.IsEnabled = true;
                            inspectionSecondsTextBox.IsEnabled = true;
                            SolveCheckBox.IsEnabled = true;

                            statsmanager.AddStatistic(new Statistic(
                                timer.Timeproperty,
                                "Date: " + DateTime.Now.ToString(@"d\.M\.yyyy") +
                                " Puzzle: " + ScrambleGenerator.Type[Type] +
                                ((AutoScramble && Scramble.Length > 0) ? (" Scramble: " + Scramble) : "")));
                            break;
                        }
                }
            }
        }

        private void TimerPropertyUpdate(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Time")
            {
                if (Phase == SolvePhase.Inspection)
                {
                    if (timer.Timeproperty.TotalMilliseconds <= 8200 && !soundplay[0])
                    {
                        soundplay[0] = true;
                        try { player.Play(); } catch { }
                        player.SoundLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/sound/countdown.wav");
                        try { player.LoadAsync(); } catch { }
                    }

                    else if (timer.Timeproperty.TotalMilliseconds <= 3200 && !soundplay[1])
                    {
                        soundplay[1] = true;
                        try { player.Play(); } catch { }
                    }

                    else if (timer.Timeproperty.TotalMilliseconds <= 2200 && !soundplay[2])
                    {
                        soundplay[2] = true;
                        try { player.Play(); } catch { }
                    }

                    else if (timer.Timeproperty.TotalMilliseconds <= 1200 && !soundplay[3])
                    {
                        soundplay[3] = true;
                        try { player.Play(); } catch { }
                        player.SoundLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/sound/countstop.wav");
                        try { player.LoadAsync(); } catch { }
                    }

                    else if (timer.Timeproperty.TotalMilliseconds <= 200 && !soundplay[4])
                    {
                        soundplay[4] = true;
                        try { player.Play(); } catch { }
                    }

                    else if (timer.Timeproperty.Ticks == 0)
                    {
                        if ((bool)SolveCheckBox.IsChecked)
                        {
                            Phase = SolvePhase.Solve;
                            timer.ResetTime();
                            timer.StartTime(0);
                        }

                        if (!soundplay[4])
                        {
                            soundplay[4] = true;
                            try { player.Play(); } catch { }
                        }
                    }
                }
            }
        }

        private void StatsManagerPropertyUpdate(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StatFileLoaded") UpdateInfo();
        }

        private void UpdateDontSave(object sender, RoutedEventArgs e)
        {
            if (dontSaveMenuItem.IsChecked)
            {
                statsmanager.ChangeCurrentFile("");
                DontSaveStats = true;
                UpdateInfo();
            }

            else
            {
                DontSaveStats = false;
                UpdateInfo();
                ApplicationCommands.Open.Execute(null, null);
            }
        }

        private void UpdateInfo()
        {
            if (statsmanager.StatFileLoaded && !DontSaveStats) Info = "Statistic is being saved into " + statsmanager.CurrentFileName + statsmanager.extension;
            else if (DontSaveStats) Info = "Statistic saving is turned off. They are temporary and will be lost on program close.";
            else Info = "Warning - Statistic is not being saved and will be lost. Please select a statistic file to be saved into.";
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
        private void ScrambleGridSize(object sender, SizeChangedEventArgs e) { if (MainGrid.ActualHeight > Main.ActualHeight - 80) AdjustMainGrid(); }

        private void AdjustMainGrid()
        {
            if (MainGrid.ActualHeight > Main.ActualHeight - 80)
            {
                double diff = Main.ActualHeight - MainGrid.ActualHeight - 80;
                MainGrid.RowDefinitions[3].Height = new GridLength(MainGrid.RowDefinitions[3].ActualHeight + diff);
            }
            else MainGrid.RowDefinitions[3].Height = new GridLength();
        }

        #region Commands
        private void CanCmdTrue(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void CanCmdNoSolve(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = (Phase == SolvePhase.Scramble || Phase == SolvePhase.End); }
        private void CanCmdOnStart(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = Phase == SolvePhase.Scramble; }
        private void CanCmdFile(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = (Phase == SolvePhase.Scramble && !DontSaveStats); }

        private void SelectPuzzle(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (MenuItem m in PuzzleSelectMenuItem.Items)
            {
                bool ischecked = false;
                if (m.IsChecked && PuzzleSelectMenuItem.Items.IndexOf(m) != Type)
                {
                    ((MenuItem)PuzzleSelectMenuItem.Items[Type]).IsChecked = false;
                    Type = byte.Parse((PuzzleSelectMenuItem.Items.IndexOf(m)).ToString());
                    CustomCommands.Generate.Execute(null, null);
                    ischecked = true;
                }

                if (!ischecked) ((MenuItem)PuzzleSelectMenuItem.Items[Type]).IsChecked = true;
            }
        }

        private void CreateFile(object sender, ExecutedRoutedEventArgs e)
        {
            string filename = "rubikstat";
            int filenamenum = -1;
            foreach (string file in statsmanager.GetStatisticFiles(false))
            {
                if (file.StartsWith(filename))
                {
                    string s = file.Substring(filename.Length);
                    int i;
                    int.TryParse(s, out i);
                    if (i > filenamenum) filenamenum = i;
                }
            }
            filenamenum++;

            FileNameDialog d = new FileNameDialog(filename + filenamenum,"Pick a name","Please pick a name for the new Statistic file:");

            FileNamePick:
            if (!((bool)d.ShowDialog())) return;
            else
            {
                if (d.FileName == "" || d.FileName == " " || d.FileName == "  ")
                {
                    MessageBox.Show("Invalid file name. File name cannot be blank.", "Invalid name picked", MessageBoxButton.OK, MessageBoxImage.Warning);
                    goto FileNamePick;
                }
                try
                {
                    if (!statsmanager.CreateCurrentFile(d.FileName, false))
                    {
                        switch (MessageBox.Show("There already exists a file called " + d.FileName + ". Do you wish to overwrite it?", "File already exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No))
                        {
                            case MessageBoxResult.Yes: statsmanager.CreateCurrentFile(d.FileName, true); break;
                            case MessageBoxResult.Cancel: case MessageBoxResult.None: return;
                            case MessageBoxResult.No:
                                {
                                    d = new FileNameDialog(d.FileName, "Pick another name", "Please pick another name for the new Statistic file:");
                                    goto FileNamePick;
                                }
                        }
                    }

                    MessageBox.Show("Successfully created and selected a new Statistic file " + d.FileName + statsmanager.extension, "File creation successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Creation of the new file failed due to the following exception: " + ex.Message, "File creation error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            UpdateInfo();
        }

        private void OpenFile(object sender, ExecutedRoutedEventArgs e)
        {
            List<string> files = statsmanager.GetStatisticFiles(false);
            if (statsmanager.StatFileLoaded) files.Remove(statsmanager.CurrentFileName);
            files = new List<string>(files);

            if (files.Count > 0)
            {
                FilePickerDialog d = new FilePickerDialog(files, "Pick another statistic file", "Please pick a file from the list to select it as the current statistic file:");
                if (!((bool)d.ShowDialog())) return;
                else
                {
                    try
                    {
                        statsmanager.ChangeCurrentFile(d.SelectedFile);
                        MessageBox.Show("Succesfully changed current statistic file to " + d.SelectedFile + statsmanager.extension, "Statistic File swap succesful", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Changing the Statistic file failed due to following Exception: " + ex.Message, "Statistic File swap Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            else if (statsmanager.StatFileLoaded) MessageBox.Show("There isn't any other Statistic file to be changed to, please select Create new statistic file (Ctrl + N) to create one.", "No Statistic file to swap to", MessageBoxButton.OK, MessageBoxImage.Information);
            else MessageBox.Show("There is no Statistic file to be changed to, please select Create new statistic file (Ctrl + N) to create one.", "No Statistic file to swap to", MessageBoxButton.OK, MessageBoxImage.Warning);

            UpdateInfo();
        }

        private void SaveFile(object sender, ExecutedRoutedEventArgs e)
        {
            SaveStats(true);
        }

        private void ChangeFolder(object sender, ExecutedRoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select the new folder for the user data";
            fbd.RootFolder = Environment.SpecialFolder.MyDocuments;
            fbd.ShowNewFolderButton = true;

            if(fbd.ShowDialog() == WFDialogResult.OK) 
            {
                try
                {
                    statsmanager.ChangeUserDirectory(fbd.SelectedPath);
                    MessageBox.Show("Succesfully changed User data folder to: " + fbd.SelectedPath, "User data folder change Succesful", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Changing the User data folder failed due to following exception: " + ex.Message, "User data folder change Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                finally
                {
                    SaveConfig();
                }
            }

            UpdateInfo();
        }

        private void EditStats(object sender, ExecutedRoutedEventArgs e)
        {
            SaveStats(true);

            List<string> files = statsmanager.GetStatisticFiles(false);
            FilePickerDialog d = new FilePickerDialog(files,"Pick a file for editing","Please pick a file from the list to be edited:");
            if (!((bool)d.ShowDialog())) return;
            else
            {
                try
                {
                    if (!statsmanager.LaunchEditor(d.SelectedFile)) MessageBox.Show("RubikStatEditor.exe file seems to be missing. Try reinstalling the program.", "Editor failed to launch", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Editor failed to launch due to following exception: " + ex.Message, "Launch failure", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void OpenFolder(object sender, ExecutedRoutedEventArgs e) { Process.Start(statsmanager.DirPath); }

        private void DisplayHelp(object sender, ExecutedRoutedEventArgs e)
        {
            HelpWindow h = new HelpWindow(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/help/timerhelp"));
            if (h.ProperlyLoaded) h.Show();
            else h = null;
        }

        private void DisplayAbout(object sender, ExecutedRoutedEventArgs e)
        {
            HelpWindow h = new HelpWindow(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources/help/timerabout"));
            if (h.ProperlyLoaded) h.Show();
            else h = null;
        }

        private void Exit(object sender, ExecutedRoutedEventArgs e) { Close(); }

        private void GenerateScramble(object sender, ExecutedRoutedEventArgs e)
        {
            if (ScrambleLenghtTextBox.Text.Length > 0)
                Scramble = gen.Generate(Type, byte.Parse(ScrambleLenghtTextBox.Text));
        }

        private void ViewStats(object sender, ExecutedRoutedEventArgs e)
        {
            SaveStats(true);
            new itemViewDialog(statsmanager).ShowDialog();
        }
        #endregion

        private void ClosingWindow(object sender, CancelEventArgs e)
        {
            if (!skipquitconfirmation)
            {
                if (MessageBox.Show("Are you sure you want to quit?", "Exit confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            SaveStats(true);
            SaveConfig();
        }

        private void SaveStats(bool showmessage)
        {
            if (!DontSaveStats)
            {
                try
                {
                    statsmanager.SaveCurrentFile();
                }

                catch (Exception ex)
                {
                    if (showmessage) MessageBox.Show("DATA LOSS WARNING:\nAttempt to save the statistic file failed due to the following exception: " + ex.Message, "Statistic file save error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                            case 6: sw.WriteLine(App.configitems[i] + DontSaveStats); break;
                            case 7: sw.WriteLine(App.configitems[i] + statsmanager.DirPath); break;
                            case 8: sw.WriteLine(App.configitems[i] + statsmanager.CurrentFileName); break;
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