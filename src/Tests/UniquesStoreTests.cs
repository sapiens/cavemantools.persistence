 
using System;
using FluentAssertions;
using Xunit;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using CavemanTools.Model.Persistence.UniqueStore;
using CavemanTools.Persistence;
using CavemanTools.Persistence.Sql;
using CavemanTools.Persistence.Sql.UniqueStore;
using SqlFu;
using SqlFu.Builders;

namespace Tests
{
    public class UniquesStoreTests 
    {
        private const string Scope = "user";
        private const string OtherAspect = "shortname";
        private IStoreUniqueValuesAsync _sut;
        private Guid _entityId;


        public UniquesStoreTests()
        {
            IdempotencyTools.InitStorage(Setup.GetFactory());
            UniqueStore.InitStorage(Setup.GetFactory(),ifExists:TableExistsAction.DropIt);
            _sut = UniqueStore.GetInstance(Setup.GetFactory());

            _entityId = Guid.NewGuid();
            var item = new UniqueStoreItem(_entityId,Guid.NewGuid()
                ,new UniqueValue("test",scope:Scope)
                ,new UniqueValue("test-p",OtherAspect,scope:Scope)

                );
            _sut.AddAsync(item,CancellationToken.None).Wait();
        }

        [Fact]
        public void inserting_one_of_the_existing_values_in_scope_throws()
        {

            _sut.Awaiting(d=>d.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid()
               , new UniqueValue("test", scope: Scope)
               , new UniqueValue("test-p", OtherAspect, scope: Scope)

               ),CancellationToken.None)).ShouldThrow<UniqueStoreDuplicateException>();

            _sut.Awaiting(async d=>await d.AddAsync(new UniqueStoreItem(Guid.NewGuid(),Guid.NewGuid(),
                new UniqueValue("test",scope:Scope)
                ),CancellationToken.None)).ShouldThrow<UniqueStoreDuplicateException>();

            _sut.Awaiting(async d=>await d.AddAsync(new UniqueStoreItem(Guid.NewGuid(),Guid.NewGuid(),
                new UniqueValue("test-p",OtherAspect,scope:Scope)
                ),CancellationToken.None)).ShouldThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public void inserting_same_value_in_different_scope_doesnt_throw()
        {
            _sut.Invoking(async d => await d.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid(),
               new UniqueValue("test", scope: OtherAspect)
               ),CancellationToken.None)).ShouldNotThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public void inserting_same_values_scope_in_different_bucket_doesnt_throw()
        {
            _sut.Invoking(async d=>await d.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid()
              , new UniqueValue("test", scope: Scope)
              , new UniqueValue("test-p", OtherAspect, scope: Scope)

              ) {Bucket = "bla"},CancellationToken.None)).ShouldNotThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public async Task updating_to_an_existing_value_throws()
        {
            await _sut.AddAsync(new UniqueStoreItem(Guid.NewGuid(),Guid.NewGuid(),new UniqueValue("bla",scope:Scope)),CancellationToken.None);

            _sut.Invoking (d =>  d.UpdateAsync(
                new UniqueStoreUpdateItem(_entityId,Guid.NewGuid(),
                new UniqueValueChange("bla")
                ),CancellationToken.None).Wait())
                .ShouldThrow<UniqueStoreDuplicateException>();

            _sut.Invoking(d => d.UpdateAsync(
                new UniqueStoreUpdateItem(_entityId,Guid.NewGuid(),
                new UniqueValueChange("bla2")
                ),CancellationToken.None).Wait())
                .ShouldNotThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public async Task delete_from_store()
        {
            
            await _sut.DeleteAsync(_entityId,CancellationToken.None);
            _sut.Invoking(async d=>await d.AddAsync(new UniqueStoreItem(_entityId, Guid.NewGuid()
               , new UniqueValue("test", scope: Scope)
               , new UniqueValue("test-p", OtherAspect, scope: Scope)

               ),CancellationToken.None)).ShouldNotThrow<UniqueStoreDuplicateException>();
        }
    }
} 
