using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for GroupHands.xaml
    /// </summary>
    public partial class GroupHands : Window
    {
        public string PlayerName { get; private set; }
        public string DMName { get; private set; }
        public bool IsCreator { get; private set; }
        private static readonly string ChipPage = "http://spartan364.hopto.org/handupdate.php";
        private static readonly string createGroupPage = "http://spartan364.hopto.org/reqid.php";
        private long LastUpdated;
        private readonly Timer updateInterval;
        private readonly object updateLock = new object();
        private static readonly int MinuteInMiliseconds = 60000;
        private bool sessionClosed = false;
        private string currentHand;

        public GroupHands(Window owner, string DMName, string PlayerName, bool isCreator)
        {
            InitializeComponent();
            this.Owner = owner;
            this.PlayerName = PlayerName;
            this.DMName = DMName;
            this.IsCreator = isCreator;
            this.Hands.Text = "empty";
            if (isCreator)
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    System.Collections.Specialized.NameValueCollection postData =
                        new System.Collections.Specialized.NameValueCollection()
                        {
                            { "DMName", DMName }
                        };
                    string result = Encoding.UTF8.GetString(wc.UploadValues(createGroupPage, postData));
                    if (!result.Equals("ready", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("ServerError");
                    }
                }
            }
            string hand = (this.Owner as MainWindow).GetHand();
            currentHand = hand;
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                System.Collections.Specialized.NameValueCollection postData =
                    new System.Collections.Specialized.NameValueCollection()
                    {
                            { "DMName", DMName },
                            {"PlayerName", PlayerName },
                            {"hand", hand},
                            {"join", "true"}
                    };
                string result = Encoding.UTF8.GetString(wc.UploadValues(ChipPage, postData));
                if (result.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("ServerError");
                }
                if (result.Equals("taken", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Name Taken");
                }
                this.Hands.Text = result;
            }
            LastUpdated = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            updateInterval = new Timer(MinuteInMiliseconds)
            {
                Enabled = true
            };
            updateInterval.Elapsed += OnTimedEvent;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            HandUpdate();
        }

        public void HandUpdate(bool manual = false)
        {
            if (sessionClosed) return;
            lock (updateLock)
            {
                if (manual == false && (LastUpdated + MinuteInMiliseconds) > DateTimeOffset.Now.ToUnixTimeMilliseconds())
                    return;
                string hand = null;
                this.Dispatcher.Invoke(() =>
                {//this refer to form in WPF application
                    hand = (this.Owner as MainWindow).GetHand();
                });
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    System.Collections.Specialized.NameValueCollection postData =
                    new System.Collections.Specialized.NameValueCollection()
                    {
                    { "DMName", DMName },
                    { "PlayerName", PlayerName },
                    };
                    if (hand != currentHand)
                    {
                        postData.Add("hand", hand);
                        currentHand = hand;
                    }
                    string result = Encoding.UTF8.GetString(wc.UploadValues(ChipPage, postData));
                    if (result.Equals("closed", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("The group was closed");
                        (this.Owner as MainWindow).GroupClosed();
                        updateInterval.Dispose();
                        this.sessionClosed = true;
                        this.Close();
                    }
                    this.Dispatcher.Invoke(() =>
                    {//this refer to form in WPF application
                        this.Hands.Text = result;
                    });
                }
                LastUpdated = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
        }

        public static bool CheckSessionExists(string DMName)
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                System.Collections.Specialized.NameValueCollection postData =
                new System.Collections.Specialized.NameValueCollection()
                {
                    { "DMName", DMName },
                    { "PlayerName", DMName }
                };
                string result = Encoding.UTF8.GetString(wc.UploadValues(ChipPage, postData));
                if (result.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sessionClosed) return;
            var res = MessageBox.Show("Leave the group?", "Close window?", MessageBoxButton.OKCancel);
            if (res.Equals(MessageBoxResult.Cancel))
            {
                e.Cancel = true;
                return;
            }
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                System.Collections.Specialized.NameValueCollection postData =
                new System.Collections.Specialized.NameValueCollection()
                {
                    { "DMName", DMName },
                    { "PlayerName", PlayerName },
                    { "close", "true" }
                };
                string result = Encoding.UTF8.GetString(wc.UploadValues(ChipPage, postData));
                if (!result.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("A server error occurred, inform Major");
                }
            }
            this.updateInterval.Stop();
            sessionClosed = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HandUpdate(true);
        }
    }
}