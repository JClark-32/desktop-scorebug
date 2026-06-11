using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using String = System.String;
using System.Diagnostics;
using System.Windows.Threading;
using System.Security.Policy;
using System.Xml;

namespace Desktop_Scorebug_WPF
{
    public partial class DynamicScoreboardTest : Scoreboard
    {

        public int BugHeight;
        public int BugWidth;
        public VerticalAlignment VAlignment;
        public HorizontalAlignment HAlignment;

        string urlDate;
        string gameName;
        string league;
        XmlDocument ScoreBugConfig = new XmlDocument();
        
        private DispatcherTimer _timer;
        public DynamicScoreboardTest()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //base.OnContentRendered(e);
            string imageFileBase = "ActiveScorebugs/Football/Default/";
            ScoreBugConfig.Load("ActiveScorebugs/Football/Default/ScorebugConfig.xml");

            XmlNode dimensions = ScoreBugConfig.SelectSingleNode("ScorebugConfig/dimensions");

            BugHeight = int.Parse(dimensions.SelectSingleNode("height").InnerText);
            BugWidth = int.Parse(dimensions.SelectSingleNode("width").InnerText);
            VAlignment = VerticalAlignment.Top;
            HAlignment = HorizontalAlignment.Center;

            XmlNode layers = ScoreBugConfig.SelectSingleNode("ScorebugConfig/layers");

            foreach (XmlNode layer in layers.ChildNodes)
            {
                string type = layer.SelectSingleNode("type").InnerText;
                if (type == "image")
                {
                    string name = layer.SelectSingleNode("name").InnerText;
                    string image = layer.SelectSingleNode("image").InnerText;
                    string hAlignment = layer.SelectSingleNode("h-alignment").InnerText;
                    string vAlignment = layer.SelectSingleNode("v-alignment").InnerText;
                    int layerHeight = int.Parse(layer.SelectSingleNode("height").InnerText);
                    int layerWidth = int.Parse(layer.SelectSingleNode("width").InnerText);
                    string margin = layer.SelectSingleNode("margin").InnerText;
                    double opacity = double.Parse(layer.SelectSingleNode("opacity").InnerText);
                    bool colorTeam1 = bool.Parse(layer.SelectSingleNode("teamColor1").InnerText);
                    bool colorTeam2 = bool.Parse(layer.SelectSingleNode("teamColor2").InnerText);

                    Debug.WriteLine(imageFileBase + image);

                    BitmapImage layerImage = new BitmapImage();
                    layerImage.BeginInit();
                    layerImage.UriSource = new Uri(System.IO.Path.GetFullPath(imageFileBase + image),UriKind.Absolute);
                    layerImage.EndInit();

                    Image imageObject = new Image
                    {
                        Name = name,
                        Source = layerImage,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Height = layerHeight,
                        Width = layerWidth,
                        Opacity = opacity,

                    };
                    
                    RootGrid.Children.Add(imageObject);

                    if (colorTeam1)
                    {
                        var TeamColor1 = System.Drawing.ColorTranslator.FromHtml("#000DFF");
                        RecolorImageWithAlpha(imageObject, TeamColor1);
                    }
                    if (colorTeam2)
                    {
                        var TeamColor1 = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        RecolorImageWithAlpha(imageObject, TeamColor1);
                    }
                }

            }

        }

        protected override void OnClosed(EventArgs e)
        {
            //_cts.Cancel();
            base.OnClosed(e);
        }
    }
}
