using FluentAssertions;
using System;
using System.Data.Common;
using CavemanTools.Model.Persistence;
using CavemanTools.Persistence.Sql;
using Xunit;

namespace Tests
{
    public abstract class AIdempotencyTests:IDisposable
    {
        protected DbConnection _db;
     

        public AIdempotencyTests()
        {
            _db = GetConnection();
            Init();
        }

        protected virtual void Init()
        {
            
        }

        protected abstract DbConnection GetConnection();

        [Fact]
        public void insert_same_operation_returns_true()
        {
            var idem = new IdempotencyId(Guid.Empty, "mymodel");
            _db.IsDuplicateOperation(idem);
            _db.IsDuplicateOperation(idem).Should().BeTrue();
        }

        [Fact]
        public void different_operation_returns_false()
        {
            var idem = new IdempotencyId(Guid.NewGuid(), "mymodel");
            _db.IsDuplicateOperation(idem);
            _db.IsDuplicateOperation(new IdempotencyId(Guid.NewGuid(), "mymodel")).Should().BeFalse();
            _db.IsDuplicateOperation(new IdempotencyId(idem.OperationId, "mymodel1")).Should().BeFalse();
        }

        protected virtual void DisposeOther()
        {
            
        }
        public void Dispose()
        {
            DisposeOther();
            _db.Dispose();
           
    
        }
    }
}