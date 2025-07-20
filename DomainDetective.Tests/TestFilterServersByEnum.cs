using System.Linq;
using DomainDetective;

namespace DomainDetective.Tests {
    public class TestFilterServersByEnum {
        [Fact]
        public void FilterServersByCountryAndLocation() {
            var analysis = new DnsPropagationAnalysis();
            analysis.LoadBuiltinServers();
            CountryIdExtensions.TryParse("Afghanistan", out var af);
            LocationIdExtensions.TryParse("Kabul", out var kab);
            var servers = analysis.FilterServers(af, kab).ToList();
            Assert.NotEmpty(servers);
            Assert.All(servers, s => {
                Assert.Equal(af, s.Country);
                Assert.Equal(kab, s.Location);
            });
        }
    }
}
