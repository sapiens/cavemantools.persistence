using System;
using System.Data.Common;
using CavemanTools.Model.Persistence;
using SqlFu;
using SqlFu.Builders;

namespace CavemanTools.Persistence
{
    public static class IdempotencyTools
    {
        public static string DefaultTableName = "IdempotencyStore";
        public static string DefaultSchema = "";
        class IdemStore
        {
            public string Hash { get; set; }
        }
        public class StoreCreator : ICreateStorage
        {
          
            private readonly IDbFactory _db;

            public StoreCreator(IDbFactory db)
            {
                _db = db;
            }

            public void CreateAndIfExists(Just ifExists)
            {
                _db.Do(db =>
                {
                    db.CreateTableFrom<IdemStore>(t =>
                    {
                        t.TableName(DefaultTableName, DefaultSchema)
                            .Column(ta => ta.Hash, c => c.HasDbType("char").HasSize(32))
                            .PrimaryKey(pk => pk.OnColumns(d => d.Hash))
                            .IfTableExists(ifExists);
                    });

                });
            }

            public void Create()
                => CreateAndIfExists(Just.Ignore);
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