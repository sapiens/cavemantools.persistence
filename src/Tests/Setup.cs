using System;
using System.Diagnostics;
using CavemanTools.Logging;
using CavemanTools.Persistence.Sql;
using SqlFu;
using SqlFu.Configuration;
using SqlFu.Providers;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]
namespace Tests
{
    public class Setup
    {

        public static string SqlServerConnectionString =>
            IsAppVeyor
                ? @"Server=(local)\SQL2016;Database=tempdb;User ID=sa;Password=Password12!"
                : @"Data Source=.\SQLExpress;Initial Catalog=tempdb;Integrated Security=True;MultipleActiveResultSets=True";

        public static string SqliteConnectionString { get; } = "Data Source=test.db;Version=3;New=True;BinaryGUID=False";

        public static IDbFactory DbFactory(DbProvider provider, string cnx, Action<SqlFuConfig> config = null)
        {
            LogManager.OutputTo(w => Trace.WriteLine(w));

            var c = new SqlFuConfig();
            SqlFuManager.UseLogManager();
            
            c.ConfigureTableForPoco<IdempotencyTools.IdemStore>(g =>
            {
               g.TableName=new TableName(IdempotencyTools.DefaultTableName);
            });
            config?.Invoke(c);
            return c.CreateFactoryForTesting(provider, cnx);

        }

     
        public static readonly bool IsAppVeyor =
            Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";

 

    }
}