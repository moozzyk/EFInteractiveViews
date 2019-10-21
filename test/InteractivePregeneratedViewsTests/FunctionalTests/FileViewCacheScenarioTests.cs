namespace InteractivePreGeneratedViews.FunctionalTests
{
    using InteractivePreGeneratedViews.Helpers;
    using System;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class FileViewCacheScenarioTests
    {
        private const string ViewFileName = "views.xml";

        // Nesting uniquifies context classes which is required
        // because each context type can be initialized only once.
        public class ViewsDontExist : IDisposable
        {
            private class FileViewCacheModel : SimpleModel { }

            [Fact]
            public void FileViewCache_creates_views_if_dont_exist()
            {
                if (File.Exists(ViewFileName))
                {
                    File.Delete(ViewFileName);
                }

                using (var ctx = new FileViewCacheModel())
                {
                    InteractiveViews.SetViewCacheFactory(
                        ctx, new FileViewCacheFactory(ViewFileName));

                    ctx.Entities.Count();

                    Assert.True(File.Exists(ViewFileName));

                    var viewsXml = XDocument.Load(ViewFileName);

                    Assert.Equal(
                        ((StorageMappingItemCollection)
                        ((IObjectContextAdapter)ctx).ObjectContext
                        .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace)).ComputeMappingHashValue(),
                        (string)viewsXml.Descendants("mapping-views").Single().Attribute("hash"));
                }
            }

            public void Dispose()
            {
                if (File.Exists(ViewFileName))
                {
                    File.Delete(ViewFileName);
                }
            }
        }

        // Nesting uniquifies context classes which is required
        // because each context type can be initialized only once.
        public class ViewsDoExist : IDisposable
        {
            private class FileViewCacheModel : SimpleModel { }

            [Fact]
            public void FileViewCache_updates_file_with_views_if_needed()
            {
                File.WriteAllText(ViewFileName, "<views />");

                using (var ctx = new FileViewCacheModel())
                {
                    InteractiveViews.SetViewCacheFactory(
                        ctx, new FileViewCacheFactory(ViewFileName));

                    ctx.Entities.Count();

                    Assert.True(File.Exists(ViewFileName));

                    var viewsXml = XDocument.Load(ViewFileName);

                    Assert.Equal(
                        ((StorageMappingItemCollection)
                        ((IObjectContextAdapter)ctx).ObjectContext
                        .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace)).ComputeMappingHashValue(),
                        (string)viewsXml.Descendants("mapping-views").Single().Attribute("hash"));
                }
            }

            public void Dispose()
            {
                if (File.Exists(ViewFileName))
                {
                    File.Delete(ViewFileName);
                }
            }
        }
    }
}
