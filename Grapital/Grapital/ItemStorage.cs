using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Diagnostics;


namespace Grapital
{
    public class ItemStorage
    {
        public List<Item> items;
        public bool autoRefreshed;
        //EVENTS
        
        public event MyDel refreshed;
        public event MyDel startRefreshing;


        public ItemStorage()
        {
            autoRefreshed = false;
        }


        protected virtual void RaiseRefreshed()
        {
            if (refreshed != null) refreshed(this);
        }

        protected virtual void RaiseStartRefreshing()
        {
            if (startRefreshing != null) startRefreshing(this);
        }

        public void refreshItems()
        {
            if (autoRefreshed == false)
            {
                autoRefreshed = true;
                Debug.WriteLine("ItemRefresh Started");
                RaiseStartRefreshing();
                App app = (App.Current as App);
                var client = new WebClient();
                client.DownloadStringCompleted += (s, ev) => {
                    try { parseJsonToItems(ev.Result);}
                    catch { MessageBox.Show("Please check Internet connection."); RaiseRefreshed(); };
                };
                string uri = (GV.server+"/api.php/items/" + app.latitude.ToString().Replace(',', '.') + "/" + app.longitude.ToString().Replace(',', '.'));
                Debug.WriteLine(uri);
                client.DownloadStringAsync(new Uri(uri));
            }
        }


        private void parseJsonToItems(string json)
        {
            var deserializedElements = new List<Item>();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var ser = new DataContractJsonSerializer(deserializedElements.GetType());
                deserializedElements = ser.ReadObject(ms) as List<Item>;
            }
            items =  deserializedElements;
            RaiseRefreshed();
        }
    }
}
