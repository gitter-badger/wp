using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using System.Diagnostics;
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;
using System.Windows.Threading;

namespace Grapital
{
    public partial class ItemInfoPage : PhoneApplicationPage
    {
        Item item = new Item();
        DispatcherTimer dt = new DispatcherTimer();

        public ItemInfoPage()
        {
            InitializeComponent();
            dt.Interval = TimeSpan.FromSeconds(4);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();
            butGetIt.Content = MyResources.GetIt;
            butDelete.Content = MyResources.DeleteIt;
            butGetIt.Style = butDelete.Style = App.Current.Resources["roundedButton"] as Style;
            butGetIt.Background = new SolidColorBrush(Color.FromArgb(170,0,0,0));
            butDelete.Background = new SolidColorBrush(Colors.Red);
        }

        void dt_Tick(object sender, EventArgs e)
        {
            imgPhoto.Visibility = Visibility.Visible;
            map.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string id;
            NavigationContext.QueryString.TryGetValue("id", out id);
            item = (App.Current as App).itemStorage.items[Convert.ToInt16(id)];
            Uri uri = new Uri(item.photo, UriKind.Absolute);
            imgPhoto.Source = new BitmapImage(uri);
            int v = item.condition;
            string cond = MyResources.Wretched;
            if (v > 2) cond = MyResources.Used;
            if (v > 4) cond = MyResources.New;

            tbDescription.Text = item.description;
            tbCondition.Text = MyResources.Condition + " " + cond;
            tbDate.Text = MyResources.Added+ " " + item.date.Remove(9);
            if (item.user == (App.Current as App).settings["email"].ToString())
            {
                butGetIt.Visibility = Visibility.Collapsed;
                butDelete.Visibility = Visibility.Visible;
            }
            else
            {
                butGetIt.Visibility = Visibility.Visible;
                butDelete.Visibility = Visibility.Collapsed;
            }

            Pushpin p = new Pushpin();
            //define it's graphic properties 
            p.Background = new SolidColorBrush(Colors.Yellow);
            p.Foreground = new SolidColorBrush(Colors.Black);
            //where to put the Pushpin 
            p.Location = new GeoCoordinate(item.loc.getLat(), item.loc.getLng());
            //What to write on it
            //p.Content = "";
            //now we add the Pushpin to the map
            map.Children.Add(p);
            map.SetView(new GeoCoordinate(item.loc.getLat(), item.loc.getLng(), 100),13); 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((App.Current as App).settings["emailCodeVerification"].ToString() == "Ok" && (App.Current as App).settings["friendVerification"].ToString() == "Ok")
            {
                EmailComposeTask emailComposeTask = new EmailComposeTask();
                emailComposeTask.Subject = "Grapital";
                emailComposeTask.Body = "";
                emailComposeTask.To = item.user;
                emailComposeTask.Show();
            }
            else
            {
                if ((App.Current as App).settings["emailCodeVerification"].ToString() != "Ok") MessageBox.Show(MyResources.CompleteRegistration); else MessageBox.Show(MyResources.AskFriendVerify);
            }


        }

        private void butDelete_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(item.id.ToString());

            var client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            string uri = (GV.server + "/api.php/items/delete/" + item.id.ToString());

            client.DownloadStringAsync(new Uri(uri));
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    (App.Current as App).newItemAdded = true;
                    (Application.Current.RootVisual as PhoneApplicationFrame).GoBack();
                }
            );
        }

        private void butMap_Click(object sender, RoutedEventArgs e)
        {
            if (imgPhoto.Visibility == Visibility.Visible)
            {
                imgPhoto.Visibility = Visibility.Collapsed;
                map.Visibility = Visibility.Visible;
            }
            else
            {
                imgPhoto.Visibility = Visibility.Visible;
                map.Visibility = Visibility.Collapsed;
            }
        }

        private void imgPhoto_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            imgPhoto.Visibility = Visibility.Collapsed;
            map.Visibility = Visibility.Visible;

        }

        private void map_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            dt.Tick -= new EventHandler(dt_Tick);
            dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(4);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();
        }

        private void map_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            dt.Stop();
        }

        private void map_MouseEnter(object sender, MouseEventArgs e)
        {
            dt.Stop();
        }

        private void map_MouseLeave(object sender, MouseEventArgs e)
        {
            dt.Tick -= new EventHandler(dt_Tick);
            dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(3500);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();
        }
    }
}