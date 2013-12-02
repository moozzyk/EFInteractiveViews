namespace InteractivePreGeneratedViews.UnitTests
{
    using System;
    using Xunit;

    public class SqlServerViewCacheFactoryTests
    {
        [Fact]
        public void Cannot_create_SqlServerViewCacheFactory_with_null_connection_string()
        {
            Assert.Equal(
                "connectionString",
                Assert.Throws<ArgumentNullException>(
                    () => new SqlServerViewCacheFactory(null)).ParamName);

            Assert.Equal(
                "connectionString",
                Assert.Throws<ArgumentNullException>(
                    () => new SqlServerViewCacheFactory(" ")).ParamName);
        }
    }
}
