using System.Threading.Tasks;

namespace DomainDetective.Tests;

public class TestRdapClient
{
    [Fact]
    public async Task ReadsDomainFromLocalFiles()
    {
        var client = new RdapClient("Data/rdap");
        var result = await client.QueryDomainAsync("example.com");
        Assert.NotNull(result);
        Assert.Equal("EXAMPLE.COM", result!.LdhName);
    }

    [Fact]
    public async Task ReadsIpNetworkFromLocalFiles()
    {
        var client = new RdapClient("Data/rdap");
        var result = await client.QueryIpAsync("192.0.2.1");
        Assert.NotNull(result);
        Assert.Equal("192.0.2.0/24", result!.Cidr);
    }
}
