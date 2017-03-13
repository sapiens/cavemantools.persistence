using System;
using System.Data.Common;
using CavemanTools.Model.Persistence;
using CavemanTools.Persistence;
using CavemanTools.Persistence.Sql;
using Xunit;
using FluentAssertions;
using SqlFu;
using SqlFu.Builders;

namespace Tests
{
    public class IdempotencyTests:IDisposable
    {
        private DbConnection _db = Setup.GetConnection();

        public IdempotencyTests()
        {
          IdempotencyTools.InitStorage(Setup.GetFactory(),"idemtest",ifExists:TableExistsAction.DropIt);
        }

        [Fact]
        public void insert_same_operation_returns_true()
        {
            var idem=new IdempotencyId(Guid.Empty, "mymodel");
            _db.IsDuplicateOperation(idem);
            _db.IsDuplicateOperation(idem).Should().BeTrue();
        }

        [Fact]
        public void different_operation_returns_false()
        {
            var idem=new IdempotencyId(Guid.NewGuid(), "mymodel");
            _db.IsDuplicateOperation(idem);
            _db.IsDuplicateOperation(new IdempotencyId(Guid.NewGuid(), "mymodel")).Should().BeFalse();
            _db.IsDuplicateOperation(new IdempotencyId(idem.OperationId, "mymodel1")).Should().BeFalse();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}