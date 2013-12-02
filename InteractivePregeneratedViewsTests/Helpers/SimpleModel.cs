using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractivePreGeneratedViews.Helpers
{
    public class Entity
    {
        public int Id { get; set; }
    }

    public class SimpleModel : DbContext
    {
        static SimpleModel()
        {
            Database.SetInitializer<SimpleModel>(null);
        }


        public SimpleModel()
            : this(TestUtils.TempDbConnectionString)
        {
        }

        public SimpleModel(string nameOrConnectionString)
            : base(nameOrConnectionString)
        { }

        public DbSet<Entity> Entities { get; set; }

        public static readonly string Views =
@"<views>
  <mapping-views hash=""a6a6726b2b18f5203f05b031fca8957087840544fa4ef224c54047135aaf069b"" conceptual-container=""SimpleModel"" store-container=""CodeFirstDatabase"">
    <view extent=""CodeFirstDatabase.Entity""><![CDATA[
    SELECT VALUE -- Constructing Entity
        [CodeFirstDatabaseSchema.Entity](T1.Entity_Id)
    FROM (
        SELECT 
            T.Id AS Entity_Id, 
            True AS _from0
        FROM SimpleModel.Entities AS T
    ) AS T1]]></view>
    <view extent=""SimpleModel.Entities""><![CDATA[
    SELECT VALUE -- Constructing Entities
        [InteractivePreGeneratedViews.Helpers.Entity](T1.Entity_Id)
    FROM (
        SELECT 
            T.Id AS Entity_Id, 
            True AS _from0
        FROM CodeFirstDatabase.Entity AS T
    ) AS T1]]></view>
  </mapping-views>
</views>";
    }
}
