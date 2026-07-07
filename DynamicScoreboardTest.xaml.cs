using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Windows.Controls.Image;
using String = System.String;

namespace Desktop_Scorebug_WPF
{
    public partial class DynamicScoreboardTest : Scoreboard
    {
        bool debugLicencedLogos = true;

        public int BugHeight;
        public int BugWidth;
        public VerticalAlignment VAlignment;
        public HorizontalAlignment HAlignment;

        string urlDate;
        string gameName;
        string league;

        //Debug text box inputs
        int debugTeam1score = 0;
        int debugTeam2score = 0;
        string debugGameTime = "00:00";
        string debugDowns = "1st and 10";
        string debugQuarter = "1st";
        //End inputs

        //Text boxes
        TextBox team1NameBox;
        TextBox team2NameBox;
        TextBox team1ScoreBox;
        TextBox team2ScoreBox;
        TextBox gameTimeBox;
        TextBox downsBox;
        TextBox quarterBox;
        //End text boxes

        //Possession arrow
        Image possessionArrow;
        bool arrowRightDefault;
        Thickness arrowRightMargin;
        Thickness arrowLeftMargin;
        //End posession arrows

        //Timeouts
        int team1TimeOuts;
        int team2TimeOuts;

        Image team1TimeOut1;
        Image team1TimeOut2;
        Image team1TimeOut3;

        Image team2TimeOut1;
        Image team2TimeOut2;
        Image team2TimeOut3;
        //End timeouts

        XmlDocument ScoreBugConfig = new XmlDocument();
        
        private DispatcherTimer _timer;
        public DynamicScoreboardTest(string league, string gameName, string urlDate)
        {
            this.urlDate = urlDate;
            this.gameName = gameName;
            this.league = league;
            InitializeComponent();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            updateScoreboard();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            buildScorebug();
            //base.OnContentRendered(e);
            
        }

        private async Task buildScorebug()
        {
            
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
            if (_VAlign == "bottom")
            {
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

            JArray events = await getEvents(urlDate, league);

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

                    string[] marginParts = margin.Split(',');

                    Thickness marginThickness = new Thickness(
                        int.Parse(marginParts[0]),
                        int.Parse(marginParts[1]),
                        int.Parse(marginParts[2]),
                        int.Parse(marginParts[3]));

                    Debug.WriteLine(imageFileBase + image);

                    BitmapImage layerImage = new BitmapImage();
                    layerImage.BeginInit();
                    layerImage.UriSource = new Uri(System.IO.Path.GetFullPath(imageFileBase + image), UriKind.Absolute);
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
                        Margin = marginThickness
                    };

                    RootGrid.Children.Add(imageObject);

                    if (colorTeam1)
                    {   
                        var TeamColor1 = System.Drawing.ColorTranslator.FromHtml(getTeamColor(gameName, 1, events));
                        RecolorImageWithAlpha(imageObject, TeamColor1);
                    }
                    if (colorTeam2)
                    {
                        var TeamColor1 = System.Drawing.ColorTranslator.FromHtml(getTeamColor(gameName, 0, events));
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

                    if (content == "name")
                    {
                        if (team == 1)
                        {
                            var teamAbr = getAbbreviation(gameName, 1, events);
                            textBox.Text = teamAbr;
                            team1NameBox = textBox;
                        }
                        else
                        { 
                            var teamAbr = getAbbreviation(gameName, 0, events);
                            textBox.Text = teamAbr;
                            team2NameBox = textBox;
                        }
                    }
                    if (content == "score")
                    {
                        if (team == 1)
                        {
                            textBox.Text = getScore(gameName, 1, events);
                            team1ScoreBox = textBox;
                        }
                        else
                        {
                            textBox.Text = getScore(gameName, 0, events);
                            team2ScoreBox = textBox;
                        }
                    }
                    if (content == "clock")
                    {
                        textBox.Text = getClock(gameName, events);
                        gameTimeBox = textBox;
                    }
                    if (content == "downs")
                    {
                        textBox.Text = getDownDistance(gameName, events);
                        downsBox = textBox;
                    }
                    if (content == "quarter")
                    {
                        textBox.Text = await getPeriod(gameName, events);
                        quarterBox = textBox;
                    }



                    AddTextOutline(textBox, Colors.Black, 10.0);

                    RootGrid.Children.Add(textBox);
                }
                if (type == "possession")
                {
                    string name = layer.SelectSingleNode("name").InnerText;
                    string image = layer.SelectSingleNode("image").InnerText;
                    string hAlignment = layer.SelectSingleNode("h-alignment").InnerText;
                    string vAlignment = layer.SelectSingleNode("v-alignment").InnerText;
                    string startOrientation = layer.SelectSingleNode("startOrientation").InnerText;
                    int layerHeight = int.Parse(layer.SelectSingleNode("height").InnerText);
                    int layerWidth = int.Parse(layer.SelectSingleNode("width").InnerText);
                    string leftMargin = layer.SelectSingleNode("leftMargin").InnerText;
                    string rightMargin = layer.SelectSingleNode("rightMargin").InnerText;
                    double opacity = double.Parse(layer.SelectSingleNode("opacity").InnerText);
                    bool colorTeam1 = bool.Parse(layer.SelectSingleNode("teamColor1").InnerText);
                    bool colorTeam2 = bool.Parse(layer.SelectSingleNode("teamColor2").InnerText);

                    string[] marginPartsR = rightMargin.Split(',');

                    Thickness rightThickness = new Thickness(
                        int.Parse(marginPartsR[0]),
                        int.Parse(marginPartsR[1]),
                        int.Parse(marginPartsR[2]),
                        int.Parse(marginPartsR[3]));

                    string[] marginPartsL = leftMargin.Split(',');

                    Thickness leftThickness = new Thickness(
                        int.Parse(marginPartsL[0]),
                        int.Parse(marginPartsL[1]),
                        int.Parse(marginPartsL[2]),
                        int.Parse(marginPartsL[3]));

                    arrowRightMargin = rightThickness;
                    arrowLeftMargin = leftThickness;


                    Thickness marginThickness;

                    if (startOrientation == "right")
                    {
                        arrowRightDefault = true;
                        marginThickness = rightThickness;
                    }
                    else {
                        arrowRightDefault = false;
                        marginThickness = leftThickness;
                    }


                    Debug.WriteLine(imageFileBase + image);

                    BitmapImage layerImage = new BitmapImage();
                    layerImage.BeginInit();
                    layerImage.UriSource = new Uri(System.IO.Path.GetFullPath(imageFileBase + image), UriKind.Absolute);
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
                        Margin = marginThickness
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

                    possessionArrow = imageObject;

                }
                if (type == "logo")
                {
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

                    //Debug.WriteLine(imageFileBase + image);

                    string[] marginParts = margin.Split(',');

                    Thickness marginThickness = new Thickness(
                        int.Parse(marginParts[0]),
                        int.Parse(marginParts[1]),
                        int.Parse(marginParts[2]),
                        int.Parse(marginParts[3]));

                    BitmapImage layerImage = new BitmapImage();

                    if (debugLicencedLogos)
                    {
                        if (team == 1)
                        {
                            layerImage = await getTeamLogo(gameName, 1, events);
                        }
                        else
                        {
                            layerImage = await getTeamLogo(gameName, 0, events);
                        }
                    }

                    //BitmapImage layerImage = new BitmapImage();
                    //layerImage.BeginInit();
                    //layerImage.UriSource = new Uri(System.IO.Path.GetFullPath(imageFileBase + image), UriKind.Absolute);
                    //layerImage.EndInit();

                    Image imageObject = new Image
                    {
                        Name = name,
                        Source = layerImage,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Height = layerHeight,
                        Width = layerWidth,
                        Opacity = opacity,
                        Margin = marginThickness,
                    };

                    RootGrid.Children.Add(imageObject);

                    if (colorTeam1)
                    {
                        var TeamColor1 = System.Drawing.ColorTranslator.FromHtml(getTeamColor(gameName, 1, events));
                        RecolorImageWithAlpha(imageObject, TeamColor1);
                    }
                    if (colorTeam2)
                    {
                        var TeamColor1 = System.Drawing.ColorTranslator.FromHtml(getTeamColor(gameName, 0, events));
                        RecolorImageWithAlpha(imageObject, TeamColor1);
                    }
                }
                if (type == "timeoutArray")
                {
                    string name = layer.SelectSingleNode("name").InnerText;
                    int team = int.Parse(layer.SelectSingleNode("team").InnerText);
                    string hAlignment = layer.SelectSingleNode("h-alignment").InnerText;
                    string vAlignment = layer.SelectSingleNode("v-alignment").InnerText;
                    string activeImage = layer.SelectSingleNode("activeImage").InnerText;
                    string lostImage = layer.SelectSingleNode("lostImage").InnerText;
                    int imageHeight = int.Parse(layer.SelectSingleNode("imageHeight").InnerText);
                    int imageWidth = int.Parse(layer.SelectSingleNode("imageWidth").InnerText);
                    string margin = layer.SelectSingleNode("margin").InnerText;
                    double opacity = double.Parse(layer.SelectSingleNode("opacity").InnerText);
                    bool colorTeam1 = bool.Parse(layer.SelectSingleNode("teamColor1").InnerText);
                    bool colorTeam2 = bool.Parse(layer.SelectSingleNode("teamColor2").InnerText);
                    string startEnd = layer.SelectSingleNode("startEnd").InnerText ;
                    string orientation = layer.SelectSingleNode("orientation").InnerText;
                    int spacing = int.Parse(layer.SelectSingleNode("spacing").InnerText);

                    string[] marginParts = margin.Split(',');

                    Thickness marginThickness = new Thickness(
                        int.Parse(marginParts[0]),
                        int.Parse(marginParts[1]),
                        int.Parse(marginParts[2]),
                        int.Parse(marginParts[3]));
                }
            }
        }

        private async void updateScoreboard()
        {
            JArray events = await getEvents(urlDate, league);
            string newPeriod = await getPeriod(gameName, events);
            string newScoreHome = getScore(gameName, 1, events);
            string newScoreAway = getScore(gameName, 0, events);
            string newTime = getClock(gameName, events);
            string newDowns = getDownDistance(gameName, events);
            string posessionID = getPossession(gameName, events);

            if (team1ScoreBox != null)
                team1ScoreBox.Text = newScoreHome;

            if (team2ScoreBox != null)
                team2ScoreBox.Text = newScoreAway;

            if (gameTimeBox != null)
                gameTimeBox.Text = newTime;

            if (downsBox != null)
                downsBox.Text = newDowns;

            if (quarterBox != null)
                quarterBox.Text = newPeriod;

            string team0ID = getTeamID(gameName, 0, events);
            string team1ID = getTeamID(gameName, 1, events);

            //Debug.WriteLine(team0ID);
            //Debug.WriteLine(posessionID);

            bool _isFlipped = false;

            if (arrowRightDefault)
            {
                if (posessionID.Equals(team1ID))
                {
                    possessionArrow.RenderTransform = new ScaleTransform(-1, 1);
                    possessionArrow.RenderTransformOrigin = new Point(0.5, 0.5);
                    possessionArrow.Margin = arrowLeftMargin;
                    _isFlipped = true;
                }
                else if (posessionID.Equals(team0ID))
                {
                    possessionArrow.Margin = arrowRightMargin;
                    if (_isFlipped)
                    {
                        possessionArrow.RenderTransform = new ScaleTransform(-1, 1);
                        possessionArrow.RenderTransformOrigin = new Point(0.5, 0.5);
                    }
                }
                else
                {
                    possessionArrow.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                if (posessionID.Equals(team1ID))
                {
                    possessionArrow.Margin = arrowLeftMargin;
                    if (_isFlipped)
                    {
                        possessionArrow.RenderTransform = new ScaleTransform(-1, 1);
                        possessionArrow.RenderTransformOrigin = new Point(0.5, 0.5);
                    }
                }
                else if (posessionID.Equals(team0ID))
                {
                    possessionArrow.RenderTransform = new ScaleTransform(-1, 1);
                    possessionArrow.RenderTransformOrigin = new Point(0.5, 0.5);
                    possessionArrow.Margin = arrowRightMargin;
                    _isFlipped = true;
                }
                else
                {
                    possessionArrow.Visibility = Visibility.Hidden;
                }
            }
        }

        private string getTeamID(String Matchup, int Team, JArray eventsArray)
        {
            string Id = "";

            foreach (JObject eventObj in eventsArray)
            {
                string name = eventObj["name"]?.ToString();
                if (name != Matchup)
                    continue;

                var competitors = eventObj["competitions"]?[0]?["competitors"] as JArray;
                if (competitors == null) continue;

                var team = competitors[Team]["team"];
                if (team == null) continue;


                Id = team["id"]?.ToString();


                break;
            }
            return Id;
        }

        private async Task<JArray> getEvents(String urlDate, String league)
        {
            string url = "https://site.api.espn.com/apis/site/v2/sports/football/" + league + "/scoreboard?dates=" + urlDate;
            JObject json = await getJsonfromEndpoint(url);
            JArray array = (JArray)json["events"];
            if (array == null)
                return [];
            return array;
        }

        private async Task<string?> getPeriod(String Matchup, JArray eventsArray)
        {
            string period = "";

            foreach (JObject eventObj in eventsArray)
            {
                string name = eventObj["name"]?.ToString();
                if (name != Matchup)
                    continue;

                var competitors = eventObj["competitions"] as JArray;
                var competition = competitors[0] as JObject;
                var status = competition["status"] as JObject;

                //Debug.Print(status.ToString());

                if (competitors == null) continue;

                var periodJSON = status["period"];
                if (periodJSON == null) continue;
                int periodNUM = periodJSON.ToObject<int>();

                switch (periodNUM)
                {
                    case 0:
                        break;
                    case <= 4:
                        period = periodNUM + getEndNumberModifier(periodNUM);
                        break;
                    case 5:
                        period = "OT";
                        break;
                    default:
                        periodNUM = periodNUM - 4;
                        period = (periodNUM) + getEndNumberModifier(periodNUM) + " OT";
                        break;
                }

                break;
            }
            return period;
        }

        private string getScore(String Matchup, int Team, JArray eventsArray)
        {
            string score = "";

            foreach (JObject eventObj in eventsArray)
            {
                string name = eventObj["name"]?.ToString();
                if (name != Matchup)
                    continue;

                var competitors = eventObj["competitions"]?[0]?["competitors"] as JArray;
                if (competitors == null) continue;

                var team = competitors[Team]["score"];
                if (team == null) continue;

                //Debug.Print(team.ToString());

                score = team.ToString();


                break;
            }
            return score;
        }

        private string getClock(String Matchup, JArray eventsArray)
        {
            string clock = "";

            foreach (JObject eventObj in eventsArray)
            {
                string name = eventObj["name"]?.ToString();
                if (name != Matchup)
                    continue;

                var competitors = eventObj["competitions"] as JArray;
                if (competitors == null) continue;

                var competition = competitors[0] as JObject;
                var status = competition["status"] as JObject;
                var type = status["type"] as JObject;
                var finished = type["completed"].ToObject<bool>();
                var detail = type["detail"].ToString();

                if (detail.Equals("Halftime"))
                {
                    clock = "HALF";
                    break;
                }

                if (finished == true)
                {
                    clock = "FINAL";
                    break;
                }

                //Debug.Print(status.ToString());

                var displayClock = status["displayClock"];
                if (displayClock == null) continue;

                clock = displayClock.ToString();

                break;
            }
            return clock;
        }

        private string getTeamColor(string Matchup, int Team, JArray eventsArray)
        {
            string defaultColor = "#FFFFFF"; // fallback color
            string color = defaultColor;

            foreach (JObject eventObj in eventsArray)
            {
                string name = eventObj["name"]?.ToString();
                if (name != Matchup)
                    continue;

                var competitors = eventObj["competitions"]?[0]?["competitors"] as JArray;
                if (competitors == null) continue;

                var team = competitors[Team]?["team"];
                if (team == null) continue;

                if (league.Equals("nfl"))
                {
                    string? baseColor = team["color"]?.ToString();
                    color = !string.IsNullOrWhiteSpace(baseColor) ? $"#{baseColor}" : defaultColor;
                }
                else
                {
                    string? altColor = team["alternateColor"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(altColor))
                    {
                        color = $"#{altColor}";
                    }
                    else
                    {
                        string? baseColor = team["color"]?.ToString();
                        color = !string.IsNullOrWhiteSpace(baseColor) ? $"#{baseColor}" : defaultColor;
                    }
                }

                break;
            }

            return color;
        }
        
        private string getAbbreviation(String Matchup, int Team, JArray eventsArray)
        {
            string abbreviation = "";

            foreach (JObject eventObj in eventsArray)
            {
                string name = eventObj["name"]?.ToString();
                if (name != Matchup)
                    continue;

                var competitors = eventObj["competitions"]?[0]?["competitors"] as JArray;
                if (competitors == null) continue;

                var team = competitors[Team]["team"];
                if (team == null) continue;


                abbreviation = team["abbreviation"]?.ToString();


                break;
            }
            return abbreviation;
        }
        
        private string getDownDistance(String Matchup, JArray eventsArray)
        {
            string DownDistance = "";

            foreach (JObject eventObj in eventsArray)
            {
                string name = eventObj["name"]?.ToString();
                //Debug.WriteLine(name);
                //Debug.WriteLine(Matchup);

                if (name != Matchup)
                    continue;

                var competitors = eventObj["competitions"] as JArray;
                if (competitors == null) continue;

                var competition = competitors[0] as JObject;

                var status = competition["situation"] as JObject;
                if (status == null) continue;

                var downDistanceText = status["shortDownDistanceText"];
                if (downDistanceText == null) continue;

                DownDistance = downDistanceText.ToString();

                break;
            }
            return DownDistance;
        }

        private string getPossession(String Matchup, JArray eventsArray)
        {
            string PossessionID = "";

            foreach (JObject eventObj in eventsArray)
            {
                string name = eventObj["name"]?.ToString();
                //Debug.WriteLine(name);
                //Debug.WriteLine(Matchup);

                if (name != Matchup)
                    continue;

                var competitors = eventObj["competitions"] as JArray;
                if (competitors == null) continue;

                var competition = competitors[0] as JObject;

                var status = competition["situation"] as JObject;
                if (status == null) continue;

                var possessionText = status["possession"];
                if (possessionText == null) continue;

                PossessionID = possessionText.ToString();

                break;
            }
            return PossessionID;
        }

        private string getEndNumberModifier(int number)
        {
            number = number % 10;
            String modifier = "";
            switch (number)
            {
                case 1:
                    modifier = "st";
                    break;
                case 2:
                    modifier = "nd";
                    break;
                case 3:
                    modifier = "rd";
                    break;
                default:
                    modifier = "th";
                    break;
            }
            return modifier;
        }

        protected override void OnClosed(EventArgs e)
        {
            //_cts.Cancel();
            base.OnClosed(e);
        }

        private async Task<BitmapImage> getTeamLogo(String Matchup, int Team, JArray eventsArray)
        {
            string logoUrl = "";
            foreach (JObject eventObj in eventsArray)
            {
                string name = eventObj["name"]?.ToString();
                if (name != Matchup)
                    continue;

                var competitors = eventObj["competitions"]?[0]?["competitors"] as JArray;
                if (competitors == null) continue;

                var team = competitors[Team]["team"];
                if (team == null) continue;

                logoUrl = team["logo"]?.ToString();

                break;
            }
            using HttpClient httpClient = new();

            Debug.WriteLine(logoUrl);

            byte[] imageBytes = await httpClient.GetByteArrayAsync(logoUrl);

            BitmapImage fillBitmap = new BitmapImage();

            using (var stream = new MemoryStream(imageBytes))
            {
                fillBitmap.BeginInit();
                fillBitmap.CacheOption = BitmapCacheOption.OnLoad;
                fillBitmap.StreamSource = stream;
                fillBitmap.EndInit();
                fillBitmap.Freeze(); // Optional but useful for threading
            }
            return fillBitmap;
        }
    }
}
