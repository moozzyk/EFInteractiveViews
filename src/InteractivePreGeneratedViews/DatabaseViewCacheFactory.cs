namespace InteractivePreGeneratedViews
{
    using System;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Base class for creating database based view cache factory.
    /// </summary>
    public abstract class DatabaseViewCacheFactory : ViewCacheFactoryBase 
    {
        private readonly string _tableName;
        private readonly string _schemaName;

        /// <summary>
        /// Creates a new instance of <see cref="DatabaseViewCacheFactory"/> class.
        /// </summary>
        /// <param name="tableName">
        /// Name of the table where views are stored. 
        /// Optional - if not provided '__ViewCache' will be used as the table name.
        /// </param>
        /// <param name="schemaName">
        /// Schema of the table where views are stored. 
        /// Optional - if not provided 'dbo' will be used as the table schema.
        /// </param>
        public DatabaseViewCacheFactory(string tableName = null, string schemaName = null)
        {
            _tableName = tableName;
            _schemaName = schemaName;
        }

        /// <summary>
        /// Loads view definitions for given conceptual and store container mapping.
        /// </summary>
        /// <param name="conceptualModelContainerName">
        /// The name of the conceptual container.
        /// </param>
        /// <param name="storeModelContainerName">
        /// The name of the store container.
        /// </param>
        /// <returns>
        /// Views as <see cref="XDocument" /> or <code>null</code> if views cannot be loaded.
        /// </returns>
        protected override XDocument Load(string conceptualModelContainerName, string storeModelContainerName)
        {
            if (string.IsNullOrWhiteSpace(conceptualModelContainerName))
            {
                throw new ArgumentNullException("conceptualModelContainerName");
            }

            if (string.IsNullOrWhiteSpace(storeModelContainerName))
            {
                throw new ArgumentNullException("storeModelContainerName");
            }

            using (var viewCacheContext = CreateDatabaseViewCacheContext())
            {
                if (ViewTableExists(viewCacheContext))
                {
                    var views = GetViews(viewCacheContext, conceptualModelContainerName, storeModelContainerName);
                    if (views != null)
                    {
                        return XDocument.Parse(views.ViewDefinitions);
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Saves views to the database.
        /// </summary>
        /// <param name="viewsXml">
        /// <see cref="XDocument"/> containing view definitions.
        /// </param>
        protected override void Save(XDocument viewsXml)
        {
            if (viewsXml == null)
            {
                throw new ArgumentNullException("viewsXml");
            }

            var conceptualModelContainerName =
                (string)viewsXml.Root.Element("mapping-views").Attribute("conceptual-container");
            var storeModelContainerName =
                (string)viewsXml.Root.Element("mapping-views").Attribute("store-container");

            using (var viewCacheContext = CreateDatabaseViewCacheContext())
            {
                if (!ViewTableExists(viewCacheContext))
                {
                    CreateViewTable(viewCacheContext);
                }

                var views = GetViews(viewCacheContext, conceptualModelContainerName, storeModelContainerName);
                if (views == null)
                {
                    views = viewCacheContext.ViewCache.Add(
                                new DatabaseViewCacheEntry
                                {
                                    ConceptualModelContainerName = conceptualModelContainerName,
                                    StoreModelContainerName = storeModelContainerName
                                });
                }

                views.ViewDefinitions = viewsXml.ToString();
                views.LastUpdated = DateTimeOffset.Now;

                viewCacheContext.SaveChanges();
            }
        }

        private DatabaseViewCacheEntry GetViews(DatabaseViewCacheContext dbViewCacheContext, string conceptualModelContainerName, string storeModelContainerName)
        {
            return
                dbViewCacheContext
                .ViewCache
                .SingleOrDefault(
                    e =>
                        e.ConceptualModelContainerName == conceptualModelContainerName &&
                        e.StoreModelContainerName == storeModelContainerName);
        }

        // virtual for mocking
        internal virtual DatabaseViewCacheContext CreateDatabaseViewCacheContext()
        {
            var connection = CreateConnection();
            if (connection == null)
            {
                throw new InvalidOperationException("Connection must not be null.");
            }

            return new DatabaseViewCacheContext(connection, _tableName, _schemaName);
        }

        /// <summary>
        /// Creates connection to be used to connect to the database.
        /// </summary>
        /// <returns>
        /// A <see cref="DbConnection"/> instance.
        /// </returns>
        abstract protected DbConnection CreateConnection();

        /// <summary>
        /// Checks if the table storing cached views exists.
        /// </summary>
        /// <param name="viewCacheContext">
        /// An <see cref="DatabaseViewCacheContext"/> instance used to handle view caching.
        /// </param>
        /// <returns>
        /// <code>true</code> if table containing views exists. Otherwise <code>false</code>.
        /// </returns>
        abstract protected bool ViewTableExists(DatabaseViewCacheContext viewCacheContext);

        /// <summary>
        /// Creates the table for storing views.
        /// </summary>
        /// <param name="viewCacheContext">
        /// An <see cref="DatabaseViewCacheContext"/> instance used to handle view caching.
        /// </param>
        /// <remarks>
        /// The table has to use <see cref="DatabaseViewCacheContext.TableName"/> 
        /// and <see cref="DatabaseViewCacheContext.SchemaName"/> as table and schema names.
        /// </remarks>
        abstract protected void CreateViewTable(DatabaseViewCacheContext viewCacheContext);
    }
}
