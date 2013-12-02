namespace InteractivePreGeneratedViews
{
    using InteractivePreGeneratedViews.Helpers;
    using Moq;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Data.SqlClient;
    using System.Xml;
    using Xunit;

    public class InteractiveViewsTests
    {
        [Fact]
        public void Cannot_pass_nulls_to_SetViewCacheFactory_DbContext()
        {
            Assert.Equal("context",
                Assert.Throws<ArgumentNullException>(
                () => InteractiveViews.SetViewCacheFactory((DbContext)null, new FileViewCacheFactory("temp")))
                    .ParamName);

            Assert.Equal("viewCacheFactory",
                Assert.Throws<ArgumentNullException>(
                    () => InteractiveViews.SetViewCacheFactory(new DbContext("abc"), null))
                        .ParamName);
        }

        [Fact]
        public void SetViewCacheFactory_registers_factory_for_DbContext()
        {
            using (var ctx = new DbContext(TestUtils.TempDbConnectionString))
            {
                var viewCacheFactory = new Mock<DbMappingViewCacheFactory>().Object;
                InteractiveViews.SetViewCacheFactory(ctx, viewCacheFactory);

                var mappingItemCollection = 
                    (StorageMappingItemCollection)
                    (((IObjectContextAdapter)ctx).ObjectContext)
                    .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);

                Assert.Same(mappingItemCollection, InteractiveViews.GetMappingItemCollection(viewCacheFactory));
                Assert.Same(viewCacheFactory, mappingItemCollection.MappingViewCacheFactory);
            }
        }

        [Fact]
        public void Cannot_pass_nulls_to_SetViewCacheFactory_ObjectContext()
        {
            Assert.Equal("context",
                Assert.Throws<ArgumentNullException>(
                () => InteractiveViews.SetViewCacheFactory((ObjectContext)null, new FileViewCacheFactory("temp")))
                    .ParamName);

            using (var ctx = new DbContext(TestUtils.TempDbConnectionString))
            {
                var objectCtx = ((IObjectContextAdapter)ctx).ObjectContext;

                Assert.Equal("viewCacheFactory",
                    Assert.Throws<ArgumentNullException>(
                        () => InteractiveViews.SetViewCacheFactory(objectCtx, null))
                            .ParamName);
            }
        }

        [Fact]
        public void SetViewCacheFactory_registers_factory_for_ObjectContext()
        {
            using (var entityConnection = new EntityConnection(TestUtils.CreateFakeMetadataWorkspace(), new SqlConnection(TestUtils.TempDbConnectionString)))
            {
                using(var objectCtx = new ObjectContext(entityConnection))
                {
                    var viewCacheFactory = new Mock<DbMappingViewCacheFactory>().Object;
                    InteractiveViews.SetViewCacheFactory(objectCtx, viewCacheFactory);

                    var mappingItemCollection =
                        (StorageMappingItemCollection)objectCtx
                        .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);

                    Assert.Same(mappingItemCollection, InteractiveViews.GetMappingItemCollection(viewCacheFactory));
                    Assert.Same(viewCacheFactory, mappingItemCollection.MappingViewCacheFactory);
                }
            }
        }

        [Fact]
        public void GetMappingItemCollection_throws_for_null_viewCacheFactory()
        {
            Assert.Equal(
                "viewCacheFactory",
                Assert.Throws<ArgumentNullException>(
                    () => InteractiveViews.GetMappingItemCollection(null)).ParamName);
        }

        [Fact]
        public void GetMappingItemCollection_throws_if_no_mapping_item_collection_for_viewCacheFactory()
        {
            Assert.Equal(
                "No StorageMappingItemCollection instance found for the provided DbMappingViewCacheFactory.",
                Assert.Throws<InvalidOperationException>(
                    () => InteractiveViews.GetMappingItemCollection(new Mock<DbMappingViewCacheFactory>().Object))
                    .Message);
        }
    }
}
