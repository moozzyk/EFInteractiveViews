namespace InteractivePreGeneratedViews
{
    using InteractivePreGeneratedViews.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class MappingViewCacheTests
    {
        [Fact]
        public void Cannot_create_instance_with_invalid_input_Xml()
        {
            Assert.Equal(
                "views",
                Assert.Throws<ArgumentNullException>(
                    () => new MappingViewCache(null)).ParamName);

            Assert.Equal(
                "The hash of the mapping cannot be null or empty string.",
                Assert.Throws<InvalidOperationException>(
                    () => new MappingViewCache(XElement.Parse("<views />"))).Message);

            Assert.Equal(
                "The hash of the mapping cannot be null or empty string.",
                Assert.Throws<InvalidOperationException>(
                    () => new MappingViewCache(XElement.Parse("<views hash=\"\"/>"))).Message);
        }

        [Fact]
        public void Cannot_create_instance_with_invalid_input_parameters()
        {
            Assert.Equal(
                "hash",
                Assert.Throws<ArgumentNullException>(
                    () => new MappingViewCache(null, null)).ParamName);

            Assert.Equal(
                "hash",
                Assert.Throws<ArgumentNullException>(
                    () => new MappingViewCache(string.Empty, null)).ParamName);

            Assert.Equal(
                "hash",
                Assert.Throws<ArgumentNullException>(
                    () => new MappingViewCache(" ", null)).ParamName);

            Assert.Equal(
                "views",
                Assert.Throws<ArgumentNullException>(
                    () => new MappingViewCache("5", null)).ParamName);
        }

        [Fact]
        public void Hash_correctly_parsed_from_Xml()
        {
            var views = XElement.Parse(
                "<views hash=\"12\"><view extent=\"a\">view definition</view></views>");

            Assert.Equal(
                "12",
                new MappingViewCache(views)
                    .MappingHashValue);
        }

        [Fact]
        public void Hash_stored_correctly()
        {
            var views = new Dictionary<EntitySetBase, DbMappingView>
            {
                { TestUtils.CreateEntitySet("C", "ES"), new DbMappingView("eSql") }
            };

            Assert.Equal(
                "12",
                new MappingViewCache("12", views).MappingHashValue);
        }

        [Fact]
        public void View_definitions_correctly_parsed_from_Xml()
        {
            var views = XElement.Parse(
                "<views hash=\"12\"><view extent=\"A.B\">view definition</view></views>");

            Assert.Equal(
                "view definition",
                new MappingViewCache(views).GetView(TestUtils.CreateEntitySet("A", "B"))
                    .EntitySql);
        }

        [Fact]
        public void View_definitions_stored_correctly()
        {
            var views = new Dictionary<EntitySetBase, DbMappingView>
            {
                { TestUtils.CreateEntitySet("C", "ES"), new DbMappingView("eSql") }
            };

            Assert.Same(
                views.Single().Value,
                new MappingViewCache("12", views).GetView(views.Single().Key));
        }

        [Fact]
        public void GetView_returns_null_for_not_registered_entity_set()
        {
            var views = new Dictionary<EntitySetBase, DbMappingView>
            {
                { TestUtils.CreateEntitySet("C", "ES"), new DbMappingView("eSql") }
            };

            Assert.Null(
                new MappingViewCache("12", views)
                    .GetView(TestUtils.CreateEntitySet("C", "ES1")));
        }
    }
}
