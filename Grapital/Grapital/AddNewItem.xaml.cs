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
using System.IO;
using System.Diagnostics;
using MarkHeath.StarRating;

namespace Grapital
{
    public partial class AddNewItem : PhoneApplicationPage
    {
        BitmapImage bmp;
        CameraCaptureTask cameraCaptureTask;
        Byte[] byteArray;
        bool isCaptured = false;
        

        public AddNewItem()
        {
            InitializeComponent();
            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
            bmp = new BitmapImage(new Uri("img/addImage.png", UriKind.Relative)); // new source address 
            bmp.ImageOpened += new EventHandler<RoutedEventArgs>(bmp_ImageOpened);
            tbDescription.TextChanged += new TextChangedEventHandler(tbDescription_TextChanged);
            imgPhoto.Source = bmp; // update source of img
            butSend.Style = App.Current.Resources["roundedButton"] as Style;
            butSend.Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0));
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            textBlockCondition.Text = MyResources.Condition + ": " + MyResources.Used;
            butSend.Content = MyResources.Publish;
        }


        void tbDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbRemain.Text = Convert.ToString(100 - tbDescription.Text.Length);
            if (tbRemain.Text == "0")
            {
                tbRemain.Opacity = 1;
                tbRemain.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                tbRemain.Opacity = 0.5;
                tbRemain.Foreground = new SolidColorBrush(Colors.White);
            }
            if (tbDescription.Text.Length > 100) tbDescription.Text = tbDescription.Text.Remove(100);
            tbDescription.Select(tbDescription.Text.Length, 0);
        }


        void bmp_ImageOpened(object sender, RoutedEventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            WriteableBitmap btmMap = new WriteableBitmap(bmp);
            btmMap.SaveJpeg(ms, 800, 800, 0, 90);
            byteArray = ms.ToArray(); 
        }


        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                bmp.SetSource(e.ChosenPhoto);
                imgPhoto.Source = bmp;
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        WriteableBitmap btmMap = new WriteableBitmap(bmp);
                        int minSize;
                        if (btmMap.PixelHeight > btmMap.PixelWidth) minSize = btmMap.PixelWidth;
                        else minSize = btmMap.PixelHeight;
                        int sH = (btmMap.PixelHeight / 2) - (minSize / 2) + 1;
                        int sW = (btmMap.PixelWidth / 2) - (minSize / 2) + 1;
                        btmMap = System.Windows.Media.Imaging.WriteableBitmapExtensions.Crop(btmMap, sW, sH, minSize - 5, minSize - 5);
                        imgPhoto.Source = btmMap;
                        btmMap.SaveJpeg(ms, 800, 800, 0, 90);
                        byteArray = ms.ToArray();
                    }
                    isCaptured = true;
                }
                catch (ArgumentNullException) { /* Nothing */ }
            }
        }


        private void imgPhoto_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            cameraCaptureTask.Show();
        }


        private void butSend_Click(object sender, RoutedEventArgs e)
        {
            App app = (App.Current as App);
            if (isCaptured == false) { MessageBox.Show(MyResources.PleaseAddPhoto); return; }
            if (tbDescription.Text.Length==0) { MessageBox.Show(MyResources.PleaseAddDescription); return; }
            butSend.IsEnabled = false;
            butSend.InvalidateMeasure();
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                {"description", tbDescription.Text},
                {"photo", byteArray},
                {"lat", app.latitude.ToString().Replace(",",".")},
                {"lng", app.longitude.ToString().Replace(",",".")},
                {"user", app.settings["email"].ToString()},
                {"condition", starControl.Rating},
                {"date", DateTime.Now}
            };
            Debug.WriteLine(app.latitude.ToString());
            Debug.WriteLine(app.longitude.ToString());
            PostSubmitter post = new PostSubmitter() { url = GV.server+"/api.php/items?"+GV.xDebug, parameters = data };
            post.uploaded += new MyDel(post_uploaded);
            post.Submit();
        }


        void post_uploaded(object sender)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                (App.Current as App).newItemAdded = true;
                butSend.IsEnabled = true;
                (Application.Current.RootVisual as PhoneApplicationFrame).GoBack();
            }
            );
        }


        private void starControl_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            int v = (sender as StarRatingControl).Rating;
            Debug.WriteLine(v);
            int newv = Convert.ToInt16((Math.Ceiling((sender as StarRatingControl).Rating / 2.0) * 2));
            if (newv == 0) newv = 2;
            (sender as StarRatingControl).Rating = newv;
            string cond = MyResources.Condition+": "+ MyResources.Wretched;
            if (v > 2) cond = MyResources.Condition + ": " + MyResources.Used;
            if (v > 4) cond = MyResources.Condition + ": " + MyResources.New;
            textBlockCondition.Text = cond;
        }

    }
}