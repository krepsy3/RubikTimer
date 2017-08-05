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
                    UpdateAllProperties();
                }
            }
        }

        public string SolveTime
        {
            get
            {
                return (IsStatistic) ? statistic.SolveTime.Ticks.ToString() : "";
            }
            set
            {
                if (IsStatistic)
                {
                    long ticks = 0;
                    long.TryParse(value, out ticks);
                    statistic.SolveTime = TimeSpan.FromTicks(ticks);
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(String.Empty));
                // String.Empty is doing the trick with updating all properties
            }
        }
    }
}
