using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using Timer = System.Timers.Timer;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for GroupHands.xaml
    /// </summary>
    public partial class GroupHands : Window
    {

        public string PlayerName { get; private set; }
        public string DMName { get; private set; }
        public readonly bool isCreator;
        private static readonly string ChipPage = "http://spartan364.hopto.org/handupdate.php";
        private static readonly string createGroupPage = "http://spartan364.hopto.org/reqid.php";
        private long LastUpdated;
        private Timer updateInterval;
        private readonly object updateLock = new object();
        private readonly Semaphore netLock;
        private const int MinuteInMiliseconds = 60000;
        private bool sessionClosed = false;
        private string currentHand;
        public bool initialized = false;

        public GroupHands(Window owner, string DMName, string PlayerName, bool isCreator)
        {
            InitializeComponent();
            netLock = new Semaphore(1, 1);
            this.Hide();
            this.Owner = owner;
            this.PlayerName = PlayerName;
            this.DMName = DMName;
            this.isCreator = isCreator;
        }

        public async Task Init()
        {
            if (initialized)
            {
                throw new Exception("Already initialized");
            }
            if (isCreator)
            {
                var stringContent = new System.Net.Http.FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("DMName", DMName),
                    });
                var res = await MainWindow.client.PostAsync(createGroupPage, stringContent);
                string textResult = await res.Content.ReadAsStringAsync();
                if (!textResult.Equals("ready", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("ServerError");
                }
                stringContent.Dispose();
            }
            this.Dispatcher.Invoke(() => currentHand = (this.Owner as MainWindow).GetHand());
            var postContent = new System.Net.Http.FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("DMName", DMName),
                        new KeyValuePair<string, string>("PlayerName", PlayerName),
                        new KeyValuePair<string, string>("hand", currentHand),
                        new KeyValuePair<string, string>("join", "true")
                });
            string postRes = await (await MainWindow.client.PostAsync(ChipPage, postContent)).Content.ReadAsStringAsync();
            if (postRes.Equals("closed", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("ServerError");
            }
            if (postRes.Equals("taken", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Name Taken");
            }
            List<GroupedHand> hands = null;
            if (!postRes.Equals("empty", StringComparison.OrdinalIgnoreCase))
            {
                hands = ConvertToHands(postRes);
            }

            this.Dispatcher.Invoke(() =>
            {
                this.Players.ItemsSource = hands;
            });
            LastUpdated = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            updateInterval = new Timer(MinuteInMiliseconds)
            {
                Enabled = true
            };
            updateInterval.Elapsed += OnTimedEvent;
            postContent.Dispose();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            HandUpdate();
        }

        public bool IsSessionClosed()
        {
            return this.sessionClosed;
        }

        public async void HandUpdate(bool manual = false)
        {
            if (sessionClosed) return;

            //lock (updateLock) //acquire mutex
            //{
            System.Net.Http.FormUrlEncodedContent postContent = null;
            netLock.WaitOne();
            try
            {
                if (manual == false && (LastUpdated + MinuteInMiliseconds) > DateTimeOffset.Now.ToUnixTimeMilliseconds())
                    return;
                string hand = null;
                this.Dispatcher.Invoke(() =>
                {
                        //Because this might not be done on the UI thread
                        hand = (this.Owner as MainWindow).GetHand();
                });

                postContent = new System.Net.Http.FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("DMName", DMName),
                        new KeyValuePair<string, string>("PlayerName", PlayerName),
                        new KeyValuePair<string, string>("hand", currentHand),
                    });
                this.currentHand = hand;
                //var httpRes = MainWindow.client.PostAsync(ChipPage, postContent);
                //httpRes.Wait();
                //var stringTask = httpRes.Result.Content.ReadAsStringAsync();
                //stringTask.Wait();
                //string result = stringTask.Result;
                string result = await (
                    await MainWindow.client.PostAsync(ChipPage, postContent)
                    ).Content.ReadAsStringAsync();
                if (result.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The group was closed");
                    (this.Owner as MainWindow).GroupClosed();
                    updateInterval.Dispose();
                    this.sessionClosed = true;
                    this.Close();
                }
                List<GroupedHand> hands = null;
                if (!result.Equals("empty", StringComparison.OrdinalIgnoreCase))
                {
                    hands = ConvertToHands(result);
                }

                //because you cannot update window properties on a different thread
                this.Dispatcher.Invoke(() => this.Players.ItemsSource = hands);
            }
            catch (Exception e)
            {
                MessageBox.Show("An error has occurred");
                MessageBox.Show(e.Message);
                return;
            }
            finally
            {
                if (postContent != null)
                {
                    postContent.Dispose();
                }
                netLock.Release();
            }
            //}
            LastUpdated = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //release mutex
        }

        public static async Task<bool> CheckSessionExists(string DMName)
        {
            var postContent = new System.Net.Http.FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("DMName", DMName),
                new KeyValuePair<string, string>("PlayerName", DMName)
            });
            string result = await (await MainWindow.client.PostAsync(ChipPage, postContent)).Content.ReadAsStringAsync();
            if (result.Equals("closed", StringComparison.OrdinalIgnoreCase))
            {
                postContent.Dispose();
                return false;
            }
            postContent.Dispose();
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
            var postContent = new System.Net.Http.FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("DMName", DMName),
                new KeyValuePair<string, string>("PlayerName", DMName),
                new KeyValuePair<string, string>("close", "true")
            });
            try
            {
                var httpRes = MainWindow.client.PostAsync(ChipPage, postContent);
                httpRes.Wait();
                var stringTask = httpRes.Result.Content.ReadAsStringAsync();
                stringTask.Wait();
                string result = stringTask.Result;
                //string result = Encoding.UTF8.GetString(wc.UploadValues(ChipPage, postData));
                if (!result.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("A server error occurred, inform Major");
                }
            }
            catch (Exception except)
            {
                MessageBox.Show("Something went wrong, inform Major");
                MessageBox.Show(except.Message);
            }
            finally
            {
                postContent.Dispose();
                this.updateInterval.Stop();
                this.updateInterval.Dispose();
                sessionClosed = true;

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((LastUpdated + (MinuteInMiliseconds / 24)) > DateTimeOffset.Now.ToUnixTimeMilliseconds())
            {
                MessageBox.Show("This was updated less than a minute ago");
                return;
            }

            Task.Run(() => HandUpdate(true));
        }

        protected class GroupedHand
        {
            public string Name { get; private set; }
            public string Hand { get; private set; }

            public GroupedHand(string name, string hand)
            {
                this.Name = name ?? throw new ArgumentNullException();
                this.Hand = hand ?? throw new ArgumentNullException();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && Players.SelectedItems != null && Players.SelectedItems.Count == 1)
            {
                DataGridRow dgr = Players.ItemContainerGenerator.ContainerFromItem(Players.SelectedItem) as DataGridRow;
                if (!dgr.IsMouseOver)
                {
                    (dgr as DataGridRow).IsSelected = false;
                }
            }
        }

        private List<GroupedHand> ConvertToHands(string json)
        {
            Dictionary<string, string[]> res;
            List<GroupedHand> hands = new List<GroupedHand>();
            try
            {
                res = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(json);
            }
            catch (JsonException e)
            {
                MessageBox.Show("An error occurred, inform Major");
                MessageBox.Show(e.Message);
                return null;
            }
            foreach (var entry in res)
            {
                Array.Sort(entry.Value);
                string desc = string.Join(", ", entry.Value);
                string name = entry.Key;
                hands.Add(new GroupedHand(name, desc));
            }
            return hands;
        }
    }
}