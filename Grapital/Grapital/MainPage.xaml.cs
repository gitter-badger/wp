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
using Microsoft.Phone.Shell;
using Coding4Fun.Phone.Controls;

namespace Grapital
{
    public partial class MainPage : PhoneApplicationPage
    {
        ApplicationBarMenuItem settingsMenu;
        ApplicationBarMenuItem aboutMenu;
        MessagePrompt prompt;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
            settingsMenu = new ApplicationBarMenuItem();
            settingsMenu.Text = MyResources.Settings;
            settingsMenu.Click += new EventHandler(menuSettings_Click);
            ApplicationBar.MenuItems.Add(settingsMenu);

            aboutMenu = new ApplicationBarMenuItem();
            aboutMenu.Text = MyResources.About;
            aboutMenu.Click += new EventHandler(aboutMenu_Click);
            ApplicationBar.MenuItems.Add(aboutMenu);
        }


        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            App app = (App.Current as App);
            while (NavigationService.BackStack.Any()) NavigationService.RemoveBackEntry();
            if (wrapPanel.Children.Count==0) updateContent();
            if (app.newItemAdded == true)
            {
                app.newItemAdded = false;
                app.itemStorage.autoRefreshed = false;
                app.itemStorage.refreshItems();
                app.itemStorage.refreshed += new MyDel(itemStorage_refreshed);
            }
            if (app.settings["friendVerification"].ToString() != "Ok" && app.settings["emailCodeVerification"].ToString() == "Ok")
            {
                //check if friend is truly friend
                var client = new WebClient();
                client.DownloadStringCompleted += (s, ev) =>
                {
                    try { if (ev.Result == "verified") app.settings["friendVerification"] = "Ok"; }
                    catch { MessageBox.Show("Please check Internet connection.");};
                };
                string uri = (GV.server + "/api.php/mailVerify/" + app.settings["email"]);
                client.DownloadStringAsync(new Uri(uri));

            }
        }

        void itemStorage_refreshed(object sender)
        {
            (App.Current as App).itemStorage.refreshed -= new MyDel(itemStorage_refreshed);
            wrapPanel.Children.Clear();
            updateContent();
            Debug.WriteLine("listRedrawed");
        }


        private void updateContent()
        {
            App app = (App.Current as App);
            List<Item> l = new List<Item>();

            if (app.itemStorage.items!=null) l = app.itemStorage.items;
            BitmapImage bmp = new BitmapImage(new Uri("img/addImage.png", UriKind.Relative)); // new source address 
            Image im = new Image();
            im.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(im_Tap);
            im.Margin = new Thickness(0,0,1,1);
            im.Height = 119;
            im.Width = 119;
            im.Source = bmp;
            wrapPanel.Children.Add(im);
            for (int i=0; i<l.Count;i++){
                ItemControl ic = new ItemControl(l[i], i);
                wrapPanel.Children.Add(ic);
            }
        }

        void im_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if ((App.Current as App).settings["emailCodeVerification"].ToString() == "Ok" && (App.Current as App).settings["friendVerification"].ToString() == "Ok") NavigationService.Navigate(new Uri("/AddNewItem.xaml", UriKind.Relative));
            else
            {
                if ((App.Current as App).settings["emailCodeVerification"].ToString() != "Ok") MessageBox.Show(MyResources.CompleteRegistration); else MessageBox.Show(MyResources.AskFriendVerify);
            }
        }


        private void butRefresh_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).itemStorage.refreshItems();
        }



        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void menuSettings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }


        private void butRefresh_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            (App.Current as App).newItemAdded = false;
            (App.Current as App).itemStorage.autoRefreshed = false;
            (App.Current as App).itemStorage.refreshItems();
            (App.Current as App).itemStorage.refreshed += new MyDel(itemStorage_refreshed);
        }


        void aboutMenu_Click(object sender, EventArgs e)
        {
            prompt = new MessagePrompt();
            prompt.ActionPopUpButtons.Clear();
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("img/popupHelpBackground.jpg", UriKind.Relative));
            imageBrush.Stretch = Stretch.UniformToFill;
            prompt.BorderThickness = new Thickness(0);
            prompt.Background = imageBrush;
            prompt.Body = new AboutUserControl();
            prompt.Show();
        }
   }
}