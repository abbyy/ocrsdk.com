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
using System.Windows.Shapes;

namespace GuiTest
{
    /// <summary>
    /// Interaction logic for CredentialsInputDialog.xaml
    /// </summary>
    public partial class CredentialsInputDialog : Window
    {
        public CredentialsInputDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if( String.IsNullOrEmpty( ApplicationId.Text ) || String.IsNullOrEmpty( Password.Text ) )
                return;

            this.DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


    }
}
