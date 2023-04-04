#nullable disable

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CheckingPing.CScript
{
    public class Logger
    {
        private static RichTextBox _log;
        public static void Set(RichTextBox logger) 
        {_log = logger;}

        public static void SendPing(string message, string ping)
        {
            OnSend($"[Ping] [{DateTime.Now}] - {message}", ping);
        }


        public static void SendLog(string message)
        {
            OnSend($"[Log] [{DateTime.Now}] - {message}");
        }

        private static void OnSend(string log, string ping = "")
        {
            if (log.Trim().Length == 0) return;
            Application.Current.Dispatcher.Invoke(new System.Action(() => {


                Brush brush = Brushes.White;
                if (log.Contains("[Ping]"))
                {
                    brush = ColorPing(ping);

                   
                }
                if (log.Contains("[Log]")) brush = Brushes.White;

                var paragraph = new Paragraph();
                var inf = new Run(log);

                var run = new Run(ping)
                {
                    Foreground = brush
                };
                paragraph.Inlines.Add(log);
                paragraph.Inlines.Add(run);
                _log.Document.Blocks.Add(paragraph);

            }
            ));
        }

        private static Brush ColorPing(string ping)
        {
            if (ping == string.Empty) return Brushes.Transparent;

            int p = int.Parse(ping);
            if (p >= 0 && p < 60) return Brushes.LimeGreen;
            else if (p >= 60 && p <= 80) return Brushes.Yellow;
            else return Brushes.Red;

        }

    }
}
