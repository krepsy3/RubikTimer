using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using RubikTimer;

namespace RubikStatEditor
{
    public class FileItem : INotifyPropertyChanged
    {
        private Statistic statistic;

        private string _lineText;
        private string _solveTime;
        private string _comment;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsStatistic { get; private set; }

        public string LineContent
        {
            get
            {
                return (IsStatistic) ? "Statistic" : _lineText;
            }
            set
            {
                if (value.ToLower() != "statistic")
                {
                    if (IsStatistic)
                    {
                        // converting to simple line in file
                        _lineText += value;
                        IsStatistic = false;
                        UpdateAllProperties();
                    }
                    else
                        _lineText = value;
                }
                else if (value == "Statistic")
                {
                    // converting to Statistic
                    IsStatistic = true;

                    if (statistic == null)
                    {
                        // creating new instance
                        statistic = new Statistic(TimeSpan.Zero, "");
                        Comment = _lineText;
                    }

                    UpdateAllProperties();
                }
            }
        }

        public string SolveTime
        {
            get
            {
                return (IsStatistic) ? statistic.SolveTime.ToString() : "";
            }
            set
            {
                if (IsStatistic)
                {
                    TimeSpan solveTime;
                    if (TimeSpan.TryParse(value, out solveTime))
                    {
                        statistic.SolveTime = solveTime;
                    }
                    else
                    {
                        MessageBox.Show("Invalid format {hh:mm:ss.FFFFFFF}.\nAn example: 1 hour, 23 minutes, 45 seconds and 6789012 ticks would look like 01:23:45.6789012", "Invalid format", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public string Info
        {
            get
            {
                return (IsStatistic) ? statistic.Info : "";
            }
            set
            {
                if (IsStatistic)
                    statistic.Info = value;
            }
        }

        public string Comment
        {
            get
            {
                return (IsStatistic) ? _comment : "";
            }
            set
            {
                if (IsStatistic)
                    _comment = value;
            }
        }

        public bool ChangeToStatOpt
        {
            get
            {
                return !IsStatistic;
            }
        }

        public FileItem(Statistic statistic, string comment, string lineText)
        {
            if (statistic != null)
            {
                IsStatistic = true;
                this.statistic = statistic;
                Comment = comment;
            }

            this._lineText = lineText;
        }

        private void UpdateAllProperties()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(String.Empty));
        }
    }
}
