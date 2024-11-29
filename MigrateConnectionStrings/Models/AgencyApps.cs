using System;

namespace MigrateConnectionStrings.Models
{
    public partial class AgencyApps
    {
        public int AgencyAppId { get; set; }
        public int AgencyId { get; set; }
        public int AppId { get; set; }
        public string Display { get; set; }
        public string StorageFolder { get; set; }
        public bool Active { get; set; }
        public string Url { get; set; }
        public bool Deleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public byte[] ConcurrencyTs { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public string WebServer { get; set; }
        public string Dbserver { get; set; }
        public string Dbname { get; set; }
        public bool AgencyHosted { get; set; }
        public decimal? PurchaseDataInMb { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int? LicensePurchase { get; set; }
        //public decimal? CurrentLDatabaseConsumptionInMB { get; set; }
        public int? CurrentLicenseConsumption { get; set; }
        //public decimal? CurrentLStorageConsumptionInMB { get; set; }
        public decimal? CurrentDataConsumptionInMB { get; set; }
        public int? RetainLogsNumberOfDays { get; set; }
        public bool IsLEFTAPurchasedWithSuite { get; set; }
        public bool IsSeparateStorage { get; set; }
        public DateTime? LastUsedDate { get; set; }
    }
}
