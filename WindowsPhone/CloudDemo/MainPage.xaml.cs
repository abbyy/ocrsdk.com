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

namespace CloudDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        CameraCaptureTask cameraCaptureTask;
        PhotoChooserTask photoChooserTask;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);

            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);

        }

        private void fromCamera_Click(object sender, RoutedEventArgs e)
        {
            cameraCaptureTask.Show();
        }

        private void fromFile_Click(object sender, RoutedEventArgs e)
        {
            photoChooserTask.Show();
        }

        void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK)
            {
                MessageBox.Show("Cannot get a file to upload");
                return;
            }

            // Save image to be processed
            AppData.Instance.Image = e.ChosenPhoto;

            // Process the image
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/ProcessingPage.xaml", UriKind.RelativeOrAbsolute));
            });
        }
    }
}