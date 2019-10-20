using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace InteractivePreGeneratedViews
{
    using InteractivePreGeneratedViews.Helpers;
    using Moq;
    using Moq.Protected;
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class DatabaseViewCacheFactoryTests
    {
        [Fact]
        public void Load_throws_for_null_conceptual_and_store_container_names()
        {
            var viewCacheFactoryInvoker =
                new Mock<DatabaseViewCacheFactoryProtectedInvoker>()
                {
                    CallBase = true
                }.Object;

            Assert.Equal("conceptualModelContainerName",
                Assert.Throws<ArgumentNullException>(
                    () => viewCacheFactoryInvoker.InvokeLoad(null, "storeContainer")).ParamName);

            Assert.Equal("conceptualModelContainerName",
                Assert.Throws<ArgumentNullException>(
                    () => viewCacheFactoryInvoker.InvokeLoad(" ", "storeContainer")).ParamName);

            Assert.Equal("storeModelContainerName",
                Assert.Throws<ArgumentNullException>(
                    () => viewCacheFactoryInvoker.InvokeLoad("model", null)).ParamName);

            Assert.Equal("storeModelContainerName",
                Assert.Throws<ArgumentNullException>(
                    () => viewCacheFactoryInvoker.InvokeLoad("model", " ")).ParamName);
        }

        [Fact]
        public void Load_does_not_try_to_load_views_if_table_does_not_exist()
        {
            var mockViewCacheFactory =
                new Mock<DatabaseViewCacheFactoryProtectedInvoker>()
                {
                    CallBase = true
                };

            mockViewCacheFactory
                .Protected()
                .Setup<bool>("ViewTableExists", ItExpr.IsAny<DatabaseViewCacheContext>())
                .Returns(false);

            var viewCacheContext =
                CreateMockDatabaseViewCacheContext(new DatabaseViewCacheEntry[0]);

            mockViewCacheFactory
                .Setup(f => f.CreateDatabaseViewCacheContext())
                .Returns(viewCacheContext);

            var views = mockViewCacheFactory.Object.InvokeLoad("M", "S");

            Assert.Null(views);

            Mock.Get(viewCacheContext).Verify(c => c.ViewCache, Times.Never);
        }

        [Fact]
        public void Load_returns_null_if_table_exists_but_views_dont()
        {
            var mockViewCacheFactory =
                new Mock<DatabaseViewCacheFactoryProtectedInvoker>()
                {
                    CallBase = true
                };

            mockViewCacheFactory
                .Protected()
                .Setup<bool>("ViewTableExists", ItExpr.IsAny<DatabaseViewCacheContext>())
                .Returns(true);

            mockViewCacheFactory
                .Setup(f => f.CreateDatabaseViewCacheContext())
                .Returns(CreateMockDatabaseViewCacheContext(new DatabaseViewCacheEntry[0]));

            var views = mockViewCacheFactory.Object.InvokeLoad("C", "S");

            Assert.Null(views);
        }

        [Fact]
        public void Load_returns_views_if_table_and_views_exist()
        {
            var mockViewCacheFactory =
                new Mock<DatabaseViewCacheFactoryProtectedInvoker>()
                {
                    CallBase = true
                };

            mockViewCacheFactory
                .Protected()
                .Setup<bool>("ViewTableExists", ItExpr.IsAny<DatabaseViewCacheContext>())
                .Returns(true);

            var viewCacheContext =
                CreateMockDatabaseViewCacheContext(
                new [] 
                { 
                    new DatabaseViewCacheEntry
                    {
                        ConceptualModelContainerName = "Model",
                        StoreModelContainerName = "StoreContainer",
                        ViewDefinitions = "<views />",
                    }
                });

            mockViewCacheFactory
                .Setup(f => f.CreateDatabaseViewCacheContext())
                .Returns(viewCacheContext);

            var views = mockViewCacheFactory.Object
                .InvokeLoad("Model", "StoreContainer");

            Assert.Equal("views", views.Root.Name.LocalName);
        }

        [Fact]
        public void CreateDatabaseViewCacheContext_throws_if_CreateConnection_returns_null()
        {
            var mockViewCache = new Mock<DatabaseViewCacheFactory>(null, null) { CallBase = true };
            mockViewCache
                .Protected()
                .Setup<DbConnection>("CreateConnection")
                .Returns((DbConnection)null);

            Assert.Equal(
                "Connection must not be null.",
                Assert.Throws<InvalidOperationException>(
                    () => mockViewCache.Object.CreateDatabaseViewCacheContext()).Message);

            mockViewCache.Protected().Verify("CreateConnection", Times.Once());
        }

        [Fact]
        public void Save_throws_for_null_views()
        {
            var viewCacheFactoryInvoker =
                new Mock<DatabaseViewCacheFactoryProtectedInvoker>()
                {
                    CallBase = true
                }.Object;

            Assert.Equal("viewsXml",
                Assert.Throws<ArgumentNullException>(
                    () => viewCacheFactoryInvoker.InvokeSave(null)).ParamName);
        }

        [Fact]
        public void Save_creates_table_if_needed_and_save_views()
        {
            var mockViewCacheFactory =
                new Mock<DatabaseViewCacheFactoryProtectedInvoker>()
                {
                    CallBase = true
                };

            mockViewCacheFactory
                .Protected()
                .Setup<bool>("ViewTableExists", ItExpr.IsAny<DatabaseViewCacheContext>())
                .Returns(false);

            var mockFactoryContext = 
                CreateMockDatabaseViewCacheContext(new DatabaseViewCacheEntry[0]);

            var resultViewCacheEntry = new DatabaseViewCacheEntry
            {
                ConceptualModelContainerName = "SimpleModel",
                StoreModelContainerName = "CodeFirstDatabase"
            };

            Mock.Get(mockFactoryContext.ViewCache)
                .Setup(s => s.Add(It.IsAny<DatabaseViewCacheEntry>()))
                .Callback<DatabaseViewCacheEntry>(
                    e =>
                    {
                        Assert.Equal("SimpleModel", e.ConceptualModelContainerName);
                        Assert.Equal("CodeFirstDatabase", e.StoreModelContainerName);
                    })
                .Returns(resultViewCacheEntry);

            mockViewCacheFactory
                .Setup(f => f.CreateDatabaseViewCacheContext())
                .Returns(mockFactoryContext);

            mockViewCacheFactory.Object
                .InvokeSave(XDocument.Parse(SimpleModel.Views));

            mockViewCacheFactory
                .Protected()
                .Verify("CreateViewTable", Times.Once(), mockFactoryContext);

            Assert.True(
                XNode.DeepEquals(
                    XDocument.Parse(SimpleModel.Views), 
                    XDocument.Parse(resultViewCacheEntry.ViewDefinitions)));

            Assert.True(resultViewCacheEntry.LastUpdated > 
                resultViewCacheEntry.LastUpdated.AddSeconds(-10));

            Mock.Get(mockFactoryContext).Verify(c => c.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Save_updates_existing_views_and_save()
        {
            var mockViewCacheFactory =
                new Mock<DatabaseViewCacheFactoryProtectedInvoker>()
                {
                    CallBase = true
                };

            mockViewCacheFactory
                .Protected()
                .Setup<bool>("ViewTableExists", ItExpr.IsAny<DatabaseViewCacheContext>())
                .Returns(true);

            var resultViewCacheEntry = new DatabaseViewCacheEntry
            {
                ConceptualModelContainerName = "SimpleModel",
                StoreModelContainerName = "CodeFirstDatabase",
                ViewDefinitions = "<views />",
                LastUpdated = new DateTimeOffset(0, TimeSpan.Zero)
            };

            var mockFactoryContext =
                CreateMockDatabaseViewCacheContext(new[] { resultViewCacheEntry });

            mockViewCacheFactory
                .Setup(f => f.CreateDatabaseViewCacheContext())
                .Returns(mockFactoryContext);

            mockViewCacheFactory.Object
                .InvokeSave(XDocument.Parse(SimpleModel.Views));

            mockViewCacheFactory
                .Protected()
                .Verify("CreateViewTable", Times.Never(), mockFactoryContext);

            Assert.True(
                XNode.DeepEquals(
                    XDocument.Parse(SimpleModel.Views),
                    XDocument.Parse(resultViewCacheEntry.ViewDefinitions)));

            Assert.True(resultViewCacheEntry.LastUpdated >
                resultViewCacheEntry.LastUpdated.AddSeconds(-10));
        }

        private DatabaseViewCacheContext CreateMockDatabaseViewCacheContext(DatabaseViewCacheEntry[] views)
        {
            var mockViewCacheContext =
                new Mock<DatabaseViewCacheContext>(new Mock<DbConnection>().Object, null, null);

            var viewsQueryable = views.AsQueryable();
            var mockViewCacheDbSet = new Mock<DbSet<DatabaseViewCacheEntry>>();
            mockViewCacheDbSet
                .As<IQueryable<DatabaseViewCacheEntry>>()
                .Setup(m => m.Provider)
                .Returns(viewsQueryable.Provider);

            mockViewCacheDbSet
                .As<IQueryable<DatabaseViewCacheEntry>>()
                .Setup(m => m.Expression)
                .Returns(viewsQueryable.Expression);

            mockViewCacheDbSet
                .As<IQueryable<DatabaseViewCacheEntry>>()
                .Setup(m => m.ElementType)
                .Returns(viewsQueryable.ElementType);

            mockViewCacheDbSet
                .As<IQueryable<DatabaseViewCacheEntry>>()
                .Setup(m => m.GetEnumerator())
                .Returns(viewsQueryable.GetEnumerator());

            mockViewCacheContext
                .Setup(c => c.ViewCache)
                .Returns(mockViewCacheDbSet.Object);

            return mockViewCacheContext.Object;
        }
    }

    internal abstract class DatabaseViewCacheFactoryProtectedInvoker : DatabaseViewCacheFactory
    {
        public XDocument InvokeLoad(string conceptualModelContainerName, string storeModelContainerName)
        {
            return Load(conceptualModelContainerName, storeModelContainerName);
        }

        public void InvokeSave(XDocument viewsXml)
        {
            Save(viewsXml);
        }
    }
}
