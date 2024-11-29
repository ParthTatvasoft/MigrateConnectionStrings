using System;

namespace MigrateConnectionStrings.Models
{
    public partial class AppConfigurations
    {
        public int AppConfigurationId { get; set; }
        public int AgencyAppId { get; set; }
        public string DBConnectionString { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int AgencyId { get; set; }
        public bool Deleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
