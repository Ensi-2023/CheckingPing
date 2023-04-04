#nullable disable
using System;

using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace CheckingPing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource _source;      

        private bool _richStatusScroll = true;
        private bool _testingStatus = true;
        private int _timer = 0;
        private Packet _packet;
        private double _avg = 0;


        #region Public
        public int TCount => _timer;
        public bool Testing
        {
            get
            {
                return _testingStatus;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(new System.Action(() => {

                    _testingStatus = value;

                    if (value)
                    {                   
                        urlAdress.IsEnabled = false;
                        b_pingTestStart.IsEnabled = false;
                        b_pingTestStop.IsEnabled = true;
                        StartTest(urlAdress.Text);
                        testingToolBar.Visibility = Visibility.Visible;
                        b_logBoxResult.Visibility = Visibility.Collapsed;
                    }
                   
                }));
            }
        }
        #endregion
              

        public MainWindow()
        {
            InitializeComponent();
            Log.Document.Blocks.Clear();
            Logger.Set(Log);
        }


        private async void StartTest(string url)
        {
            _source = new CancellationTokenSource();
            _timer = 0;
            _avg = 0;
            t_maxPing.Text = "0";
            t_minPing.Text = "0";
            t_avgPing.Text = "0";
            await testPing(url.ToLower().Replace("http://","").Replace("https://", ""));
        }

        private async Task testPing(string url)
        {
            bool _error = false;

            Ping pingSender = new Ping();
            _packet = new Packet() { CountPacketSuccess = 0, CountPacketFailure=0 };
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 12000;
            PingOptions options = new PingOptions(64, false);
            Logger.SendLog($"Проверка подключения к хосту: {url}");
  
            while (Testing)
            {
                if (_source.IsCancellationRequested)
                {                 
                    break;
                }

                try
                {
                    DisplayReply(await pingSender.SendPingAsync(url, timeout, buffer, options),ref _packet);
                    t_toolBatSuccesPacket.Text = _packet.CountPacketSuccess.ToString();
                    t_toolBatBadPacket.Text = _packet.CountPacketFailure.ToString();
                    t_toolBatBadPacketPercent.Text = _packet.PercentTimeOutPacket();
                }
                catch (Exception ex)
                {
                    Logger.SendLog($"Error: {ex.Message}");
                    Testing = false;
                    _error = true;
                    break;
                }    
              
                _timer++;
                t_toolBatTimer.Text = $"{TCount}";

                await Task.Delay(1000);
            }

         
            //Вышли из цикла значит тест окончен, выводим результаты
            Application.Current.Dispatcher.Invoke(new System.Action(() =>
            {
                urlAdress.IsEnabled = true;
                b_pingTestStart.IsEnabled = true;        
                _source.Cancel();
                testingToolBar.Visibility = Visibility.Collapsed;
                if (_error) { b_pingTestStop.IsEnabled = false;  return; }
                Logger.SendLog($"Тест окончен");
                SetOverlay(_packet, url);
            }));

        }

        private void SetOverlay(Packet packet, string url)
        {
            t_overlayMaxPing.Text = string.Format("{0} ms",t_maxPing.Text);
            t_overlayMinPing.Text = string.Format("{0} ms", t_minPing.Text);
            t_overlayAvgPing.Text = string.Format("{0} ms", t_avgPing.Text);
            t_overlayPacketFailed.Text = packet.CountPacketFailure.ToString();
            t_overlayPacketSuccess.Text = packet.CountPacketSuccess.ToString();
            t_overlayPercentPacketFailed.Text = packet.PercentTimeOutPacket();
            t_overlayUrl.Text = url;
            Overlay.Visibility = Visibility.Visible;
        }

        public void DisplayReply(PingReply reply,ref Packet _packet)
        {

            switch (reply.Status)
            { 
                case IPStatus.Success:
                    _packet.CountPacketSuccess++;
                    string info = $"[{reply.Address}] TTL [{reply.Options.Ttl}] buffer [{reply.Buffer.Length}] ping: ";
                    Logger.SendPing(info, $"{reply.RoundtripTime}");
                    SetInfoPing(reply.RoundtripTime.ToString());
                    break;

                case IPStatus.TimedOut:
                    _packet.CountPacketFailure++;
                    Logger.SendPing("packet loss", $"");
             
                    break;

                case IPStatus.BadRoute:
                    _packet.CountPacketFailure++;
                    Logger.SendPing("Bad Route", $"");
               
                    break;

                default:
                    Logger.SendPing("Uncommand", $""); break;

            }

        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

    
        private void SetInfoPing(string ping)
        {
            if (ping == string.Empty) 
                return;

            int p = int.Parse(ping);
            int minPing  = int.Parse(t_minPing.Text);
            int maxPing  = int.Parse(t_maxPing.Text);
           
            _avg += p;

            t_avgPing.Text = $"{Math.Round(_avg / (_packet.CountPacketSuccess + _packet.CountPacketFailure))}";

            if (minPing == 0 && maxPing == 0)
            {
                t_minPing.Text = ping;
                t_maxPing.Text = ping;
                return;
            }

            if(minPing >= p)
                t_minPing.Text = p.ToString();

            if (maxPing <= p)
                t_maxPing.Text = p.ToString();

        }

        private void b_pingTestStop_Click(object sender, RoutedEventArgs e)
        {
            b_pingTestStop.IsEnabled = false;
            Testing = false;
        }

        private void b_pingTestStart_Click(object sender, RoutedEventArgs e)
        {
            StartCheck(urlAdress.Text);
        }

        private void b_windiwClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void b_windowMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void b_logBoxClear_Click(object sender, RoutedEventArgs e)
        {
            Log.Document.Blocks.Clear();
            b_logBoxClear.Visibility = Visibility.Collapsed;
            b_logBoxResult.Visibility = Visibility.Collapsed;
            t_maxPing.Text = "0";
            t_minPing.Text = "0";
            t_avgPing.Text = "0";
        }


        private void Log_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!_richStatusScroll) return;
            var obj = (RichTextBox)sender;
            obj.ScrollToEnd();
            if (obj.Document.Blocks.Count >= 1)
            {
                b_logBoxClear.Visibility = Visibility.Visible;
            }
        }
        private void Log_MouseEnter(object sender, MouseEventArgs e)
        {
            _richStatusScroll = false;
        }
        private void Log_MouseLeave(object sender, MouseEventArgs e)
        {

            _richStatusScroll = true;
        }

        private void urlAdress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartCheck(urlAdress.Text);
            }
        }

        private void StartCheck(string text)
        {
            if (text == string.Empty)
            {
                Logger.SendLog("Поле адреса должно быть заполнено");
                return;
            }

            Testing = true;
        }

        private void b_overlayClose_Click(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Collapsed;
            b_logBoxResult.Visibility = Visibility.Visible;
        }

        private void b_logBoxResult_Click(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{ 
        //    string inf = TestMethod(inf="", 0, 0);
        //    inf = TestMethod(inf, 0, 1);
        //    inf = TestMethod(inf, 1, 0);
        //    inf = TestMethod(inf, 1, 1);
        //    inf = TestMethod(inf, 1, 3);
        //    inf = TestMethod(inf, 0, 2);
        //    inf = TestMethod(inf, 1, 6);
        //    inf = TestMethod(inf, 3, 3);
        //    inf = TestMethod(inf, 3, 4);
        //    inf = TestMethod(inf, 4, 3);
        //    inf = TestMethod(inf, 4, 2);
        //    inf = TestMethod(inf, 4, 1);
        //    inf = TestMethod(inf, 4, 56);

        //    MessageBox.Show(inf);
        //}

        //private string TestMethod(string inf, double v, double v1)
        //{
        //    _packet = new packet() { CountPacketSuccess = v, CountPacketFailure = v1 };

        //    inf += $"[{v} | {v1}] = {_packet.PercentTimeOutPacket()}\n";
        //    return inf;
        //}
    }
}
