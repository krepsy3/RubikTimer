using System;
using System.Collections.Generic;
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
            List<FileItem> items = new List<FileItem>();

            // DOPICI, POKUD NEKDO Z NAS COMMITNE SOUBOR, KTERY TEN DRUHY MA TAKY UPRAVENY ALE NE COMMITNUTY TAK TO DELA PICOVINY, TAKZE POKUD NEDELAS NEJAKE VELKE ZMENY A UPRAVUJES JENOM NAZVY PROMENNYCH TAK TO NECH BYT. TED JSEM DELAL CELE DOPOLEDNE NA EDITORU A KVULI TVE UPRAVE TO MUSIM ZNOVA NAPSAT

            return items;
        }
    }
}
