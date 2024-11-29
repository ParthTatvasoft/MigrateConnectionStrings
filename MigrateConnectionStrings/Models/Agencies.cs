using MigrateConnectionStrings.Models;
using System;
using System.Collections.Generic;

namespace MigrateConnectionStrings.Models
{
    public partial class Agencies
    {
        public Agencies()
        {
        }

        public int AgencyId { get; set; }
        public string Name { get; set; }
        public string Display { get; set; }
        public bool Active { get; set; }
        public string TimeZone { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string AltPhone { get; set; }
        public string BillingStreetAddress { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingZip { get; set; }
        public string BillingPhone { get; set; }
        public string BillingAltPhone { get; set; }
        public bool Itsupport { get; set; }
        public string Comments { get; set; }
        public bool Deleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public string VendorNumber { get; set; }
        public string LogoUrl { get; set; }
        public int? CustomTimeout { get; set; }
        public string AgencyIdentifier { get; set; }
        public int? LicensePurchase { get; set; }
        public decimal? PurchaseDataInMb { get; set; }
        public int? CurrentLicenseConsumption { get; set; }
        public bool SHIELDSuite { get; set; }
        public bool EnableSegments { get; set; }
        //public decimal? CurrentStorageConsumptionInMB { get; set; } //this will be dropped
        public decimal? CurrentDataConsumptionInMB { get; set; }
        public decimal? SystemDataConsumptionInMB { get; set; }
        public bool EnableShieldDashboard { get; set; }
        public int? MaxImageUploadSizeinMB { get; set; }
        public string MetadataAddress { get; set; }
        public string Wtrealm { get; set; }
        public string ClientID { get; set; }
        public string MobileAppTenantID { get; set; }
        public bool IsDBConsolidate { get; set; }
    }
}
