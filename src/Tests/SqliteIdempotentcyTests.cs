using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using CavemanTools.Persistence.Sql;
using SqlFu;
using SqlFu.Providers.Sqlite;
using SqlFu.Providers.SqlServer;
using Xunit;

namespace Tests
{
    [Collection("Sqlite")]
    public class SqliteIdempotentcyTests : AIdempotencyTests
    {
        private IDbFactory _factory;

        public SqliteIdempotentcyTests()
        {
          
        }

        protected override DbConnection GetConnection()
        {
            _factory = Setup.DbFactory(new SqliteProvider(SQLiteFactory.Instance.CreateConnection),
                Setup.SqliteConnectionString);
            return _factory.Create();
        }

        protected override void Init()
        {
            IdempotencyTools.CreateStorage(_db);
        }

        protected override void DisposeOther()
        {
            _db.Execute($"drop table {_db.GetTableName<IdempotencyTools.IdemStore>()}");
        }
    }

    [Collection("SqlServer")]
    public class SqlServerIdemTest : AIdempotencyTests
    {
        private IDbFactory _factory;

        public SqlServerIdemTest()
        {

        }

        protected override DbConnection GetConnection()
        {
            _factory = Setup.DbFactory(new SqlServer2012Provider(SqlClientFactory.Instance.CreateConnection),
                Setup.SqlServerConnectionString);
            return _factory.Create();
        }

        protected override void Init()
        {
            IdempotencyTools.CreateStorage(_db);
        }

        protected override void DisposeOther()
        {
            _db.Execute($"drop table {_db.GetTableName<IdempotencyTools.IdemStore>()}");
        }
    }
}