using System.Windows;

namespace AutoBackUpToCloud
{
    /// <summary>
    /// Interaction logic for WindowSettingSecurity.xaml
    /// </summary>
    public partial class WindowSettingSecurity : Window
    {
        public WindowSettingSecurity()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtAccount.Text         = Properties.Settings.Default.UserName;
            txtPassword.Text        = Properties.Settings.Default.Password;
            txtClientID.Text        = Properties.Settings.Default.ClientID;
            txtTenantID.Text        = Properties.Settings.Default.TenantID;
            txtCloudDirectory.Text  = Properties.Settings.Default.CloudDirectory;
        }

        private void btnSaveSetting_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UserName = txtAccount.Text;
            Properties.Settings.Default.Password = txtPassword.Text;
            Properties.Settings.Default.ClientID = txtClientID.Text;
            Properties.Settings.Default.TenantID = txtTenantID.Text;
            Properties.Settings.Default.CloudDirectory = txtCloudDirectory.Text;
            Properties.Settings.Default.Save();
            _ = MessageBox.Show("Success");
        }
    }
}
