using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MigrateConnectionStrings.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        List<(string tenantId, bool dbConsolidate)> consolidateTenantIds = new();
        List<(string tenantId, bool dbConsolidate)> facilitateTenantIds = new();

        // Get consolidated tenant IDs
        consolidateTenantIds = GetTenantIds(true, "consolidate", false, consolidateTenantIds);

        // Get deconsolidated tenant IDs
        consolidateTenantIds = GetTenantIds(false, "nonconsolidate", true, consolidateTenantIds);

        if (!consolidateTenantIds.Any()) return;

        // Print all tenant IDs
        Console.WriteLine("\nTenant IDs:");
        foreach ((string tenantId, bool dbConsolidate) in consolidateTenantIds)
        {
            string action = !dbConsolidate ? "Will Non-Consolidate" : "Will Consolidate";
            Console.WriteLine($"Tenant ID: {tenantId} {action}");
        }

        // Load appsettings.json
        IConfigurationRoot config = new ConfigurationManager()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        MultitenancyOptions multitenancy = config.GetSection("Multitenancy").Get<MultitenancyOptions>();

        if (consolidateTenantIds.Any(x => x.tenantId == "-1"))
        {
            bool isDBConsolidate = false;
            foreach (AppTenant tenant in multitenancy.Tenants){
                if (consolidateTenantIds.Any(x => x.tenantId == "-1" && x.dbConsolidate || x.tenantId == tenant.TenantID.ToString() 
                && x.dbConsolidate))
                            isDBConsolidate = true;
                            else {
                                isDBConsolidate = false;
                            }

                facilitateTenantIds.Add(new ValueTuple<string, bool>(tenant.TenantID.ToString(), isDBConsolidate));
            }

            consolidateTenantIds = facilitateTenantIds;
        }

        using (AppDbContext context = new AppDbContext(ConfigureDbContext(multitenancy.ShieldDBConnectionString)))
        {
            Console.WriteLine("\nYou have selected below Tenant IDs:");
            foreach ((string tenantId, bool dbConsolidate) in consolidateTenantIds)
            {
                if (!int.TryParse(tenantId, out int agencyId))
                {
                    Console.WriteLine($"Invalid Tenant ID: {tenantId}");
                    continue;
                }

                Agencies agency = await context.Agencies.FirstOrDefaultAsync(x => x.AgencyId == agencyId);
                Console.WriteLine(agency != null
                    ? $"{tenantId} - {agency.Name}"
                    : $"No agency exists with the specified Tenant ID: {tenantId}");
            }
        }

        Console.WriteLine("Are you sure to proceed with this Tenant IDs ? Enter 'y' for yes or 'n' for no");
        while (true)
        {
            string input = Console.ReadLine();
            if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Proceeding with the execution...");
                foreach ((string tenantId, bool dbConsolidate) in consolidateTenantIds)
                {
                    AppTenant tenant = multitenancy.Tenants.FirstOrDefault(x => x.TenantID == int.Parse(tenantId));
                    if (tenant != null)
                    {
                        using (AppDbContext context = new AppDbContext(ConfigureDbContext(multitenancy.ShieldDBConnectionString)))
                        {
                            if (!(await context.AppConfigurations.CountAsync(x => x.AgencyId == tenant.TenantID) == 1 && dbConsolidate)
                                && !(await context.AppConfigurations.CountAsync(x => x.AgencyId == tenant.TenantID) > 1 && !dbConsolidate))
                            {
                                Agencies agency = await context.Agencies.FirstOrDefaultAsync(x => x.AgencyId == tenant.TenantID);
                                if (agency != null)
                                {
                                    agency.IsDBConsolidate = dbConsolidate;
                                    agency.Wtrealm = tenant.Wtrealm ?? null;
                                    agency.MetadataAddress = tenant.MetadataAddress ?? null;
                                    agency.ClientID = tenant.ClientID ?? null;
                                    agency.MobileAppTenantID = tenant.MobileAppTenantID ?? null;

                                    context.Agencies.Update(agency);
                                    _ = await context.SaveChangesAsync();

                                    context.AppConfigurations.RemoveRange(context.AppConfigurations.Where(x => x.AgencyId == tenant.TenantID).ToArray());
                                    _ = await context.SaveChangesAsync();

                                    List<AgencyApps> agencyAppsList = new();
                                    agencyAppsList = await context.AgencyApps.Where(x => x.AgencyId == tenant.TenantID).ToListAsync();

                                    AgencyApps agencyApp = new()
                                    {
                                        AgencyId = tenant.TenantID,
                                        AppId = -1,
                                        AgencyAppId = -1,
                                        Deleted = false
                                    };
                                    agencyAppsList.Add(agencyApp);

                                    DataTable connectionStringTable = GenerateConnectionStringTable(agencyAppsList, tenant, dbConsolidate, agency.Name);
                                    if (connectionStringTable.Rows.Count > 0)
                                        await ExecuteDatabaseInsertionAsync(connectionStringTable, context);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No Tenant ID - {tenantId} is found in the appsettings configuration.");
                    }
                }

                Console.WriteLine("\nAll configurations have been inserted into the database.");
                Environment.Exit(0);
            }
            else if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("\nExiting the application...");
                Console.ReadLine();
                Environment.Exit(0); // Exits the application with code 0 (success).
            }
            else
                Console.WriteLine("Invalid input. Please type 'y' or 'n'.");
        }
    }

    private static ConnectionStringSections GetConnectionStringByAppId(int appId, AppTenant appTenant, string dbName)
    {
        if (appId == 5)
        {
            // Get the first matching connection string based on DBName
            string? leftaConnectionString = appTenant.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(string)) // Filter for string properties
                .Select(p => (string)p.GetValue(appTenant))   // Get property values
                .Where(value => !string.IsNullOrEmpty(value)) // Exclude null or empty values
                .FirstOrDefault(value =>
                {
                    try
                    {
                        return new SqlConnectionStringBuilder(value).InitialCatalog == dbName;
                    }
                    catch
                    {
                        return false; // Skip invalid connection strings
                    }
                });

            return string.IsNullOrEmpty(leftaConnectionString)
                ? new ConnectionStringSections()
                : ExtractConnectionString(leftaConnectionString);
        }

        // Match appId to corresponding property
        string? connectionString = appId switch
        {
            -1 => appTenant.LoggingDBConnectionString,
            0 => appTenant.DBConnectionString,
            (int)AppType.Facts => appTenant.FactsDBConnectionString,
            (int)AppType.Acad => appTenant.AcadDBConnectionString,
            (int)AppType.Ifir => appTenant.IfirDBConnectionString,
            (int)AppType.Pass => appTenant.PassDBConnectionString,
            (int)AppType.Metr => appTenant.MetrDBConnectionString,
            (int)AppType.Etc => appTenant.EtcDBConnectionString,
            (int)AppType.Vipr => appTenant.ViprDBConnectionString,
            (int)AppType.InternalAffairs => appTenant.InternalAffairsDBConnectionString,
            (int)AppType.Emcot => appTenant.EmcotDBConnectionString,
            (int)AppType.Recap => appTenant.RecapDBConnectionString,
            _ => null
        };

        return string.IsNullOrEmpty(connectionString)
            ? new ConnectionStringSections()
            : ExtractConnectionString(connectionString);
    }

    private static ConnectionStringSections ExtractConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return new ConnectionStringSections();

        try
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);

            // Create and populate ConnectionStringSections
            ConnectionStringSections connectionStringSections = new ConnectionStringSections
            {
                UserID = builder.UserID,
                Password = builder.Password,
                ConnectionString = new SqlConnectionStringBuilder(connectionString)
                {
                    UserID = string.Empty,
                    Password = string.Empty
                }.ConnectionString
            };

            return connectionStringSections;
        }
        catch (Exception ex)
        {
            // Log the exception if necessary
            Console.WriteLine($"Error parsing connection string: {ex.Message}");

            // Return an empty object if parsing fails
            return new ConnectionStringSections();
        }
    }

    private static List<(string tenantId, bool dbConsolidate)> GetTenantIds(bool value, string type, bool oppositeValue, List<(string tenantId, bool dbConsolidate)> consolidateTenantIds)
    {
        if (consolidateTenantIds.Any(t => t.tenantId == "-1")) return consolidateTenantIds;

        Console.WriteLine($"\nEnter Tenant IDs which we need to {type}:\n");
        Console.WriteLine("1. Type '-1' for all Tenant IDs");
        Console.WriteLine("2. Type 'none' to ignore the execution\n");

        Console.Write("Enter Tenant ID(s) (comma-separated or single ID): ");
        string input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Invalid input. Please enter a valid Tenant ID.");
            return consolidateTenantIds;
        }

        if (input.Contains("none", StringComparison.OrdinalIgnoreCase))
            input = string.Empty;

        if (string.Equals(input, "-1", StringComparison.OrdinalIgnoreCase))
        {
            consolidateTenantIds.Add(new ValueTuple<string, bool>(input, value));
            return consolidateTenantIds;
        }

        HashSet<string> tenantIds = input
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet();

        foreach (string tenantId in tenantIds)
        {
            if (!long.TryParse(tenantId, out _))
            {
                Console.WriteLine($"Invalid Tenant ID '{tenantId}'. IDs must be numeric.");
                continue;
            }

            if (consolidateTenantIds.Exists(t => t.tenantId == tenantId && t.dbConsolidate == value))
                Console.WriteLine($"Tenant ID '{tenantId}' already exists in the {type} list.");
            else if (consolidateTenantIds.Exists(t => t.tenantId == tenantId && t.dbConsolidate == oppositeValue))
                Console.WriteLine($"Tenant ID '{tenantId}' already exists in the {(type == "consolidate" ? "nonconsolidate" : "consolidated")} list.");
            else
                consolidateTenantIds.Add(new ValueTuple<string, bool>(tenantId, value));
        }

        return consolidateTenantIds;
    }
    private static DbContextOptions<AppDbContext> ConfigureDbContext(string connectionString)
    {
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(connectionString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        });

        return optionsBuilder.Options;
    }

    // Helper Method: Generate Connection String DataTable
    private static DataTable GenerateConnectionStringTable(List<AgencyApps> agencyApps, AppTenant tenant, bool dbConsolidate, string agencyName)
    {
        DataTable table = new();
        table.Columns.AddRange(new[]
        {
            new DataColumn("AgencyAppId", typeof(int)),
            new DataColumn("DBConnectionString", typeof(string)),
            new DataColumn("DBSchema", typeof(string)),
            new DataColumn("Username", typeof(string)),
            new DataColumn("Password", typeof(string)),
            new DataColumn("AgencyId", typeof(int)),
            new DataColumn("Deleted", typeof(bool))
        });

        foreach (AgencyApps app in agencyApps)
        {
            ConnectionStringSections connectionString = GetConnectionStringByAppId(app.AppId, tenant, app.Dbname);

            if (dbConsolidate)
            {
                connectionString.ConnectionString = new SqlConnectionStringBuilder(connectionString.ConnectionString)
                {
                    InitialCatalog = $"{agencyName.Replace(" ", "")}_Combine",
                    UserID = string.Empty,
                    Password = string.Empty,
                }.ConnectionString;
                app.AgencyAppId = 0;
            }

            table.Rows.Add(
                app.AgencyAppId,
                connectionString.ConnectionString ?? string.Empty,
                "dbo",
                connectionString.UserID ?? string.Empty,
                connectionString.Password ?? string.Empty,
                app.AgencyId,
                app.Deleted
            );

            if (dbConsolidate)
                break;
        }

        return table;
    }

    // Helper Method: Execute Database Insertion
    private async static Task ExecuteDatabaseInsertionAsync(DataTable dataTable, AppDbContext context)
    {
        await context.Database.OpenConnectionAsync();
        try
        {
            using DbCommand command = context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 230;
            command.CommandText = "InsertEncryptedUser";

            SqlParameter param = new("@AppConfigurations_TableType", SqlDbType.Structured)
            {
                TypeName = "AppConfigurations_TableType",
                Value = dataTable
            };

            command.Parameters.Add(param);
            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }
}
