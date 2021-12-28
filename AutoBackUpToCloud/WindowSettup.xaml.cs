using System;
using System.IO;
using System.Linq;
using LibraryCloud;
using LibrarySqlite;
using System.Windows;
using IWshRuntimeLibrary;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AutoBackUpToCloud
{
    /// <summary>
    /// Interaction logic for WindowSettup.xaml
    /// </summary>
    public partial class WindowSettup : Window
    {
        private ObservableCollection<Class.Directory> Directories { get; set; }

        private ObservableCollection<Class.Directory> DirectoriesExclude { get; set; }

        readonly NotifyIcon notifyIcon;

        public WindowSettup()
        {
            InitializeComponent();

            // Show icon in Taskbar with double click is open Program
            notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("Main.ico"),
                Visible = true
            };
            notifyIcon.DoubleClick += showWindow_DoubleClick;
        }

        #region Prevent launching Application multiple time

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == User32API.WM_SHOWME)
            {
                if (!IsVisible)
                {
                    Show();
                }

                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }

                Activate();
                Topmost = true;  // important
                Topmost = false; // important
                Focus();         // important
            }

            return IntPtr.Zero;
        }

        #endregion

        private void showWindow_DoubleClick(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Show();
                WindowState = WindowState.Normal;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FirstTimeRun();

            string connectionString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CloudBackup",  "LocalDatabase.db");
            connectionString = "Data Source=" + connectionString;

            DatabaseHandle.Init(connectionString);

            Directories = DatabaseHandle.BackupDirectory.GetDirectories();
            DirectoriesExclude = DatabaseHandle.BackupDirectoryExclude.GetBackupDirectoryExclude();

            gridDirectory.ItemsSource = Directories;
            gridDirectoryExclude.ItemsSource = DirectoriesExclude;

            txtBackupCycle.Text = Properties.Settings.Default.BackupCycle.ToString();
            txtStartBackupTime.Text = Properties.Settings.Default.StartBackupTime.ToString();

            cbStartWithSystem.IsChecked = DatabaseHandle.ApplicationState.StartWithSystem.Get();

            bool? isAppRunning = DatabaseHandle.ApplicationState.AppRunning.Get();

            if (isAppRunning == true)
            {
                startProgram();
            }
            else if(isAppRunning == false)
            {
                stopProgram();
            }
            else
            {
                System.Windows.MessageBox.Show("Can not read App running state");
            }
        }

        private void FirstTimeRun()
        {
            string localDatabaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CloudBackup");
            string localDatabasePath = Path.Combine(localDatabaseDirectory, "LocalDatabase.db");

            // This is first time run
            if (!Directory.Exists(localDatabaseDirectory))
            {
                //Create directory
                Directory.CreateDirectory(localDatabaseDirectory);

                // Copy database to local
                string databasePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "LocalDatabase.db");
                System.IO.File.Copy(databasePath, localDatabasePath, false);
            }
        }

        private void btnNewDirectory_Click(object sender, RoutedEventArgs e)
        {
            using (var folder = new FolderBrowserDialog())
            {
                DialogResult result = folder.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(folder.SelectedPath))
                {
                    if (cbBackupDirectory.IsChecked == true)
                    {
                        Directories.Add(new Class.Directory() { Text = folder.SelectedPath });
                    }
                    else
                    {
                        DirectoriesExclude.Add(new Class.Directory() { Text = folder.SelectedPath });
                    }

                    btnStart.IsEnabled = false;
                    btnInsertToDatabase.BorderBrush = System.Windows.Media.Brushes.Red;
                }
            }
        }

        private void btnDeleteDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (cbBackupDirectory.IsChecked == true && gridDirectory.SelectedItems != null)
            {
                List<Class.Directory> directories = new List<Class.Directory>();
                foreach (Class.Directory directory in gridDirectory.SelectedItems)
                {
                    directories.Add(directory);
                }

                foreach (var temp in directories)
                {
                    Directories.Remove(temp);
                }
            }
            else if (cbBackupDirectory.IsChecked == false && gridDirectoryExclude.SelectedItems != null)
            {
                List<Class.Directory> directories = new List<Class.Directory>();
                foreach (Class.Directory directory in gridDirectoryExclude.SelectedItems)
                {
                    directories.Add(directory);
                }

                foreach (var temp in directories)
                {
                    DirectoriesExclude.Remove(temp);
                }
            }

            btnStart.IsEnabled = false;
            btnInsertToDatabase.BorderBrush = System.Windows.Media.Brushes.Red;
        }

        private void btnInsertToDatabase_Click(object sender, RoutedEventArgs e)
        {
            if (checkDuplicateValues() == false) return;
            if (checkBackupTimeSetting() == false) return;

            bool success = true;

            if (DatabaseHandle.BackupDirectory.Delete() && DatabaseHandle.BackupDirectoryExclude.Delete())
            {
                if (Directories != null)
                {
                    if (DatabaseHandle.BackupDirectory.Insert(Directories) == false)
                    {
                        success = false;
                    }
                }

                if (DirectoriesExclude != null)
                {
                    if (DatabaseHandle.BackupDirectoryExclude.Insert(DirectoriesExclude) == false)
                    {
                        success = false;
                    }
                }

                Properties.Settings.Default.StartBackupTime = TimeSpan.Parse(txtStartBackupTime.Text);
                Properties.Settings.Default.BackupCycle = uint.Parse(txtBackupCycle.Text);
                Properties.Settings.Default.Save();

                // Start with system
                bool startWithSystem = (bool)cbStartWithSystem.IsChecked;
                DatabaseHandle.ApplicationState.StartWithSystem.Update(startWithSystem);
                StartWithSystem(startWithSystem);
                // End start with system
            }

            if (success == true)
            {
                System.Windows.MessageBox.Show("Success");
            }
            else
            {
                System.Windows.MessageBox.Show("Error");
            }

            btnStart.IsEnabled = true;
            btnInsertToDatabase.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
        }


        private void StartWithSystem (bool value)
        {
            string shortcutLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Cloud backup.lnk");

            if (System.IO.File.Exists(shortcutLocation))
            {
                System.IO.File.Delete(shortcutLocation);
            }

            if (value == true) //Set start with system
            {

                string appLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

                shortcut.TargetPath = appLocation;
                shortcut.Description = "BackupCloud shortcut";
                shortcut.WorkingDirectory = Path.GetDirectoryName(appLocation);
                shortcut.IconLocation = Path.Combine(Path.GetDirectoryName(appLocation), "Main.ico");
                shortcut.Save();
            }
        }

        private bool checkBackupTimeSetting()
        {
            if (string.IsNullOrEmpty(txtBackupCycle.Text))
            {
                System.Windows.MessageBox.Show("Backup Time Cycle is Empty");
                return false;
            }

            if (string.IsNullOrEmpty(txtStartBackupTime.Text))
            {
                System.Windows.MessageBox.Show("Backup Time is Empty");
                return false;
            }

            if (!uint.TryParse(txtBackupCycle.Text, out _))
            {
                System.Windows.MessageBox.Show("Backup Time Cycle is wrong");
                return false;
            }

            if (!TimeSpan.TryParse(txtStartBackupTime.Text, out _))
            {
                System.Windows.MessageBox.Show("Backup Time is wrong");
                return false;
            }

            return true;
        }

        private bool checkDuplicateValues()
        {
            if (Directories != null)
            {
                var query = Directories.GroupBy(x => x)
                    .Where(y => y.Count() > 1)
                    .Select(y => new { Element = y.Key, Counter = y.Count() })
                    .ToList();

                if (query.Count > 0)
                {
                    string message = string.Empty;
                    foreach (var temp in query)
                    {
                        message += "   -  " + temp.Element + "  :  " + temp.Counter.ToString() + "\n";
                    }
                    _ = System.Windows.MessageBox.Show("BackupDirectory has Duplicate Value: \n" + message);
                    return false;
                }
            }

            if (DirectoriesExclude != null)
            {
                var query = DirectoriesExclude.GroupBy(x => x)
                    .Where(y => y.Count() > 1)
                    .Select(y => new { Element = y.Key, Counter = y.Count() })
                    .ToList();

                if (query.Count > 0)
                {
                    string message = string.Empty;
                    foreach (var temp in query)
                    {
                        message += "   -  " + temp.Element + "  :  " + temp.Counter.ToString() + "\n";
                    }
                    System.Windows.MessageBox.Show("BackupDirectoryExclude has Duplicate Value: \n" + message);
                    return false;
                }
            }

            return true;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (! DatabaseHandle.ApplicationState.AppRunning.Update(true))
            {
                System.Windows.MessageBox.Show("Cannot save state apprunning!");
            }

            startProgram();
        }

        private void startProgram()
        {
            System.Threading.Tasks.Task.Factory.StartNew(GetChangedFileAndUpload.Start);

            CloudHandle.Init(
                Properties.Settings.Default.UserName,
                Properties.Settings.Default.Password,
                Properties.Settings.Default.ClientID,
                Properties.Settings.Default.TenantID,
                Properties.Settings.Default.CloudDirectory);

            unEnableAllButton();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Visibility = Visibility.Hidden;
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (! DatabaseHandle.ApplicationState.AppRunning.Update(false))
            {
                System.Windows.MessageBox.Show("Cannot save state apprunning!");
            }

            stopProgram();
        }

        private void stopProgram()
        {
            GetChangedFileAndUpload.Stop();
            enableAllButton();
        }

        private void unEnableAllButton()
        {
            btnNewDirectory.IsEnabled = false;
            btnDeleteDirectory.IsEnabled = false;
            btnInsertToDatabase.IsEnabled = false;
            btnSetSecurityInformation.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        private void enableAllButton()
        {
            btnNewDirectory.IsEnabled = true;
            btnDeleteDirectory.IsEnabled = true;
            btnInsertToDatabase.IsEnabled = true;
            btnSetSecurityInformation.IsEnabled = true;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private void btnShowBackupLog_Click(object sender, RoutedEventArgs e)
        {
            WindowBackupLog windowBackupLog = new WindowBackupLog();
            windowBackupLog.ShowDialog();
        }

        private void btnSetSecurityInformation_Click(object sender, RoutedEventArgs e)
        {
            WindowSettingSecurity windowSettingSecurity = new WindowSettingSecurity();
            windowSettingSecurity.ShowDialog();
        }

        private void gridDirectory_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            System.Windows.MessageBox.Show(sender.GetType().ToString());
        }

        private void cbBackupDirectory_Checked(object sender, RoutedEventArgs e)
        {
            gridDirectoryExclude.IsEnabled = false;
            gridDirectory.IsEnabled = true;
        }

        private void cbBackupDirectory_Unchecked(object sender, RoutedEventArgs e)
        {
            gridDirectoryExclude.IsEnabled = true;
            gridDirectory.IsEnabled = false;
        }

        private void txtStartBackupTime_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            btnStart.IsEnabled = false;
            btnInsertToDatabase.BorderBrush = System.Windows.Media.Brushes.Red;
        }

        private void txtBackupCycle_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            btnStart.IsEnabled = false;
            btnInsertToDatabase.BorderBrush = System.Windows.Media.Brushes.Red;
        }

        private void cbStartWithSystem_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            btnInsertToDatabase.BorderBrush = System.Windows.Media.Brushes.Red;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Icon = null;
        }
    }
}
