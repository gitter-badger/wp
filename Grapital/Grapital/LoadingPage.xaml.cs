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

namespace Grapital
{
    public partial class LoadingPage : PhoneApplicationPage
    {
        public LoadingPage()
        {
            InitializeComponent();
            (App.Current as App).itemStorage.refreshed += new MyDel(itemStorage_refreshed);
            (App.Current as App).itemStorage.startRefreshing+=new MyDel(itemStorage_startRefreshing);
            if ((App.Current as App).itemStorage.items != null)
            {
                (App.Current as App).itemStorage.refreshed -= new MyDel(itemStorage_refreshed);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            tbStatus.Text = MyResources.FindingLocation;
            Loaded += new RoutedEventHandler(LoadingPage_Loaded);
            
        }

        void LoadingPage_Loaded(object sender, RoutedEventArgs e)
        {
            (App.Current as App).watcher.Start();
        }

        void itemStorage_startRefreshing(object sender)
        {
            tbStatus.Text = MyResources.LoadingObjects;
        }

        void itemStorage_refreshed(object sender)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}