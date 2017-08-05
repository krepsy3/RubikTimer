using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using RubikTimer;

namespace RUbil
{
    public class FileItem
    {
        private bool isStatistic;
        private Statistic statistic;

        private string lineText;
        private string _solveTime;
        private string _info;

        public string LineContent
        {
            get
            {
                return (isStatistic) ? "Statistic" : lineText;
            }
            set
            {
                if (value.ToLower() != "statistic")
                {
                    if (isStatistic)
                    {
                        lineText += value;
                        isStatistic = false;
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
                return (isStatistic) ? statistic.SolveTime.ToString() : _solveTime;
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
                return (isStatistic) ? statistic.Info : _info;
            }
            set
            {
                _info = value;
            }
        }

        public string Comment { get; set; }

        public FileItem(Statistic statistic, string comment, string lineText)
        {
            if (statistic != null)
            {
                isStatistic = true;
                this.statistic = statistic;
                Comment = comment;
            }

            this.lineText = lineText;
        }
    }
}
