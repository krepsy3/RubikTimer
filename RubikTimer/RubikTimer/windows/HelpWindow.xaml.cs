using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;

namespace RubikTimer
{
    public partial class HelpWindow : Window
    {
        public bool ProperlyLoaded { get; private set; }

        private string root;
        private List<string> ImageSourceNames;
        private List<List<string>> ImageNames;
        private Dictionary<string, string> EmbeddedImages;
        public Dictionary<string, string> Chapters { get; private set; }

        public HelpWindow(string rootpath)
        {
            ProperlyLoaded = true;
            root = rootpath;
            if (!File.Exists(Path.Combine(rootpath, "text.xml"))) Close();
            ImageSourceNames = new List<string>();
            ImageNames = new List<List<string>>();
            EmbeddedImages = new Dictionary<string, string>();
            Chapters = new Dictionary<string, string>();
            InitializeComponent();

            ImageSourceNames = new List<string>();
            ImageNames = new List<List<string>>();
            EmbeddedImages = new Dictionary<string, string>();
            Chapters = new Dictionary<string, string>();
            FlowDocument flwdoc;
            try
            {
                string doc = LoadXml();
                flwdoc = (FlowDocument)XamlReader.Load(new XmlTextReader(new StringReader(doc)));
                mainFlowDocViewer.Document = flwdoc;
            }
            catch { ProperlyLoaded = false; }

            if (ProperlyLoaded)
            {
                AssociateImages();
                if (Chapters.Count < 2) chapterSelectionWrapPanel.Visibility = Visibility.Collapsed;
                else chapterSelectComboBox.ItemsSource = Chapters;
            }
        }

        private string LoadXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(root, "text.xml"));
            XmlNode helpwincontentNode = doc.DocumentElement;
            switch (((XmlElement)helpwincontentNode).GetAttribute("Type"))
            {
                case "Help": Title = "Help"; Icon = (new IconBitmapDecoder(new Uri("pack://application:,,,/resources/help.ico", UriKind.RelativeOrAbsolute),BitmapCreateOptions.None, BitmapCacheOption.Default)).Frames[0]; break;
                case "About": Title = "About"; Icon = (new IconBitmapDecoder(new Uri("pack://application:,,,/resources/about.ico", UriKind.RelativeOrAbsolute), BitmapCreateOptions.None, BitmapCacheOption.Default)).Frames[0]; break;
                default: Title = "undefined_help_text"; Icon = (new IconBitmapDecoder(new Uri("pack://application:,,,/resources/main.ico", UriKind.RelativeOrAbsolute), BitmapCreateOptions.None, BitmapCacheOption.Default)).Frames[0]; break;
            }

            try { ResizeMode = bool.Parse(((XmlElement)helpwincontentNode).GetAttribute("Resize")) ? ResizeMode.CanResize : ResizeMode.CanMinimize; }
            catch { ResizeMode = ResizeMode.CanMinimize; }

            string flwdoc = "";
            foreach (XmlNode currentNode in helpwincontentNode)
            {
                switch (currentNode.Name)
                {
                    case "Document":
                        {
                            flwdoc = ((XmlElement)currentNode).InnerText;
                            break;
                        }
                        
                    case "Pictures":
                        {
                            foreach (XmlNode xn in currentNode.ChildNodes)
                            {
                                if (xn.Name == "Picture")
                                {
                                    XmlElement xe = (XmlElement)xn;
                                    string name = xe.GetAttribute("Name");
                                    if (name == "") break;
                                    else
                                    {
                                        List<string> names = new List<string>();
                                        ImageSourceNames.Add(name);
                                        foreach (XmlNode img in xe.ChildNodes)
                                        {
                                            if (img.Name == "Image") names.Add(((XmlElement)img).InnerText);
                                        }

                                        ImageNames.Add(names);
                                    }
                                }

                                else if (xn.Name == "EmbeddedPicture")
                                {
                                    string n = ((XmlElement)xn).GetAttribute("Name");
                                    if (n == "") break;
                                    else
                                    {
                                        EmbeddedImages.Add(n, ((XmlElement)xn).InnerText);
                                    }
                                }
                            }
                            break;
                        }

                    case "Chapters":
                        {
                            foreach (XmlNode xn in currentNode.ChildNodes)
                            {
                                if (xn.Name == "Chapter")
                                {
                                    XmlElement xe = (XmlElement)xn;
                                    string name = xe.GetAttribute("Name");
                                    if (name.Length > 0) Chapters.Add(name, (xe.InnerText.Length > 0) ? xe.InnerText : name);
                                }
                            }

                            break;
                        }

                     case "Dimensions":
                        {
                            foreach (XmlNode xn in currentNode.ChildNodes)
                            {
                                if (xn.Name == "Dimension")
                                {
                                    XmlElement xe = ((XmlElement)xn);
                                    switch (xe.GetAttribute("Name"))
                                    {
                                        case "Width": try { Width = double.Parse(xe.InnerText); } catch { } break;
                                        case "Height": try { Height = double.Parse(xe.InnerText); } catch { } break;
                                        case "MinWidth": try { MinWidth = double.Parse(xe.InnerText); } catch { } break;
                                        case "MinHeight": try { MinHeight = double.Parse(xe.InnerText); } catch { } break;
                                        case "MaxWidth": try { MaxWidth = double.Parse(xe.InnerText); } catch { } break;
                                        case "MaxHeight": try { MaxHeight = double.Parse(xe.InnerText); } catch { } break;
                                    }
                                }
                            }

                            break;
                        }
                }
            }

            return flwdoc;
        }

        private void AssociateImages()
        {
            Image[] imgs = mainFlowDocViewer.Document.Blocks.SelectMany(b => FindImages(b)).ToArray();

            for (int i = 0; i < imgs.ToList().Count; i++)
            {
                ImageSource src;
                try { src = new BitmapImage(new Uri(Path.Combine(root, ImageSourceNames[i]))); }
                catch { src = new BitmapImage(); }

                foreach (Image img in imgs)
                {
                    try { foreach (string s in ImageNames[i]) if (img.Name == s) img.Source = src; }
                    catch { }

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
                var result = new List<Image>();

                result.AddRange(((Paragraph)b).Inlines
                    .OfType<InlineUIContainer>()
                    .Where(x => x.Child is Image)
                    .Select(x => x.Child as Image));

                var i = ((Paragraph)b).Inlines
                    .OfType<InlineUIContainer>()
                    .Where(x => x.Child is Panel)
                    .Select(x => x.Child as Panel);

                foreach (UIElementCollection uec in i.Select(x => x.Children)) foreach (UIElement ue in uec) if (ue is Image) result.Add(ue as Image);

                return result;
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

        private void DisplayChapter(object sender, SelectionChangedEventArgs e)
        {
            if (chapterSelectComboBox.SelectedIndex >= 0)
            {
                string chapter = Chapters.Keys.ToList()[chapterSelectComboBox.SelectedIndex];
                foreach (Block b in mainFlowDocViewer.Document.Blocks)
                {
                    if (b is Paragraph)
                    {
                        if (((Paragraph)b).Name == chapter)
                        {
                            b.BringIntoView();
                            break;
                        }
                    }
                }
                chapterSelectComboBox.SelectedIndex = -1;
            }
        }
    }
}
