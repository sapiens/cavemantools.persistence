//using SqlFu;
//using SqlFu.Builders.CreateTable;

//namespace CavemanTools.Persistence.Sql.UniqueStore
//{
//    public class UniqueStorageCreator : ATypedStorageCreator<UniqueStoreRow>
//    {
//        public const string DefaultTableName = "uniques";
//        public const string DefaultSchema = "";

//        public UniqueStorageCreator(IDbFactory db) : base(db)
//        {
//        }

       
//        protected override void Configure(IConfigureTable<UniqueStoreRow> cfg)
//        {
//            cfg.Column(d => d.Scope, c => c.HasDbType("char").HasSize(32).NotNull())
//                .Column(d => d.Aspect, c => c.HasDbType("char").HasSize(32).NotNull())
//                .Column(d => d.Value, c => c.HasDbType("char").HasSize(32).NotNull())
//                .Column(d => d.Bucket, c => c.HasDbType("char").HasSize(32).NotNull())
//                .Index(i => i.OnColumns(c=>c.Bucket,c => c.Scope,c=>c.Aspect,c=>c.Value).Unique())
//                .Index(d=>d.OnColumns(c=>c.EntityId))
//                .HandleExisting(HandleExistingTable);
//        }
//    }
//}