namespace InteractivePreGeneratedViews
{
    using InteractivePreGeneratedViews.Helpers;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class UtilsTests
    {
        [Fact]
        public void GetExtentFullName_returns_qualified_entity_set_name()
        {
            Assert.Equal(
                "ModelContainer.Customers",
                Utils.GetExtentFullName(
                    TestUtils.CreateEntitySet("ModelContainer", "Customers")));
        }
    }
}
