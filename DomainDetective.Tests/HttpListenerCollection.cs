using Xunit;

namespace DomainDetective.Tests;

[CollectionDefinition("HttpListener", DisableParallelization = true)]
public class HttpListenerCollection : ICollectionFixture<object>
{
}
