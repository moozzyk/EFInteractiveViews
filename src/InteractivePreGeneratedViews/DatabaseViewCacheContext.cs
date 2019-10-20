namespace InteractivePreGeneratedViews
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;

    /// <summary>
    /// Context used to handle database view cache.
    /// </summary>
    public class DatabaseViewCacheContext : DbContext
    {
        private readonly string _tableName;
        private readonly string _schemaName;

        static DatabaseViewCacheContext()
        {
            Database.SetInitializer<DatabaseViewCacheContext>(null);
        }

        /// <summary>
        /// Creates a new instance of <see cref="DatabaseViewCacheContext"/>.
        /// </summary>
        /// <param name="connection">
        /// <see cref="DbConnection"/> used to connect to the database containing cached views.
        /// </param>
        /// <param name="tableName">
        /// Name of the table where views are stored. 
        /// Optional - if not provided '__ViewCache' will be used as the table name.
        /// </param>
        /// <param name="schemaName">
        /// Schema of the table where views are stored. 
        /// Optional - if not provided 'dbo' will be used as the table schema.
        /// </param>
        public DatabaseViewCacheContext(DbConnection connection, string tableName = null, string schemaName = null)
            : base(connection, true)
        {
            _tableName = tableName ?? "__ViewCache";
            _schemaName = schemaName ?? "dbo";
        }

        /// <summary>
        /// Gets the name of the table where views are stored.
        /// </summary>
        public string TableName
        {
            get { return _tableName; }
        }

        /// <summary>
        /// Gets the schema of the table where views are used.
        /// </summary>
        public string SchemaName
        {
            get { return _schemaName; }
        }

        /// <summary>
        /// Gets or sets the DbSet with cached views.
        /// </summary>
        public virtual DbSet<DatabaseViewCacheEntry> ViewCache { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var viewCacheConfiguration = modelBuilder.Entity<DatabaseViewCacheEntry>();
            viewCacheConfiguration.ToTable(_tableName, _schemaName);
            viewCacheConfiguration.HasKey(e => new { e.ConceptualModelContainerName, e.StoreModelContainerName });
            viewCacheConfiguration
                .Property(p => p.ConceptualModelContainerName)
                .IsUnicode()
                .HasMaxLength(255);
            viewCacheConfiguration
                .Property(p => p.StoreModelContainerName)
                .IsUnicode()
                .HasMaxLength(255);
            viewCacheConfiguration.Property(p => p.ViewDefinitions)
                .IsUnicode()
                .IsMaxLength();
        }
    }
}
