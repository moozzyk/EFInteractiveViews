namespace InteractivePreGeneratedViews
{
    using Moq;
    using System;
    using System.Xml.Linq;
    using Xunit;

    public class FileViewCacheFactoryTests
    {
        [Fact]
        public void Cannot_create_FileViewCacheFactoryTests_with_null_or_empty_file_name()
        {
            Assert.Equal(
                "viewFilePath",
                Assert.Throws<ArgumentNullException>(
                    () => new FileViewCacheFactory(null)).ParamName);

            Assert.Equal(
                "viewFilePath",
                Assert.Throws<ArgumentNullException>(
                    () => new FileViewCacheFactory(string.Empty)).ParamName);

            Assert.Equal(
                "viewFilePath",
                Assert.Throws<ArgumentNullException>(
                    () => new FileViewCacheFactory("  ")).ParamName);
        }

        [Fact]
        public void Load_throws_for_null_conceptual_and_store_container_names()
        {
            var viewCacheFactoryInvoker =
                new Mock<FileViewCacheFactoryProtectedInvoker>()
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
        public void Save_throws_for_null_views()
        {
            var viewCacheFactoryInvoker =
                new Mock<FileViewCacheFactoryProtectedInvoker>()
                {
                    CallBase = true
                }.Object;

            Assert.Equal("viewsXml",
                Assert.Throws<ArgumentNullException>(
                    () => viewCacheFactoryInvoker.InvokeSave(null)).ParamName);
        }
    }

    internal abstract class FileViewCacheFactoryProtectedInvoker : FileViewCacheFactory
    {
        public FileViewCacheFactoryProtectedInvoker()
            : base("views.xml")
        { }

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
