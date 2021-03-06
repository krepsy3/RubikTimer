﻿using System;
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

        public enum LineContents { Statistic, TextComment, InvalidStatisticLine }
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

        public string Comment { get; set; }

        public bool ChangeToStatOpt
        {
            get
            {
                return _lineContent == LineContents.InvalidStatisticLine;
            }
        }

        #region ForSaving
        public LineContents ItemContent
        {
            get
            {
                return _lineContent;
            }
        }

        public Statistic Statistic
        {
            get
            {
                return statistic;
            }
        }
        #endregion

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
            }
            else
                _lineContent = LineContents.TextComment;
        }

        private void UpdateAllProperties()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            // String.Empty is doing the trick with updating all properties
        }

        public void ConvertToTextComment()
        {
            while (Comment.StartsWith("_"))
                Comment = Comment.Substring(1);
            _lineContent = LineContents.TextComment;
            UpdateAllProperties();
        }
    }
}
