namespace MigrateConnectionStrings.Models
{
    public class AppTenant
    {
        public int TenantID { get; set; }
        public string DBConnectionString { get; set; }
        public string IfirDBConnectionString { get; set; }
        public string PassDBConnectionString { get; set; }
        public string FactsDBConnectionString { get; set; }
        public string LeftaDBConnectionString { get; set; }
        public string LeftaDB2ConnectionString { get; set; }
        public string LeftaDB3ConnectionString { get; set; }
        public string LeftaDB4ConnectionString { get; set; }
        public string LeftaDB5ConnectionString { get; set; }
        public string LeftaDB6ConnectionString { get; set; }
        public string LeftaDB7ConnectionString { get; set; }
        public string LeftaDB8ConnectionString { get; set; }
        public string LeftaDB9ConnectionString { get; set; }
        public string LeftaDB10ConnectionString { get; set; }
        public string MetrDBConnectionString { get; set; }
        public string LoggingDBConnectionString { get; set; }
        public string AcadDBConnectionString { get; set; }
        public string ViprDBConnectionString { get; set; }
        public string EtcDBConnectionString { get; set; }
        public string InternalAffairsDBConnectionString { get; set; }
        public string RecapDBConnectionString { get; set; }
        public string EmcotDBConnectionString { get; set; }
        public string Wtrealm { get; set; }
        public string MetadataAddress { get; set; }
        public string ClientID { get; set; }
        public string MobileAppTenantID { get; set; }
    }
}
