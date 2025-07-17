using DomainDetective;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;
namespace DomainDetective.Tests {
    public class TestPortScanAnalysis {
        [Fact]
        public async Task DetectsTcpAndUdpOpenPorts() {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var tcpPort = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            var tcpAccept = tcpListener.AcceptTcpClientAsync();

            var udpServer = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            var udpPort = ((IPEndPoint)udpServer.Client.LocalEndPoint!).Port;
            var udpTask = Task.Run(async () => {
                var r = await udpServer.ReceiveAsync();
                await udpServer.SendAsync(new byte[] { 1 }, 1, r.RemoteEndPoint);
            });

            try {
                var analysis = new PortScanAnalysis { Timeout = TimeSpan.FromMilliseconds(200) };
                await analysis.Scan("127.0.0.1", new[] { tcpPort, udpPort }, new InternalLogger());
                using var _ = await tcpAccept; // ensure connection completes

                Assert.True(analysis.Results[tcpPort].TcpOpen);
                Assert.True(analysis.Results[udpPort].UdpOpen);
            } finally {
                tcpListener.Stop();
                udpServer.Close();
                await udpTask;
            }
        }

        [Fact]
        public async Task DetectsIpv6TcpAndUdpOpenPorts() {
            var tcpListener = new TcpListener(IPAddress.IPv6Loopback, 0);
            tcpListener.Start();
            var tcpPort = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            var tcpAccept = tcpListener.AcceptTcpClientAsync();

            var udpServer = new UdpClient(new IPEndPoint(IPAddress.IPv6Loopback, 0));
            var udpPort = ((IPEndPoint)udpServer.Client.LocalEndPoint!).Port;
            var udpTask = Task.Run(async () => {
                var r = await udpServer.ReceiveAsync();
                await udpServer.SendAsync(new byte[] { 1 }, 1, r.RemoteEndPoint);
            });

            try {
                var analysis = new PortScanAnalysis { Timeout = TimeSpan.FromMilliseconds(200) };
                await analysis.Scan("::1", new[] { tcpPort, udpPort }, new InternalLogger());
                using var _ = await tcpAccept;

                Assert.True(analysis.Results[tcpPort].TcpOpen);
                Assert.True(analysis.Results[udpPort].UdpOpen);
            } finally {
                tcpListener.Stop();
                udpServer.Close();
                await udpTask;
            }
        }

        [Fact]
        public async Task ConfirmsIpv6Reachability() {
            var listener = new TcpListener(IPAddress.IPv6Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var accept = listener.AcceptTcpClientAsync();

            try {
                var reachable = await PortScanAnalysis.IsIPv6Reachable("localhost", port);
                using var _ = await accept;
                Assert.True(reachable);
            } finally {
                listener.Stop();
            }
        }

        [Fact]
        public async Task UdpPortClosedWhenNoResponseData() {
            var udpServer = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            var udpPort = ((IPEndPoint)udpServer.Client.LocalEndPoint!).Port;
            var udpTask = Task.Run(async () => {
                var r = await udpServer.ReceiveAsync();
                await udpServer.SendAsync(Array.Empty<byte>(), 0, r.RemoteEndPoint);
            });

            try {
                var analysis = new PortScanAnalysis { Timeout = TimeSpan.FromMilliseconds(200) };
                await analysis.Scan("127.0.0.1", new[] { udpPort }, new InternalLogger());

                Assert.False(analysis.Results[udpPort].UdpOpen);
                Assert.False(string.IsNullOrEmpty(analysis.Results[udpPort].Error));
            } finally {
                udpServer.Close();
                await udpTask;
            }
        }

        [Fact]
        public async Task DetectsTcpClosedPort() {
            var port = GetFreePort();
            var analysis = new PortScanAnalysis { Timeout = TimeSpan.FromMilliseconds(200) };
            await analysis.Scan("127.0.0.1", new[] { port }, new InternalLogger());
            Assert.False(analysis.Results[port].TcpOpen);
            Assert.False(string.IsNullOrEmpty(analysis.Results[port].Error));
        }

        [Fact]
        public async Task UnresolvableHostRecordsError() {
            var analysis = new PortScanAnalysis { Timeout = TimeSpan.FromMilliseconds(200) };
            await analysis.Scan("nonexistent.example.invalid", new[] { 80 }, new InternalLogger());
            Assert.False(analysis.Results[80].TcpOpen);
            Assert.False(string.IsNullOrEmpty(analysis.Results[80].Error));
        }

        [Fact]
        public async Task ScansUsingSmbProfile() {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var accept = listener.AcceptTcpClientAsync();
            try {
                PortScanAnalysis.OverrideProfilePorts(PortScanProfile.SMB, new[] { port });
                var analysis = new PortScanAnalysis { Timeout = TimeSpan.FromMilliseconds(200) };
                await analysis.Scan("127.0.0.1", PortScanProfile.SMB, new InternalLogger());
                using var _ = await accept;
                Assert.True(analysis.Results[port].TcpOpen);
            } finally {
                listener.Stop();
                PortScanAnalysis.OverrideProfilePorts(PortScanProfile.SMB, new[] { 445, 139 });
            }
        }

        [Fact]
        public async Task ScansUsingNtpProfile() {
            var server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            var port = ((IPEndPoint)server.Client.LocalEndPoint!).Port;
            var task = Task.Run(async () => {
                var r = await server.ReceiveAsync();
                await server.SendAsync(new byte[] { 1 }, 1, r.RemoteEndPoint);
            });
            try {
                PortScanAnalysis.OverrideProfilePorts(PortScanProfile.NTP, new[] { port });
                var analysis = new PortScanAnalysis { Timeout = TimeSpan.FromMilliseconds(200) };
                await analysis.Scan("127.0.0.1", PortScanProfile.NTP, new InternalLogger());
                Assert.True(analysis.Results[port].UdpOpen);
            } finally {
                server.Close();
                await task;
                PortScanAnalysis.OverrideProfilePorts(PortScanProfile.NTP, new[] { 123 });
            }
        }

        [Fact]
        public async Task DetectsServiceBanners() {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var serverTask = Task.Run(async () => {
                using var client = await listener.AcceptTcpClientAsync();
                using var stream = client.GetStream();
                var data = System.Text.Encoding.ASCII.GetBytes("SSH-2.0-Test\r\n");
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
            });

            try {
                var analysis = new PortScanAnalysis { Timeout = TimeSpan.FromMilliseconds(200) };
                await analysis.Scan("127.0.0.1", new[] { port }, new InternalLogger());
                Assert.Equal("SSH-2.0-Test", analysis.Results[port].Banner);
            } finally {
                await serverTask;
                listener.Stop();
            }
        }

        private static int GetFreePort() {
            return PortHelper.GetFreePort();
        }
    }
}