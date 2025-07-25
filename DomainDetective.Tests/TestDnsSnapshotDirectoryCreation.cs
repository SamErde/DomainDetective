using DnsClientX;
using DomainDetective;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Xunit;

namespace DomainDetective.Tests {
    public class TestDnsSnapshotDirectoryCreation {
        [Fact]
        public void CreatesDirectoryWhenSavingSnapshot() {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var analysis = new DnsPropagationAnalysis { SnapshotDirectory = dir };
            var results = new List<DnsPropagationResult> {
                new() {
                    Server = new PublicDnsEntry { IPAddress = IPAddress.Parse("1.1.1.1"), Enabled = true },
                    RecordType = DnsRecordType.A,
                    Records = new[] { "1.2.3.4" },
                    Success = true
                }
            };

            analysis.SaveSnapshot("example.com", DnsRecordType.A, results);

            var files = Directory.GetFiles(dir);
            Assert.Single(files);
            Directory.Delete(dir, true);
        }
    }
}
