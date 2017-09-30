using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Collections.ObjectModel;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> ImageSourceNames;
        private List<List<string>> ImageNames;
        private Dictionary<string, string> EmbeddedImages;
        private Dictionary<string, string> Chapters;
        private Dictionary<string, uint> Dimensions;

        public MainWindow()
        {
            InitializeComponent();
            viewer.DataContext = this;
            ImageSourceNames = new List<string>(new string[] { "appwindow.png", "statview.png", "deletestat.png" });
            ImageNames = new List<List<string>>(new List<string>[] { new List<string>() { "Image1" }, new List<string>() { "Image2" }, new List<string>() { "Image3" } });
            EmbeddedImages = new Dictionary<string, string>();
            EmbeddedImages.Add("EmbImage1","resources/timer.png");
            EmbeddedImages.Add("EmbImage2", "resources/file.png");

            Chapters = new Dictionary<string, string>();
            Chapters.Add("app", "Application overview");
            Chapters.Add("controls", "Controls overview");
            Chapters.Add("timer", "Timer function");
            Chapters.Add("scrambler", "Scramble generator");
            Chapters.Add("stats", "Statistics");
            Chapters.Add("statsview", "Statistics - view");
            Chapters.Add("statssave", "Statistics - saving");
            Chapters.Add("other", "Other");

            Dimensions = new Dictionary<string, uint>();
            Dimensions.Add("MinWidth", 500);
            Dimensions.Add("MinHeight", 550);

            AssociateImages();
        }

        private void AssociateImages()
        {
            Image[] imgs = viewer.Document.Blocks.SelectMany(b => FindImages(b)).ToArray();

            for (int i = 0; i < ImageSourceNames.Count; i++)
            {
                ImageSource src;
                try { src = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,ImageSourceNames[i]))); }
                catch { src = new BitmapImage(); }

                foreach (Image img in imgs)
                {
                    foreach (string s in ImageNames[i]) if (img.Name == s) img.Source = src;
                    try
                    {
                        string tmp = EmbeddedImages[img.Name];
                        img.Source = new BitmapImage(new Uri("pack://application:,,,/" + tmp));
                    }
                    catch { }
                }
            }
        }

        private IEnumerable<Image> FindImages(Block b)
        {
            if (b is Table)
            {
                return ((Table)b).RowGroups
                    .SelectMany(x => x.Rows)
                    .SelectMany(x => x.Cells)
                    .SelectMany(x => x.Blocks)
                    .SelectMany(innerBlock => FindImages(innerBlock));
            }

            else if (b is Paragraph)
            {
                return ((Paragraph)b).Inlines
                    .OfType<InlineUIContainer>()
                    .Where(x => x.Child is Image)
                    .Select(x => x.Child as Image);
            }

            else if (b is BlockUIContainer)
            {
                Image i = ((BlockUIContainer)b).Child as Image;
                return i == null ? new List<Image>() : new List<Image>(new Image[] { i });
            }

            else if (b is List)
            {
                return ((List)b).ListItems.SelectMany(listItem => listItem.Blocks.SelectMany(innerBlock => FindImages(innerBlock)));
            }

            else throw new InvalidOperationException("Unknown block type: " + b.GetType());
        }

        private void SaveXml(object sender, RoutedEventArgs e)
        {
            Image[] imgs = viewer.Document.Blocks.SelectMany(b => FindImages(b)).ToArray();
            foreach (Image i in imgs) i.Source = null;

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.CheckCharacters = true;
            XmlWriter xw = XmlWriter.Create("text.xml", xws);
            xw.WriteStartDocument();
            xw.WriteStartElement("HelpWindowContents");
            xw.WriteAttributeString("Type", "Help");
            xw.WriteAttributeString("Resize", "True");

            xw.WriteStartElement("Document");

            xw.WriteString(XamlWriter.Save(viewer.Document));
            xw.WriteEndElement();

            if (ImageSourceNames.Count > 0 && ImageNames.Count == ImageSourceNames.Count)
            {
                xw.WriteStartElement("Pictures");
                for (int i = 0; i < ImageSourceNames.Count; i++)
                {
                    xw.WriteStartElement("Picture");
                    xw.WriteAttributeString("Name", ImageSourceNames[i]);
                    foreach (string s in ImageNames[i]) xw.WriteElementString("Image",s);
                    xw.WriteEndElement();
                }

                for (int i = 0; i < EmbeddedImages.Count; i++)
                {
                    xw.WriteStartElement("EmbeddedPicture");
                    xw.WriteAttributeString("Name", EmbeddedImages.Keys.ToList()[i]);
                    xw.WriteString(EmbeddedImages.Values.ToList()[i]);
                    xw.WriteEndElement();
                }
                xw.WriteEndElement();
            }

            if (Chapters.Count > 0)
            {
                xw.WriteStartElement("Chapters");
                for(int i = 0; i < Chapters.Count; i++)
                {
                    xw.WriteStartElement("Chapter");
                    xw.WriteAttributeString("Name", Chapters.Keys.ToList()[i]);
                    xw.WriteString(Chapters.Values.ToList()[i]);
                    xw.WriteEndElement();
                }
                xw.WriteEndElement();
            }

            xw.WriteStartElement("Dimensions");
            for (int i = 0; i < Dimensions.Count; i++)
            {
                xw.WriteStartElement("Dimension");
                xw.WriteAttributeString("Name", Dimensions.Keys.ToList()[i]);
                xw.WriteString(Dimensions.Values.ToList()[i].ToString());
                xw.WriteEndElement();
            }

            xw.WriteEndDocument();
            xw.Flush();

            AssociateImages();
        }
    }
}