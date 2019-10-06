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
    public partial class HandTab : UserControl
    {
        public string PlayerName { get; private set; }
        public string DMName { get; private set; }
        public bool isCreator;
        private static readonly string ChipPage = "http://spartan364.hopto.org/handupdate.php";
        private static readonly string createGroupPage = "http://spartan364.hopto.org/reqid.php";
        private long LastUpdated;
        private Timer updateInterval;
        private readonly Semaphore netLock;
        private const int MinuteInMiliseconds = 60000;
        public bool SessionClosed { get; private set; } = true;
        private string currentHand;
        public bool initialized = false;

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

        public async Task JoinGroup(string DMName, string PlayerName, bool isCreator)
        {
            if (!SessionClosed)
            {
                MessageBox.Show("You are already in a group");
                return;
            }
            this.PlayerName = PlayerName;
            this.DMName = DMName;
            this.isCreator = isCreator;
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
            currentHand = GetHand();
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
            await this.Dispatcher.BeginInvoke((Action)(() =>
            {
                Players.Visibility = Visibility.Visible;
                GroupRefreshButton.Visibility = Visibility.Visible;
                GroupLeaveButton.Visibility = Visibility.Visible;
                SetTabVisibility(Visibility.Visible);
            }));
            updateInterval.Elapsed += OnTimedEvent;
            postContent.Dispose();
            SessionClosed = false;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            HandUpdate();
        }

        public async void HandUpdate(bool manual = false)
        {
            if (SessionClosed) return;

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
                    hand = GetHand();
                });

                postContent = new System.Net.Http.FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("DMName", DMName),
                        new KeyValuePair<string, string>("PlayerName", PlayerName),
                        new KeyValuePair<string, string>("hand", currentHand),
                });
                this.currentHand = hand;
                string result = await (
                    await MainWindow.client.PostAsync(ChipPage, postContent)
                    ).Content.ReadAsStringAsync();
                if (result.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The group was closed");
                    this.Dispatcher.Invoke(() =>
                    {
                        updateInterval.Dispose();
                        this.SessionClosed = true;
                        Players.Visibility = Visibility.Hidden;
                        GroupRefreshButton.Visibility = Visibility.Hidden;
                        GroupLeaveButton.Visibility = Visibility.Hidden;
                        if (this.currentHand.Length == 0)
                        {
                            SetTabVisibility(Visibility.Hidden);
                        }
                    });
                    return;
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
                postContent?.Dispose();
                netLock.Release();
            }
            //}
            LastUpdated = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //release mutex
        }

        public struct GroupedHand
        {
            public string Name { get; private set; }
            public string Hand { get; private set; }

            public GroupedHand(string name, string hand)
            {
                this.Name = name ?? throw new ArgumentNullException();
                this.Hand = hand ?? throw new ArgumentNullException();
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

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if ((LastUpdated + (MinuteInMiliseconds / 24)) > DateTimeOffset.Now.ToUnixTimeMilliseconds())
            {
                MessageBox.Show("This was updated less than a minute ago");
                return;
            }

            Task.Run(() => HandUpdate(true));
        }

        public async Task LeaveGroup()
        {
            if (SessionClosed) return;
            var postContent = new System.Net.Http.FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("DMName", DMName),
                new KeyValuePair<string, string>("PlayerName", PlayerName),
                new KeyValuePair<string, string>("close", "true")
            });

            /*var httpRes = MainWindow.client.PostAsync(ChipPage, postContent);
            httpRes.Wait();
            var stringTask = httpRes.Result.Content.ReadAsStringAsync();
            stringTask.Wait();
            string result = stringTask.Result;*/
            await MainWindow.client.PostAsync(ChipPage, postContent);
            postContent.Dispose();
        }

        private async void Leave_Click(object sender, RoutedEventArgs e)
        {
            if (SessionClosed) return;
            var res = MessageBox.Show("Leave the group?", "Close window?", MessageBoxButton.OKCancel);
            if (res.Equals(MessageBoxResult.Cancel))
            {
                return;
            }
            var postContent = new System.Net.Http.FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("DMName", DMName),
                new KeyValuePair<string, string>("PlayerName", PlayerName),
                new KeyValuePair<string, string>("close", "true")
            });
            try
            {
                /*var httpRes = MainWindow.client.PostAsync(ChipPage, postContent);
                httpRes.Wait();
                var stringTask = httpRes.Result.Content.ReadAsStringAsync();
                stringTask.Wait();
                string result = stringTask.Result;*/
                string result = await (await MainWindow.client.PostAsync(ChipPage, postContent)).Content.ReadAsStringAsync();
                //string result = Encoding.UTF8.GetString(wc.UploadValues(ChipPage, postData));
                if (!result.Equals("closed", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("A server error occurred, inform Major");
                }
            }
            catch (Exception except)
            {
                MainWindow.ErrorWindow();
                MessageBox.Show(except.Message);
            }
            finally
            {
                postContent.Dispose();
                this.updateInterval.Stop();
                this.updateInterval.Dispose();
                this.Dispatcher.Invoke(() =>
                {
                    updateInterval.Dispose();
                    this.SessionClosed = true;
                    Players.Visibility = Visibility.Hidden;
                    GroupRefreshButton.Visibility = Visibility.Hidden;
                    GroupLeaveButton.Visibility = Visibility.Hidden;
                    if (this.ChipsInHand.Count == 0)
                    {
                        SetTabVisibility(Visibility.Hidden);
                    }
                });
            }
        }
    }
}