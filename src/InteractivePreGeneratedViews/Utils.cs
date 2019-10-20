
namespace InteractivePreGeneratedViews
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Linq;

    internal class Utils
    {
        public static string GetExtentFullName(EntitySetBase entitySet)
        {
            Debug.Assert(entitySet != null, "entitySet != null");

            return string.Format("{0}.{1}", entitySet.EntityContainer.Name, entitySet.Name);
        }
    }
}