namespace InteractivePreGeneratedViews
{
    using InteractivePreGeneratedViews.Helpers;
    using Moq;
    using System;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Xunit;

    public class DatabaseViewCacheContextTests
    {
        private static readonly XNamespace CsdlNs = "http://schemas.microsoft.com/ado/2009/11/edm";
        private static readonly XNamespace SsdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";

        // used to avoid EF memoization
        private class FakeDatabaseViewCacheContext : DatabaseViewCacheContext
        {
            public FakeDatabaseViewCacheContext(DbConnection connection, string tableName = null, string schemaName = null)
                : base(connection, tableName, schemaName)
            { }
        }

        [Fact]
        public void Cannot_create_with_null_connection()
        {
            Assert.Equal(
                "existingConnection",
                Assert.Throws<ArgumentNullException>(
                    () => new DatabaseViewCacheContext(null)).ParamName);
        }

        [Fact]
        public void Table_schema_name_set_to_default_values_if_names_not_provided()
        {
            using (var ctx = new DatabaseViewCacheContext(new Mock<DbConnection>().Object))
            {
                Assert.Equal("__ViewCache", ctx.TableName);
                Assert.Equal("dbo", ctx.SchemaName);
            }
        }

        [Fact]
        public void Can_set_table_schema_name()
        {
            using (var ctx = new DatabaseViewCacheContext(
                new Mock<DbConnection>().Object, "table", "schema"))
            {
                Assert.Equal("table", ctx.TableName);
                Assert.Equal("schema", ctx.SchemaName);
            }
        }

        [Fact]
        public void DatabaseViewCacheContextTests_allow_seting_table_and_schema_name()
        {
            MetadataWorkspace.ClearCache();

            // TODO: mock connection?
            using (var ctx = new FakeDatabaseViewCacheContext(
                new SqlConnection(TestUtils.TempDbConnectionString), "MyViews", "schema"))
            {
                VerifyModel(DumpEdmx(ctx), "MyViews", "schema");
            }
        }

        private XDocument DumpEdmx(DatabaseViewCacheContext context)
        {
            var stringBuilder = new StringBuilder();
            using(var writer = XmlWriter.Create(stringBuilder))
            {
                EdmxWriter.WriteEdmx(context, writer);
            }

            return XDocument.Parse(stringBuilder.ToString());
        }

        private void VerifyModel(XDocument modelEdmx, string tableName, string schemaName)
        {
            var expectedCsdl =
              "<Schema Namespace=\"InteractivePreGeneratedViews\" Alias=\"Self\" annotation:UseStrongSpatialTypes=\"false\" xmlns:annotation=\"http://schemas.microsoft.com/ado/2009/02/edm/annotation\" xmlns:customannotation=\"http://schemas.microsoft.com/ado/2013/11/edm/customannotation\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\">" +
              "  <EntityType Name=\"DatabaseViewCacheEntry\" customannotation:ClrType=\"InteractivePreGeneratedViews.DatabaseViewCacheEntry, InteractivePreGeneratedViews, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null\">" +
              "    <Key>" +
              "      <PropertyRef Name=\"ConceptualModelContainerName\" />" +
              "      <PropertyRef Name=\"StoreModelContainerName\" />" +
              "    </Key>" +
              "    <Property Name=\"ConceptualModelContainerName\" Type=\"String\" MaxLength=\"255\" FixedLength=\"false\" Unicode=\"true\" Nullable=\"false\" />" +
              "    <Property Name=\"StoreModelContainerName\" Type=\"String\" MaxLength=\"255\" FixedLength=\"false\" Unicode=\"true\" Nullable=\"false\" />" +
              "    <Property Name=\"ViewDefinitions\" Type=\"String\" MaxLength=\"Max\" FixedLength=\"false\" Unicode=\"true\" />" +
              "    <Property Name=\"LastUpdated\" Type=\"DateTimeOffset\" Nullable=\"false\" />" +
              "  </EntityType>" +
              "  <EntityContainer Name=\"FakeDatabaseViewCacheContext\" customannotation:UseClrTypes=\"true\">" +
              "    <EntitySet Name=\"ViewCache\" EntityType=\"Self.DatabaseViewCacheEntry\" />" +
              "  </EntityContainer>" +
              "</Schema>";

            var expectedSsdl =
              "<Schema Namespace=\"CodeFirstDatabaseSchema\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\" Alias=\"Self\" xmlns:customannotation=\"http://schemas.microsoft.com/ado/2013/11/edm/customannotation\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\">" +
              "  <EntityType Name=\"DatabaseViewCacheEntry\" customannotation:Index=\"{{ }}\">" +
              "    <Key>" +
              "      <PropertyRef Name=\"ConceptualModelContainerName\" />" +
              "      <PropertyRef Name=\"StoreModelContainerName\" />" +
              "    </Key>" +
              "    <Property Name=\"ConceptualModelContainerName\" Type=\"nvarchar\" MaxLength=\"255\" Nullable=\"false\" />" +
              "    <Property Name=\"StoreModelContainerName\" Type=\"nvarchar\" MaxLength=\"255\" Nullable=\"false\" />" +
              "    <Property Name=\"ViewDefinitions\" Type=\"nvarchar(max)\" Nullable=\"true\" />" +
              "    <Property Name=\"LastUpdated\" Type=\"datetimeoffset\" Precision=\"7\" Nullable=\"false\" />" +
              "  </EntityType>" +
              "  <EntityContainer Name=\"CodeFirstDatabase\">" +
              "    <EntitySet Name=\"DatabaseViewCacheEntry\" EntityType=\"Self.DatabaseViewCacheEntry\" Schema=\"{1}\" Table=\"{0}\" />" +
              "  </EntityContainer>" +
              "</Schema>";

            Assert.True(
                XNode.DeepEquals(
                    XElement.Parse(expectedCsdl),
                    modelEdmx.Descendants(CsdlNs + "Schema").Single()));

            Assert.True(
                XNode.DeepEquals(
                    XElement.Parse(string.Format(expectedSsdl, tableName, schemaName)),
                    modelEdmx.Descendants(SsdlNs + "Schema").Single()));
        }
    }
}
