using Xunit;
using System;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace DomainDetective.Tests {
    public class TestCertificateMonitor {
        [Fact]
        public async Task ProducesSummaryCounts() {
            var monitor = new CertificateMonitor();
            await monitor.Analyze(new[] { "https://www.google.com", "https://nonexistent.invalid" });
            Skip.If(monitor.Results.TrueForAll(r => !r.Analysis.IsReachable), "Hosts not reachable");
            Assert.Equal(2, monitor.Results.Count);
            Assert.True(monitor.ValidCount >= 1);
            Assert.True(monitor.FailedCount >= 1);

            var reachable = monitor.Results.Find(r => r.Analysis.IsReachable);
            Assert.NotNull(reachable);
            Assert.Equal(reachable!.Analysis.TlsProtocol, reachable.Protocol);
        }

        [Fact]
        public void TimerStopsAfterDispose() {
            var monitor = new CertificateMonitor();
            monitor.Start(Array.Empty<string>(), TimeSpan.FromMilliseconds(10));
            Assert.True(monitor.IsRunning);
            monitor.Dispose();
            Assert.False(monitor.IsRunning);
        }

        [Fact]
        public async Task CanStartAndStopMultipleTimes() {
            var monitor = new CertificateMonitor();
            var timerField = typeof(CertificateMonitor).GetField("_timer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            for (int i = 0; i < 3; i++) {
                monitor.Start(Array.Empty<string>(), TimeSpan.FromMilliseconds(1));
                Assert.NotNull(timerField.GetValue(monitor));
                await monitor.StopAsync();
                Assert.Null(timerField.GetValue(monitor));
            }
        }
    }
}
