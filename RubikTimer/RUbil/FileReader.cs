using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using RubikTimer;

namespace RUbil
{
    class FileReader
    {
        public List<FileItem> LoadFileItemsFromFile(string path)
        {
            List<FileItem> fileItems = new List<FileItem>();

            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                if (line.Trim().Length == 0) // empty line; skip loop
                    continue;

                string lineText = "";
                Statistic statistic = null;
                string comment = "";

                if (line.StartsWith("_"))
                {
                    // it is a statistic
                    string[] data = line.Split('~');

                    long timeInTicks = 0;
                    long.TryParse(data[0].Substring(1), out timeInTicks);

                    statistic = new Statistic(TimeSpan.FromTicks(timeInTicks), (data.Length >= 2) ? data[1] : "");
                    comment = (data.Length == 3) ? data[2] : "";
                }

                lineText = line;

                fileItems.Add(new FileItem(statistic, comment, lineText));
            }

            return fileItems;
        }
    }
}
