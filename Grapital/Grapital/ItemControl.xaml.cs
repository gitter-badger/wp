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
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Diagnostics;


namespace Grapital
{
    public partial class ItemControl : UserControl
    {
        Item item;
        int numberIncoolection;

        public ItemControl()
        {
            InitializeComponent();
        }

        public ItemControl(Item item, int i)
        {
            InitializeComponent();
            numberIncoolection = i;
            this.item = item;
            Uri uri = new Uri(item.photo, UriKind.Absolute);
            image.Source = new BitmapImage(uri);
            distance.Text = this.item.getDistanceString();
            if (item.user == (App.Current as App).settings["email"].ToString()) grid.Background = new SolidColorBrush(Color.FromArgb(255,57,150,25));
        }

        private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/ItemInfoPage.xaml?id=" + numberIncoolection.ToString(), UriKind.Relative));
        }

    }
}
