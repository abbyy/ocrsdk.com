using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GuiTest
{
    /// <summary>
    /// Interaction logic for ImageControl.xaml
    /// </summary>
    public partial class ImageControl : UserControl
    {
        private string _sourceFile;

        public ImageControl()
        {
            InitializeComponent();
            selectBox.Visibility = Visibility.Hidden;
        }

        public string Source
        {
            set { image.Source = new BitmapImage(new Uri(value));
            _sourceFile = value;
            }
        }

        public event EventHandler<RegionSelectedEventArgs> RegionSelected;

        private void OnRegionSelected( RegionSelectedEventArgs e )
        {
            if (RegionSelected != null)
                RegionSelected(this, e);
        }

        private bool hasImage()
        {
            return image.Source != null;
        }

        private Point imageCaptureStart;

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            image.ReleaseMouseCapture();
           // Get actual rectangle
            if( selectBox.Visibility != Visibility.Visible)
                return;

            Vector imageOffset = VisualTreeHelper.GetOffset(image);
            double visualLeft = Canvas.GetLeft( selectBox );
            double visualTop = Canvas.GetTop(selectBox);

            double width = selectBox.Width;
            double height = selectBox.Height;

            BitmapSource bmpSource = image.Source as BitmapSource;

            double scaleX = bmpSource.PixelWidth / image.ActualWidth;
            double scaleY = bmpSource.PixelHeight / image.ActualHeight;

            double newWidth = width * scaleX;
            double newHeight = height * scaleY;

            if ((int)newWidth == 0 || (int)newHeight == 0)
                // Doesn't make sense, further code can't handle this.
                return;

            double newX = (visualLeft - imageOffset.X) * scaleX;
            double newY = (visualTop - imageOffset.Y) * scaleY;

            System.Drawing.Bitmap src = System.Drawing.Image.FromFile(_sourceFile) as System.Drawing.Bitmap;
            var target = new System.Drawing.Bitmap((int)newWidth, (int)newHeight);
            target.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            using (var g = System.Drawing.Graphics.FromImage(target))
            {
                var rect = new System.Drawing.Rectangle((int)newX, (int)newY, (int)newWidth, (int)newHeight);
                g.DrawImage(src, new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
                                 rect,
                                 System.Drawing.GraphicsUnit.Pixel);
            }

            RegionSelectedEventArgs ev = new RegionSelectedEventArgs(new Rect(newX, newY, newWidth, newHeight), target);
            OnRegionSelected(ev);

            selectBox.Visibility = Visibility.Hidden;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!hasImage())
                return;

            // Special processing for double click
            if (e.ClickCount == 2)
            {
            }
            else
            {
                imageCaptureStart = e.GetPosition(canvas);
                image.CaptureMouse();
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!hasImage())
                return;

            if (!image.IsMouseCaptured) return;

            Vector imageOffset = VisualTreeHelper.GetOffset(image);
            Point currentPosition = e.GetPosition(canvas);


            // Do not move outside image region
            if( currentPosition.X < imageOffset.X )
                currentPosition.X = imageOffset.X;

            if (currentPosition.Y < imageOffset.Y)
                currentPosition.Y = imageOffset.Y;

            if (currentPosition.X - imageOffset.X > image.ActualWidth)
                currentPosition.X = imageOffset.X + image.ActualWidth;

            if (currentPosition.Y - imageOffset.Y > image.ActualHeight)
                currentPosition.Y = imageOffset.Y + image.ActualHeight;

            // Draw rectangle on image
            selectBox.Visibility = Visibility.Visible;
            selectBox.Stroke = new SolidColorBrush(Colors.Gray);

            selectBox.Width = Math.Abs(currentPosition.X - imageCaptureStart.X);
            selectBox.Height = Math.Abs(currentPosition.Y - imageCaptureStart.Y);
            Canvas.SetLeft(selectBox, Math.Min(imageCaptureStart.X, currentPosition.X));
            Canvas.SetTop(selectBox, Math.Min(imageCaptureStart.Y, currentPosition.Y));

        }

    }

    public class RegionSelectedEventArgs : EventArgs
    {
        public RegionSelectedEventArgs(Rect r, System.Drawing.Bitmap cropped)
        {
            SelectedRectangle = r;
            CroppedImage = cropped;
        }

        public Rect SelectedRectangle
        {
            get;
            set;
        }

        public System.Drawing.Bitmap CroppedImage
        {
            get;
            set;
        }
    }
}
