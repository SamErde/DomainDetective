using System.Text.Json;
using DomainDetective.Helpers;

namespace DomainDetective.Tests {
    public class TestJsonSerialization {
        [Fact]
        public void HealthCheckSerializationConsistent() {
            var hc = new DomainHealthCheck();
            var json1 = hc.ToJson();
            var json2 = JsonSerializer.Serialize(hc, JsonOptions.Default);
            Assert.Equal(json1, json2);
        }

        [Fact]
        public void SummarySerializationConsistent() {
            var hc = new DomainHealthCheck();
            var summary = hc.BuildSummary();
            var json1 = JsonSerializer.Serialize(summary, JsonOptions.Default);
            var json2 = JsonSerializer.Serialize(summary, JsonOptions.Default);
            Assert.Equal(json1, json2);
        }
    }
}
