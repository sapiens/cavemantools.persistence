using System;
using System.Data.Common;
using CavemanTools.Model.Persistence;
using SqlFu;
using SqlFu.Builders;
using SqlFu.Builders.CreateTable;

namespace CavemanTools.Persistence
{
    public static class IdempotencyTools
    {
        public static string DefaultTableName = "IdempotencyStore";
        public static string DefaultSchema = "";

        public class IdemStore
        {
            public string Hash { get; set; }
        }

        public static void InitStorage<T>(T factory,TableExistsAction ifExists=TableExistsAction.Ignore) where T : IDbFactory
            =>new StoreCreator(factory).IfExists(ifExists).Create();

        public class StoreCreator : ATypedStorageCreator<IdemStore>
        {

         
            protected override void Configure(IConfigureTable<IdemStore> cfg)
            {
                cfg.TableName(DefaultTableName, DefaultSchema)
                            .Column(ta => ta.Hash, c => c.HasDbType("char").HasSize(32))
                            .PrimaryKey(pk => pk.OnColumns(d => d.Hash))
                            .HandleExisting(HandleExistingTable);
            }

            public StoreCreator(IDbFactory db) : base(db)
            {
            }
        }
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
    }

  
}