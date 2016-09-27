using System.Windows.Forms;

namespace Sample
{
    class ProgressBarForm : Form, IMessageFilter
    {
        public ProgressBarForm( Form owner )
        {
            Text = "Please wait...";
            Height = 90;
            Width = 350;
            MinimizeBox = false;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            label = new Label();
            label.Dock = DockStyle.Top;
            label.Height = 26;
            label.Padding = new Padding( 5 );
            Controls.Add( label );
            progressBar = new ProgressBar();
            progressBar.Top = label.Bottom + 5;
            progressBar.Left = 5;
            progressBar.Width = ClientSize.Width - 10;
            Controls.Add( progressBar );

            Show( owner );
            CenterToParent();
            
            Application.UseWaitCursor = true;
            Application.AddMessageFilter( this );
            
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_NOCLOSE = 0x200;
                const int WS_EX_APPWINDOW = 0x40000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_NOCLOSE;
                cp.ExStyle &= ~WS_EX_APPWINDOW;
                return cp;
            }
        }

        private const int WM_MOUSEFIRST = 0x0200;
        private const int WM_MOUSELAST  = 0x020A;
        private const int WM_KEYFIRST = 0x0100;
        private const int WM_KEYLAST = 0x0109;

        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            if( m.Msg >= WM_MOUSEFIRST && m.Msg <= WM_MOUSELAST ) { 
                return true;
            }
            if( m.Msg >= WM_KEYFIRST && m.Msg <= WM_KEYLAST ) { 
                return true;
            }
            return false;
        }
        
        public void ShowProgress( int progress ) 
        { 
            progressBar.Value = progress;
            
            CenterToParent();
            Application.DoEvents();
        }

        public void ShowMessage( string message ) 
        { 
            label.Text = message;
            
            CenterToParent();
            Application.DoEvents();
        }

        public void EndProgress()
        {
            Application.UseWaitCursor = false;
            Application.RemoveMessageFilter( this );
            base.Close();
        }

        ProgressBar progressBar;
        Label label;
    }
}
