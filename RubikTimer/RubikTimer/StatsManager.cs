using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

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

        private List<Statistic> _stats;
        public List<Statistic> Stats
        {
            get { return _stats; }
            private set
            {
                _stats = value;
                UpdateProperty("Stats");
            }
        }
        #endregion
        #region statproperties
        public Statistic Last { get { return Stats[-Stats.Count]; } }

        public TimeSpan Best
        {
            get
            {
                Statistic b = Stats[0];
                Stats.ForEach(s => { if (s < b) b = s; });
                return b.SolveTime;
            }
        }

        public TimeSpan Worst
        {
            get
            {
                Statistic w = Stats[0];
                Stats.ForEach(s => { if (s > w) w = s; });
                return w.SolveTime;
            }
        }

        public TimeSpan Average
        {
            get
            {
                if (Stats.Count > 0)
                {
                    long result = 0;
                    Stats.ForEach(s => result += s.SolveTime.Ticks);
                    return new TimeSpan(result / Stats.Count);
                }

                else return new TimeSpan();
            }
        }

        public TimeSpan Median
        {
            get
            {
                if (Stats.Count > 0)
                {
                    List<Statistic> temp = new List<Statistic>(Stats);
                    temp.Sort();
                    if (temp.Count % 2 == 1) return temp[(temp.Count - 1) / 2].SolveTime;
                    else return ((temp[(temp.Count / 2) - 1] + temp[temp.Count / 2]) / 2).SolveTime;
                }

                else return new TimeSpan();
            }
        }

        public TimeSpan AverageLastFive
        {
            get
            {
                if (Stats.Count > 4)
                {
                    long result = 0;
                    for (int i = Stats.Count - 5; i < Stats.Count; i++) result += Stats[i].SolveTime.Ticks;
                    return new TimeSpan(result / 5);
                }

                else return new TimeSpan();
            }
        }

        public TimeSpan AverageLastTen
        {
            get
            {
                if (Stats.Count > 9)
                {
                    long result = 0;
                    for (int i = Stats.Count - 10; i < Stats.Count; i++) result += Stats[i].SolveTime.Ticks;
                    return new TimeSpan(result / 10);
                }

                else return new TimeSpan();
            }
        }

        public TimeSpan AverageLastThreeOfFive
        {
            get
            {
                if (Stats.Count > 5)
                {
                    List<Statistic> temp = new List<Statistic>();
                    for (int i = Stats.Count - 5; i < Stats.Count; i++) temp.Add(Stats[i]);
                    temp.RemoveAt(4);
                    temp.RemoveAt(0);
                    long result = 0;
                    temp.ForEach((s) => result += s.SolveTime.Ticks);
                    return new TimeSpan(result / 3);
                }

                else return new TimeSpan();
            }
        }

        public TimeSpan AverageLastTenOfTwelve
        {
            get
            {
                if (Stats.Count > 9)
                {
                    List<Statistic> temp = new List<Statistic>();
                    for (int i = Stats.Count - 12; i < Stats.Count; i++) temp.Add(Stats[i]);
                    temp.RemoveAt(11);
                    temp.RemoveAt(0);
                    long result = 0;
                    temp.ForEach((s) => result += s.SolveTime.Ticks);
                    return new TimeSpan(result / 10);
                }

                else return new TimeSpan();
            }
        }
        #endregion

        public StatsManager(string dirpath, string currentfilename)
        {
            DirPath = dirpath;
            CurrentFileName = currentfilename;
            StatFileLoaded = LoadCurrentFile();
        }

        public void AddStatistic(Statistic statistic) { Stats.Add(statistic); }
        public void RemoveLastStatistic() { if (Stats.Count > 0) Stats.RemoveAt(-Stats.Count); }

        private bool LoadCurrentFile()
        {
            bool result = true;
            Stats = new List<Statistic>();

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
                LoadCurrentFile();
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
