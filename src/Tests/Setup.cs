﻿using System.Data.Common;
using System.Data.SqlClient;
using CavemanTools.Logging;
using SqlFu;
using SqlFu.Providers.SqlServer;

namespace Tests
{
    public class Setup
    {
        public const string Connex = @"Data Source=.\SQLExpress;Initial Catalog=tempdb;Integrated Security=True;MultipleActiveResultSets=True";

        static Setup()
        {
            LogManager.OutputTo(s=>System.Diagnostics.Debug.WriteLine(s));
            SqlFuManager.Configure(c =>
            {
                c.AddProfile(new SqlServer2012Provider(SqlClientFactory.Instance.CreateConnection),Connex);              
            });
        }

        public static IDbFactory GetFactory() => SqlFuManager.GetDbFactory();

        public static DbConnection GetConnection() => GetFactory().Create();


    }
}