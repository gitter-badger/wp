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

namespace Grapital
{
    public class GV
    {
        public static int accuracy = 500;
        public static char decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
        //public static string server = "http://127.0.0.1";
        public static string server = "http://grapital.com";
        public static string xDebug = "w=1";//"XDEBUG_SESSION_START=1";
    }
}
