using System.Net;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace DomainDetective.Tests;

public class TestDirectoryExposureAnalysis
{
    [Fact]
    public async Task DetectsAccessibleDirectories()
    {
        if (!HttpListener.IsSupported)
        {
            throw SkipException.ForSkip("HttpListener not supported");
        }
        using var listener = new HttpListener();
        var prefix = $"http://localhost:{GetFreePort()}/";
        listener.Prefixes.Add(prefix);
        listener.Start();
        var serverTask = Task.Run(async () =>
        {
            while (listener.IsListening)
            {
                var ctx = await listener.GetContextAsync();
                if (ctx.Request.Url.AbsolutePath.StartsWith("/.git"))
                {
                    ctx.Response.StatusCode = 200;
                }
                else
                {
                    ctx.Response.StatusCode = 404;
                }
                ctx.Response.Close();
            }
        });

        try
        {
            var hc = new DomainHealthCheck();
            await hc.VerifyDirectoryExposure(prefix.Replace("http://", string.Empty).TrimEnd('/'));
            Assert.Contains(".git/", hc.DirectoryExposureAnalysis.ExposedPaths);
        }
        finally
        {
            listener.Stop();
            await Task.Delay(50);
        }
    }

    [Fact]
    public async Task NoExposedDirectoriesWhenNoneAccessible()
    {
        if (!HttpListener.IsSupported)
        {
            throw SkipException.ForSkip("HttpListener not supported");
        }
        using var listener = new HttpListener();
        var prefix = $"http://localhost:{GetFreePort()}/";
        listener.Prefixes.Add(prefix);
        listener.Start();
        var serverTask = Task.Run(async () =>
        {
            var ctx = await listener.GetContextAsync();
            ctx.Response.StatusCode = 404;
            ctx.Response.Close();
        });

        try
        {
            var hc = new DomainHealthCheck();
            await hc.VerifyDirectoryExposure(prefix.Replace("http://", string.Empty).TrimEnd('/'));
            Assert.Empty(hc.DirectoryExposureAnalysis.ExposedPaths);
        }
        finally
        {
            listener.Stop();
            await Task.Delay(50);
        }
    }

    private static int GetFreePort() => PortHelper.GetFreePort();
}
