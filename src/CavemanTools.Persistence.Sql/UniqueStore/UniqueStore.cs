using System;
using System.Data.Common;
using System.Linq;
using CavemanTools.Model.Persistence.UniqueStore;
using SqlFu;
using SqlFu.Builders;

namespace CavemanTools.Persistence.UniqueStore
{
    public class UniqueStore:IStoreUniqueValues
    {
        private readonly IDbFactory _db;
        public const string Table = "UniqueStore";

       
        /// <summary>
        /// Creates an instance of <see cref="UniqueStore"/>. This method should be registered in the DI Container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IStoreUniqueValues GetInstance<T>(T factory) where T : IDbFactory
        => new UniqueStore(factory);

        protected UniqueStore(IDbFactory db)
        {
            _db = db;
        }

        public void Add(UniqueStoreItem item)
            =>
                _db.Do(db =>
                {
                    using (var t = db.BeginTransaction())
                    {
                        if (db.IsDuplicateOperation(item.ToIdempotencyId())) return;

                        try
                        {

                            item.Uniques
                            .Select(d =>new UniqueStoreRow(item.EntityId,d.Scope,d.Aspect,d.Value,item.Bucket))
                            .ForEach(row=> db.Insert(row));

                            t.Commit();
                        }
                        catch (DbException ex) when (db.IsUniqueViolation(ex))
                        {
                            throw new UniqueStoreDuplicateException(ex.Message);                            
                        }
                    }
                });




        public void Delete(Guid entityId)
        {
            _db.Do(db =>
            {
                db.DeleteFrom<UniqueStoreRow>(d => d.EntityId == entityId);
            });
        }

        public void Delete(string bucketId)
        {
            _db.Do(db =>
            {
                db.DeleteFrom<UniqueStoreRow>(d => d.Bucket == UniqueStoreRow.Pack(bucketId));
            });
        }

        public void Delete(UniqueStoreDeleteItem item) => _db.Do(db =>
        {
            db.DeleteFrom<UniqueStoreRow>(
                d => d.EntityId == item.EntityId && d.Aspect == UniqueStoreRow.Pack(item.Aspect));
        });

        public void Update(UniqueStoreUpdateItem item) => _db.Do(db =>
        {
            using (var t = db.BeginTransaction())
            {
                if (db.IsDuplicateOperation(item.ToIdempotencyId())) return;

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
        });

        /// <summary>
        /// Creates the table used for tracking uniques
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory"></param>
        public static void InitStorage<T>(T factory, TableExistsAction ifExists = TableExistsAction.Ignore) where T : IDbFactory
            => new UniqueStorageCreator(factory).IfExists(ifExists).Create();
    }
}