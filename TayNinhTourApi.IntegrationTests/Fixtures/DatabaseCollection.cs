using Xunit;

namespace TayNinhTourApi.IntegrationTests.Fixtures
{
    /// <summary>
    /// Database collection definition để ensure test isolation
    /// Tất cả tests trong collection này sẽ chạy sequentially để tránh database conflicts
    /// </summary>
    [CollectionDefinition("Database")]
    public class DatabaseCollection
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
