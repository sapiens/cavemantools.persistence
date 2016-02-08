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
        public const string DefaultTableName = "IdempotencyStore";
        public const string DefaultSchema = "";

        public class IdemStore
        {
            public string Hash { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory"></param>
        /// <param name="name"></param>
        /// <param name="schema"></param>
        /// <param name="ifExists"></param>
        public static void InitStorage<T>(T factory,string name=DefaultTableName,string schema=DefaultSchema,TableExistsAction ifExists=TableExistsAction.Ignore) where T : IDbFactory
            =>new StoreCreator(factory).WithTableName(name,schema).IfExists(ifExists).Create();

        public class StoreCreator : ATypedStorageCreator<IdemStore>
        {

         
            protected override void Configure(IConfigureTable<IdemStore> cfg)
            {
                cfg
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