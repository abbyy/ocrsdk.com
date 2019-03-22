using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using Abbyy.CloudOcrSdk;

namespace CloudDemo
{
    public partial class ProcessingPage : PhoneApplicationPage
    {
        RestServiceClientAsync abbyyClient;

        public ProcessingPage()
        {
            InitializeComponent();

            RestServiceClient syncClient = new RestServiceClient();
            #error Please provide application id and password and remove this line !!!
            // To create an application and obtain a password,
            // register at https://cloud.ocrsdk.com/Account/Register
            // More info on getting your application id and password at
            // https://ocrsdk.com/documentation/faq/#faq3

			// Name of application you created
            syncClient.ApplicationId = "";
			// Password should be sent to your e-mail after application was created
            syncClient.Password = "";

            abbyyClient = new RestServiceClientAsync(syncClient);

            abbyyClient.UploadFileCompleted += UploadCompleted;
            abbyyClient.TaskProcessingCompleted += ProcessingCompleted;
            abbyyClient.DownloadFileCompleted += DownloadCompleted;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Stream imageStream = AppData.Instance.Image;
            if (imageStream == null)
                return;

            string localPath = "image.jpg";
            saveImageToFile(imageStream, localPath);

            ProcessingSettings settings = new ProcessingSettings();
            settings.SetLanguage("English,Russian");
            settings.OutputFormat = OutputFormat.txt;

            displayMessage("Uploading..");
            abbyyClient.ProcessImageAsync(localPath, settings, settings);
        }

        private void saveImageToFile(Stream imageStream, string localPath)
        {
            imageStream.Seek(0, SeekOrigin.Begin);

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists(localPath))
                    storage.DeleteFile(localPath);

                using (IsolatedStorageFileStream file = storage.CreateFile(localPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.SetSource(imageStream);
                    WriteableBitmap wb = new WriteableBitmap(bitmap);
                    wb.SaveJpeg(file, wb.PixelWidth, wb.PixelHeight, 0, 85);
                }
            }
        }

        private void UploadCompleted(object sender, UploadCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
           {
               displayMessage("Upload completed. Processing..");
           }
           );
        }

        private void ProcessingCompleted(object sender, TaskEventArgs e)
        {
            if (e.Error != null)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    displayMessage("Processing error: " + e.Error.Message);
                }
                );
                return;
            }

            Dispatcher.BeginInvoke(() =>
                {
                    displayMessage("Processing completed. Downloading..");
                }
                );

            // Download a file
            string outputPath = "result.txt";
            OcrSdkTask task = e.Result;
            abbyyClient.DownloadFileAsync(task, outputPath, outputPath);
        }

        private void DownloadCompleted(object sender, TaskEventArgs e)
        {
            string message = "";
            if (e.Error != null)
            {
                message = "Error downloading: " + e.Error.Message;
            }
            else
            {
                message = "Downloaded.\nResult:";

                string txtFilePath = e.UserState as string;
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream file = storage.OpenFile(txtFilePath, FileMode.Open, FileAccess.Read);
                using (StreamReader reader = new StreamReader(file))
                {
                    message += reader.ReadToEnd();
                }
            }

            Dispatcher.BeginInvoke(() =>
            {
                displayMessage(message);
            }
            );
        }

        private void displayMessage(string text)
        {
            TextBlock block = new TextBlock();
            block.TextWrapping = TextWrapping.Wrap;
            block.Text = text;

            ContentPanel.Children.Add(block);
        }
    }
}
