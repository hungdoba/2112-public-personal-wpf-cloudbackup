using System;
using System.IO;
using LibraryCloud;
using LibrarySqlite;
using System.Timers;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace AutoBackUpToCloud
{
    class GetChangedFileAndUpload
    {
        static private Timer backupFolderSchedule { get; set; }
        static private ObservableCollection<Class.Directory> directories;
        static private ObservableCollection<Class.Directory> directoriesExcludes;

        static async public Task Start()
        {
            uint backupCycle =  Properties.Settings.Default.BackupCycle;

            TimeSpan setTime = Properties.Settings.Default.StartBackupTime;
            TimeSpan timeNow = DateTime.Now.TimeOfDay;

            directories = DatabaseHandle.BackupDirectory.GetDirectories();
            directoriesExcludes = DatabaseHandle.BackupDirectoryExclude.GetBackupDirectoryExclude();

            // Wait until the first run time
            if (timeNow > setTime) setTime += new TimeSpan(24, 0, 0);
            TimeSpan sleepTime = setTime - timeNow;

            await Task.Delay(sleepTime);

            // Start Cycle to backup time
            backupFolderSchedule = new Timer(backupCycle);
            backupFolderSchedule.Elapsed += new ElapsedEventHandler(backupFolderSchedule_Elapsed);
            backupFolderSchedule.Enabled = true;

            // Run on the first time
            await Task.Factory.StartNew(backupProcess);
        }

        static public void Stop()
        {
            if (backupFolderSchedule != null)
                backupFolderSchedule.Elapsed -= new ElapsedEventHandler(backupFolderSchedule_Elapsed);
        }

        static private void backupFolderSchedule_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Factory.StartNew(backupProcess);
        }

        private static async Task backupProcess()
        {
            if (directories.Count > 0)
            {
                foreach (var directory in directories)
                {
                    try
                    {
                        if (directory.Text.Length > 246)
                        {
                            DatabaseHandle.BackupLog.Insert(directory.Text, "Directory too long");
                            continue;
                        }
                        await backupDirectory(directory.Text);
                    }
                    catch (Exception ex)
                    {
                        DatabaseHandle.BackupLog.Insert(directory.Text, ex.Message);
                    }
                }
            }
            return;
        }

        private static async Task backupDirectory(string directory)
        {
            foreach(string file in Directory.GetFiles(directory))
            {
                if (file.Length > 258)
                {
                    DatabaseHandle.BackupLog.Insert(file, "Filename too long");
                    continue;
                }
                if (isBackup(file))
                {
                    await CloudHandle.UploadFileAsync(file);
                }
            }

            foreach(string subDirectory in Directory.GetDirectories(directory))
            {
                if (directoriesExcludes.Contains(new Class.Directory() { Text = subDirectory}))
                {
                    continue;
                }
                await backupDirectory(subDirectory);
            }
        }

        private static bool isBackup(string fileFullName)
        {
            if (File.GetAttributes(fileFullName).HasFlag(FileAttributes.Directory)) return false;

            DateTime lastestTime = LastestTime(fileFullName);

            return DateTime.Compare(lastestTime, DateTime.Now.AddMilliseconds(-Properties.Settings.Default.BackupCycle)) > 0;
        }


        private static DateTime LastestTime(string path)
        {
            DateTime lastestTime = new DateTime(1900, 1, 1);

            try { lastestTime = File.GetLastWriteTime(path); }
            catch
            {
                DatabaseHandle.BackupLog.Insert(path, "Not has last write time");
            }

            return lastestTime;
        }
    }
}
