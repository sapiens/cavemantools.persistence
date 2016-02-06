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
        public class StoreCreator : ICreateStorage
        {
            class IdemStore
            {
                public string Hash { get; set; }
            }
            private readonly IDbFactory _db;

            public StoreCreator(IDbFactory db)
            {
                _db = db;
            }

            public void Create()
            {
                _db.Do(db =>
                {
                    db.CreateTableFrom<IdemStore>(t =>
                    {
                        t.TableName(DefaultTableName,DefaultSchema)
                            .Column(ta => ta.Hash, c => c.HasDbType("char").HasSize(32))
                            .PrimaryKey(pk => pk.OnColumns(d => d.Hash))
                            .IfTableExists(Just.Ignore);
                    });

                });
            }
        }
        public static bool IsDuplicateOperation(this DbConnection db, IdempotencyId data)
        {
            data.MustNotBeNull();
            try
            {
                db.Insert(data);
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