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
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Device.Location;
using System.Diagnostics;


namespace Grapital
{
    public class Loc
    {
        public string lat;
        public string lng;

        public double getLat(){
            return Convert.ToDouble(lat.Replace('.',GV.decimalSeparator));
        }

        public double getLng()
        {
            return Convert.ToDouble(lng.Replace('.', GV.decimalSeparator));
        }
    }

    [DataContract]
    public class Item
    {
        [DataMember(Name = "photo")] // this must match the json property name
        public string photo { get; set; }
        [DataMember(Name = "description")] // this must match the json property name
        public string description { get; set; }
        [DataMember(Name = "loc")] // this must match the json property name
        public Loc loc;
        [DataMember(Name = "condition")] // this must match the json property name
        public int condition { get; set; }
        [DataMember(Name = "date")] // this must match the json property name
        public string date;
        [DataMember(Name = "user")] // this must match the json property name
        public string user;
        [DataMember(Name = "id")] // this must match the json property name
        public string id;


        public DateTime getDate()
        {
            return Convert.ToDateTime(date);
        }

        public Image image = new Image();


        public double getDistanceDouble(double lat, double lng)
        { //1 is ad, 2 is me
            GeoCoordinate itemLocation = new GeoCoordinate(Convert.ToDouble(loc.lat.Replace('.', GV.decimalSeparator)), Convert.ToDouble(loc.lng.Replace('.', GV.decimalSeparator)));
            GeoCoordinate currentLocation = new GeoCoordinate(lat, lng);
            double distanceInMeter;
            distanceInMeter = currentLocation.GetDistanceTo(itemLocation);
            if (distanceInMeter < 1) distanceInMeter = 1.12;
            return distanceInMeter / 1000;
        }

        public string getDistanceString()
        { //1 is ad, 2 is me
            var app = (App.Current as App);
            return doubleToDistance(getDistanceDouble(app.latitude, app.longitude));
        }

        static public string doubleToDistance(double sum)
        {
            String distance = "";
            String[] sumArray;
            sumArray = sum.ToString().Split(GV.decimalSeparator);

            if (sumArray[0] == "0" && sumArray.Length == 1) return "0" + MyResources.m;
            String a = (sumArray[1][0] + sumArray[1][1] + "0" + MyResources.m);


            if (sumArray[0] == "0")
            {
                double d = sum * 10;
                int dd = Convert.ToInt32(d);
                if (dd == 0) dd = 1;
                return (dd.ToString() + "00"+MyResources.m);
            }

            if (sumArray[0].Length > 1) distance = sumArray[0] + MyResources.km;
            else
                distance = sumArray[0] + "." + sumArray[1][0] + MyResources.km;
            return distance;
        }
    }
}
