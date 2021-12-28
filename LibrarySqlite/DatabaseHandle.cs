using System;
using System.Linq;
using System.Data.SQLite;
using System.Collections.ObjectModel;

namespace LibrarySqlite
{
    public class DatabaseHandle
    {
        static private string _connectionString;

        static public void Init(string connectionString)
        {
            _connectionString = connectionString;
        }

        public class BackupDirectory
        {
            static public bool Insert(ObservableCollection<Class.Directory> directories)
            {
                using (var connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        var vs = from directory in directories
                                 where !string.IsNullOrEmpty(directory.Text)
                                 select directory.Text;

                        if (vs.Count() == 0) return true;

                        string command = string.Join("'),('", vs);

                        connect.Open();
                        SQLiteCommand sQLiteCommand = connect.CreateCommand();
                        sQLiteCommand.CommandText = $"INSERT INTO BackupDirectory (Directory) VALUES ('{command}');";
                        sQLiteCommand.ExecuteNonQuery();
                        connect.Close();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            static public bool Delete()
            {
                using (SQLiteConnection connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        connect.Open();
                        using (SQLiteCommand sQLiteCommand = new SQLiteCommand("DELETE FROM BackupDirectory", connect))
                        {
                            sQLiteCommand.ExecuteNonQuery();
                        }
                        connect.Close();
                        return true;
                    }
                    catch 
                    {
                        return false;
                    }
                }
            }

            static public ObservableCollection<Class.Directory> GetDirectories()
            {
                using (var connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        ObservableCollection<Class.Directory> directories = new ObservableCollection<Class.Directory>();
                        connect.Open();
                        SQLiteCommand sQLiteCommand = connect.CreateCommand();
                        sQLiteCommand.CommandText = $"SELECT (Directory) FROM BackupDirectory";
                        SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
                        while (sQLiteDataReader.Read())
                        {
                            if (sQLiteDataReader["Directory"] != null)
                            {
                                directories.Add(new Class.Directory() { Text = sQLiteDataReader["Directory"].ToString()});
                            }
                        }
                        connect.Close();
                        return directories;
                    }
                    catch //(Exception ex)
                    {
                        //Console.WriteLine(ex);
                        return null;
                    }
                }
            }

        }

        public class BackupDirectoryExclude
        {
            static public bool Insert(ObservableCollection<Class.Directory> directories)
            {
                using (var connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        var vs = from directory in directories
                                 where !string.IsNullOrEmpty(directory.Text)
                                 select directory.Text;

                        if (vs.Count() == 0) return true;

                        string command = string.Join("'),('", vs);

                        connect.Open();
                        SQLiteCommand sQLiteCommand = connect.CreateCommand();
                        sQLiteCommand.CommandText = $"INSERT INTO BackupDirectoryExclude (Directory) VALUES ('{command}');";
                        sQLiteCommand.ExecuteNonQuery();
                        connect.Close();
                        return true;
                    }
                    catch 
                    {
                        return false;
                    }
                }
            }

            static public bool Delete()
            {
                using (var connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        connect.Open();
                        SQLiteCommand sQLiteCommand = connect.CreateCommand();
                        sQLiteCommand.CommandText = $"DELETE FROM BackupDirectoryExclude";
                        sQLiteCommand.ExecuteNonQuery();
                        connect.Close();
                        return true;
                    }
                    catch 
                    {
                        return false;
                    }
                }
            }

            static public ObservableCollection<Class.Directory> GetBackupDirectoryExclude()
            {
                using (var connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        ObservableCollection<Class.Directory> directories = new ObservableCollection<Class.Directory>();
                        connect.Open();
                        SQLiteCommand sQLiteCommand = connect.CreateCommand();
                        sQLiteCommand.CommandText = $"SELECT (Directory) FROM BackupDirectoryExclude";
                        SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
                        while (sQLiteDataReader.Read())
                        {
                            if (sQLiteDataReader["Directory"] != null)
                            {
                                directories.Add(new Class.Directory() { Text = sQLiteDataReader["Directory"].ToString()});
                            }
                        }
                        connect.Close();
                        return directories;
                    }
                    catch // (Exception ex)
                    {
                        //Console.WriteLine(ex);
                        return null;
                    }
                }
            }
        }

        public class BackupLog
        {
            static public bool Insert(string fileFullName, string Comment)
            {
                using (var connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        connect.Open();
                        SQLiteCommand sQLiteCommand = connect.CreateCommand();
                        sQLiteCommand.CommandText = $"INSERT INTO BackupLog (FileFullName, BackupTime, Comment) VALUES ('{fileFullName}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{Comment}');";
                        sQLiteCommand.ExecuteNonQuery();
                        connect.Close();
                        return true;
                    }
                    catch 
                    {
                        return false;
                    }
                }
            }

            public static ObservableCollection<Class.BackupLog> GetData()
            {
                using (SQLiteConnection connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        ObservableCollection<Class.BackupLog> backupLogs = new ObservableCollection<Class.BackupLog>();

                        connect.Open();
                        SQLiteCommand sQLiteCommand = connect.CreateCommand();
                        sQLiteCommand.CommandText = $"SELECT * FROM BackupLog ORDER by BackupTime DESC LIMIT 200";
                        SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
                        while (sQLiteDataReader.Read())
                        {
                            backupLogs.Add(new Class.BackupLog()
                            {
                                Id = int.TryParse(sQLiteDataReader["ID"].ToString(), out int id)? id: 0,
                                FileFullName = sQLiteDataReader["FileFullName"].ToString(),
                                BackupTime = sQLiteDataReader["BackupTime"].ToString(),
                                Comment = sQLiteDataReader["Comment"].ToString(),
                            });
                        }
                        connect.Close();
                        return backupLogs;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            static public ObservableCollection<Class.BackupLog> GetData(string fileName, string backupTime, string comment)
            {
                using (var connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        ObservableCollection<Class.BackupLog> backupLogs = new ObservableCollection<Class.BackupLog>();

                        connect.Open();
                        SQLiteCommand sQLiteCommand = connect.CreateCommand();
                        sQLiteCommand.CommandText = $"SELECT * FROM BackupLog Where FileFullName LIKE '%{fileName}%' AND BackupTime LIKE '%{backupTime}%' AND Comment LIKE '%{comment}%'";
                        Console.WriteLine(sQLiteCommand.CommandText);
                        SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
                        while (sQLiteDataReader.Read())
                        {
                            backupLogs.Add(new Class.BackupLog()
                            {
                                Id = int.TryParse(sQLiteDataReader["ID"].ToString(), out int id)? id: 0,
                                FileFullName = sQLiteDataReader["FileFullName"].ToString(),
                                BackupTime = sQLiteDataReader["BackupTime"].ToString(),
                                Comment = sQLiteDataReader["Comment"].ToString(),
                            });
                        }
                        connect.Close();
                        return backupLogs;
                    }
                    catch 
                    {
                        return null;
                    }
                }
            }

            static public bool Delete(int Id)
            {
                using (var connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        connect.Open();
                        SQLiteCommand sQLiteCommand = connect.CreateCommand();
                        sQLiteCommand.CommandText = $"DELETE FROM BackupLog WHERE ID = '{Id}'";
                        sQLiteCommand.ExecuteNonQuery();
                        connect.Close();
                        return true;
                    }
                    catch 
                    {
                        //Console.WriteLine(ex.Message);
                        return false;
                    }
                }
            }

            static public bool DeleteAll()
            {
                using (var connect = new SQLiteConnection(_connectionString))
                {
                    try
                    {
                        connect.Open();
                        SQLiteCommand sQLiteCommand = connect.CreateCommand();
                        sQLiteCommand.CommandText = $"DELETE FROM BackupLog";
                        sQLiteCommand.ExecuteNonQuery();
                        connect.Close();
                        return true;
                    }
                    catch 
                    {
                        //Console.WriteLine(ex.Message);
                        return false;
                    }
                }
            }

        }

        public class ApplicationState
        {
            static public class AppRunning
            {
                static public bool? Get()
                {
                    using (var connect = new SQLiteConnection(_connectionString))
                    {
                        try
                        {
                            int result = -1;
                            connect.Open();
                            using (SQLiteCommand sQLiteCommand = new SQLiteCommand("SELECT AppRunning from ApplicationState", connect))
                            {
                                SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
                                while (sQLiteDataReader.Read())
                                {
                                    string temp = sQLiteDataReader["AppRunning"].ToString();
                                    int.TryParse(temp, out result);
                                }
                                connect.Close();
                                if (result > 0)
                                {
                                    return true;
                                }
                                else if (result == 0)
                                {
                                    return false;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                        catch // (Exception e)
                        {
                            //Console.WriteLine(e.Message);
                            return null;
                        }
                    }
                }

                static public bool Update(bool value)
                {
                    using (var connect = new SQLiteConnection(_connectionString))
                    {
                        try
                        {
                            connect.Open();
                            using(SQLiteCommand sQLiteCommand = new SQLiteCommand($"UPDATE ApplicationState SET AppRunning = {value}", connect))
                            {
                                sQLiteCommand.ExecuteNonQuery();
                            }
                            connect.Close();
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
            }

            static public class StartWithSystem
            {
                static public bool? Get()
                {
                    using (var connect = new SQLiteConnection(_connectionString))
                    {
                        try
                        {
                            int result = -1;
                            connect.Open();
                            using (SQLiteCommand sQLiteCommand = new SQLiteCommand("SELECT StartWithSystem from ApplicationState", connect))
                            {
                                SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
                                while (sQLiteDataReader.Read())
                                {
                                    string temp = sQLiteDataReader["StartWithSystem"].ToString();
                                    int.TryParse(temp, out result);
                                }
                                connect.Close();
                                if (result > 0)
                                {
                                    return true;
                                }
                                else if (result == 0)
                                {
                                    return false;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                        catch // (Exception e)
                        {
                            //Console.WriteLine(e.Message);
                            return null;
                        }
                    }
                }

                static public bool Update(bool value)
                {
                    using (var connect = new SQLiteConnection(_connectionString))
                    {
                        try
                        {
                            connect.Open();
                            using(SQLiteCommand sQLiteCommand = new SQLiteCommand($"UPDATE ApplicationState SET StartWithSystem = {value}", connect))
                            {
                                sQLiteCommand.ExecuteNonQuery();
                            }
                            connect.Close();
                            return true;
                        }
                        catch // (Exception ex)
                        {
                            //Console.WriteLine(ex.Message);
                            return false;
                        }
                    }
                }
            }
        }
    }
}
