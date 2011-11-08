using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
using Abbyy.CloudOcrSdk;

namespace GuiTest
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private RestServiceClient restClient;
        private RestServiceClientAsync restClientAsync;


        public Window1()
        {
            InitializeComponent();

            restClient = new RestServiceClient();
            restClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
            restClient.ServerUrl = Properties.Settings.Default.ServerAddress;

            if (!String.IsNullOrEmpty(Properties.Settings.Default.UserName))
                restClient.UserName = Properties.Settings.Default.UserName;

            if (!String.IsNullOrEmpty(Properties.Settings.Default.Password))
                restClient.Password = Properties.Settings.Default.Password;

            restClientAsync = new RestServiceClientAsync(restClient);

            restClientAsync.UploadFileCompleted += UploadCompleted;
            restClientAsync.TaskProcessingCompleted += ProcessingCompleted;
            restClientAsync.DownloadFileCompleted += DownloadCompleted;
            restClientAsync.ListTasksCompleted += TaskListObtained;

            fieldLevelImage.RegionSelected += fieldSelected;
        }

        // Not completed tasks in left list
        private ObservableCollection<UserTask> _userTasks = new ObservableCollection<UserTask>();

        // Completed and failed tasks in right list 
        private ObservableCollection<UserTask> _completedTasks = new ObservableCollection<UserTask>();

        // List of tasks on server
        private ObservableCollection<UserTask> _serverTasks = new ObservableCollection<UserTask>();

        // List of field-level tasks
        private ObservableCollection<UserTask> _fieldLevelTasks = new ObservableCollection<UserTask>();

        public ObservableCollection<UserTask> UserTasks
        {
            get
            {
                return _userTasks;
            }
        }

        public ObservableCollection<UserTask> CompletedTasks
        {
            get
            {
                return _completedTasks;
            }
        }

        public ObservableCollection<UserTask> ServerTasks
        {
            get { return _serverTasks; }
        }

        public ObservableCollection<UserTask> FieldLevelTasks
        {
            get { return _fieldLevelTasks; }
        }

        private void activeTaskList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true) == true)
            {
                string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop, true);

                foreach (string file in filenames)
                {
                    if (File.Exists(file))
                    {
                        addFileTask(file);
                    }

                }
            }
        }

        string getOutputDir()
        {
            string outputDir = Properties.Settings.Default.OutputDirectory;
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            return outputDir;
        }

        void addFileTask(string filePath)
        {
            // Initialize output directory
            string outputDir = getOutputDir();

            ProcessingSettings settings = GetProcessingSettings();

            UserTask task = new UserTask(filePath);
            task.TaskStatus = "Uploading";
            task.OutputFilePath = System.IO.Path.Combine(
                outputDir,
                System.IO.Path.GetFileNameWithoutExtension(filePath) + settings.OutputFileExt);

            _userTasks.Add(task);

            settings.Description = String.Format("{0} -> {1}", 
                System.IO.Path.GetFileName(filePath),
                settings.OutputFileExt);

            restClientAsync.UploadFileAsync(filePath, settings, task);
        }

        // Move task from _userTasks to _completedTasks
        void moveTaskToCompleted(UserTask task)
        {
            _userTasks.Remove(task);
            _completedTasks.Add(task);
        }

        void updateServerTasksList()
        {
            restClientAsync.ListTasksAsync(Guid.NewGuid());
        }

        #region Async client callbacks
        private void UploadCompleted(object sender, UploadCompletedEventArgs e)
        {
            UserTask task = e.UserState as UserTask;
            task.TaskStatus = "Processing";

            task.TaskId = e.Result.Id.ToString();
        }


        private void ProcessingCompleted(object sender, TaskEventArgs e)
        {
            UserTask task = e.UserState as UserTask;

            if (task.SourceIsTempFile)
            {
                File.Delete(task.SourceFilePath);
            }

            if (e.Error != null)
            {
                task.TaskStatus = "Processing error";
                task.OutputFilePath = "<error>";
                task.ErrorMessage = e.Error.Message;
                moveTaskToCompleted(task);
                return;
            }

            if (e.Result.Status == TaskStatus.NotEnoughCredits)
            {
                task.TaskStatus = "Not enough credits";
                moveTaskToCompleted(task);
                return;
            }

            if (e.Result.Status != TaskStatus.Completed)
            {
                task.TaskStatus = "Internal server error";
                moveTaskToCompleted(task);
                return;
            }

            task.TaskStatus = "Downloading";
            // Start downloading
            restClientAsync.DownloadFileAsync(e.Result, task.OutputFilePath, task);
        }

        private void DownloadCompleted(object sender, TaskEventArgs e)
        {
            UserTask task = e.UserState as UserTask;
            if (e.Error != null)
            {
                task.TaskStatus = "Downloading error";
                task.OutputFilePath = "<error>";
                task.ErrorMessage = e.Error.Message;
                moveTaskToCompleted(task);
                return;
            }

            if (task.IsFieldLevel)
            {
                task.RecognizedText = FieldLevelXml.ReadText(task.OutputFilePath);
            }

            task.TaskStatus = "Ready";
            moveTaskToCompleted(task);
        }

        private void TaskListObtained(object sender, ListTaskEventArgs e)
        {
            if (e.Error == null)
            {
                Task[] serverTasks = e.Result;

                // move to ServerTasks collection
                ServerTasks.Clear();
                foreach (Task task in serverTasks.OrderByDescending(t => t.RegistrationTime))
                {
                    UserTask userTask = new UserTask(task);

                    ServerTasks.Add(userTask);
                }
            }
            else
            {
                MessageBox.Show("Cannot obtain list of server tasks:\n" + e.Error.Message, "error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Interface with Abbyy Online SDK
        ProcessingSettings GetProcessingSettings()
        {
            ProcessingSettings result = new ProcessingSettings();
            result.Language = getLanguage();
            result.OutputFormat = getOutputFormat();
            return result;
        }

        RecognitionLanguage getLanguage()
        {
            if( langEn.IsChecked == true )
                return RecognitionLanguage.English;
            else if( langFr.IsChecked == true )
                return RecognitionLanguage.French;
            else if( langIt.IsChecked == true )
                return RecognitionLanguage.Italian;
            else if( langDe.IsChecked == true )
                return RecognitionLanguage.German;
            else if( langEs.IsChecked == true )
                return RecognitionLanguage.Spanish;
            else if( langRu.IsChecked == true )
                return RecognitionLanguage.Russian;
            else if( langZh.IsChecked == true )
                return RecognitionLanguage.ChinesePRC;
            else if( langJa.IsChecked == true )
                return RecognitionLanguage.Japanese;
            else if( langKo.IsChecked == true )
                return RecognitionLanguage.Korean;

            return RecognitionLanguage.English;
        }

        OutputFormat getOutputFormat()
        {
            if( formatPdfSearchable.IsChecked == true )
                return OutputFormat.pdfSearchable;
            else if( formatPdfText.IsChecked == true )
                return OutputFormat.pdfTextAndImages;
            else if( formatTxt.IsChecked == true )
                return OutputFormat.txt;
            else if( formatDocx.IsChecked == true )
                return OutputFormat.docx;
            else if( formatPptx.IsChecked == true )
                return OutputFormat.pptx;
            else if( formatRtf.IsChecked == true )
                return OutputFormat.rtf;
            else if( formatXlsx.IsChecked == true )
                return OutputFormat.xlsx;
            else if( formatXml.IsChecked == true )
                return OutputFormat.xml;

            return OutputFormat.pdfSearchable;
        }

        #endregion

        private void completedTaskList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if( completedTaskList.SelectedItem == null )
                return;
            UserTask activeTaskItem = completedTaskList.SelectedItem as UserTask;

            if (System.IO.File.Exists(activeTaskItem.OutputFilePath))
            {
                System.Diagnostics.Process.Start(activeTaskItem.OutputFilePath);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.Source is TabControl))
                return;

            // When entering 'Server' tab, update information from server
            if (!serverTasksTab.IsSelected)
                return;

            updateServerTasksList();
        }

        #region Field-level tasks

        private void fieldLevelImage_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true) == true)
            {
                string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop, true);

                foreach (string file in filenames)
                {
                    fieldLevelImage.Source = file;
                    return;
                }
            }
        }

        private void fieldSelected(object sender, RegionSelectedEventArgs e)
        {
            string tempFilePath = System.IO.Path.GetTempFileName();
            e.CroppedImage.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Tiff);

            string outputDir = getOutputDir();

            UserTask task = new UserTask(tempFilePath);
            task.TaskStatus = "Uploading";
            task.SourceIsTempFile = true;
            task.IsFieldLevel = true;

            // TODO: correct output name
            task.OutputFilePath = System.IO.Path.Combine(
                outputDir,
                "field-level" + System.IO.Path.GetRandomFileName() + ".xml");

            task.SourceImage = e.CroppedImage;

            _userTasks.Add(task);
            _fieldLevelTasks.Add(task);

            

            // Select mode: text, barcode, checkmark
            if (flModeText.IsChecked == true)
            {
                TextFieldProcessingSettings settings = new TextFieldProcessingSettings();
                restClientAsync.ProcessTextFieldAsync(tempFilePath, settings, task);
            }
            else if (flModeBarcode.IsChecked == true)
            {
                BarcodeFieldProcessingSettings settings = new BarcodeFieldProcessingSettings();
                restClientAsync.ProcessBarcodeFieldAsync(tempFilePath, settings, task);
            }
            else
            {
                CheckmarkFieldProcessingSettings settings = new CheckmarkFieldProcessingSettings();
                string userSettings = Properties.Settings.Default.CheckmarkOptions;
                if (!String.IsNullOrEmpty(userSettings))
                    settings.Params = userSettings;

                restClientAsync.ProcessCheckmarkFieldAsync(tempFilePath, settings, task);
            }

            // temp file will be deleted in ProcessingCompleted callback
        }

        #endregion

    }


    public class UserTask : INotifyPropertyChanged
    {
        public UserTask(string filePath)
        {
            SourceFilePath = filePath;
            TaskId = "<unknown>";
            TaskStatus = "<initializing>";
            SourceIsTempFile = false;
        }

        public UserTask(Task task)
        {
            SourceFilePath = null;
            TaskId = task.Id.ToString();
            TaskStatus = task.Status.ToString();
            PagesCount = task.PagesCount;
            Description = task.Description;
            RegistrationTime = task.RegistrationTime;
            StatusChangeTime = task.StatusChangeTime;

            SourceIsTempFile = false;
        }

        public bool SourceIsTempFile
        {
            get;
            set;
        }

        public string SourceFilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                if (!String.IsNullOrEmpty(value))
                {
                    _fileName = System.IO.Path.GetFileName(_filePath);
                }
                else
                {
                    _fileName = null;
                }
            }
        }


        public string SourceFileName { get { return _fileName; } }
        public string TaskId
        {
            get
            {
                return _taskId;
            }
            set
            {
                _taskId = value;
                NotifyPropertyChanged("TaskId");
            }
        }

        public string TaskStatus
        {
            get
            {
                return _taskStatus;
            }
            set
            {
                _taskStatus = value;
                NotifyPropertyChanged("TaskStatus");
            }
        }


        public string OutputFilePath
        {
            get
            {
                return _outputFilePath;
            }
            set
            {
                _outputFilePath = value;
                NotifyPropertyChanged("OutputFilePath");
            }
        }

        public int PagesCount
        {
            get { return _pagesCount; }
            set { _pagesCount = value; NotifyPropertyChanged("PagesCount"); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; NotifyPropertyChanged("Description"); }
        }

        public DateTime RegistrationTime
        {
            get { return _registrationTime; }
            set
            {
                _registrationTime = value;
                NotifyPropertyChanged("RegistrationTime");
            }
        }

        public DateTime StatusChangeTime
        {
            get { return _statusChangeTime; }
            set
            {
                _statusChangeTime = value;
                NotifyPropertyChanged("StatusChangeTime");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public bool IsFieldLevel
        {
            get;
            set;
        }

        public string RecognizedText
        {
            get { return _recognizedText; }
            set { _recognizedText = value; NotifyPropertyChanged("RecognizedText"); }
        }

        public System.Drawing.Image SourceImage
        {
            get { return _sourceImage; }
            set { _sourceImage = value; NotifyPropertyChanged("SourceImage"); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; NotifyPropertyChanged("ErrorMessage"); }
        }

        private string _filePath;
        private string _fileName;
        private string _taskId;
        private string _taskStatus;
        private string _outputFilePath;

        private int _pagesCount;
        private string _description;


        private DateTime _registrationTime;
        private DateTime _statusChangeTime;

        private string _recognizedText = null;
        private System.Drawing.Image _sourceImage;

        private string _errorMessage;
    }

       
}
