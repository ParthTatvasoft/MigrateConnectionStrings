using System;
using System.Collections.Generic;

namespace MigrateConnectionStrings.Models
{
    public partial class Apps
    {
        public Apps()
        {
            AgencyApps = new HashSet<AgencyApps>();
        }

        public int AppId { get; set; }
        public string Name { get; set; }
        public string Display { get; set; }
        public bool Deleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public byte[] ConcurrencyTs { get; set; }

        public virtual ICollection<AgencyApps> AgencyApps { get; set; }
    }
}
