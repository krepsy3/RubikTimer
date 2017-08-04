using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;

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

        }

        public void ChangeCurrentFile(string filename)
        {
            SaveCurrentFile();
            CurrentFileName = filename;
            LoadCurrentFile();
        }
    }
}
