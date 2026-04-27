namespace Api.Tests;

/// <summary>
/// All integration tests share one OpenPsaFactory (one Postgres + Redis container)
/// to avoid WebApplicationFactory&lt;Program&gt; races and reduce container churn.
/// </summary>
[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<OpenPsaFactory> {
    public const string Name = "Integration";
}
