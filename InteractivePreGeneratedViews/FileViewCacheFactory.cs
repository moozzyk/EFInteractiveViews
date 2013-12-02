namespace InteractivePreGeneratedViews
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// An implementation of the <see cref="DbMappingViewCacheFactory"/> class that stores views in a file.
    /// </summary>
    public class FileViewCacheFactory : ViewCacheFactoryBase
    {
        private readonly string _viewFilePath;

        private readonly object _lockObject = new object();

        /// <summary>
        /// Creates an instance of the <see cref="FileViewCacheFactory"/> class.
        /// </summary>
        /// <param name="viewFilePath">A path to the file where the views are stored.</param>
        public FileViewCacheFactory(string viewFilePath)
        {
            if (string.IsNullOrWhiteSpace(viewFilePath))
            {
                throw new ArgumentNullException("viewFilePath");
            }

            _viewFilePath = viewFilePath;
        }

        /// <summary>
        /// Loads views from a file.
        /// </summary>
        /// <param name="conceptualModelContainerName">Not used for loading views from a file.</param>
        /// <param name="storeModelContainerName">Not used for loading views from a file.</param>
        /// <returns>Views loaded from a file or null if views could not be loaded.</returns>
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

            try
            {
                lock (_lockObject)
                {
                    if (File.Exists(_viewFilePath))
                    {
                        return XDocument.Load(_viewFilePath);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// Saves views to a file.
        /// </summary>
        /// <param name="viewsXml">View definitions to be saved.</param>
        protected override void Save(XDocument viewsXml)
        {
            if (viewsXml == null)
            {
                throw new ArgumentNullException("viewsXml");
            }

            lock (_lockObject)
            {
                viewsXml.Save(_viewFilePath);
            }
        }
    }
}
