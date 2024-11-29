using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateConnectionStrings.Models
{
    public class ConnectionStringSections
    {
        public string? ConnectionString { get; set; }
        public string? InitialCatalog { get; set; }
        public string? UserID { get; set; }
        public string? Password { get; set; }
    }
}
