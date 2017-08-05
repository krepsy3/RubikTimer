using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using RubikTimer;

namespace RubikStatEditor
{
    public class FileItem
    {
        private Statistic statistic;

        private string lineText;
        private string _solveTime;
        private string _info;

        public bool IsStatistic { get; private set; }

        public string LineContent
        {
            get
            {
                return (IsStatistic) ? "Statistic" : lineText;
            }
            set
            {
                if (value.ToLower() != "statistic")
                {
                    if (IsStatistic)
                    {
                        lineText += value;
                        IsStatistic = false;
                    }
                    else
                        lineText = value;
                }
            }
        }

        public string SolveTime
        {
            get
            {
                return (IsStatistic) ? statistic.SolveTime.ToString() : _solveTime;
            }
            set
            {
                _solveTime = value;
            }
        }

        public string Info
        {
            get
            {
                return (IsStatistic) ? statistic.Info : _info;
            }
            set
            {
                _info = value;
            }
        }

        public string Comment { get; set; }

        public bool ChangeToStatOpt { get; private set; }

        public FileItem(Statistic statistic, string comment, string lineText)
        {
            if (statistic != null)
            {
                IsStatistic = true;
                this.statistic = statistic;
                Comment = comment;
                ChangeToStatOpt = false;
            }
            else
                ChangeToStatOpt = true;

            this.lineText = lineText;
        }
    }
}
