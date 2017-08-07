using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RubikTimer;

namespace RubikStatEditor
{
    class FileManager
    {
        private string filePath;

        public List<FileItem> LoadFileItemsFromFile(string path)
        {
            filePath = path;

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
                        comment = line; // it's an invalid line that looks like statistic
                }
                else
                    comment = line;

                fileItems.Add(new FileItem(statistic, comment));
            }

            return fileItems;
        }

        public void SaveFileItems(List<FileItem> fileItems)
        {

        }

        public void SaveFileItemsToFile(List<FileItem> fileItems, string path)
        {

        }
    }
}
