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
    public class MyResourcesWrapper
    {
        public MyResourcesWrapper()
        {
        }

        private static MyResources _myWrappedResources = new MyResources();

        public MyResources MyWrappedResources { get { return _myWrappedResources; } }
    }
}
