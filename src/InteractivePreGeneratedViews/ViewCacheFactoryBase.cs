namespace InteractivePreGeneratedViews
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Linq;

    public abstract class ViewCacheFactoryBase : DbMappingViewCacheFactory
    {
        protected abstract XDocument Load(string conceptualModelContainerName, string storeModelContainerName);

        protected abstract void Save(XDocument viewsXml);

        /// <summary>
        /// Creates a new instance of the <see cref="DbMappingViewCache"/> class for 
        /// the given <paramref name="conceptualModelContainerName"/> and <paramref name="storeModelContainerName"/>.
        /// </summary>
        /// <param name="conceptualModelContainerName">The name of the conceptual model container.</param>
        /// <param name="storeModelContainerName">The name of the store model container.</param>
        /// <returns>A new instance of the <see cref="DbMappingViewCache"/> class.</returns>
        public override DbMappingViewCache Create(string conceptualModelContainerName, string storeModelContainerName)
        {
            var mappingItemCollection = GetMappingItemCollection();

            if (mappingItemCollection == null)
            {
                throw new InvalidOperationException("View cache not set for this mapping item collection");
            }

            var viewsXml = Load(conceptualModelContainerName, storeModelContainerName);

            var hash = mappingItemCollection.ComputeMappingHashValue(conceptualModelContainerName, storeModelContainerName);

            if (viewsXml != null)
            {
                var viewsForMapping = GetViewsForMapping(viewsXml, conceptualModelContainerName, storeModelContainerName);

                if (viewsForMapping != null && (string)viewsForMapping.Attribute("hash") == hash)
                {
                    return new MappingViewCache(viewsForMapping);
                }
            }

            var views = GenerateViews(mappingItemCollection, conceptualModelContainerName, storeModelContainerName);

            viewsXml = CreateOrUpdateViewsXml(viewsXml, hash, conceptualModelContainerName, storeModelContainerName, views);

            Save(viewsXml);

            return new MappingViewCache(hash, views);
        }

        // virtual for mocking
        internal virtual StorageMappingItemCollection GetMappingItemCollection()
        {
            return InteractiveViews.GetMappingItemCollection(this);
        }

        // virtual for mocking
        internal virtual Dictionary<EntitySetBase, DbMappingView> GenerateViews(StorageMappingItemCollection mappingItemCollection, string conceptualModelContainerName, string storeModelContainerName)
        {
            Debug.Assert(mappingItemCollection != null, "mappingItemCollection != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(conceptualModelContainerName), "cspace container name must not be null or empty");
            Debug.Assert(!string.IsNullOrWhiteSpace(storeModelContainerName), "cspace container name must not be null or empty");

            var errors = new List<EdmSchemaError>();
            return mappingItemCollection.GenerateViews(conceptualModelContainerName, storeModelContainerName, errors);
        }

        private static XDocument CreateOrUpdateViewsXml(XDocument viewsXml, string hash, string conceptualModelContainerName, string storeModelContainerName, Dictionary<EntitySetBase, DbMappingView> views)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(hash), "hash is null or empty");
            Debug.Assert(!string.IsNullOrWhiteSpace(conceptualModelContainerName), "conceptualModelContainerName is null or empty");
            Debug.Assert(!string.IsNullOrWhiteSpace(storeModelContainerName), "storeModelContainerName is null or empty");

            if (viewsXml == null)
            {
                viewsXml = new XDocument(new XElement("views"));
            }

            var viewsForMapping = GetViewsForMapping(viewsXml, conceptualModelContainerName, storeModelContainerName);
            if (viewsForMapping != null)
            {
                viewsForMapping.Remove();
            }

            viewsXml.Root.Add(
                new XElement("mapping-views",
                    new XAttribute("hash", hash),
                    new XAttribute("conceptual-container", conceptualModelContainerName),
                    new XAttribute("store-container", storeModelContainerName),
                    views.Select(
                        viewDefinition =>
                            new XElement(
                                "view",
                                new XAttribute("extent", Utils.GetExtentFullName(viewDefinition.Key)),
                                new XCData(viewDefinition.Value.EntitySql)))));

            return viewsXml;
        }

        private static XElement GetViewsForMapping(XDocument viewsXml, string conceptualModelContainerName, string storeModelContainerName)
        {
            Debug.Assert(viewsXml != null, "viewsXml is null");
            Debug.Assert(!string.IsNullOrWhiteSpace(conceptualModelContainerName), "conceptualModelContainerName is null or empty");
            Debug.Assert(!string.IsNullOrWhiteSpace(storeModelContainerName), "storeModelContainerName is null or empty");

            return
                viewsXml.Root
                    .Elements("mapping-views")
                    .SingleOrDefault(e => (string)e.Attribute("conceptual-container") == conceptualModelContainerName
                        && (string)e.Attribute("store-container") == storeModelContainerName);
        }
    }
}
