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
using System.Diagnostics;
using System.IO;


namespace Grapital
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        WebClient client;

        public SettingsPage()
        {
            InitializeComponent();
            tbEmail.Text = (App.Current as App).settings["email"].ToString();
            tbEmail.TextChanged += new TextChangedEventHandler(tbEmail_TextChanged);
            tbEmailInvitation.Text = (App.Current as App).settings["emailInvitation"].ToString();
            tbEmailInvitation.TextChanged += new TextChangedEventHandler(tbEmailInvitation_TextChanged);
            client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
        }

        protected override void  OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if ((App.Current as App).settings["emailCodeVerification"].ToString() == "Ok")
            {
                tbCode.IsEnabled = false;
                tbCode.Visibility = Visibility.Collapsed;
                tbText3.Visibility = Visibility.Collapsed;
                butVerify.Visibility = Visibility.Collapsed;
                tbEmail.IsEnabled = false;
                tbEmailInvitation.IsEnabled = false;

                if ((App.Current as App).settings["friendVerification"].ToString() == "Ok") tbStatus.Text = MyResources.LoggedIn;
            }
            else
            {
                if (tbEmail.Text == "") tbCode.IsEnabled = false;
                butVerify.Visibility = Visibility.Visible;
                tbCode.Visibility = Visibility.Visible;
                tbText3.Visibility = Visibility.Visible;
                tbStatus.Text = MyResources.LoggedOut;
                butSignOut.Visibility = Visibility.Collapsed;
            }
 	        base.OnNavigatedTo(e);
        }

        void tbEmailInvitation_TextChanged(object sender, TextChangedEventArgs e)
        {
            (App.Current as App).settings["emailInvitation"] = tbEmailInvitation.Text;
        }

        void tbEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            App app = App.Current as App;
            if (app.settings["email"].ToString() != tbEmail.Text)
            {
                app.settings["emailCodeVerification"] = "";
                (App.Current as App).settings["email"] = tbEmail.Text;
            }
        }

        private void butVerify_Click(object sender, RoutedEventArgs e)
        {
            string uri = GV.server + "/api.php/mailAdd/"+tbEmail.Text+"/"+tbEmailInvitation.Text;
            client.DownloadStringAsync(new Uri(uri));
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Result.Contains("code:"))
            {
                Debug.WriteLine(e.Result);
                (App.Current as App).settings["emailCodeVerification"] = e.Result.Remove(0, 5);
                tbCode.IsEnabled = true;
                if ((App.Current as App).settings["email"].ToString() == "test@grapital.com") (App.Current as App).settings["emailCodeVerification"] = "P1MSDW";
                MessageBox.Show(String.Format(MyResources.EmailActivateSended, tbEmail.Text, tbEmailInvitation.Text));
            }
            if (e.Result.Contains("error1")) MessageBox.Show(MyResources.CheckFriendEmail);
        }

        private void tbCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbCode.Text == (App.Current as App).settings["emailCodeVerification"].ToString())
            {
                (App.Current as App).settings["emailCodeVerification"] = "Ok";
                MessageBox.Show("Your account registered.");
                tbCode.IsEnabled = false;
                tbEmail.IsEnabled = false;
                tbEmailInvitation.IsEnabled = false;
                butVerify.Visibility = Visibility.Collapsed;
                tbCode.Visibility = Visibility.Collapsed;
                tbText3.Visibility = Visibility.Collapsed;
                butSignOut.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void butSignOut_Click(object sender, RoutedEventArgs e)
        {
            App app = App.Current as App;
            app.settings["email"] = "";
            app.settings["emailInvitation"] = "";
            app.settings["emailCodeVerification"] = "";
            tbEmail.Text = "";
            tbEmailInvitation.Text = "";
            tbStatus.Text = MyResources.LoggedOut;
            tbCode.IsEnabled = false;
            tbCode.Text = "";
            butVerify.Visibility = Visibility.Visible;
            tbCode.Visibility = Visibility.Visible;
            tbText3.Visibility = Visibility.Visible;
            tbEmail.IsEnabled = true;
            tbEmailInvitation.IsEnabled = true;
            butSignOut.Visibility = System.Windows.Visibility.Collapsed;
            (App.Current as App).settings["friendVerification"] = "";
        }
    }
}