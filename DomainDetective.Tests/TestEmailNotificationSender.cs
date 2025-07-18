using DomainDetective.Monitoring;
using MailKit.Net.Smtp;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace DomainDetective.Tests;

public class TestEmailNotificationSender {
    private class CountingSmtpClient : SmtpClient {
        public int ConnectCount { get; private set; }
        public int DisposeCount { get; private set; }

        public CountingSmtpClient() {
            Connected += (_, _) => ConnectCount++;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                DisposeCount++;
            }
            base.Dispose(disposing);
        }
    }

    [Fact]
    public async Task DisposesClientAndConnectsOnce() {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var serverTask = Task.Run(async () => {
            using var client = await listener.AcceptTcpClientAsync();
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true, NewLine = "\r\n" };
            await writer.WriteLineAsync("220 local ESMTP");
            await reader.ReadLineAsync();
            await writer.WriteLineAsync("250 hello");
            await reader.ReadLineAsync();
            await writer.WriteLineAsync("250 OK");
            await reader.ReadLineAsync();
            await writer.WriteLineAsync("250 OK");
            await reader.ReadLineAsync();
            await writer.WriteLineAsync("354 Start mail input; end with <CRLF>.<CRLF>");
            string? line;
            while ((line = await reader.ReadLineAsync()) != ".") { }
            await writer.WriteLineAsync("250 OK");
            await reader.ReadLineAsync();
            await writer.WriteLineAsync("221 bye");
        });

        var client = new CountingSmtpClient();
        var original = EmailNotificationSender.CreateClient;
        EmailNotificationSender.CreateClient = () => client;
        try {
            var sender = new EmailNotificationSender {
                SmtpHost = "localhost",
                Port = port,
                From = "from@example.com",
                To = "to@example.com"
            };
            await sender.SendAsync("test");
        } finally {
            EmailNotificationSender.CreateClient = original;
            listener.Stop();
            await serverTask;
        }

        Assert.Equal(1, client.ConnectCount);
        Assert.Equal(1, client.DisposeCount);
    }
}