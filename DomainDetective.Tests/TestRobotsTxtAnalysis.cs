using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace DomainDetective.Tests;

[Collection("HttpListener")]
public class TestRobotsTxtAnalysis
{
    [Fact]
    public async Task ValidRobotsTxtIsParsed()
    {
        using var listener = StartListener(out var prefix);
        var content = "User-agent: *\nDisallow: /private\nAllow: /public\nSitemap: https://example.com/sitemap.xml";
        var serverTask = Task.Run(async () =>
        {
            var ctx = await listener.GetContextAsync();
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "text/plain";
            var buffer = Encoding.UTF8.GetBytes(content);
            await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            ctx.Response.Close();
        });

        try
        {
            var healthCheck = new DomainHealthCheck();
            await healthCheck.Verify(prefix.Replace("http://", string.Empty).TrimEnd('/'), new[] { HealthCheckType.ROBOTS });
            Assert.True(healthCheck.RobotsTxtAnalysis.RecordPresent);
            Assert.Contains("/private", healthCheck.RobotsTxtAnalysis.Robots!.Groups[0].Directives[0].Value);
            Assert.Contains("https://example.com/sitemap.xml", healthCheck.RobotsTxtAnalysis.Robots.Sitemaps);
        }
        finally
        {
            listener.Stop();
            await serverTask;
        }
    }

    [Fact]
    public async Task MissingRobotsTxtIsNotPresent()
    {
        using var listener = StartListener(out var prefix);
        var serverTask = Task.Run(async () =>
        {
            var ctx = await listener.GetContextAsync();
            ctx.Response.StatusCode = 404;
            ctx.Response.Close();
        });

        try
        {
            var healthCheck = new DomainHealthCheck();
            await healthCheck.Verify(prefix.Replace("http://", string.Empty).TrimEnd('/'), new[] { HealthCheckType.ROBOTS });
            Assert.False(healthCheck.RobotsTxtAnalysis.RecordPresent);
        }
        finally
        {
            listener.Stop();
            await serverTask;
        }
    }

    [Fact]
    public async Task DetectsAiBots()
    {
        using var listener = StartListener(out var prefix);
        var content = "User-agent: GPTBot\nDisallow: /private";
        var serverTask = Task.Run(async () =>
        {
            var ctx = await listener.GetContextAsync();
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "text/plain";
            var buffer = Encoding.UTF8.GetBytes(content);
            await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            ctx.Response.Close();
        });

        try
        {
            var healthCheck = new DomainHealthCheck();
            await healthCheck.Verify(prefix.Replace("http://", string.Empty).TrimEnd('/'), new[] { HealthCheckType.ROBOTS });
            Assert.Contains(KnownAiBot.GptBot, healthCheck.RobotsTxtAnalysis.AiBots.Keys);
            Assert.True(healthCheck.RobotsTxtAnalysis.HasAiBotRules);
        }
        finally
        {
            listener.Stop();
            await serverTask;
        }
    }

    private static int GetFreePort()
    {
        return PortHelper.GetFreePort();
    }

    private static HttpListener StartListener(out string prefix)
    {
        if (!HttpListener.IsSupported)
        {
            throw SkipException.ForSkip("HttpListener not supported");
        }

        while (true)
        {
            var port = GetFreePort();
            prefix = $"http://127.0.0.1:{port}/";
            var l = new HttpListener();
            l.Prefixes.Add(prefix);
            try
            {
                l.Start();
                PortHelper.ReleasePort(port);
                return l;
            }
            catch (HttpListenerException)
            {
                l.Close();
                PortHelper.ReleasePort(port);
            }
        }
    }
}
