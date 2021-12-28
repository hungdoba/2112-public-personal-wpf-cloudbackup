using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySqlite
{
    public class BackupLog
    {
        public int Id { get; set; }

        public string FileFullName { get; set; }

        public string BackupTime { get; set; }

        public string Error { get; set; }
    }
}
