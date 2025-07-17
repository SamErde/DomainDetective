namespace DomainDetective.Tests;

public class TestOpenResolverAnalysis {
    [Fact]
    public async Task DetectsRecursionAllowed() {
        var analysis = new OpenResolverAnalysis { RecursionTestOverride = (_, _) => Task.FromResult(true) };
        await analysis.AnalyzeServer("server", 53, new InternalLogger());
        Assert.True(analysis.ServerResults["server:53"]);
    }

    [Fact]
    public async Task DetectsRecursionDisabled() {
        var analysis = new OpenResolverAnalysis { RecursionTestOverride = (_, _) => Task.FromResult(false) };
        await analysis.AnalyzeServer("server", 53, new InternalLogger());
        Assert.False(analysis.ServerResults["server:53"]);
    }

    [Fact]
    public async Task ResultsResetBetweenRuns() {
        var analysis = new OpenResolverAnalysis { RecursionTestOverride = (_, _) => Task.FromResult(true) };
        await analysis.AnalyzeServer("a", 53, new InternalLogger());
        await analysis.AnalyzeServer("b", 53, new InternalLogger());
        Assert.False(analysis.ServerResults.ContainsKey("a:53"));
        Assert.True(analysis.ServerResults["b:53"]);
    }
}