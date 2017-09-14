using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace RubikTimer
{
    class StatsManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void UpdateProperty(string propertyname) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname)); }

        #region properties
        private string _dirpath;
        public string DirPath { get { return _dirpath; } private set { _dirpath = value; UpdateProperty("DirPath"); } }
        public readonly string extension = ".stxt";
        public readonly string editorname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RubikStatEditor.exe");
        public string CurrentFileName { get; private set; }
        private string CurrentFile { get { return Path.Combine(DirPath, CurrentFileName + extension); } }

        private bool _statfileloaded;
        public bool StatFileLoaded { get { return _statfileloaded; } set { _statfileloaded = value; UpdateProperty("StatFileLoaded"); } }
        private bool dontSaveStats;

        private ObservableCollection<Statistic> _stats;
        public ObservableCollection<Statistic> Stats
        {
            get { return _stats; }
            private set
            {
                _stats = value;
                UpdateProperty("Stats");
                UpdateStats();
            }
        }
        #endregion
        #region statproperties
        private void ResetStats()
        {
            Last = new Statistic(new TimeSpan(0), "");
            Best = new TimeSpan(0);
            Worst = new TimeSpan(0);
            Average = new TimeSpan(0);
            Median = new TimeSpan(0);
            AvgBestFive = new TimeSpan(0);
            AvgLastFive = new TimeSpan(0);
            AvgBestTen = new TimeSpan(0);
            AvgLastTen = new TimeSpan(0);
            AvgBestThreeFive = new TimeSpan(0);
            AvgLastThreeFive = new TimeSpan(0);
            AvgBestTenTwelve = new TimeSpan(0);
            AvgLastTenTwelve = new TimeSpan(0);
        }

        private void UpdateStats()
        {

        }

        #region statpropertiesdefinitions
        public Statistic Last { get; private set; } 
        public TimeSpan Best { get; private set; }
        public TimeSpan Worst { get; private set; }
        public TimeSpan Average { get; private set; }
        public TimeSpan Median { get; private set; }
        public TimeSpan AvgLastFive { get; private set; }
        public TimeSpan AvgBestFive { get; private set; }
        public TimeSpan AvgLastTen { get; private set; }
        public TimeSpan AvgBestTen { get; private set; }
        public TimeSpan AvgLastThreeFive { get; private set; }
        public TimeSpan AvgBestThreeFive { get; private set; }
        public TimeSpan AvgLastTenTwelve { get; private set; }
        public TimeSpan AvgBestTenTwelve { get; private set; }
        #endregion
        #endregion

        public StatsManager(string dirpath, string currentfilename, bool dontSaveStats)
        {
            DirPath = dirpath;
            CurrentFileName = currentfilename;
            this.dontSaveStats = dontSaveStats;

            if (!dontSaveStats) StatFileLoaded = LoadCurrentFile();
            else
            {
                StatFileLoaded = false;
                ResetStats();
                Stats = new ObservableCollection<Statistic>();
            }
        }

        public void AddStatistic(Statistic statistic) { Stats.Add(statistic); }
        public void RemoveLastStatistic() { if (Stats.Count > 0) Stats.RemoveAt(-Stats.Count); }

        private bool LoadCurrentFile()
        {
            bool result = true;
            Stats = new ObservableCollection<Statistic>();
            ResetStats();

            if (Directory.Exists(DirPath))
            {
                try
                {
                    string[] temp = File.ReadAllLines(CurrentFile);
                    foreach (string s in temp)
                    {
                        if (s.StartsWith("_"))
                        {
                            string[] stat = s.Split('~');
                            long time = 0;
                            if (long.TryParse(stat[0].Remove(0, 1).Trim(), out time)) Stats.Add(new Statistic(new TimeSpan(time), stat.Length >= 2 ? stat[1] : ""));
                        }
                    }
                }

                catch
                {
                    result = false;
                }
            }

            else
            {
                Directory.CreateDirectory(DirPath);
                result = false;
            }

            return result;
        }

        public void SaveCurrentFile()
        {
            if (File.Exists(CurrentFile))
            {
                string[] lines = File.ReadAllLines(CurrentFile);
                List<string> newlines = new List<string>();
                int index = 0;

                foreach (string line in lines)
                {
                    if (line.StartsWith("_"))
                    {
                        if (index < Stats.Count)
                        {
                            string[] temp = line.Split('~');
                            string comment = "";
                            if (temp.Length >= 3)
                            {
                                for (int i = 2; i < temp.Length; i++)
                                {
                                    comment += "~";
                                    comment += temp[i];
                                }
                            }

                            newlines.Add("_" + Stats[index].SolveTime.Ticks.ToString() + "~" + Stats[index].Info + comment);
                        }
                        index++;
                    }

                    else
                    {
                        newlines.Add(line);
                    }
                }

                while (index < Stats.Count)
                {
                    newlines.Add("_" + Stats[index].SolveTime.Ticks.ToString() + "~" + Stats[index].Info);
                    index++;
                }

                File.WriteAllLines(CurrentFile, newlines);
            }
        }

        public void ChangeCurrentFile(string filename)
        {
            if (filename != CurrentFileName)
            {
                SaveCurrentFile();
                CurrentFileName = filename;
                StatFileLoaded = LoadCurrentFile();
            }
        }

        public bool CreateCurrentFile(string filename, bool overwrite)
        {
            if (File.Exists(Path.Combine(DirPath, filename + extension)) && !overwrite) return false;
            else
            {
                File.Create(Path.Combine(DirPath, filename + extension));
                ChangeCurrentFile(filename);
                return true;
            }
        }

        public void ChangeUserDirectory(string newpath)
        {
            SaveCurrentFile();

            foreach(string file in Directory.GetFiles(DirPath,"*",SearchOption.TopDirectoryOnly))
            {
                File.Move(file, Path.Combine(newpath, (new FileInfo(file).Name)));
            }

            if (Directory.GetDirectories(DirPath, "*", SearchOption.AllDirectories).Length == 0 && Directory.GetFiles(DirPath, "*", SearchOption.AllDirectories).Length == 0)
            {
                Directory.Delete(DirPath);
            }

            DirPath = newpath;

            LoadCurrentFile();
        }

        public List<string> GetStatisticFiles(bool getfullpath)
        {
            List<string> result = new List<string>();

            string[] files = Directory.GetFiles(DirPath, "*" + extension, SearchOption.TopDirectoryOnly);
            foreach(string file in files)
            {
                string cleanfile = file;
                if(!getfullpath)
                {
                    cleanfile = (new FileInfo(file)).Name;
                    cleanfile = cleanfile.Remove(cleanfile.Length - extension.Length);
                }

                result.Add(cleanfile);
            }

            return result;
        }

        public bool LaunchEditor(string filename)
        {
            bool result = true;

            if (File.Exists(editorname))
            {
                Process editor = new Process();
                editor.StartInfo = new ProcessStartInfo(editorname, Path.Combine(DirPath, filename + extension) + " TIMER");
                editor.Start();
            }
            else result = false;

            return result;
        }
    }
}
