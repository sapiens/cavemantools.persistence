using System;
using System.Data.Common;
using System.Linq;
using CavemanTools.Model.Persistence.UniqueStore;
using SqlFu;
using SqlFu.Builders.CreateTable;

namespace CavemanTools.Persistence.UniqueStore
{
    public class GenericUniqueStore<TDbFactory>:IStoreUniqueValues where TDbFactory:IDbFactory
    {
        private readonly TDbFactory _db;
        public const string Table = "UniqueStore";
      
      
        public GenericUniqueStore(TDbFactory db)
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
    }

    public class UniqueStoreRow
    {
        public Guid EntityId { get; set; }
        public string Scope { get; set; }
        public string Aspect { get; set; }
        public string Value { get; set; }
        public string Bucket { get; set; }

        private UniqueStoreRow()
        {
            
        }

     
        public UniqueStoreRow(Guid entityId, string scope, string aspect, string value, string bucket)
        {
            EntityId = entityId;
            Scope = Pack(scope);
            Aspect = Pack(aspect);
            Value = Pack(value);
            Bucket = Pack(bucket);
        }

        public static string Pack(string value) => value?.ToUpper().MurmurHash().ToBase64();        
    }

    public class UniqueStoreCreator<TDbFactory> : ATypedStorageCreator<UniqueStoreRow> where TDbFactory:IDbFactory
    {
        public static string DefaultTableName = "uniques";
        public static string DefaultSchema = "";

        public UniqueStoreCreator(TDbFactory db) : base(db)
        {
        }

        protected override void Configure(IConfigureTable<UniqueStoreRow> cfg)
        {
            cfg
                .TableName(DefaultTableName,DefaultSchema)
                .Column(d => d.Scope, c => c.HasDbType("char").HasSize(32).NotNull())
                .Column(d => d.Aspect, c => c.HasDbType("char").HasSize(32).NotNull())
                .Column(d => d.Value, c => c.HasDbType("char").HasSize(32).NotNull())
                .Column(d => d.Bucket, c => c.HasDbType("char").HasSize(32).NotNull())
                .Index(i => i.OnColumns(c=>c.Bucket,c => c.Scope,c=>c.Aspect,c=>c.Value).Unique())
                .Index(d=>d.OnColumns(c=>c.EntityId));
        }
    }

  


}