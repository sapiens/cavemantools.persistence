 
using System;
using FluentAssertions;
using Xunit;
using System.Data.Common;
using CavemanTools.Model.Persistence.UniqueStore;
using CavemanTools.Persistence;
using CavemanTools.Persistence.UniqueStore;
using SqlFu;
using SqlFu.Builders;

namespace Tests
{
    public class UniquesStoreTests 
    {
        private const string Scope = "user";
        private const string OtherAspect = "shortname";
        private IStoreUniqueValues _sut;
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
            _sut.Add(item);
        }

        [Fact]
        public void inserting_one_of_the_existing_values_in_scope_throws()
        {
            _sut.Invoking(d=>d.Add(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid()
               , new UniqueValue("test", scope: Scope)
               , new UniqueValue("test-p", OtherAspect, scope: Scope)

               ))).ShouldThrow<UniqueStoreDuplicateException>();

            _sut.Invoking(d=>d.Add(new UniqueStoreItem(Guid.NewGuid(),Guid.NewGuid(),
                new UniqueValue("test",scope:Scope)
                ))).ShouldThrow<UniqueStoreDuplicateException>();

            _sut.Invoking(d=>d.Add(new UniqueStoreItem(Guid.NewGuid(),Guid.NewGuid(),
                new UniqueValue("test-p",OtherAspect,scope:Scope)
                ))).ShouldThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public void inserting_same_value_in_different_scope_doesnt_throw()
        {
            _sut.Invoking(d => d.Add(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid(),
               new UniqueValue("test", scope: OtherAspect)
               ))).ShouldNotThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public void inserting_same_values_scope_in_different_bucket_doesnt_throw()
        {
            _sut.Invoking(d=>d.Add(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid()
              , new UniqueValue("test", scope: Scope)
              , new UniqueValue("test-p", OtherAspect, scope: Scope)

              ) {Bucket = "bla"})).ShouldNotThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public void updating_to_an_existing_value_throws()
        {
            _sut.Add(new UniqueStoreItem(Guid.NewGuid(),Guid.NewGuid(),new UniqueValue("bla",scope:Scope)));

            _sut.Invoking(d => d.Update(
                new UniqueStoreUpdateItem(_entityId,Guid.NewGuid(),
                new UniqueValueChange("bla")
                )))
                .ShouldThrow<UniqueStoreDuplicateException>();

            _sut.Invoking(d => d.Update(
                new UniqueStoreUpdateItem(_entityId,Guid.NewGuid(),
                new UniqueValueChange("bla2")
                )))
                .ShouldNotThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public void delete_from_store()
        {
            _sut.Delete(_entityId);
            _sut.Invoking(d=>d.Add(new UniqueStoreItem(_entityId, Guid.NewGuid()
               , new UniqueValue("test", scope: Scope)
               , new UniqueValue("test-p", OtherAspect, scope: Scope)

               ))).ShouldNotThrow<UniqueStoreDuplicateException>();
        }
    }
} 
