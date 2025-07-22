using DomainDetective;
using System.Threading.Tasks;

namespace DomainDetective.Example;

public static partial class Program
{
    /// <summary>
    /// Demonstrates deserializing RDAP JSON into strongly typed objects.
    /// </summary>
    public static async Task ExampleDeserializeRdap()
    {
        var client = new RdapClient();
        if (await client.QueryDomainAsync("example.com") is { } domain)
        {
            Helpers.ShowPropertiesTable("RDAP domain data", domain);
        }

        if (await client.QueryIpAsync("192.0.2.0/24") is { } network)
        {
            Helpers.ShowPropertiesTable("RDAP IP data", network);
        }

        if (await client.QueryAutnumAsync("AS65536") is { } asn)
        {
            Helpers.ShowPropertiesTable("RDAP AS data", asn);
        }

        if (await client.QueryEntityAsync("ABCDE") is { } entity)
        {
            Helpers.ShowPropertiesTable("RDAP entity data", entity);
        }

        if (await client.QueryNameserverAsync("ns1.example.com") is { } ns)
        {
            Helpers.ShowPropertiesTable("RDAP nameserver data", ns);
        }
    }
}
