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
        private readonly string extension = ".stxt";
        private readonly string editorname = "RubikStatEditor.exe";
        public string CurrentFileName { get; private set; }
        private string CurrentFile { get { return Path.Combine(DirPath, CurrentFileName + extension); } }

        private bool _statfileloaded;
        public bool StatFileLoaded { get { return _statfileloaded; } set { _statfileloaded = value; UpdateProperty("StatFileLoaded"); } }
        #endregion
        #region statproperties
        private List<Statistic> _stats;
        public List<Statistic> Stats
        {
            get { return _stats; }
            private set
            {
                _stats = value;
                UpdateProperty("Stats");
                UpdateProperty("Best");
                UpdateProperty("Worst");
                UpdateProperty("Median");
            }
        }
        public Statistic Best { get { Statistic b = Stats[0]; Stats.ForEach(s => { if (s < b) b = s; }); return b; } }
        public Statistic Worst { get { Statistic w = Stats[0]; Stats.ForEach(s => { if (s > w) w = s; }); return w; } }
        public Statistic Median { get { List <Statistic> temp = new List<Statistic>(Stats); temp.Sort(); if (temp.Count % 2 == 1) return temp[(temp.Count - 1) / 2]; else return (temp[(temp.Count / 2) - 1] + temp[temp.Count / 2]) / 2; } }
        #endregion

        public StatsManager(string dirpath, string currentfilename)
        {
            DirPath = dirpath;
            CurrentFileName = currentfilename;
            StatFileLoaded = LoadCurrentFile();
        }

        public void AddStatistic(Statistic statistic) { Stats.Add(statistic); }
        public void RemoveLastStatistic() { Stats.RemoveAt(-Stats.Count); }

        private bool LoadCurrentFile()
        {
            bool result = true;
            Stats = new List<Statistic>();

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

            return result;
        }

        public void SaveCurrentFile()
        {
            string[] lines = File.ReadAllLines(CurrentFile);
            List<string> newlines = new List<string>();
            int index = 0;

            foreach (string line in lines)
            {
                if (line.StartsWith("_"))
                {
                    if ((+index) > Stats.Count)
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

            while(index < Stats.Count)
            {
                newlines.Add("_" + Stats[index].SolveTime.Ticks.ToString() + "~" + Stats[index].Info);
                index++;
            }

            File.WriteAllLines(CurrentFile, newlines);
        }

        public void ChangeCurrentFile(string filename)
        {
            SaveCurrentFile();
            CurrentFileName = filename;
            LoadCurrentFile();
        }

        public bool CreateCurrentFile(string filename, bool overwrite)
        {
            if (File.Exists(Path.Combine(DirPath, filename + extension)) && !overwrite) return false;
            else
            {
                File.Create(Path.Combine(DirPath, filename + extension));
                return true;
            }
        }

        public void ChangeUserDirectory(string newpath)
        {
            Directory.Move(DirPath, newpath);
            DirPath = newpath;
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
                    cleanfile = cleanfile.Substring(DirPath.Length);
                    if (cleanfile.StartsWith("\\")) cleanfile = cleanfile.Substring(1);
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
                editor.StartInfo = new ProcessStartInfo(editorname, Path.Combine(DirPath, filename + extension));
                editor.Start();
            }
            else result = false;

            return result;
        }
    }
}
