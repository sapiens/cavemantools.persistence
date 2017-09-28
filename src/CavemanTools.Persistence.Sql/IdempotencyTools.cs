using CavemanTools.Model.Persistence;
using SqlFu;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using SqlFu.Configuration;

namespace CavemanTools.Persistence.Sql
{
    public static class IdempotencyTools
    {
        public const string DefaultTableName = "IdempotencyStore";
        public const string DefaultSchema = "";

        public class IdemStore
        {

            public string Hash { get; set; }
            public DateTime UtcTimestamp { get; set; }  =DateTime.UtcNow;
        }

        public static void CreateSqliteStorage(DbConnection db)
            => InitStorage(db, CreateSqlite());
        public static void CreateMSSqlStorage(DbConnection db,string schema)
            => InitStorage(db, CreateMSSql(new TableName(DefaultTableName,schema??DefaultSchema)));


        private static void InitStorage(DbConnection db,string sql,string schema = null)
        {
            db.AddDbObjectOrIgnore(sql);            
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="factory"></param>
        ///// <param name="name"></param>
        ///// <param name="schema"></param>
        ///// <param name="ifExists"></param>
        //public static void InitStorage<T>(T factory,string name=DefaultTableName,string schema=DefaultSchema,TableExistsAction ifExists=TableExistsAction.Ignore) where T : IDbFactory        
        //{
        //    new StoreCreator(factory).WithTableName(name, schema).IfExists(ifExists).Create();
        //    //SqlFuManager.Config.ConfigureTableForPoco<IdemStore>(
        //    //    c => c.Table = new SqlFu.Configuration.TableName(name, schema));
        //}

        //public class StoreCreator : ATypedStorageCreator<IdemStore>
        //{

         
        //    protected override void Configure(IConfigureTable<IdemStore> cfg)
        //    {
        //        cfg
        //        .Column(ta => ta.Hash, c => c.HasDbType("char").HasSize(32))
        //        .PrimaryKey(pk => pk.OnColumns(d => d.Hash))
        //        .HandleExisting(HandleExistingTable);
        //    }

        //    public StoreCreator(IDbFactory db) : base(db)
        //    {
        //    }
        //}
        public static bool IsDuplicateOperation(this DbConnection db, IdempotencyId data)
        {
            data.MustNotBeNull();
            try
            {
                db.Insert(new IdemStore() {Hash = data.GetStorableHash()});
            }
            catch (DbException ex)
            {
                if (db.IsUniqueViolation(ex)) return true;
                throw;
            }
            return false;
        }
        public static async Task<bool> IsDuplicateOperationAsync(this DbConnection db, IdempotencyId data,CancellationToken cancel)
        {
            data.MustNotBeNull();
            try
            {
                await db.InsertAsync(new IdemStore() {Hash = data.GetStorableHash()},cancel).ConfigureFalse();
            }
            catch (DbException ex)
            {
                if (db.IsUniqueViolation(ex)) return true;
                throw;
            }
            return false;
        }

        static string CreateSqlite() => $"create table if not exists {DefaultTableName} (Hash text primary key not null, UtcTimestamp text not null)";
        

        static string CreateMSSql(TableName name)
        => $"create table  {name} (Hash char(32) primary key not null, UtcTimestamp date not null)";
    }
    
  
}