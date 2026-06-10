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

        string urlDate;
        string gameName;
        string league;
        XmlDocument ScoreBugConfig = new XmlDocument();
        
        private DispatcherTimer _timer;
        public DynamicScoreboardTest()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string imageFileBase = "ActiveScorebugs/Football/Default/";
            ScoreBugConfig.Load("ActiveScorebugs/Football/Default/BugSetup.xml");
            base.OnContentRendered(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            //_cts.Cancel();
            base.OnClosed(e);
        }
    }
}
