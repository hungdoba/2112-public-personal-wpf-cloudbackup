using System;
using LibrarySqlite;
using System.Windows;

namespace AutoBackUpToCloud
{
    /// <summary>
    /// Interaction logic for WindowBackupLog.xaml
    /// </summary>
    public partial class WindowBackupLog : Window
    {
        public WindowBackupLog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtBackupTime.Text = DateTime.Today.ToString("yyyy-MM-dd");

            gridBackupLog.ItemsSource = DatabaseHandle.BackupLog.GetData();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) => gridBackupLog.ItemsSource = DatabaseHandle.BackupLog.GetData(txtFileName.Text, txtBackupTime.Text, txtError.Text);

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if(gridBackupLog.SelectedItems != null)
            {
                foreach(var selectedItem in gridBackupLog.SelectedItems)
                {
                    if(selectedItem is Class.BackupLog @backupLog)
                    {
                        DatabaseHandle.BackupLog.Delete(backupLog.Id);
                    }
                }

                gridBackupLog.ItemsSource = DatabaseHandle.BackupLog.GetData(txtFileName.Text, txtBackupTime.Text, txtError.Text);
            }
        }
    }
}
