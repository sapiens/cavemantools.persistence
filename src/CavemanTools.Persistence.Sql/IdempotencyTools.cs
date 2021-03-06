﻿using CavemanTools.Model.Persistence;
using SqlFu;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using SqlFu.Configuration;
using SqlFu.Providers.Sqlite;
using SqlFu.Providers.SqlServer;

namespace CavemanTools.Persistence.Sql
{
    public static class IdempotencyTools
    {
        public const string DefaultTableName = "IdempotencyStore";
        public const string DefaultSchema = "";

        public class IdemStore
        {

            public string Hash { get; set; }
            public DateTime UtcTimestamp { get; set; }  =DateTime.UtcNow;
        }

        static void ConfigSql(SqlFuConfig cfg, TableName name) =>
            cfg.ConfigureTableForPoco<IdemStore>(t => t.TableName = name);

        public static void CreateStorage(DbConnection db,string schema=null)
        {
            
            var name = new TableName(DefaultTableName, schema ?? DefaultSchema);
            db.CreateStorage(
            pp=>pp
            .When<SqlServer2012Provider>($"create table  {name} (Hash char(32) primary key not null, UtcTimestamp date not null)")
            .When<SqliteProvider>($"create table if not exists {DefaultTableName} (Hash text primary key not null, UtcTimestamp text not null)")
            , c=>ConfigSql(c,name));
                        
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
        public static async Task<bool> IsDuplicateOperationAsync(this DbConnection db, IdempotencyId data,CancellationToken cancel)
        {
            data.MustNotBeNull();
            try
            {
                await db.InsertAsync(new IdemStore() {Hash = data.GetStorableHash()},cancel).ConfigureFalse();
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