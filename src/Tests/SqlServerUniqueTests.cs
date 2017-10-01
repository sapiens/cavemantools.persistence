using System.Data.SqlClient;
using CavemanTools.Persistence.Sql.UniqueStore;
using SqlFu;
using SqlFu.Providers.SqlServer;
using Xunit;

namespace Tests
{
    [Collection("sqlserver unique")]
    public class SqlServerUniqueTests : AUniqueStoreTests
    {
        public static string ConnectionString =>
            Setup.IsAppVeyor
                ? @"Server=(local)\SQL2016;Database=tempdb;User ID=sa;Password=Password12!"
                : @"Data Source=.\SQLExpress;Initial Catalog=tempdb;Integrated Security=True;MultipleActiveResultSets=True";

        public SqlServerUniqueTests()
        {

        }

        protected override IDbFactory GetFactory()
            => new SqlFuConfig().CreateFactoryForTesting(new SqlServer2012Provider(SqlClientFactory.Instance.CreateConnection),
                ConnectionString);

        protected override void Init()
        {
            using (var db = _db.Create())
            {
                UniqueStore.InitStorage(db);
            }
            
        }

        protected override void DisposeOther()
        {
            using (var db = _db.Create())
            {
                var tableInfo = db.GetTableName<UniqueStoreRow>();
                db.Execute($"drop table {tableInfo}");
            }
        }
    }
}