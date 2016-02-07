 
using System;
using FluentAssertions;
using Xunit;
using System.Data.Common;
using CavemanTools.Persistence;
using SqlFu;

namespace Tests
{
    public class UniquesStoreTests : IDisposable
    {
        private DbConnection _db = Setup.GetConnection();

        public UniquesStoreTests()
        {
          UniqueStore<> _sut=new UniqueStore<>();
        }

        [Fact]
        public void testName()
        {
            
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
} 
