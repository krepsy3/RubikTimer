using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RubikTimer;

namespace RubikStatEditor
{
    class FileReader
    {
        public List<FileItem> LoadFileItemsFromFile(string path)
        {
            List<FileItem> fileItems = new List<FileItem>();

            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                Statistic statistic = null;
                string comment = "";

                if (line.StartsWith("_"))
                {
                    // it is a statistic
                    string[] data = line.Split('~');

                    long timeInTicks = 0;
                    if (long.TryParse(data[0].Substring(1), out timeInTicks))
                    {
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
    }
}
