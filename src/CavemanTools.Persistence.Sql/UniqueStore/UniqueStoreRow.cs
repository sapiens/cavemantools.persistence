using System;

namespace CavemanTools.Persistence.Sql.UniqueStore
{
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
}