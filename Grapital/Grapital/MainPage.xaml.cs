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
using Microsoft.Phone.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using System.Device;
using System.Device.Location;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;


namespace Grapital
{
    public partial class MainPage : PhoneApplicationPage
    {
        CameraCaptureTask cameraCaptureTask;
        Byte[] byteArray;
        BitmapImage bmp;
        double latitude, longitude = 0.0;
        GeoCoordinateWatcher watcher;

        public class Loc{
            public string lat;
            public string lng;
        }

        [DataContract]
        public class Item
        {
            [DataMember(Name = "title")] // this must match the json property name
            public string title { get; set; }
            [DataMember(Name = "photo")] // this must match the json property name
            public string photo { get; set; }
            //[DataMember(Name = "loc")] // this must match the json property name
            //public string loc { get; set; }
            [DataMember(Name = "loc")] // this must match the json property name
            public Loc loc;
            [DataMember(Name = "distance")] // this must match the json property name
            public string distance;

            
            public double getDistance(double lat, double lng){ //1 is ad, 2 is me
                GeoCoordinate itemLocation = new GeoCoordinate(Convert.ToDouble(loc.lat.Replace('.', ',')), Convert.ToDouble(loc.lng.Replace('.', ',')));
                GeoCoordinate currentLocation = new GeoCoordinate(lat, lng);
                double distanceInMeter;
                distanceInMeter = currentLocation.GetDistanceTo(itemLocation);
                return distanceInMeter/1000;
            }
        }


        private List<Item> ReadToObject(string json)
        {
            var deserializedElements = new List<Item>();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var ser = new DataContractJsonSerializer(deserializedElements.GetType());
                deserializedElements = ser.ReadObject(ms) as List<Item>;
            }
            return deserializedElements;
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);  // using high accuracy 
                watcher.MovementThreshold = 20; // use MovementThreshold to ignore noise in the signal
                watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            }
            watcher.Start();

            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);

            bmp = new BitmapImage(new Uri("img/cat.jpeg", UriKind.Relative)); // new source address   
            myImage.Source = bmp; // update source of img
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }


        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            geoText.Text = e.Position.Location.Latitude.ToString() + " " + e.Position.Location.Longitude.ToString();
            latitude = e.Position.Location.Latitude;
            longitude = e.Position.Location.Longitude;
        }


        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                WriteableBitmap btmMap = new WriteableBitmap(bmp);
                Extensions.SaveJpeg(btmMap, ms, bmp.PixelWidth, bmp.PixelHeight, 0, 100);
                byteArray = ms.ToArray();
            }
        }

        private void refreshItems()
        {
            var client = new WebClient();
            client.DownloadStringCompleted += (s, ev) => { updateContent(ev.Result); };
            string uri = ("http://grapital.gopagoda.com/api.php/items/" + latitude.ToString().Replace(',', '.') + "/" + longitude.ToString().Replace(',', '.'));
            client.DownloadStringAsync(new Uri(uri));
        }

        private void updateContent(string p)
        {
            List<Item> l = ReadToObject(p);
            for (int i=0; i<l.Count;i++){
                Grid g = new Grid();
                g.Margin = new Thickness(5);
                Image im = new Image();
                im.Margin = new Thickness(2);
                im.Height = 100;
                Uri uri = new Uri(l[i].photo, UriKind.Absolute);
                im.Source = new BitmapImage(uri);

                g.Children.Add(im);
                TextBlock tb = new TextBlock();
                double distance = l[i].getDistance(latitude, longitude);
                tb.Text = numToMoney(distance);
                tb.Margin = new Thickness(10, 120, 0, 0);
                g.Children.Add(tb);
                //spContent.Children.Add(im);
                wrapPanel.Children.Add(g);
            }
        }


        static public string numToMoney(double sum, int decimalPart = 0) // 0 - по настройкам, 2 - два числа после запятой всегда, 4 - четыре числа
        {
            string dotSum = "";
            string IncomeSum = "";
            string[] split = sum.ToString().Split('.');
            string sumt = split[0];
            if (split.Length == 2) dotSum = split[1];

            int j = 1;
            for (int i = sumt.Length - 1; sumt.Length > 0; i--)
            {
                if (j == 4 || j == 8) IncomeSum = ' ' + IncomeSum;
                else
                {
                    IncomeSum = sumt.Substring(sumt.Length - 1, 1) + IncomeSum;
                    sumt = sumt.Remove(sumt.Length - 1);
                }
                j++;
            }
                if (dotSum.Length > 4) dotSum = dotSum.Substring(0, 4);
                if (decimalPart == 0 && dotSum.Length > 2) dotSum = dotSum.Substring(0, 2);
                if (dotSum != "")
                {
                    IncomeSum += '.' + dotSum;
                    if (dotSum.Length == 1) IncomeSum += '0';
                }
                else IncomeSum += ".00";
            return IncomeSum;
        }



        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                bmp.SetSource(e.ChosenPhoto);
                myImage.Source = bmp;

                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        WriteableBitmap btmMap = new WriteableBitmap(bmp);
                        Extensions.SaveJpeg(btmMap, ms, bmp.PixelWidth, bmp.PixelHeight, 0, 100);
                        byteArray = ms.ToArray();
                    }
                }
                catch (ArgumentNullException) { /* Nothing */ }
            }
        }


        private void butAddNew_Click(object sender, RoutedEventArgs e)
        {
            cameraCaptureTask.Show();
        }

        private void butSend_Click(object sender, RoutedEventArgs e)
        {
            butSend.IsEnabled = false;
            butSend.InvalidateMeasure();
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                {"description", "Its very short description."},
                {"photo", byteArray},
                {"title", "wow"},
                {"lat", latitude.ToString()},
                {"lng", longitude.ToString()},
                {"user", "kruil"}
            };
            PostSubmitter post = new PostSubmitter() { url = "http://grapital.gopagoda.com/api.php/items", parameters = data };
            post.uploaded += new PostSubmitter.MyDel(post_uploaded);
            post.Submit();
        }

        void post_uploaded(object sender)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                butSend.IsEnabled = true
            );   
        }

        private void butRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshItems();
        }


   }
}