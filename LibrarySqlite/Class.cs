namespace LibrarySqlite
{
    public class Class
    {
        public class BackupLog
        {
            public int Id { get; set; }

            public string Comment { get; set; }

            public string BackupTime { get; set; }

            public string FileFullName { get; set; }

        }

        public class Directory
        {
            public string Text { get; set; }
        }
    }
}
