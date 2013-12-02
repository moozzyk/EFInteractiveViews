namespace InteractivePreGeneratedViews.Helpers
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.IO;
    using System.Xml;

    internal class TestUtils
    {
        public static readonly string TempDbConnectionString = "Data Source=(LocalDb)\\v11.0;Initial Catalog=tempdb;Integrated Security=SSPI";

        private const string Csdl = 
            "<Schema Namespace=\"Empty\" Alias=\"Self\" annotation:UseStrongSpatialTypes=\"false\" xmlns:annotation=\"http://schemas.microsoft.com/ado/2009/02/edm/annotation\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm\">" +
            "    <EntityContainer Name=\"MyContext\" />" +
            "</Schema>";

        private const string Ssdl = 
            "<Schema Namespace=\"DatabaseSchema\" Provider=\"System.Data.SqlClient\" ProviderManifestToken=\"2012\" Alias=\"Self\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/edm/ssdl\">" +
            "    <EntityContainer Name=\"DatabaseContainer\" />" + 
            "</Schema>";

        private const string Msl =
            "<Mapping Space=\"C-S\" xmlns=\"http://schemas.microsoft.com/ado/2009/11/mapping/cs\">"+
            "   <EntityContainerMapping StorageEntityContainer=\"DatabaseContainer\" CdmEntityContainer=\"MyContext\" />" +
            "</Mapping>";

        public static EntitySetBase CreateEntitySet(string containerName, string entitySetName, string entityTypeName = "E", string modelNamespace = "Model")
        {
            var entity = EntityType.Create(entityTypeName, modelNamespace, DataSpace.CSpace, new string[0], new EdmMember[0], null);
            var entitySet = EntitySet.Create(entitySetName, modelNamespace, null, null, entity, null);
            EntityContainer.Create(containerName, DataSpace.CSpace, new[] { entitySet }, null, null);

            return entitySet;
        }

        public static MetadataWorkspace CreateFakeMetadataWorkspace()
        {
            EdmItemCollection edmItemCollection;
            StoreItemCollection storeItemCollection;
            StorageMappingItemCollection mappingItemCollection;

            using(var reader = XmlReader.Create(new StringReader(Csdl)))
            {
                edmItemCollection = new EdmItemCollection(new [] {reader});
            }

            using(var reader = XmlReader.Create(new StringReader(Ssdl)))
            {
                storeItemCollection = new StoreItemCollection(new[] {reader});
            }
            
            using(var reader = XmlReader.Create(new StringReader(Msl)))
            {
                mappingItemCollection = 
                    new StorageMappingItemCollection(
                        edmItemCollection, storeItemCollection, new [] {reader});
            }

            return new MetadataWorkspace(
                () => edmItemCollection, 
                () => storeItemCollection, 
                () => mappingItemCollection);
        }
    }
}
