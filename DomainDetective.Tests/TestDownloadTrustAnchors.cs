using System;
using System.IO;
using System.Threading.Tasks;
using DomainDetective;
using Xunit;

namespace DomainDetective.Tests {
    public class TestDownloadTrustAnchors {
        [Fact]
        public async Task FetchesAnchors() {
            var anchors = await DnsSecAnalysis.DownloadTrustAnchors();
            Assert.NotEmpty(anchors);
        }

        [Fact]
        public async Task MalformedXmlReturnsEmpty() {
            string cacheDir = Path.Combine(Path.GetTempPath(), "DomainDetective");
            string cacheFile = Path.Combine(cacheDir, "root-anchors.xml");
            Directory.CreateDirectory(cacheDir);
            File.WriteAllText(cacheFile, "<trustanchors><bad></trust>");
            File.SetLastWriteTimeUtc(cacheFile, DateTime.UtcNow);

            try {
                var anchors = await DnsSecAnalysis.DownloadTrustAnchors();
                Assert.Empty(anchors);
            } finally {
                File.Delete(cacheFile);
            }
        }
    }
}
