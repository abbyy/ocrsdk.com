using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Xml;

namespace Sample
{
    /// <summary>
    /// Summary description for MainForm.
    /// </summary>
    public class MainForm : System.Windows.Forms.Form
    {
        private TextBox imageFileTextBox;
        private ComboBox countryComboBox;
        private Label imageFileLabel;
        private Button browseButton;
        private Label countryLabel;
        private SplitContainer sourceDataSplitContainer;
        private SplitContainer resultSplitContainer;
        private Panel sourceFileTopPanel;
        private Panel sourceFilePanel;
        private Label sourceFileLabel;
        private SplitContainer resultDataSplitContainer;
        private Panel receiptPanel;
        private Panel receiptTopPanel;
        private Label receiptLabel;
        private Panel textTopPanel;
        private Label textLabel;
        private Panel textPanel;
        private PictureBox sourceFilePictureBox;
        private PictureBox receiptPictureBox;
        private Label promptLabel;
        private OpenFileDialog openFileDialog;
        private Processor processor;
        private int activeFieldIndex;
        private float sourceScaleToFit;
        private float receiptScaleToFit;
        private Dictionary<String, CountryOfOrigin> receiptCountries;
        private Label vendorNameLabelTitle;
        private Label vendorNameLabel;
        private Label dateTimeLabelTitle;
        private Label dateTimeLabel;
        private Label totalSumLabel;
        private Label totalSumLabelTitle;
        private Label phoneFaxLabel;
        private Label phoneFaxLabelTitle;
        private Label addressLabel;
        private Label addressLabelTitle;
        private Color activeFieldColor;
        private Label startRecognitionLabel;
        private Panel arrowPanel;
        private CheckBox treatAsPhotoCheckBox;
        private Label subtotalLabel;
        private Label subtotalLabelTitle;
        private Label totalTaxLabel;
        private Label totalTaxLabelTitle;
        private Label tenderLabel;
        private Label tenderLabelTitle;
        private Label purchaseTypeLabel;
        private Label purchaseTypeLabelTitle;
        private Label bankCardLabel;
        private Label bankCardLabelTitle;
        private TableLayoutPanel taxFieldsPanel;
        private Label taxFieldsLabel;
        private TableLayoutPanel lineItemsPanel;
        private Label lineItemsLabel;
        private IContainer components = null;

        public MainForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            Application.EnableVisualStyles();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing ) {
                //deleteImageTempFileName();
                processor = null;
                if( components != null ) {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.countryComboBox = new System.Windows.Forms.ComboBox();
            this.countryLabel = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.imageFileTextBox = new System.Windows.Forms.TextBox();
            this.imageFileLabel = new System.Windows.Forms.Label();
            this.sourceDataSplitContainer = new System.Windows.Forms.SplitContainer();
            this.treatAsPhotoCheckBox = new System.Windows.Forms.CheckBox();
            this.arrowPanel = new System.Windows.Forms.Panel();
            this.startRecognitionLabel = new System.Windows.Forms.Label();
            this.resultSplitContainer = new System.Windows.Forms.SplitContainer();
            this.sourceFilePanel = new System.Windows.Forms.Panel();
            this.promptLabel = new System.Windows.Forms.Label();
            this.sourceFilePictureBox = new System.Windows.Forms.PictureBox();
            this.sourceFileTopPanel = new System.Windows.Forms.Panel();
            this.sourceFileLabel = new System.Windows.Forms.Label();
            this.resultDataSplitContainer = new System.Windows.Forms.SplitContainer();
            this.receiptPanel = new System.Windows.Forms.Panel();
            this.receiptPictureBox = new System.Windows.Forms.PictureBox();
            this.receiptTopPanel = new System.Windows.Forms.Panel();
            this.receiptLabel = new System.Windows.Forms.Label();
            this.textPanel = new System.Windows.Forms.Panel();
            this.taxFieldsLabel = new System.Windows.Forms.Label();
            this.taxFieldsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lineItemsLabel = new System.Windows.Forms.Label();
            this.lineItemsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.bankCardLabel = new System.Windows.Forms.Label();
            this.bankCardLabelTitle = new System.Windows.Forms.Label();
            this.purchaseTypeLabel = new System.Windows.Forms.Label();
            this.purchaseTypeLabelTitle = new System.Windows.Forms.Label();
            this.tenderLabel = new System.Windows.Forms.Label();
            this.tenderLabelTitle = new System.Windows.Forms.Label();
            this.totalTaxLabel = new System.Windows.Forms.Label();
            this.totalTaxLabelTitle = new System.Windows.Forms.Label();
            this.subtotalLabel = new System.Windows.Forms.Label();
            this.subtotalLabelTitle = new System.Windows.Forms.Label();
            this.totalSumLabel = new System.Windows.Forms.Label();
            this.totalSumLabelTitle = new System.Windows.Forms.Label();
            this.phoneFaxLabel = new System.Windows.Forms.Label();
            this.phoneFaxLabelTitle = new System.Windows.Forms.Label();
            this.addressLabel = new System.Windows.Forms.Label();
            this.addressLabelTitle = new System.Windows.Forms.Label();
            this.dateTimeLabel = new System.Windows.Forms.Label();
            this.dateTimeLabelTitle = new System.Windows.Forms.Label();
            this.vendorNameLabel = new System.Windows.Forms.Label();
            this.vendorNameLabelTitle = new System.Windows.Forms.Label();
            this.textTopPanel = new System.Windows.Forms.Panel();
            this.textLabel = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.sourceDataSplitContainer.Panel1.SuspendLayout();
            this.sourceDataSplitContainer.Panel2.SuspendLayout();
            this.sourceDataSplitContainer.SuspendLayout();
            this.resultSplitContainer.Panel1.SuspendLayout();
            this.resultSplitContainer.Panel2.SuspendLayout();
            this.resultSplitContainer.SuspendLayout();
            this.sourceFilePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sourceFilePictureBox)).BeginInit();
            this.sourceFileTopPanel.SuspendLayout();
            this.resultDataSplitContainer.Panel1.SuspendLayout();
            this.resultDataSplitContainer.Panel2.SuspendLayout();
            this.resultDataSplitContainer.SuspendLayout();
            this.receiptPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.receiptPictureBox)).BeginInit();
            this.receiptTopPanel.SuspendLayout();
            this.textPanel.SuspendLayout();
            this.textTopPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // countryComboBox
            // 
            this.countryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.countryComboBox.FormattingEnabled = true;
            this.countryComboBox.Location = new System.Drawing.Point(300, 25);
            this.countryComboBox.Name = "countryComboBox";
            this.countryComboBox.Size = new System.Drawing.Size(172, 21);
            this.countryComboBox.TabIndex = 4;
            this.countryComboBox.SelectedIndexChanged += new System.EventHandler(this.parametersChanged);
            this.countryComboBox.TextChanged += new System.EventHandler(this.parametersChanged);
            // 
            // countryLabel
            // 
            this.countryLabel.AutoSize = true;
            this.countryLabel.Location = new System.Drawing.Point(297, 8);
            this.countryLabel.Name = "countryLabel";
            this.countryLabel.Size = new System.Drawing.Size(78, 13);
            this.countryLabel.TabIndex = 3;
            this.countryLabel.Text = "Select country:";
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(256, 23);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(24, 23);
            this.browseButton.TabIndex = 2;
            this.browseButton.Text = "...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // imageFileTextBox
            // 
            this.imageFileTextBox.Location = new System.Drawing.Point(14, 25);
            this.imageFileTextBox.Name = "imageFileTextBox";
            this.imageFileTextBox.Size = new System.Drawing.Size(236, 20);
            this.imageFileTextBox.TabIndex = 1;
            this.imageFileTextBox.TextChanged += new System.EventHandler(this.parametersChanged);
            // 
            // imageFileLabel
            // 
            this.imageFileLabel.AutoSize = true;
            this.imageFileLabel.Location = new System.Drawing.Point(11, 8);
            this.imageFileLabel.Name = "imageFileLabel";
            this.imageFileLabel.Size = new System.Drawing.Size(87, 13);
            this.imageFileLabel.TabIndex = 0;
            this.imageFileLabel.Text = "Select image file:";
            // 
            // sourceDataSplitContainer
            // 
            this.sourceDataSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sourceDataSplitContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.sourceDataSplitContainer.IsSplitterFixed = true;
            this.sourceDataSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.sourceDataSplitContainer.Name = "sourceDataSplitContainer";
            // 
            // sourceDataSplitContainer.Panel1
            // 
            this.sourceDataSplitContainer.Panel1.Controls.Add(this.treatAsPhotoCheckBox);
            this.sourceDataSplitContainer.Panel1.Controls.Add(this.imageFileTextBox);
            this.sourceDataSplitContainer.Panel1.Controls.Add(this.imageFileLabel);
            this.sourceDataSplitContainer.Panel1.Controls.Add(this.countryLabel);
            this.sourceDataSplitContainer.Panel1.Controls.Add(this.countryComboBox);
            this.sourceDataSplitContainer.Panel1.Controls.Add(this.browseButton);
            // 
            // sourceDataSplitContainer.Panel2
            // 
            this.sourceDataSplitContainer.Panel2.Controls.Add(this.arrowPanel);
            this.sourceDataSplitContainer.Panel2.Controls.Add(this.startRecognitionLabel);
            this.sourceDataSplitContainer.Panel2.Resize += new System.EventHandler(this.sourceDataSplitContainerPanel2_Resize);
            this.sourceDataSplitContainer.Size = new System.Drawing.Size(884, 61);
            this.sourceDataSplitContainer.SplitterDistance = 677;
            this.sourceDataSplitContainer.TabIndex = 0;
            // 
            // treatAsPhotoCheckBox
            // 
            this.treatAsPhotoCheckBox.AutoSize = true;
            this.treatAsPhotoCheckBox.Location = new System.Drawing.Point(478, 27);
            this.treatAsPhotoCheckBox.Name = "treatAsPhotoCheckBox";
            this.treatAsPhotoCheckBox.Size = new System.Drawing.Size(95, 17);
            this.treatAsPhotoCheckBox.TabIndex = 5;
            this.treatAsPhotoCheckBox.Text = "Treat as photo";
            this.treatAsPhotoCheckBox.UseVisualStyleBackColor = true;
            this.treatAsPhotoCheckBox.CheckedChanged += new System.EventHandler(this.parametersChanged);
            // 
            // arrowPanel
            // 
            this.arrowPanel.BackgroundImage = global::Sample.Properties.Resources.arrow_30x30;
            this.arrowPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.arrowPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.arrowPanel.Location = new System.Drawing.Point(39, 15);
            this.arrowPanel.Name = "arrowPanel";
            this.arrowPanel.Size = new System.Drawing.Size(30, 30);
            this.arrowPanel.TabIndex = 0;
            this.arrowPanel.Click += new System.EventHandler(this.startRecognition_Click);
            // 
            // startRecognitionLabel
            // 
            this.startRecognitionLabel.AutoSize = true;
            this.startRecognitionLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.startRecognitionLabel.Location = new System.Drawing.Point(75, 24);
            this.startRecognitionLabel.Name = "startRecognitionLabel";
            this.startRecognitionLabel.Size = new System.Drawing.Size(89, 13);
            this.startRecognitionLabel.TabIndex = 0;
            this.startRecognitionLabel.Text = "Start Recognition";
            this.startRecognitionLabel.Click += new System.EventHandler(this.startRecognition_Click);
            // 
            // resultSplitContainer
            // 
            this.resultSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultSplitContainer.Location = new System.Drawing.Point(0, 61);
            this.resultSplitContainer.Name = "resultSplitContainer";
            // 
            // resultSplitContainer.Panel1
            // 
            this.resultSplitContainer.Panel1.Controls.Add(this.sourceFilePanel);
            this.resultSplitContainer.Panel1.Controls.Add(this.sourceFileTopPanel);
            // 
            // resultSplitContainer.Panel2
            // 
            this.resultSplitContainer.Panel2.Controls.Add(this.resultDataSplitContainer);
            this.resultSplitContainer.Size = new System.Drawing.Size(884, 680);
            this.resultSplitContainer.SplitterDistance = 338;
            this.resultSplitContainer.SplitterWidth = 10;
            this.resultSplitContainer.TabIndex = 0;
            // 
            // sourceFilePanel
            // 
            this.sourceFilePanel.BackColor = System.Drawing.Color.White;
            this.sourceFilePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sourceFilePanel.Controls.Add(this.promptLabel);
            this.sourceFilePanel.Controls.Add(this.sourceFilePictureBox);
            this.sourceFilePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceFilePanel.Location = new System.Drawing.Point(0, 33);
            this.sourceFilePanel.Name = "sourceFilePanel";
            this.sourceFilePanel.Size = new System.Drawing.Size(338, 647);
            this.sourceFilePanel.TabIndex = 0;
            this.sourceFilePanel.Resize += new System.EventHandler(this.sourceFilePanel_Resize);
            // 
            // promptLabel
            // 
            this.promptLabel.AutoSize = true;
            this.promptLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.promptLabel.Location = new System.Drawing.Point(128, 295);
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Size = new System.Drawing.Size(76, 24);
            this.promptLabel.TabIndex = 0;
            this.promptLabel.Text = "Prompt";
            this.promptLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sourceFilePictureBox
            // 
            this.sourceFilePictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceFilePictureBox.Location = new System.Drawing.Point(0, 0);
            this.sourceFilePictureBox.MinimumSize = new System.Drawing.Size(250, 0);
            this.sourceFilePictureBox.Name = "sourceFilePictureBox";
            this.sourceFilePictureBox.Size = new System.Drawing.Size(336, 645);
            this.sourceFilePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.sourceFilePictureBox.TabIndex = 0;
            this.sourceFilePictureBox.TabStop = false;
            // 
            // sourceFileTopPanel
            // 
            this.sourceFileTopPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sourceFileTopPanel.Controls.Add(this.sourceFileLabel);
            this.sourceFileTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.sourceFileTopPanel.Location = new System.Drawing.Point(0, 0);
            this.sourceFileTopPanel.Name = "sourceFileTopPanel";
            this.sourceFileTopPanel.Size = new System.Drawing.Size(338, 33);
            this.sourceFileTopPanel.TabIndex = 0;
            // 
            // sourceFileLabel
            // 
            this.sourceFileLabel.AutoSize = true;
            this.sourceFileLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.sourceFileLabel.Location = new System.Drawing.Point(10, 10);
            this.sourceFileLabel.Name = "sourceFileLabel";
            this.sourceFileLabel.Size = new System.Drawing.Size(109, 13);
            this.sourceFileLabel.TabIndex = 0;
            this.sourceFileLabel.Text = "Source Image File";
            // 
            // resultDataSplitContainer
            // 
            this.resultDataSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultDataSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.resultDataSplitContainer.Name = "resultDataSplitContainer";
            // 
            // resultDataSplitContainer.Panel1
            // 
            this.resultDataSplitContainer.Panel1.Controls.Add(this.receiptPanel);
            this.resultDataSplitContainer.Panel1.Controls.Add(this.receiptTopPanel);
            this.resultDataSplitContainer.Panel1MinSize = 100;
            // 
            // resultDataSplitContainer.Panel2
            // 
            this.resultDataSplitContainer.Panel2.Controls.Add(this.textPanel);
            this.resultDataSplitContainer.Panel2.Controls.Add(this.textTopPanel);
            this.resultDataSplitContainer.Panel2MinSize = 100;
            this.resultDataSplitContainer.Size = new System.Drawing.Size(536, 680);
            this.resultDataSplitContainer.SplitterDistance = 259;
            this.resultDataSplitContainer.SplitterWidth = 10;
            this.resultDataSplitContainer.TabIndex = 0;
            // 
            // receiptPanel
            // 
            this.receiptPanel.BackColor = System.Drawing.Color.White;
            this.receiptPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.receiptPanel.Controls.Add(this.receiptPictureBox);
            this.receiptPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.receiptPanel.Location = new System.Drawing.Point(0, 33);
            this.receiptPanel.Name = "receiptPanel";
            this.receiptPanel.Size = new System.Drawing.Size(259, 647);
            this.receiptPanel.TabIndex = 0;
            this.receiptPanel.Resize += new System.EventHandler(this.receiptPanel_Resize);
            // 
            // receiptPictureBox
            // 
            this.receiptPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.receiptPictureBox.Location = new System.Drawing.Point(0, 0);
            this.receiptPictureBox.MinimumSize = new System.Drawing.Size(0, 150);
            this.receiptPictureBox.Name = "receiptPictureBox";
            this.receiptPictureBox.Size = new System.Drawing.Size(257, 645);
            this.receiptPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.receiptPictureBox.TabIndex = 0;
            this.receiptPictureBox.TabStop = false;
            // 
            // receiptTopPanel
            // 
            this.receiptTopPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.receiptTopPanel.Controls.Add(this.receiptLabel);
            this.receiptTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.receiptTopPanel.Location = new System.Drawing.Point(0, 0);
            this.receiptTopPanel.Name = "receiptTopPanel";
            this.receiptTopPanel.Size = new System.Drawing.Size(259, 33);
            this.receiptTopPanel.TabIndex = 0;
            // 
            // receiptLabel
            // 
            this.receiptLabel.AutoSize = true;
            this.receiptLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.receiptLabel.Location = new System.Drawing.Point(10, 10);
            this.receiptLabel.Name = "receiptLabel";
            this.receiptLabel.Size = new System.Drawing.Size(51, 13);
            this.receiptLabel.TabIndex = 0;
            this.receiptLabel.Text = "Receipt";
            // 
            // textPanel
            // 
            this.textPanel.BackColor = System.Drawing.Color.White;
            this.textPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textPanel.Controls.Add(this.taxFieldsLabel);
            this.textPanel.Controls.Add(this.taxFieldsPanel);
            this.textPanel.Controls.Add(this.lineItemsLabel);
            this.textPanel.Controls.Add(this.lineItemsPanel);
            this.textPanel.Controls.Add(this.bankCardLabel);
            this.textPanel.Controls.Add(this.bankCardLabelTitle);
            this.textPanel.Controls.Add(this.purchaseTypeLabel);
            this.textPanel.Controls.Add(this.purchaseTypeLabelTitle);
            this.textPanel.Controls.Add(this.tenderLabel);
            this.textPanel.Controls.Add(this.tenderLabelTitle);
            this.textPanel.Controls.Add(this.totalTaxLabel);
            this.textPanel.Controls.Add(this.totalTaxLabelTitle);
            this.textPanel.Controls.Add(this.subtotalLabel);
            this.textPanel.Controls.Add(this.subtotalLabelTitle);
            this.textPanel.Controls.Add(this.totalSumLabel);
            this.textPanel.Controls.Add(this.totalSumLabelTitle);
            this.textPanel.Controls.Add(this.phoneFaxLabel);
            this.textPanel.Controls.Add(this.phoneFaxLabelTitle);
            this.textPanel.Controls.Add(this.addressLabel);
            this.textPanel.Controls.Add(this.addressLabelTitle);
            this.textPanel.Controls.Add(this.dateTimeLabel);
            this.textPanel.Controls.Add(this.dateTimeLabelTitle);
            this.textPanel.Controls.Add(this.vendorNameLabel);
            this.textPanel.Controls.Add(this.vendorNameLabelTitle);
            this.textPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textPanel.Location = new System.Drawing.Point(0, 33);
            this.textPanel.Name = "textPanel";
            this.textPanel.Size = new System.Drawing.Size(267, 647);
            this.textPanel.TabIndex = 0;
            this.textPanel.Resize += new System.EventHandler(this.textPanel_Resize);
            // 
            // taxFieldsLabel
            // 
            this.taxFieldsLabel.AutoSize = true;
            this.taxFieldsLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.taxFieldsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.taxFieldsLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.taxFieldsLabel.Location = new System.Drawing.Point(10, 315);
            this.taxFieldsLabel.Name = "taxFieldsLabel";
            this.taxFieldsLabel.Size = new System.Drawing.Size(66, 13);
            this.taxFieldsLabel.TabIndex = 0;
            this.taxFieldsLabel.Text = "Tax fields:";
            this.taxFieldsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // taxFieldsPanel
            // 
            this.taxFieldsPanel.AutoScroll = true;
            this.taxFieldsPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.taxFieldsPanel.ColumnCount = 4;
            this.taxFieldsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.taxFieldsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.taxFieldsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.taxFieldsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.taxFieldsPanel.Location = new System.Drawing.Point(14, 340);
            this.taxFieldsPanel.Name = "taxFieldsPanel";
            this.taxFieldsPanel.RowCount = 1;
            this.taxFieldsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 119F));
            this.taxFieldsPanel.Size = new System.Drawing.Size(241, 120);
            this.taxFieldsPanel.TabIndex = 0;
            // 
            // lineItemsLabel
            // 
            this.lineItemsLabel.AutoSize = true;
            this.lineItemsLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lineItemsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lineItemsLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lineItemsLabel.Location = new System.Drawing.Point(11, 477);
            this.lineItemsLabel.Name = "lineItemsLabel";
            this.lineItemsLabel.Size = new System.Drawing.Size(68, 13);
            this.lineItemsLabel.TabIndex = 0;
            this.lineItemsLabel.Text = "Line items:";
            this.lineItemsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lineItemsPanel
            // 
            this.lineItemsPanel.AutoScroll = true;
            this.lineItemsPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.lineItemsPanel.ColumnCount = 3;
            this.lineItemsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.lineItemsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.lineItemsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.lineItemsPanel.Location = new System.Drawing.Point(14, 502);
            this.lineItemsPanel.Name = "lineItemsPanel";
            this.lineItemsPanel.RowCount = 1;
            this.lineItemsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 115F));
            this.lineItemsPanel.Size = new System.Drawing.Size(240, 116);
            this.lineItemsPanel.TabIndex = 0;
            // 
            // bankCardLabel
            // 
            this.bankCardLabel.AutoSize = true;
            this.bankCardLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bankCardLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bankCardLabel.Location = new System.Drawing.Point(106, 285);
            this.bankCardLabel.Name = "bankCardLabel";
            this.bankCardLabel.Size = new System.Drawing.Size(55, 13);
            this.bankCardLabel.TabIndex = 0;
            this.bankCardLabel.Text = "bank card";
            this.bankCardLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bankCardLabel.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // bankCardLabelTitle
            // 
            this.bankCardLabelTitle.AutoSize = true;
            this.bankCardLabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bankCardLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bankCardLabelTitle.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.bankCardLabelTitle.Location = new System.Drawing.Point(10, 285);
            this.bankCardLabelTitle.Name = "bankCardLabelTitle";
            this.bankCardLabelTitle.Size = new System.Drawing.Size(69, 13);
            this.bankCardLabelTitle.TabIndex = 0;
            this.bankCardLabelTitle.Text = "Bank card:";
            this.bankCardLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bankCardLabelTitle.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // purchaseTypeLabel
            // 
            this.purchaseTypeLabel.AutoSize = true;
            this.purchaseTypeLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.purchaseTypeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.purchaseTypeLabel.Location = new System.Drawing.Point(106, 225);
            this.purchaseTypeLabel.Name = "purchaseTypeLabel";
            this.purchaseTypeLabel.Size = new System.Drawing.Size(76, 13);
            this.purchaseTypeLabel.TabIndex = 0;
            this.purchaseTypeLabel.Text = "PurchaseType";
            this.purchaseTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.purchaseTypeLabel.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // purchaseTypeLabelTitle
            // 
            this.purchaseTypeLabelTitle.AutoSize = true;
            this.purchaseTypeLabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.purchaseTypeLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.purchaseTypeLabelTitle.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.purchaseTypeLabelTitle.Location = new System.Drawing.Point(10, 225);
            this.purchaseTypeLabelTitle.Name = "purchaseTypeLabelTitle";
            this.purchaseTypeLabelTitle.Size = new System.Drawing.Size(92, 13);
            this.purchaseTypeLabelTitle.TabIndex = 0;
            this.purchaseTypeLabelTitle.Text = "Purchase type:";
            this.purchaseTypeLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.purchaseTypeLabelTitle.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // tenderLabel
            // 
            this.tenderLabel.AutoSize = true;
            this.tenderLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tenderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tenderLabel.Location = new System.Drawing.Point(106, 255);
            this.tenderLabel.Name = "tenderLabel";
            this.tenderLabel.Size = new System.Drawing.Size(37, 13);
            this.tenderLabel.TabIndex = 0;
            this.tenderLabel.Text = "tender";
            this.tenderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tenderLabel.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // tenderLabelTitle
            // 
            this.tenderLabelTitle.AutoSize = true;
            this.tenderLabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tenderLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tenderLabelTitle.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.tenderLabelTitle.Location = new System.Drawing.Point(10, 255);
            this.tenderLabelTitle.Name = "tenderLabelTitle";
            this.tenderLabelTitle.Size = new System.Drawing.Size(51, 13);
            this.tenderLabelTitle.TabIndex = 0;
            this.tenderLabelTitle.Text = "Tender:";
            this.tenderLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tenderLabelTitle.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // totalTaxLabel
            // 
            this.totalTaxLabel.AutoSize = true;
            this.totalTaxLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.totalTaxLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.totalTaxLabel.Location = new System.Drawing.Point(106, 195);
            this.totalTaxLabel.Name = "totalTaxLabel";
            this.totalTaxLabel.Size = new System.Drawing.Size(44, 13);
            this.totalTaxLabel.TabIndex = 0;
            this.totalTaxLabel.Text = "total tax";
            this.totalTaxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.totalTaxLabel.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // totalTaxLabelTitle
            // 
            this.totalTaxLabelTitle.AutoSize = true;
            this.totalTaxLabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.totalTaxLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.totalTaxLabelTitle.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.totalTaxLabelTitle.Location = new System.Drawing.Point(10, 195);
            this.totalTaxLabelTitle.Name = "totalTaxLabelTitle";
            this.totalTaxLabelTitle.Size = new System.Drawing.Size(61, 13);
            this.totalTaxLabelTitle.TabIndex = 0;
            this.totalTaxLabelTitle.Text = "Total tax:";
            this.totalTaxLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.totalTaxLabelTitle.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // subtotalLabel
            // 
            this.subtotalLabel.AutoSize = true;
            this.subtotalLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.subtotalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.subtotalLabel.Location = new System.Drawing.Point(106, 165);
            this.subtotalLabel.Name = "subtotalLabel";
            this.subtotalLabel.Size = new System.Drawing.Size(44, 13);
            this.subtotalLabel.TabIndex = 0;
            this.subtotalLabel.Text = "subtotal";
            this.subtotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.subtotalLabel.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // subtotalLabelTitle
            // 
            this.subtotalLabelTitle.AutoSize = true;
            this.subtotalLabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.subtotalLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.subtotalLabelTitle.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.subtotalLabelTitle.Location = new System.Drawing.Point(10, 165);
            this.subtotalLabelTitle.Name = "subtotalLabelTitle";
            this.subtotalLabelTitle.Size = new System.Drawing.Size(58, 13);
            this.subtotalLabelTitle.TabIndex = 0;
            this.subtotalLabelTitle.Text = "Subtotal:";
            this.subtotalLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.subtotalLabelTitle.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // totalSumLabel
            // 
            this.totalSumLabel.AutoSize = true;
            this.totalSumLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.totalSumLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.totalSumLabel.Location = new System.Drawing.Point(106, 135);
            this.totalSumLabel.Name = "totalSumLabel";
            this.totalSumLabel.Size = new System.Drawing.Size(27, 13);
            this.totalSumLabel.TabIndex = 0;
            this.totalSumLabel.Text = "total";
            this.totalSumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.totalSumLabel.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // totalSumLabelTitle
            // 
            this.totalSumLabelTitle.AutoSize = true;
            this.totalSumLabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.totalSumLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.totalSumLabelTitle.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.totalSumLabelTitle.Location = new System.Drawing.Point(10, 135);
            this.totalSumLabelTitle.Name = "totalSumLabelTitle";
            this.totalSumLabelTitle.Size = new System.Drawing.Size(40, 13);
            this.totalSumLabelTitle.TabIndex = 0;
            this.totalSumLabelTitle.Text = "Total:";
            this.totalSumLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.totalSumLabelTitle.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // phoneFaxLabel
            // 
            this.phoneFaxLabel.AutoSize = true;
            this.phoneFaxLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.phoneFaxLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.phoneFaxLabel.Location = new System.Drawing.Point(106, 75);
            this.phoneFaxLabel.Name = "phoneFaxLabel";
            this.phoneFaxLabel.Size = new System.Drawing.Size(62, 13);
            this.phoneFaxLabel.TabIndex = 0;
            this.phoneFaxLabel.Text = "phone / fax";
            this.phoneFaxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.phoneFaxLabel.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // phoneFaxLabelTitle
            // 
            this.phoneFaxLabelTitle.AutoSize = true;
            this.phoneFaxLabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.phoneFaxLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.phoneFaxLabelTitle.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.phoneFaxLabelTitle.Location = new System.Drawing.Point(10, 75);
            this.phoneFaxLabelTitle.Name = "phoneFaxLabelTitle";
            this.phoneFaxLabelTitle.Size = new System.Drawing.Size(81, 13);
            this.phoneFaxLabelTitle.TabIndex = 0;
            this.phoneFaxLabelTitle.Text = "Phone / Fax:";
            this.phoneFaxLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.phoneFaxLabelTitle.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // addressLabel
            // 
            this.addressLabel.AutoSize = true;
            this.addressLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.addressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.addressLabel.Location = new System.Drawing.Point(106, 105);
            this.addressLabel.Name = "addressLabel";
            this.addressLabel.Size = new System.Drawing.Size(44, 13);
            this.addressLabel.TabIndex = 0;
            this.addressLabel.Text = "address";
            this.addressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.addressLabel.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // addressLabelTitle
            // 
            this.addressLabelTitle.AutoSize = true;
            this.addressLabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.addressLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.addressLabelTitle.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.addressLabelTitle.Location = new System.Drawing.Point(10, 105);
            this.addressLabelTitle.Name = "addressLabelTitle";
            this.addressLabelTitle.Size = new System.Drawing.Size(56, 13);
            this.addressLabelTitle.TabIndex = 0;
            this.addressLabelTitle.Text = "Address:";
            this.addressLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.addressLabelTitle.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // dateTimeLabel
            // 
            this.dateTimeLabel.AutoSize = true;
            this.dateTimeLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.dateTimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dateTimeLabel.Location = new System.Drawing.Point(106, 45);
            this.dateTimeLabel.Name = "dateTimeLabel";
            this.dateTimeLabel.Size = new System.Drawing.Size(58, 13);
            this.dateTimeLabel.TabIndex = 0;
            this.dateTimeLabel.Text = "date / time";
            this.dateTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.dateTimeLabel.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // dateTimeLabelTitle
            // 
            this.dateTimeLabelTitle.AutoSize = true;
            this.dateTimeLabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.dateTimeLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dateTimeLabelTitle.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dateTimeLabelTitle.Location = new System.Drawing.Point(10, 45);
            this.dateTimeLabelTitle.Name = "dateTimeLabelTitle";
            this.dateTimeLabelTitle.Size = new System.Drawing.Size(75, 13);
            this.dateTimeLabelTitle.TabIndex = 0;
            this.dateTimeLabelTitle.Text = "Date / time:";
            this.dateTimeLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.dateTimeLabelTitle.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // vendorNameLabel
            // 
            this.vendorNameLabel.AutoSize = true;
            this.vendorNameLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.vendorNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.vendorNameLabel.Location = new System.Drawing.Point(106, 15);
            this.vendorNameLabel.Name = "vendorNameLabel";
            this.vendorNameLabel.Size = new System.Drawing.Size(33, 13);
            this.vendorNameLabel.TabIndex = 0;
            this.vendorNameLabel.Text = "name";
            this.vendorNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.vendorNameLabel.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // vendorNameLabelTitle
            // 
            this.vendorNameLabelTitle.AutoSize = true;
            this.vendorNameLabelTitle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.vendorNameLabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.vendorNameLabelTitle.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.vendorNameLabelTitle.Location = new System.Drawing.Point(10, 15);
            this.vendorNameLabelTitle.Name = "vendorNameLabelTitle";
            this.vendorNameLabelTitle.Size = new System.Drawing.Size(85, 13);
            this.vendorNameLabelTitle.TabIndex = 0;
            this.vendorNameLabelTitle.Text = "Vendor name:";
            this.vendorNameLabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.vendorNameLabelTitle.Click += new System.EventHandler(this.fieldLabel_Click);
            // 
            // textTopPanel
            // 
            this.textTopPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textTopPanel.Controls.Add(this.textLabel);
            this.textTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.textTopPanel.Location = new System.Drawing.Point(0, 0);
            this.textTopPanel.Name = "textTopPanel";
            this.textTopPanel.Size = new System.Drawing.Size(267, 33);
            this.textTopPanel.TabIndex = 0;
            // 
            // textLabel
            // 
            this.textLabel.AutoSize = true;
            this.textLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textLabel.Location = new System.Drawing.Point(10, 10);
            this.textLabel.Name = "textLabel";
            this.textLabel.Size = new System.Drawing.Size(88, 13);
            this.textLabel.TabIndex = 0;
            this.textLabel.Text = "Receipt Fields";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = resources.GetString("openFileDialog.Filter");
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(884, 741);
            this.Controls.Add(this.resultSplitContainer);
            this.Controls.Add(this.sourceDataSplitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(765, 680);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Receipt Recognition";
            this.Shown += new System.EventHandler(this.mainForm_Shown);
            this.sourceDataSplitContainer.Panel1.ResumeLayout(false);
            this.sourceDataSplitContainer.Panel1.PerformLayout();
            this.sourceDataSplitContainer.Panel2.ResumeLayout(false);
            this.sourceDataSplitContainer.Panel2.PerformLayout();
            this.sourceDataSplitContainer.ResumeLayout(false);
            this.resultSplitContainer.Panel1.ResumeLayout(false);
            this.resultSplitContainer.Panel2.ResumeLayout(false);
            this.resultSplitContainer.ResumeLayout(false);
            this.sourceFilePanel.ResumeLayout(false);
            this.sourceFilePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sourceFilePictureBox)).EndInit();
            this.sourceFileTopPanel.ResumeLayout(false);
            this.sourceFileTopPanel.PerformLayout();
            this.resultDataSplitContainer.Panel1.ResumeLayout(false);
            this.resultDataSplitContainer.Panel2.ResumeLayout(false);
            this.resultDataSplitContainer.ResumeLayout(false);
            this.receiptPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.receiptPictureBox)).EndInit();
            this.receiptTopPanel.ResumeLayout(false);
            this.receiptTopPanel.PerformLayout();
            this.textPanel.ResumeLayout(false);
            this.textPanel.PerformLayout();
            this.textTopPanel.ResumeLayout(false);
            this.textTopPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run( new MainForm() );
        }

        private void mainForm_Shown( object sender, EventArgs e )
        {
            if( !initDefaults() ) {
                Application.Exit();
            }
        }

        private bool initDefaults()
        {
            try {
                activeFieldColor = Color.FromArgb( 50, Color.Blue );

                initReceiptCountries();
                fillCountryComboBox();
                processor = new Processor();
                ProcessingSettings defaultSettings = new ProcessingSettings();

                countryComboBox.Text = "USA";
                treatAsPhotoCheckBox.Checked = defaultSettings.TreatAsPhoto;

                showPrompt( "Select image file and country, then click \"Start Recognition\"", Color.Black );

                imageFileTextBox.Select();
            } catch( Exception exception ) {
                MessageBox.Show( this, exception.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
                return false;
            }
            return true;
        }

        private void initReceiptCountries()
        {
            receiptCountries = new Dictionary<String, CountryOfOrigin>();
            receiptCountries.Add("UK", CountryOfOrigin.UK);
            receiptCountries.Add("Usa", CountryOfOrigin.Usa);
            receiptCountries.Add("Japan", CountryOfOrigin.Japan);
            receiptCountries.Add("Germany", CountryOfOrigin.Germany);
            receiptCountries.Add("Italy", CountryOfOrigin.Italy);
            receiptCountries.Add("France", CountryOfOrigin.France);
            receiptCountries.Add("Brazil", CountryOfOrigin.Brazil);
            receiptCountries.Add("Russia", CountryOfOrigin.Russia);
            receiptCountries.Add("China", CountryOfOrigin.China);
            receiptCountries.Add("Korea", CountryOfOrigin.Korea);
            receiptCountries.Add("Spain", CountryOfOrigin.Spain);
            receiptCountries.Add("Singapore", CountryOfOrigin.Singapore);
            receiptCountries.Add("Taiwan", CountryOfOrigin.Taiwan);
            receiptCountries.Add("Netherlands", CountryOfOrigin.Netherlands);
        }

        private void fillCountryComboBox()
        {
            if( countryComboBox.Items.Count == 0 ) {
                countryComboBox.Sorted = true;
                foreach( var entry in receiptCountries ) {
                    countryComboBox.Items.Add( entry.Key );
                }
            }
        }

        private void browseButton_Click( object sender, EventArgs e )
        {
            try {
                string samplesFolder = "";
                if( imageFileTextBox.Text.Trim().Length > 0 && File.Exists( imageFileTextBox.Text ) ) {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName( imageFileTextBox.Text );
                    openFileDialog.FileName = Path.GetFileName( imageFileTextBox.Text );
                } else {
                    openFileDialog.InitialDirectory = samplesFolder;
                }
                if( openFileDialog.ShowDialog() == DialogResult.OK ) {
                    imageFileTextBox.Text = openFileDialog.FileName;
                    resultSplitContainer.Panel2Collapsed = true;
                    sourceFileTopPanel.Visible = false;
                    countryComboBox.Enabled = true;
                    treatAsPhotoCheckBox.Enabled = true;
                    sourceDataSplitContainer.Panel2.Enabled = true;
                    refreshGraphics();
                }
            } catch( Exception exception ) {
                MessageBox.Show( this, exception.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void scaleSourceImage( Image newImage )
        {
            sourceFilePictureBox.Image = DrawingTools.ScaleImage(newImage, sourceFilePictureBox.Size);
            sourceScaleToFit = DrawingTools.ScaleToFit(newImage.Size, sourceFilePictureBox.Size);
        }

        private void scaleReceiptImage(Image newImage)
        {
            receiptPictureBox.Image = DrawingTools.ScaleImage(newImage, receiptPictureBox.Size);
            receiptScaleToFit = DrawingTools.ScaleToFit(newImage.Size, receiptPictureBox.Size);
        }

        private void refreshGraphics()
        {
            if( !File.Exists( imageFileTextBox.Text ) ) {
                return;
            }
            try {
                //refreshReceipt();

                Image image = Image.FromFile( imageFileTextBox.Text );
                scaleSourceImage( image );
                //sourceFilePictureBox.Image = DrawingTools.ScaleImage( image, sourceFilePictureBox.Size );
                //sourceScaleToFit = DrawingTools.ScaleToFit( image.Size, sourceFilePictureBox.Size );
                promptLabel.Visible = false;

                sourceFilePictureBox.Refresh();

                image = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            } catch( Exception /*e*/ ) {
                // Preview unavailable
                showPrompt( "Preview unavailable", Color.Red );
            }
        }

        private void refreshReceipt()
        {
            showReceipt();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void startRecognition_Click( object sender, EventArgs e )
        {
            try {
                sourceDataSplitContainer.Enabled = false;
                resultSplitContainer.Enabled = false;

                ProgressBarForm progress = new ProgressBarForm( this );
                try {
                    processor.StepChangedAction = progress.ShowMessage;
                    processor.ProgressChangedAction = progress.ShowProgress;

                    ProcessingSettings settings = new ProcessingSettings();
                    settings.Country = receiptCountries[countryComboBox.Text];
                    settings.TreatAsPhoto = treatAsPhotoCheckBox.Checked;

                    string result = processor.Process( imageFileTextBox.Text, settings );
                    receiptPictureBox.Image = sourceFilePictureBox.Image;
                    if( !String.IsNullOrEmpty( result ) ) {
                        fillReceiptFields( result );
                    }

                    resultSplitContainer.Panel1Collapsed = true;
                    //refreshGraphics();
                    showReceipt();
                } finally {
                    processor.ProgressChangedAction = null;
                    processor.StepChangedAction = null;
                    progress.EndProgress();
                }
            } catch( Exception exception ) {
                MessageBox.Show( this, exception.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
            } finally {
                sourceDataSplitContainer.Enabled = true;
                resultSplitContainer.Enabled = true;
            }
        }

        private void initActiveReceiptField()
        {
            restoreDefaultLabelsColors();

            activeFieldIndex = 0;
            vendorNameLabelTitle.ForeColor = activeFieldColor;
            vendorNameLabel.ForeColor = activeFieldColor;
        }

        private void parametersChanged( object sender, EventArgs e )
        {
            initActiveReceiptField();

            sourceScaleToFit = 1.0f;
            receiptScaleToFit = 1.0f;
            resultSplitContainer.Panel1Collapsed = false;
            resultSplitContainer.Panel2Collapsed = true;
            sourceFileTopPanel.Visible = false;

            if( File.Exists( imageFileTextBox.Text ) ) {
                countryComboBox.Enabled = true;
                treatAsPhotoCheckBox.Enabled = true;
                sourceDataSplitContainer.Panel2.Enabled = true;
                refreshGraphics();
            } else {
                countryComboBox.Enabled = false;
                treatAsPhotoCheckBox.Enabled = false;
                sourceDataSplitContainer.Panel2.Enabled = false;
                showPrompt( "Source image file not found. Please check your settings.", Color.Red );
            }
        }

        private void showPrompt( string prompt, Color color )
        {
            sourceFilePictureBox.Image = DrawingTools.CreateImage( sourceFilePictureBox.Size, new SolidBrush( Color.White ) );
            promptLabel.Text = prompt;
            promptLabel.ForeColor = color;
            int promptLabelX = ( resultSplitContainer.Panel1.Width - promptLabel.Width ) / 2;
            int promptLabelY = ( resultSplitContainer.Panel1.Height - promptLabel.Height ) / 2;
            promptLabel.Location = new System.Drawing.Point( promptLabelX, promptLabelY );
            promptLabel.Visible = true;
        }

        private void normalizeCoordinates( ref int left, ref int right )
        {
            if( right < left ) {
                int temp = left;
                left = right;
                right = temp;
            }
        }

        private void showReceipt()
        {
            scaleReceiptImage(receiptPictureBox.Image);
        }

        Label createLabel( string text, ContentAlignment align, bool isHeader )
        {
            var label = new Label() {
                Dock = DockStyle.Fill, 
                Margin = new Padding( 0 ),
                TextAlign = align
            };
            label.Text = text;
            if( isHeader ) {
                label.BackColor = Color.Gray;
                label.ForeColor = Color.White;
            }
            return label;
        }

        void clearPreviousResults()
        {
            vendorNameLabel.Text = "";
            dateTimeLabel.Text = "";
            phoneFaxLabel.Text = "";
            addressLabel.Text = "";
            totalSumLabel.Text = "";
            subtotalLabel.Text = "";
            totalTaxLabel.Text = "";
            purchaseTypeLabel.Text = "";
            tenderLabel.Text = "";
            bankCardLabel.Text = "";

            taxFieldsPanel.Controls.Clear();
            taxFieldsPanel.RowStyles.Clear();
            taxFieldsPanel.RowCount = 1;
            addTaxFieldRow( "¹", "Tax name", "Value", "Rate", true, 0);
            // reset table size
            taxFieldsPanel.AutoScroll = false;
            taxFieldsPanel.AutoScroll = true;

            lineItemsPanel.Controls.Clear();
            lineItemsPanel.RowStyles.Clear();
            lineItemsPanel.RowCount = 1;
            addLineItemRow( "¹", "Item name", "Total", true, 0);
            // reset table size
            lineItemsPanel.AutoScroll = false;
            lineItemsPanel.AutoScroll = true;
        }

        void addTaxFieldRow(string index, string name, string value, string rate, bool isHeader, int row)
        {
            taxFieldsPanel.Controls.Add(createLabel(index, ContentAlignment.MiddleCenter, isHeader), 0, row);
            taxFieldsPanel.Controls.Add(createLabel(name, ContentAlignment.MiddleCenter, isHeader), 1, row);
            taxFieldsPanel.Controls.Add(createLabel(value, ContentAlignment.MiddleCenter, isHeader), 2, row);
            taxFieldsPanel.Controls.Add(createLabel(rate, ContentAlignment.MiddleCenter, isHeader), 3, row);
        }

        private void fillTaxField(XmlNode taxFieldNode)
        {
            int row = taxFieldsPanel.RowCount;
            string type = "";
            string rate = "";
            foreach (XmlAttribute attribute in taxFieldNode.Attributes)
            {
                if (attribute.Name == "type")
                {
                    type = attribute.Value;
                }
                else if (attribute.Name == "rate")
                {
                    rate = attribute.Value;
                }
            }
            string value = taxFieldNode["normalizedValue"].InnerText;
            addTaxFieldRow(row.ToString(), type, value, rate, false, row);
            taxFieldsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            taxFieldsPanel.RowCount++;
        }

        void addLineItemRow(string index, string name, string total, bool isHeader, int row)
        {
            lineItemsPanel.Controls.Add( createLabel( index, ContentAlignment.MiddleCenter, isHeader ), 0, row );
            lineItemsPanel.Controls.Add( createLabel( name, ContentAlignment.MiddleLeft, isHeader ), 1, row );
            lineItemsPanel.Controls.Add( createLabel( total, ContentAlignment.MiddleCenter, isHeader ), 2, row );
        }

        private void fillReceiptLineItems( XmlNode lineItemsNode )
        {
            foreach (XmlNode node in lineItemsNode.ChildNodes)
            {
                string itemName = node["name"]["text"].InnerText;
                if (String.IsNullOrEmpty(itemName))
                {
                    continue;
                }
                string totalText = node["total"]["normalizedValue"].InnerText;
                int row = lineItemsPanel.RowCount;
                addLineItemRow( row.ToString(), itemName, totalText, false, row );
                lineItemsPanel.RowStyles.Add( new System.Windows.Forms.RowStyle( System.Windows.Forms.SizeType.Absolute, 20F ) );
                lineItemsPanel.RowCount++;
            }
        }

        private void fillReceiptFields( string result )
        {
            clearPreviousResults();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            if(doc.DocumentElement.ChildNodes.Count > 1)
            {
                throw new Exception( "Only one receipt per image is supported" );
            }
            foreach (XmlNode node in doc.DocumentElement.ChildNodes[0].ChildNodes)
            {
                    if (node.Name == "vendor")
                    {
                        foreach (XmlNode vendorNode in node.ChildNodes)
                        {
                            if (vendorNode.Name == "name")
                            {
                                // classifiedValue has higher priority
                                XmlElement classifiedValueElement = vendorNode["classifiedValue"];
                                if (classifiedValueElement == null || String.IsNullOrEmpty(classifiedValueElement.InnerText))
                                {
                                    XmlNode vendorNameNode = vendorNode["recognizedValue"];
                                    vendorNameLabel.Text = vendorNameNode["text"].InnerText;
                                }
                                else
                                {
                                    vendorNameLabel.Text = classifiedValueElement.InnerText;
                                }
                            }
                            else if (vendorNode.Name == "address")
                            {
                                addressLabel.Text = vendorNode["text"].InnerText.Replace("\n", " ");
                            }
                            else if (vendorNode.Name == "phone")
                            {
                                phoneFaxLabel.Text = vendorNode["normalizedValue"].InnerText;
                            }
                            else if (vendorNode.Name == "purchaseType")
                            {
                                purchaseTypeLabel.Text = vendorNode.InnerText;
                            }
                        }
                    }
                    else if (node.Name == "date")
                    {
                        dateTimeLabel.Text = node["normalizedValue"].InnerText + dateTimeLabel.Text;
                    }
                    else if (node.Name == "time")
                    {
                        dateTimeLabel.Text += " / " + node["normalizedValue"].InnerText;
                    }
                    else if (node.Name == "subTotal")
                    {
                        subtotalLabel.Text = node["normalizedValue"].InnerText;
                    }
                    else if (node.Name == "total")
                    {
                        totalSumLabel.Text = node["normalizedValue"].InnerText;
                    }
                    else if (node.Name == "tax")
                    {
                        if (node.Attributes["total"].Value == "true")
                        {
                            totalTaxLabel.Text = node["normalizedValue"].InnerText;
                        }
                        else
                        {
                            fillTaxField(node);
                        }
                    }
                    else if (node.Name == "payment")
                    {
                        tenderLabel.Text = node.Attributes["type"].InnerText;
                        if (tenderLabel.Text == "Card")
                        {
                            tenderLabel.Text = "Card";
                            bankCardLabel.Text = "<" + node.Attributes["cardType"].InnerText + ">";
                            XmlNode bankCardNode = node["cardNumber"];
                            if (bankCardNode != null)
                            {
                                bankCardLabel.Text += " <" + bankCardNode["normalizedValue"].InnerText + ">";
                            }
                        }
                    }
                    else if (node.Name == "recognizedItems")
                    {
                        fillReceiptLineItems(node);
                    }
            }
            addLineItemRow("", "", "", false, lineItemsPanel.RowCount);
            addTaxFieldRow("", "", "", "", false, taxFieldsPanel.RowCount);
        }

        private void sourceFilePanel_Resize( object sender, EventArgs e )
        {
            refreshGraphics();
        }

        private void receiptPanel_Resize(object sender, EventArgs e)
        {
            refreshGraphics();
        }

        private void restoreDefaultLabelsColors()
        {
            Color labelTitleColor = System.Drawing.SystemColors.ControlDarkDark;
            Color labelColor = System.Drawing.SystemColors.ControlText;

            vendorNameLabelTitle.ForeColor = labelTitleColor;
            vendorNameLabel.ForeColor = labelColor;
            dateTimeLabelTitle.ForeColor = labelTitleColor;
            dateTimeLabel.ForeColor = labelColor;
            phoneFaxLabelTitle.ForeColor = labelTitleColor;
            phoneFaxLabel.ForeColor = labelColor;
            addressLabelTitle.ForeColor = labelTitleColor;
            addressLabel.ForeColor = labelColor;
            totalSumLabelTitle.ForeColor = labelTitleColor;
            totalSumLabel.ForeColor = labelColor;
            subtotalLabelTitle.ForeColor = labelTitleColor;
            subtotalLabel.ForeColor = labelColor;
            totalTaxLabelTitle.ForeColor = labelTitleColor;
            totalTaxLabel.ForeColor = labelColor;
            tenderLabelTitle.ForeColor = labelTitleColor;
            tenderLabel.ForeColor = labelColor;
            bankCardLabelTitle.ForeColor = labelTitleColor;
            bankCardLabel.ForeColor = labelColor;
        }

        private void fieldLabel_Click( object sender, EventArgs e )
        {
            try {
                Label label = ( Label )sender;
                if( label != null ) {
                    Color color = activeFieldColor;
                    restoreDefaultLabelsColors();

                    if( label.Name.Contains( "vendor" ) ) {
                        activeFieldIndex = 0;
                        vendorNameLabelTitle.ForeColor = color;
                        vendorNameLabel.ForeColor = color;
                    }
                    if( label.Name.Contains( "date" ) ) {
                        activeFieldIndex = 1;
                        dateTimeLabelTitle.ForeColor = color;
                        dateTimeLabel.ForeColor = color;
                    }
                    if( label.Name.Contains( "phone" ) ) {
                        activeFieldIndex = 2;
                        phoneFaxLabelTitle.ForeColor = color;
                        phoneFaxLabel.ForeColor = color;
                    }
                    if( label.Name.Contains( "address" ) ) {
                        activeFieldIndex = 3;
                        addressLabelTitle.ForeColor = color;
                        addressLabel.ForeColor = color;
                    }
                    if( label.Name.Contains( "totalSum" ) ) {
                        activeFieldIndex = 5;
                        totalSumLabelTitle.ForeColor = color;
                        totalSumLabel.ForeColor = color;
                    }
                    if( label.Name.Contains( "subtotal" ) ) {
                        activeFieldIndex = 6;
                        subtotalLabelTitle.ForeColor = color;
                        subtotalLabel.ForeColor = color;
                    }
                    if( label.Name.Contains( "totalTax" ) ) {
                        activeFieldIndex = 7;
                        totalTaxLabelTitle.ForeColor = color;
                        totalTaxLabel.ForeColor = color;
                    }
                    if( label.Name.Contains( "tender" ) ) {
                        activeFieldIndex = 8;
                        tenderLabelTitle.ForeColor = color;
                        tenderLabel.ForeColor = color;
                    }
                    if( label.Name.Contains( "bankCard" ) ) {
                        activeFieldIndex = 9;
                        bankCardLabelTitle.ForeColor = color;
                        bankCardLabel.ForeColor = color;
                    }
                    refreshReceipt();
                }
            } catch( Exception exception ) {
                MessageBox.Show( this, exception.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        void sourceDataSplitContainerPanel2_Resize( object sender, EventArgs e )
        {
            int spaceWidth = 6;
            int width = arrowPanel.Width + spaceWidth + startRecognitionLabel.Width;
            int arrowPanelX = ( sourceDataSplitContainer.Panel2.Width - width ) / 2;
            arrowPanel.Location = new System.Drawing.Point( arrowPanelX, arrowPanel.Location.Y );
            int startRecognitionLabelX = arrowPanelX + arrowPanel.Width + spaceWidth;
            startRecognitionLabel.Location = new System.Drawing.Point( startRecognitionLabelX, startRecognitionLabel.Location.Y );
        }

        void textPanel_Resize( object sender, EventArgs e )
        {
            int width = textPanel.Size.Width - 26;
            int height = lineItemsPanel.Location.Y  - taxFieldsPanel.Location.Y;
            taxFieldsPanel.Size = new System.Drawing.Size( width, height - 60 );

            height = textPanel.Size.Height - ( taxFieldsPanel.Location.Y + taxFieldsPanel.Size.Height ) - 60 - 20;
            lineItemsPanel.Size = new System.Drawing.Size(width, height );
        }
    }
}
