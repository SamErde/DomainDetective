using Xunit;
using System;
using System.Threading.Tasks;

namespace DomainDetective.Tests {
    public class TestCertificateMonitor {
        [Fact]
        public async Task ProducesSummaryCounts() {
            var monitor = new CertificateMonitor();
            await monitor.Analyze(new[] { "https://www.google.com", "https://nonexistent.invalid" });
            Assert.Equal(2, monitor.Results.Count);
            Assert.True(monitor.ValidCount >= 1);
            Assert.True(monitor.FailedCount >= 1);
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
        public void CanStartAndStopMultipleTimes() {
            var monitor = new CertificateMonitor();
            var timerField = typeof(CertificateMonitor).GetField("_timer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            for (int i = 0; i < 3; i++) {
                monitor.Start(Array.Empty<string>(), TimeSpan.FromMilliseconds(1));
                Assert.NotNull(timerField.GetValue(monitor));
                monitor.Stop();
                Assert.Null(timerField.GetValue(monitor));
            }
        }
    }
}
