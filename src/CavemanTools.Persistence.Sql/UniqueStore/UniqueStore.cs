//using System;
//using System.Data.Common;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using CavemanTools.Model.Persistence.UniqueStore;
//using SqlFu;
//using SqlFu.Builders;

//namespace CavemanTools.Persistence.Sql.UniqueStore
//{
//    public class UniqueStore : IStoreUniqueValuesAsync
//    {
//        private readonly IDbFactory _db;
//        public const string Table = "UniqueStore";


//        /// <summary>
//        /// Creates an instance of <see cref="UniqueStore"/>. This method should be registered in the DI Container
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="factory"></param>
//        /// <returns></returns>
//        public static IStoreUniqueValuesAsync GetInstance<T>(T factory) where T : IDbFactory
//        => new UniqueStore(factory);

//        protected UniqueStore(IDbFactory db)
//        {
//            _db = db;
//        }

//        public async Task AddAsync(UniqueStoreItem item, CancellationToken cancel)
//        {

//            using (var db = await _db.CreateAsync(cancel).ConfigureFalse())
//            {
//                using (var t = db.BeginTransaction())
//                {
//                    if (await db.IsDuplicateOperationAsync(item.ToIdempotencyId(), cancel)) return;

//                    try
//                    {
//                        item.Uniques
//                            .Select(d => new UniqueStoreRow(item.EntityId, d.Scope, d.Aspect, d.Value, item.Bucket))
//                            .ForEach(row => db.Insert(row));

//                        t.Commit();
//                    }
//                    catch (DbException ex) when (db.IsUniqueViolation(ex))
//                    {
//                        throw new UniqueStoreDuplicateException(ex.Message);
//                    }
//                }
//            }
        
//        }


//        public async Task DeleteAsync(Guid entityId, CancellationToken cancel)
//        {
//            using (var db = await _db.CreateAsync(cancel).ConfigureFalse())
//            {
//                await db.DeleteFromAsync<UniqueStoreRow>(cancel, d => d.EntityId == entityId).ConfigureFalse();
//            }

//        }

//        public Task DeleteAsync(string bucketId, CancellationToken cancel)
//        => _db.RetryOnTransientErrorAsync(cancel, q => q.Connection.DeleteFromAsync<UniqueStoreRow>(q.Cancel, d => d.Bucket == UniqueStoreRow.Pack(bucketId)));

//        public Task DeleteAsync(UniqueStoreDeleteItem item, CancellationToken cancel)
//            => _db.RetryOnTransientErrorAsync(cancel, q => q.Connection.DeleteFromAsync<UniqueStoreRow>(q.Cancel, d => d.EntityId == item.EntityId && d.Aspect == UniqueStoreRow.Pack(item.Aspect)));

//        public Task UpdateAsync(UniqueStoreUpdateItem item, CancellationToken cancel)
//            => _db.RetryOnTransientErrorAsync(cancel, async q =>
//             {
//                 var db = q.Connection;
//                 using (var t = db.BeginTransaction())
//                 {
//                     if (await db.IsDuplicateOperationAsync(item.ToIdempotencyId(), q.Cancel)) return;

//                     try
//                     {
//                         item.Changes.ForEach(d =>
//                        {
//                         db.Update<UniqueStoreRow>()
//                             .Set(r => r.Value, UniqueStoreRow.Pack(d.Value))
//                             .Where(r => r.EntityId == item.EntityId && r.Aspect == UniqueStoreRow.Pack(d.Aspect))
//                             .Execute();
//                     });
//                         t.Commit();
//                     }
//                     catch (DbException ex) when (db.IsUniqueViolation(ex))
//                     {
//                         throw new UniqueStoreDuplicateException(ex.Message);
//                     }
//                 }
//             });

//        /// <summary>
//        /// Creates the table used for tracking uniques
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="factory"></param>
//        /// <param name="name"></param>
//        /// <param name="schema"></param>
//        /// <param name="ifExists"></param>
//        public static void InitStorage<T>(T factory, string name = UniqueStorageCreator.DefaultTableName, string schema = UniqueStorageCreator.DefaultSchema, TableExistsAction ifExists = TableExistsAction.Ignore) where T : IDbFactory
//        {
//            new UniqueStorageCreator(factory).WithTableName(name, schema).IfExists(ifExists).Create();
//            //SqlFuManager.Config.ConfigureTableForPoco<UniqueStoreRow>(
//            //   c => c.Table = new SqlFu.Configuration.TableName(name, schema));
//        }
//    }
//}