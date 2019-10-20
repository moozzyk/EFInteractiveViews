namespace InteractivePreGeneratedViews
{
    using InteractivePreGeneratedViews.Helpers;
    using Moq;
    using Moq.Protected;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class ViewCacheFactoryBaseTests
    {
        [Fact]
        public void Create_throws_if_view_cache_factory_not_registered_for_mapping()
        {
            var mockViewCacheFactoryBase = new Mock<ViewCacheFactoryBase>() { CallBase = true };
            mockViewCacheFactoryBase
                .Setup(f => f.GetMappingItemCollection())
                .Returns<StorageMappingItemCollection>(null);

            Assert.Equal(
                "View cache not set for this mapping item collection",
                Assert.Throws<InvalidOperationException>( 
                    () => mockViewCacheFactoryBase.Object.Create("DbContext", "CodeFirstDatabase"))
                        .Message);
        }

        [Fact]
        public void Create_does_not_generate_views_if_views_can_be_loaded()
        {
            using (var ctx = new SimpleModel())
            {
                var mappingItemCollection = (StorageMappingItemCollection)
                    ((IObjectContextAdapter)ctx).ObjectContext
                    .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);

                var mockViewCacheFactoryBase = new Mock<ViewCacheFactoryBase>() { CallBase = true };
                mockViewCacheFactoryBase
                    .Setup(f => f.GetMappingItemCollection())
                    .Returns(mappingItemCollection);

                mockViewCacheFactoryBase
                    .Protected()
                    .Setup<XDocument>("Load", "SimpleModel", "CodeFirstDatabase")
                    .Returns(XDocument.Parse(SimpleModel.Views));

                var mappingViewCache =
                    mockViewCacheFactoryBase.Object.Create("SimpleModel", "CodeFirstDatabase");

                Assert.NotNull(mappingViewCache);
                Assert.Equal(mappingItemCollection.ComputeMappingHashValue(), mappingViewCache.MappingHashValue);

                mockViewCacheFactoryBase
                    .Verify(f => f.GenerateViews(
                        It.IsAny<StorageMappingItemCollection>(), 
                        It.IsAny<string>(), 
                        It.IsAny<string>()), 
                        Times.Never);

                mockViewCacheFactoryBase
                    .Protected()
                    .Verify("Save", Times.Never(), ItExpr.IsAny<XDocument>());
            }
        }

        [Fact]
        public void Create_generates_views_if_hash_does_not_match()
        {
            using (var ctx = new SimpleModel())
            {
                var mappingItemCollection = (StorageMappingItemCollection)
                    ((IObjectContextAdapter)ctx).ObjectContext
                    .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);

                var mockViewCacheFactoryBase = new Mock<ViewCacheFactoryBase>() { CallBase = true };
                mockViewCacheFactoryBase
                    .Setup(f => f.GetMappingItemCollection())
                    .Returns(mappingItemCollection);

                var views = XDocument.Parse(SimpleModel.Views);
                views.Root.Element("mapping-views").Attribute("hash").Value += "42";

                mockViewCacheFactoryBase
                    .Protected()
                    .Setup<XDocument>("Load", "SimpleModel", "CodeFirstDatabase")
                    .Returns(views);

                var mappingViewCache =
                    mockViewCacheFactoryBase.Object.Create("SimpleModel", "CodeFirstDatabase");

                Assert.NotNull(mappingViewCache);
                Assert.Equal(mappingItemCollection.ComputeMappingHashValue(), mappingViewCache.MappingHashValue);

                mockViewCacheFactoryBase
                    .Verify(f => f.GenerateViews(
                        It.IsAny<StorageMappingItemCollection>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()),
                        Times.Once);

                mockViewCacheFactoryBase
                    .Protected()
                    .Verify("Save", Times.Once(), ItExpr.IsAny<XDocument>());
            }
        }

        [Fact]
        public void Create_generates_views_if_views_for_given_container_not_found()
        {
            using (var ctx = new SimpleModel())
            {
                var mappingItemCollection = (StorageMappingItemCollection)
                    ((IObjectContextAdapter)ctx).ObjectContext
                    .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);

                var mockViewCacheFactoryBase = new Mock<ViewCacheFactoryBase>() { CallBase = true };
                mockViewCacheFactoryBase
                    .Setup(f => f.GetMappingItemCollection())
                    .Returns(mappingItemCollection);

                var views = XDocument.Parse(SimpleModel.Views);
                views.Root.Element("mapping-views").SetAttributeValue("store-container", "DatabaseSecondDatabase");

                mockViewCacheFactoryBase
                    .Protected()
                    .Setup<XDocument>("Load", "SimpleModel", "CodeFirstDatabase")
                    .Returns(views);

                var mappingViewCache =
                    mockViewCacheFactoryBase.Object.Create("SimpleModel", "CodeFirstDatabase");

                Assert.NotNull(mappingViewCache);
                Assert.Equal(mappingItemCollection.ComputeMappingHashValue(), mappingViewCache.MappingHashValue);

                mockViewCacheFactoryBase
                    .Verify(f => f.GenerateViews(
                        It.IsAny<StorageMappingItemCollection>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()),
                        Times.Once);

                mockViewCacheFactoryBase
                    .Protected()
                    .Verify("Save", Times.Once(), ItExpr.IsAny<XDocument>());
            }
        }

        [Fact]
        public void Create_generates_saves_correct_views()
        {
            using (var ctx = new SimpleModel())
            {
                var mappingItemCollection = (StorageMappingItemCollection)
                    ((IObjectContextAdapter)ctx).ObjectContext
                    .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);

                var mockViewCacheFactoryBase = new Mock<ViewCacheFactoryBase>() { CallBase = true };
                mockViewCacheFactoryBase
                    .Setup(f => f.GetMappingItemCollection())
                    .Returns(mappingItemCollection);

                mockViewCacheFactoryBase
                    .Protected()
                    .Setup("Save", ItExpr.IsAny<XDocument>())
                    .Callback<XDocument>(
                        (views) => XNode.DeepEquals(views, XDocument.Parse(SimpleModel.Views)));

                var mappingViewCache =
                    mockViewCacheFactoryBase.Object.Create("SimpleModel", "CodeFirstDatabase");

                Assert.NotNull(mappingViewCache);
                Assert.Equal(mappingItemCollection.ComputeMappingHashValue(), mappingViewCache.MappingHashValue);
            }
        }

        [Fact]
        public void Create_does_not_remove_existing_unrelated_views()
        {
            using (var ctx = new SimpleModel())
            {
                var mappingItemCollection = (StorageMappingItemCollection)
                    ((IObjectContextAdapter)ctx).ObjectContext
                    .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);

                var mockViewCacheFactoryBase = new Mock<ViewCacheFactoryBase>() { CallBase = true };
                mockViewCacheFactoryBase
                    .Setup(f => f.GetMappingItemCollection())
                    .Returns(mappingItemCollection);

                var unrelatedViews =
                    XElement.Parse("<mapping-views hash=\"a\" store-model=\"S\" conceptual-model=\"C\" />");

                var views = XDocument.Parse(SimpleModel.Views);
                views.Root.Element("mapping-views").SetAttributeValue("hash", "brokenHash");
                views.Root.AddFirst(unrelatedViews);

                mockViewCacheFactoryBase
                    .Protected()
                    .Setup<XDocument>("Load", "SimpleModel", "CodeFirstDatabase")
                    .Returns(views);

                mockViewCacheFactoryBase
                .Protected()
                .Setup("Save", ItExpr.IsAny<XDocument>())
                .Callback<XDocument>(
                    (v) => 
                        {
                            Assert.Same(unrelatedViews, v.Root.Elements().First());
                            Assert.Equal(2, v.Root.Elements("mapping-views").Count());
                        });

                var mappingViewCache =
                    mockViewCacheFactoryBase.Object.Create("SimpleModel", "CodeFirstDatabase");

                Assert.NotNull(mappingViewCache);
                Assert.Equal(mappingItemCollection.ComputeMappingHashValue(), mappingViewCache.MappingHashValue);
            }
        }
    }
}
