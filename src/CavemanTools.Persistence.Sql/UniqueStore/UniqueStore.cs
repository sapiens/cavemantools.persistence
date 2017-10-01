using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CavemanTools.Model.Persistence.UniqueStore;
using SqlFu;
using SqlFu.Builders;
using SqlFu.Configuration;
using SqlFu.Providers;
using SqlFu.Providers.Sqlite;
using SqlFu.Providers.SqlServer;

namespace CavemanTools.Persistence.Sql.UniqueStore
{
    public class UniqueStore : IStoreUniqueValuesAsync
    {
        private readonly IDbFactory _db;
        public const string Table = "UniqueStore";


        /// <summary>
        /// Creates an instance of <see cref="UniqueStore"/>. This method should be registered in the DI Container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IStoreUniqueValuesAsync GetInstance<T>(T factory) where T : IDbFactory
        => new UniqueStore(factory);

        public UniqueStore(IDbFactory db)
        {
            _db = db;
        }

        public async Task AddAsync(UniqueStoreItem item, CancellationToken cancel)
        {

            using (var db = await _db.CreateAsync(cancel).ConfigureFalse())
            {
                using (var t = db.BeginTransaction())
                {
                    if (await db.IsDuplicateOperationAsync(item.ToIdempotencyId(), cancel)) return;

                    try
                    {
                        item.Uniques
                            .Select(d => new UniqueStoreRow(item.EntityId, d.Scope, d.Aspect, d.Value, item.Bucket))
                            .ForEach(row => db.Insert(row));

                        t.Commit();
                    }
                    catch (DbException ex) when (db.IsUniqueViolation(ex))
                    {
                        throw new UniqueStoreDuplicateException(ex.Message);
                    }
                }
            }

        }


        public async Task DeleteAsync(Guid entityId, CancellationToken cancel)
        {
            using (var db = await _db.CreateAsync(cancel).ConfigureFalse())
            {
                await db.DeleteFromAsync<UniqueStoreRow>(cancel, d => d.EntityId == entityId).ConfigureFalse();
            }

        }

        public async Task DeleteAsync(string bucketId, CancellationToken cancel)       
        {
            using (var db = await _db.CreateAsync(cancel).ConfigureFalse())
            {
                await db.DeleteFromAsync<UniqueStoreRow>(cancel, d => d.Bucket == UniqueStoreRow.Pack(bucketId)).ConfigureFalse();
            }

        }

        public async Task DeleteAsync(UniqueStoreDeleteItem item, CancellationToken cancel)
        {
            using (var db = await _db.CreateAsync(cancel).ConfigureFalse())
            {
                await db.DeleteFromAsync<UniqueStoreRow>(cancel, d => d.EntityId == item.EntityId && d.Aspect == UniqueStoreRow.Pack(item.Aspect)).ConfigureFalse();
            }

        }

        public async Task UpdateAsync(UniqueStoreUpdateItem item, CancellationToken cancel)
        {
            using (var db = await _db.CreateAsync(cancel).ConfigureFalse())
            {
                using (var t = db.BeginTransaction())
                {
                    if (await db.IsDuplicateOperationAsync(item.ToIdempotencyId(), cancel)) return;

                    try
                    {
                        item.Changes.ForEach(d =>
                        {
                            db.Update<UniqueStoreRow>()
                                .Set(r => r.Value, UniqueStoreRow.Pack(d.Value))
                                .Where(r => r.EntityId == item.EntityId && r.Aspect == UniqueStoreRow.Pack(d.Aspect))
                                .Execute();
                        });
                        t.Commit();
                    }
                    catch (DbException ex) when (db.IsUniqueViolation(ex))
                    {
                        throw new UniqueStoreDuplicateException(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the table used for tracking uniques
        /// </summary>
        
        public static void InitStorage(DbConnection db,string tableName=Table,string schema=null)
        {
            IdempotencyTools.CreateStorage(db,schema);
          var name = db.Provider().EscapeTableName(new TableName(tableName, schema));         

            db.CreateStorage(cf =>
            {
                cf.When<SqlServer2012Provider>(
                    $@"
create table {name}(
Id int not null identity(1,1) primary key,
EntityId uniqueidentifier not null,
[Scope] char(32) not null,
[Aspect] char(32) not null,
[Value] char(32) not null,
[Bucket] char(32) not null,
unique([Bucket],[Scope],[Aspect],[Value])
);
create index idx_US_Values on {name} (EntityId)
").When<SqliteProvider>(
                    $@"
create table if not exists {name}(
EntityId uniqueidentifier not null,
[Scope] char(32) not null,
[Aspect] char(32) not null,
[Value] char(32) not null,
[Bucket] char(32) not null,
unique([Bucket],[Scope],[Aspect],[Value])
);
create index if not exists idx_US_Values on {name} (EntityId)
");


            }, c =>
            {
            
                c.ConfigureTableForPoco<UniqueStoreRow>(t =>
                {
                    t.TableName=name;                    
                });
            });
            
        }
    }
}