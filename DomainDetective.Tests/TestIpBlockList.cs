using System.Net;
using System.Net.Http;
using RichardSzalay.MockHttp;

namespace DomainDetective.Tests {
    public class TestIpBlockList {
        [Fact]
        public async Task UpdateParsesRanges() {
            var analysis = new IpBlockListAnalysis();
            analysis.Entries.Add(new BlockListEntry {
                Name = "test",
                Url = "http://example.com/list.txt"
            });

            var handler = new MockHttpMessageHandler();
            handler.When("http://example.com/list.txt").Respond("text/plain", "1.2.3.0/24\n1.2.4.5/32");
            using var client = handler.ToHttpClient();

            await analysis.UpdateAsync(client: client);

            Assert.Contains("test", analysis.ListsContaining(IPAddress.Parse("1.2.3.4")));
            Assert.Contains("test", analysis.ListsContaining(IPAddress.Parse("1.2.4.5")));
        }
    }
}
