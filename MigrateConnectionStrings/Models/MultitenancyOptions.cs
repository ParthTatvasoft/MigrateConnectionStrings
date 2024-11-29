using System.Collections.ObjectModel;

namespace MigrateConnectionStrings.Models
{
    public class MultitenancyOptions
    {
        public string ShieldDBConnectionString { get; set; }
        public Collection<AppTenant> Tenants { get; set; }
    }
}
