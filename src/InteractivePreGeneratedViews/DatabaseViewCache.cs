namespace InteractivePreGeneratedViews
{
    using System;

    /// <summary>
    /// Contains information about views for a given conceptual-store container mapping.
    /// </summary>
    public class DatabaseViewCacheEntry
    {
        /// <summary>
        /// Gets or sets the name of the conceptual container.
        /// </summary>
        public string ConceptualModelContainerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the store container.
        /// </summary>
        public string StoreModelContainerName { get; set; }

        /// <summary>
        /// Gets or sets views.
        /// </summary>
        public string ViewDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the date of the last update.
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; }
    }
}
