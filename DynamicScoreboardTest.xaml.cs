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
using System.Windows.Controls.Primitives;

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

        //Text box inputs
        string team1name = "blu";
        string team2name = "red";
        int team1score = 0;
        int team2score = 0;
        string gameTime = "12:00";
        string downs = "2nd and 1";
        string quarter = "4th";
        //End inputs

        //Text box names
        string team1NameBox;
        string team2NameBox;
        string team1ScoreBox;
        string team2ScoreBox;
        string gameTimeBox;
        string downsBox;
        string quarterBox;
        //End names

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

            String _VAlign = dimensions.SelectSingleNode("v-alignment").InnerText;

            if (_VAlign == "top")
            {
                VAlignment = VerticalAlignment.Top;
            }
            if (_VAlign == "bottom") {
                VAlignment = VerticalAlignment.Bottom;
            }
            else
            {
                VAlignment = VerticalAlignment.Center;
            }

            String _HAlign = dimensions.SelectSingleNode("h-alignment").InnerText;

            if (_HAlign == "left")
            {
                HAlignment = HorizontalAlignment.Left;
            }
            if (_HAlign == "right")
            {
                HAlignment = HorizontalAlignment.Right;
            }
            else
            {
                HAlignment = HorizontalAlignment.Center;
            }
            VerticalAlignment = VAlignment;
            HorizontalAlignment = HAlignment;
            Height = BugHeight;
            Width = BugWidth;

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
                if (type == "text")
                {
                    string content = layer.SelectSingleNode("content").InnerText;
                    string name = layer.SelectSingleNode("name").InnerText;
                    int team = int.Parse(layer.SelectSingleNode("team").InnerText);
                    string hAlignment = layer.SelectSingleNode("h-alignment").InnerText;
                    string vAlignment = layer.SelectSingleNode("v-alignment").InnerText;
                    int layerHeight = int.Parse(layer.SelectSingleNode("height").InnerText);
                    int layerWidth = int.Parse(layer.SelectSingleNode("width").InnerText);
                    string margin = layer.SelectSingleNode("margin").InnerText;
                    double opacity = double.Parse(layer.SelectSingleNode("opacity").InnerText);
                    bool colorTeam1 = bool.Parse(layer.SelectSingleNode("teamColor1").InnerText);
                    bool colorTeam2 = bool.Parse(layer.SelectSingleNode("teamColor2").InnerText);
                    string boxText = "";

                    string[] marginParts = margin.Split(',');

                    Thickness marginThickness = new Thickness(
                        int.Parse(marginParts[0]),
                        int.Parse(marginParts[1]),
                        int.Parse(marginParts[2]),
                        int.Parse(marginParts[3]));

                    if (content == "name")
                    {
                        if (team == 1)
                        {
                            boxText = team1name;
                            team1NameBox = name;
                        }
                        else
                        {
                            boxText = team2name;
                            team2NameBox = name;
                        }
                    }
                    if (content == "score")
                    {
                        if (team == 1)
                        {
                            boxText = team1score.ToString();
                            team1ScoreBox = name;
                        }
                        else
                        {
                            boxText = team2score.ToString();
                            team2ScoreBox = name;
                        }
                    }
                    if (content == "clock")
                    {
                        boxText = gameTime;
                        gameTimeBox = name;
                    }
                    if (content == "downs")
                    {
                        boxText = downs;
                        downsBox = name;
                    }
                    if (content == "quarter")
                    {
                        boxText = quarter;
                        quarterBox = name;
                    }

                    TextBox textBox = new TextBox
                    {
                        Name = name,
                        Width = layerWidth,
                        Height = layerHeight,
                        FontSize = layerHeight * 0.9,
                        TextAlignment = TextAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        Margin = marginThickness,
                        Foreground = Brushes.White,
                        FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Font/#Bebas Neue"),
                        Text = boxText,
                    };

                    AddTextOutline(textBox, Colors.Black, 10.0);

                    RootGrid.Children.Add(textBox);
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
