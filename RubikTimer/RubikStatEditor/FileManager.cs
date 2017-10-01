using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using RubikTimer;

namespace RubikStatEditor
{
    class FileManager
    {
        private string filePath;
        private string dirPath;
        private string extension = ".stxt";

        public List<FileItem> LoadFileItemsFromFile(string path)
        {
            filePath = path.Replace(extension, "");
            dirPath = (new FileInfo(path)).DirectoryName;

            List<FileItem> fileItems = new List<FileItem>();
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                Statistic statistic = null;
                string comment = "";

                if (line.StartsWith("_"))
                {
                    string[] data = line.Split('~');

                    long timeInTicks = 0;
                    if (long.TryParse(data[0].Substring(1), out timeInTicks))
                    {
                        // it is a statistic
                        statistic = new Statistic(TimeSpan.FromTicks(timeInTicks), (data.Length >= 2) ? data[1] : "");
                        comment = (data.Length == 3) ? data[2] : "";
                    }
                    else
                        comment = line;
                }
                else
                    comment = line;

                fileItems.Add(new FileItem(statistic, comment));
            }

            return fileItems;
        }

        public void SaveFileItems(List<FileItem> fileItems)
        {
            Save(fileItems, filePath);
        }

        public bool SaveFileItemsToFile(List<FileItem> fileItems, string filename, bool overwrite)
        {
            if (!overwrite && File.Exists(Path.Combine(dirPath, filename + extension))) return false;
            Save(fileItems, Path.Combine(dirPath, filename));
            return true;
        }

        private void Save(List<FileItem> fileItems, string path)
        {
            List<string> lines = new List<string>();

            foreach (FileItem item in fileItems)
            {
                string line = "";

                if (item.ItemContent == FileItem.LineContents.Statistic)
                {
                    line = "_" + item.Statistic.SolveTime.Ticks;
                    line += (item.Info.Length > 0) ? "~" + item.Info : (item.Comment.Length > 0) ? "~" : "";
                    line += (item.Comment.Length > 0) ? "~" + item.Comment : "";
                }
                else if (item.ItemContent == FileItem.LineContents.TextComment)
                {
                    line = item.Info + "_" + item.Comment;
                    while (line.StartsWith("_")) line = line.Substring(1);
                }
                else
                {
                    line = "_" + item.Comment;
                }

                lines.Add(line);
            }

            File.WriteAllLines(path + extension, lines);
        }

        public List<string> GetStatisticFiles()
        {
            List<string> result = new List<string>();

            string[] files = Directory.GetFiles(dirPath, "*" + extension, SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string cleanfile = file;
                cleanfile = (new FileInfo(file)).Name;
                cleanfile = cleanfile.Remove(cleanfile.Length - extension.Length);
                result.Add(cleanfile);
            }

            return result;
        }
    }
}
