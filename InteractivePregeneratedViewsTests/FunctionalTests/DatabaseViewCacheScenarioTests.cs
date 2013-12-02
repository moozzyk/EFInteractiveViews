namespace InteractivePreGeneratedViews.FunctionalTests
{
    using InteractivePreGeneratedViews.Helpers;
    using System.Data.Entity;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class DatabaseViewCacheScenarioTests
    {
        private class DatabaseViewCacheModel : SimpleModel
        {
            static DatabaseViewCacheModel()
            {
                Database.SetInitializer(
                    new DropCreateDatabaseAlways<DatabaseViewCacheModel>());
            }

            public DatabaseViewCacheModel()
                : base("Data Source=(LocalDb)\\v11.0;Initial Catalog=DatabaseViewCacheModel;Integrated Security=SSPI")
            { }
        }

        [Fact]
        public void FileViewCache_creates_views_if_dont_exist()
        {
            using (var ctx = new DatabaseViewCacheModel())
            {
                var connectionString = ctx.Database.Connection.ConnectionString;

                InteractiveViews
                    .SetViewCacheFactory(
                        ctx, 
                        new SqlServerViewCacheFactory(connectionString));
                
                ctx.Entities.Count();

                XDocument viewsXml;
                using (var viewCacheContext = 
                    new DatabaseViewCacheContext(new SqlConnection(connectionString)))
                {
                    viewsXml = XDocument.Parse(viewCacheContext.ViewCache.Single().ViewDefinitions);
                }

                Assert.Equal(
                    ((StorageMappingItemCollection)
                    ((IObjectContextAdapter)ctx).ObjectContext
                    .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace)).ComputeMappingHashValue(),
                    (string)viewsXml.Descendants("mapping-views").Single().Attribute("hash"));
            }
        }
    }
}
