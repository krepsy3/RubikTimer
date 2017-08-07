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
        private LineContents _lineContent;
        private string _comment;

        enum LineContents { Statistic, TextComment, InvalidStatisticLine }
        private readonly string[] lineContentsRep = new string[] { "Statistic", "Text Comment", "Invalid Statistic Line"};

        public event PropertyChangedEventHandler PropertyChanged;

        public string LineContent
        {
            get
            {
                return lineContentsRep[(int)_lineContent];
            }
        }

        public string SolveTime
        {
            get
            {
                return (_lineContent == LineContents.Statistic) ? statistic.SolveTime.ToString() : "";
            }
        }

        public string Info
        {
            get
            {
                return (_lineContent == LineContents.Statistic) ? statistic.Info : "";
            }
            set
            {
                statistic.Info = value;
            }
        }

        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value;

                if (value.StartsWith("_"))
                {
                    // converting to InvalidStatisticLine
                    _lineContent = LineContents.InvalidStatisticLine;
                    UpdateAllProperties();
                }
            }
        }

        public bool ChangeToStatOpt
        {
            get
            {
                return _lineContent == LineContents.InvalidStatisticLine;
            }
        }

        public FileItem(Statistic statistic, string comment)
        {
            Comment = comment;

            if (statistic != null)
            {
                this.statistic = statistic;
                _lineContent = LineContents.Statistic;
            }
            else if (comment.StartsWith("_"))
            {
                _lineContent = LineContents.InvalidStatisticLine;
                Comment = Comment.Substring(1); // remove '_' from start of comment
            }
            else
                _lineContent = LineContents.TextComment;
        }

        private void UpdateAllProperties()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(String.Empty));
            // String.Empty is doing the trick with updating all properties
        }

        public void ConvertToTextComment()
        {
            Comment = Comment.Remove(0, 1); // remove "_" at position 0 in the string
            _lineContent = LineContents.TextComment;
            UpdateAllProperties();
        }
    }
}
