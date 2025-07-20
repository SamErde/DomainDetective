using System.IO;

namespace DomainDetective.Tests;

public class TestMessageHeaderIssues {
    [Fact]
    public void MissingArcIsReported() {
        var raw = File.ReadAllText("Data/dkimvalidator-headers.txt");
        var analysis = new MessageHeaderAnalysis();
        analysis.Parse(raw, new InternalLogger());
        Assert.Contains(MessageHeaderIssue.MissingArc, analysis.Issues);
    }

    [Fact]
    public void InvalidDkimSignatureIsReported() {
        var raw = File.ReadAllText("Data/dkim-bad-padding.txt");
        var analysis = new MessageHeaderAnalysis();
        analysis.Parse(raw, new InternalLogger());
        Assert.Contains(MessageHeaderIssue.InvalidDkim, analysis.Issues);
    }
}
