using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DomainDetective.Tests {
    public class TestAddServerConcurrency {
        [Fact]
        public async Task AddServerHandlesConcurrency() {
            var analysis = new DnsPropagationAnalysis();

            var tasks = Enumerable.Range(0, 20)
                .Select(i => Task.Run(() => analysis.AddServer(new PublicDnsEntry {
                    IPAddress = IPAddress.Parse($"192.0.2.{i}"),
                    Enabled = true
                })));

            await Task.WhenAll(tasks);
            Assert.Equal(20, analysis.Servers.Count);
        }
    }
}