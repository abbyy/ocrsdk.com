using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CloudDemo
{
    public class AppData
    {
        private static AppData instance = new AppData();

        public static AppData Instance { get { return instance; } }

        public Stream Image { get; set; }
    }
}
