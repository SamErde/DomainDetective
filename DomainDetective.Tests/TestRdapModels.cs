using System.Text.Json;

namespace DomainDetective.Tests;

public class TestRdapModels
{
    [Fact]
    public void DomainSerializationRoundTrip()
    {
        const string json = "{\"ldhName\":\"example.com\",\"status\":[\"active\"],\"nameservers\":[{\"ldhName\":\"ns1.example.net\"},{\"ldhName\":\"ns2.example.net\"}],\"events\":[{\"eventAction\":\"registration\",\"eventDate\":\"2000-01-01T00:00:00Z\"},{\"eventAction\":\"expiration\",\"eventDate\":\"2030-01-01T00:00:00Z\"}],\"entities\":[{\"handle\":\"123\",\"roles\":[\"registrar\"],\"vcardArray\":[\"vcard\",[[\"fn\",{},\"text\",\"Registrar Inc\"]]]}]}";

        var domain = JsonSerializer.Deserialize<RdapDomain>(json, RdapJson.Options)!;
        Assert.Equal("example.com", domain.LdhName);
        Assert.Equal("ns1.example.net", domain.Nameservers[0].LdhName);
        Assert.Equal(RdapEventAction.Registration, domain.Events[0].Action);
        Assert.Equal(RdapDomainStatus.Active, domain.Status[0]);

        var serialized = JsonSerializer.Serialize(domain, RdapJson.Options);
        var round = JsonSerializer.Deserialize<RdapDomain>(serialized, RdapJson.Options)!;
        Assert.Equal(domain.LdhName, round.LdhName);
        Assert.Equal(domain.Nameservers[1].LdhName, round.Nameservers[1].LdhName);
    }

    [Fact]
    public void IpNetworkParsing()
    {
        const string json = "{\"startAddress\":\"192.0.2.0\",\"endAddress\":\"192.0.2.255\",\"cidr\":\"192.0.2.0/24\"}";
        var net = JsonSerializer.Deserialize<RdapIpNetwork>(json, RdapJson.Options)!;
        Assert.Equal("192.0.2.0", net.StartAddress);
        Assert.Equal("192.0.2.255", net.EndAddress);
        Assert.Equal("192.0.2.0/24", net.Cidr);
    }

    [Fact]
    public void AutnumParsing()
    {
        const string json = "{\"handle\":\"AS65536\",\"startAutnum\":65536,\"endAutnum\":65536,\"name\":\"Example ASN\"}";
        var asn = JsonSerializer.Deserialize<RdapAutnum>(json, RdapJson.Options)!;
        Assert.Equal("AS65536", asn.Handle);
        Assert.Equal(65536, asn.Start);
        Assert.Equal(65536, asn.End);
        Assert.Equal("Example ASN", asn.Name);
    }

    [Fact]
    public void EntityParsing()
    {
        const string json = "{\"handle\":\"ABC123\",\"roles\":[\"registrant\"],\"status\":[\"validated\"],\"vcardArray\":[\"vcard\",[[\"fn\",{},\"text\",\"John Doe\"]]]}";
        var entity = JsonSerializer.Deserialize<RdapEntity>(json, RdapJson.Options)!;
        Assert.Equal("ABC123", entity.Handle);
        Assert.Contains("registrant", entity.Roles);
        Assert.True(entity.VcardArray.HasValue);
        Assert.Contains(RdapDomainStatus.Validated, entity.Status);
    }

    [Fact]
    public void NameserverParsing()
    {
        const string json = "{\"ldhName\":\"ns1.example.com\"}";
        var ns = JsonSerializer.Deserialize<RdapNameserver>(json, RdapJson.Options)!;
        Assert.Equal("ns1.example.com", ns.LdhName);
    }
}
