
//using System;
//using FluentAssertions;
//using Xunit;
//using System.Data.Common;
//using System.Threading;
//using System.Threading.Tasks;
//using CavemanTools;
//using CavemanTools.Logging;
//using CavemanTools.Model.Persistence.UniqueStore;
//using CavemanTools.Persistence;
//using CavemanTools.Persistence.Sql;
//using CavemanTools.Persistence.Sql.UniqueStore;
//using SqlFu;
//using SqlFu.Builders;
//using Xunit.Abstractions;

//namespace Tests
//{
//    public class UniquesStoreTests : IDisposable
//    {
//        private const string Scope = "user";
//        private const string OtherAspect = "shortname";
//        private IStoreUniqueValuesAsync _sut;
//        private Guid _entityId;


//        public UniquesStoreTests(ITestOutputHelper log)
//        {
//            LogManager.OutputTo(log.WriteLine);
//            IdempotencyTools.InitStorage(Setup.GetFactory(), ifExists: TableExistsAction.Ignore);
//            UniqueStore.InitStorage(Setup.GetFactory(), ifExists: TableExistsAction.DropIt);
//            _sut = UniqueStore.GetInstance(Setup.GetFactory());

//            _entityId = Guid.NewGuid();

//        }

//        private Task Insert()
//        {
//            var item = new UniqueStoreItem(_entityId, Guid.NewGuid()
//                , new UniqueValue("test", scope: Scope)
//                , new UniqueValue("test-p", OtherAspect, scope: Scope)
//            );
//            return _sut.AddAsync(item, CancellationToken.None);
//        }

//        public void Dispose()
//        {
//            LogManager.OutputTo(Empty.ActionOf<string>());
//        }

//        [Fact]
//        public async Task inserting_one_of_the_existing_values_in_scope_throws()
//        {
//            await Insert();
//            _sut.Awaiting(async d => await d.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid()
//               , new UniqueValue("test", scope: Scope)
//               , new UniqueValue("test-p", OtherAspect, scope: Scope)

//               ), CancellationToken.None)).ShouldThrow<UniqueStoreDuplicateException>();

//            _sut.Awaiting(async d => await d.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid(),
//                new UniqueValue("test", scope: Scope)
//                ), CancellationToken.None)).ShouldThrow<UniqueStoreDuplicateException>();

//            _sut.Awaiting(async d => await d.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid(),
//                new UniqueValue("test-p", OtherAspect, scope: Scope)
//                ), CancellationToken.None)).ShouldThrow<UniqueStoreDuplicateException>();
//        }

//        [Fact]
//        public async Task inserting_same_value_in_different_scope_doesnt_throw()
//        {
//            await Insert();
//            _sut.Awaiting(async d => await d.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid(),
//               new UniqueValue("test", scope: OtherAspect)
//               ), CancellationToken.None)).ShouldNotThrow<UniqueStoreDuplicateException>();
//        }

//        [Fact]
//        public async Task inserting_same_values_scope_in_different_bucket_doesnt_throw()
//        {
//            await Insert();
//            _sut.Awaiting(async d => await d.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid()
//              , new UniqueValue("test", scope: Scope)
//              , new UniqueValue("test-p", OtherAspect, scope: Scope)

//              )
//            { Bucket = "bla" }, CancellationToken.None)).ShouldNotThrow<UniqueStoreDuplicateException>();
//        }

//        [Fact]
//        public async Task updating_to_an_existing_value_throws()
//        {
//            await Insert();
//            await _sut.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid(), new UniqueValue("bla", scope: Scope)), CancellationToken.None);

//            _sut.Awaiting(async d => await d.UpdateAsync(
//               new UniqueStoreUpdateItem(_entityId, Guid.NewGuid(),
//               new UniqueValueChange("bla")
//               ), CancellationToken.None))
//                .ShouldThrow<UniqueStoreDuplicateException>();

//            _sut.Awaiting(async d => await d.UpdateAsync(
//                new UniqueStoreUpdateItem(_entityId, Guid.NewGuid(),
//                new UniqueValueChange("bla2")
//                ), CancellationToken.None))
//                .ShouldNotThrow<UniqueStoreDuplicateException>();
//        }

//        [Fact]
//        public async Task delete_from_store()
//        {
//            await Insert();
//            await _sut.DeleteAsync(_entityId, CancellationToken.None);
//            _sut.Invoking(async d => await d.AddAsync(new UniqueStoreItem(_entityId, Guid.NewGuid()
//               , new UniqueValue("test", scope: Scope)
//               , new UniqueValue("test-p", OtherAspect, scope: Scope)

//               ), CancellationToken.None)).ShouldNotThrow<UniqueStoreDuplicateException>();
//        }
//    }
//}
