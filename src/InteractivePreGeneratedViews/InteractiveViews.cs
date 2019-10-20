namespace InteractivePreGeneratedViews
{
    using System.Data.Entity;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Collections.Concurrent;
    using System;
    using System.Data.Entity.Core.Objects;

    /// <summary>
    /// Entry point for Interactive pre-generated views. Allows registering view cache factory 
    /// for mapping which will be used to provide EF with view definitions. Factory must be 
    /// registered before EF tries to generate views (typically before sending the first query
    /// or .SaveChanges() request).
    /// </summary>
    public static class InteractiveViews
    {
        private static ConcurrentDictionary<DbMappingViewCacheFactory, StorageMappingItemCollection> _mappingItemCollectionLookUp = 
            new ConcurrentDictionary<DbMappingViewCacheFactory, StorageMappingItemCollection>();

        /// <summary>
        /// Sets a <paramref name="viewCacheFactory"/> for a mapping represented 
        /// by a <see cref="DbContext"/> derived class.
        /// </summary>
        /// <param name="context">A <see cref="DbContext"/> derived class instance containing 
        /// mapping to set the view cache factory for.</param>
        /// <param name="viewCacheFactory">View cache factory</param>
        /// <remarks>
        /// This method must be called before EntityFramework generates views for the mapping 
        /// (which typically happens on the first query).
        /// <paramref name="viewCacheFactory"/> cannot be set more than once for the same 
        /// <paramref name="context"/>.
        /// /// </remarks>
        public static void SetViewCacheFactory(DbContext context, DbMappingViewCacheFactory viewCacheFactory)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (viewCacheFactory == null)
            {
                throw new ArgumentNullException("viewCacheFactory");
            }

            SetViewCacheFactory(((IObjectContextAdapter)context).ObjectContext, viewCacheFactory);
        }

        /// <summary>
        /// Sets a <paramref name="viewCacheFactory"/> for a mapping represented 
        /// by an <see cref="ObjectContext"/> derived class.
        /// </summary>
        /// <param name="context">A <see cref="ObjectContext"/> derived class instance containing
        /// mapping to set the view cache factory for.</param>
        /// <param name="viewCacheFactory">View cache factory</param>
        /// <remarks>
        /// This method must be called before EntityFramework generates views for the mapping 
        /// (which typically happens on the first query).
        /// <paramref name="viewCacheFactory"/> cannot be set more than once for the same 
        /// <paramref name="context"/>.
        /// </remarks>
        public static void SetViewCacheFactory(ObjectContext context, DbMappingViewCacheFactory viewCacheFactory)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (viewCacheFactory == null)
            {
                throw new ArgumentNullException("viewCacheFactory");
            }

            var storageMappingItemCollection = 
                (StorageMappingItemCollection)context.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
            storageMappingItemCollection.MappingViewCacheFactory = viewCacheFactory;
            _mappingItemCollectionLookUp.TryAdd(viewCacheFactory, storageMappingItemCollection);
        }

        /// <summary>
        /// Returns a <see cref="StorageMappingItemCollection"/> instance for the <paramref name="viewCacheFactory" />.
        /// </summary>
        /// <param name="viewCacheFactory">View cache factory to return <see cref="StorageMappingItemCollection"/> for.</param>
        /// <returns>A <see cref="StorageMappingItemCollection"/> instance for the <paramref name="viewCacheFactory" />.</returns>
        public static StorageMappingItemCollection GetMappingItemCollection(DbMappingViewCacheFactory viewCacheFactory)
        { 
            if (viewCacheFactory == null)
            {
                throw new ArgumentNullException("viewCacheFactory");
            }

            StorageMappingItemCollection mappingItemCollection;

            if(!_mappingItemCollectionLookUp.TryGetValue(viewCacheFactory, out mappingItemCollection))
            {
                throw new InvalidOperationException("No StorageMappingItemCollection instance found for the provided DbMappingViewCacheFactory.");
            }

            return mappingItemCollection;
        }
    }
}
