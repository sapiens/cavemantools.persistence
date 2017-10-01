using System.Data.SQLite;
using CavemanTools.Persistence.Sql.UniqueStore;
using SqlFu;
using SqlFu.Providers.Sqlite;
using Xunit;

namespace Tests
{
    [Collection("sqlite unique")]
    public class SqliteUniqueTests : AUniqueStoreTests
    {
        public static string ConnectionString { get; } = "Data Source=test.db;Version=3;New=True;BinaryGUID=False";
        public SqliteUniqueTests()
        {

        }

        protected override IDbFactory GetFactory()
            => new SqlFuConfig().CreateFactoryForTesting(new SqliteProvider(SQLiteFactory.Instance.CreateConnection),
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