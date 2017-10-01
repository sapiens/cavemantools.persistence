 
using FluentAssertions;
using Xunit;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CavemanTools.Logging;
using CavemanTools.Model.Persistence.UniqueStore;
using CavemanTools.Persistence.Sql.UniqueStore;
using SqlFu;

namespace Tests
{
    public abstract class AUniqueStoreTests : IDisposable
    {
        protected IDbFactory _db;

        public AUniqueStoreTests()
        {
            _entityId = Guid.NewGuid();

            LogManager.OutputTo(w => Trace.WriteLine(w));
            _db = GetFactory();
            _sut=new UniqueStore(_db);
            Init();
        }

        protected virtual void Init()
        {

        }

        protected abstract IDbFactory GetFactory();

        private const string Scope = "user";
        private const string OtherAspect = "shortname";
        private IStoreUniqueValuesAsync _sut;
        private Guid _entityId;

        private Task Insert()
        {
            var item = new UniqueStoreItem(_entityId, Guid.NewGuid()
                , new UniqueValue("test", scope: Scope)
                , new UniqueValue("test-p", OtherAspect, scope: Scope)
            );
            return _sut.AddAsync(item, CancellationToken.None);
        }

       [Fact]
        public async Task inserting_one_of_the_existing_values_in_scope_throws()
        {
            await Insert();

            Func<Task> act = async () =>
            {
                await _sut.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid()
                    , new UniqueValue("test", scope: Scope)
                    , new UniqueValue("test-p", OtherAspect, scope: Scope)

                ), CancellationToken.None);
            };
                
            act.ShouldThrow<UniqueStoreDuplicateException>();

            act = async () =>
            {
                await _sut.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid(),
                    new UniqueValue("test", scope: Scope)
                ), CancellationToken.None);
            };

            act.ShouldThrow<UniqueStoreDuplicateException>();
            act = async () =>
            {
                await _sut.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid(),
                    new UniqueValue("test-p", OtherAspect, scope: Scope)
                ), CancellationToken.None);
            };

           act.ShouldThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public async Task inserting_same_value_in_different_scope_doesnt_throw()
        {
            await Insert();
            Func<Task> act = async () =>
            {
                await _sut.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid(),
                    new UniqueValue("test", scope: OtherAspect)
                ), CancellationToken.None);
            };
            act.ShouldNotThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public async Task inserting_same_values_scope_in_different_bucket_doesnt_throw()
        {
            await Insert();
            Func<Task> act = async () =>
            {
                await _sut.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid()
                        , new UniqueValue("test", scope: Scope)
                        , new UniqueValue("test-p", OtherAspect, scope: Scope)

                    )
                    {Bucket = "bla"}, CancellationToken.None);
            };
            act.ShouldNotThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public async Task updating_to_an_existing_value_throws()
        {
            await Insert();
            await _sut.AddAsync(new UniqueStoreItem(Guid.NewGuid(), Guid.NewGuid(), new UniqueValue("bla", scope: Scope)), CancellationToken.None);
            Func<Task> act = async () =>
            {
                await _sut.UpdateAsync(
                    new UniqueStoreUpdateItem(_entityId, Guid.NewGuid(),
                        new UniqueValueChange("bla")
                    ), CancellationToken.None);
            };


            act.ShouldThrow<UniqueStoreDuplicateException>();

            act = async () =>
            {
                await _sut.UpdateAsync(
                    new UniqueStoreUpdateItem(_entityId, Guid.NewGuid(),
                        new UniqueValueChange("bla2")
                    ), CancellationToken.None);
            };

            act.ShouldNotThrow<UniqueStoreDuplicateException>();
        }

        [Fact]
        public async Task delete_from_store()
        {
            await Insert();
            await _sut.DeleteAsync(_entityId, CancellationToken.None);

            Func<Task> act = async () =>
            {
                await _sut.AddAsync(new UniqueStoreItem(_entityId, Guid.NewGuid()
                    , new UniqueValue("test", scope: Scope)
                    , new UniqueValue("test-p", OtherAspect, scope: Scope)

                ), CancellationToken.None);
            };

            act.ShouldNotThrow<UniqueStoreDuplicateException>();
        }

        protected virtual void DisposeOther()
        {

        }

        public void Dispose()
        {
            DisposeOther();
         


        }

    }
} 
