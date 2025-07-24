namespace DomainDetective;

/// <summary>
/// Known AI-related user agents that may appear in robots.txt files.
/// </summary>
public enum KnownAiBot
{
    /// <summary>Unrecognized bot.</summary>
    Unknown,
    /// <summary>The OpenAI GPTBot crawler.</summary>
    GptBot,
    /// <summary>ChatGPT bot user agent.</summary>
    ChatGpt,
    /// <summary>User agent used by ChatGPT browser plugin.</summary>
    ChatGptUser,
    /// <summary>The CommonCrawl bot.</summary>
    CcBot,
    /// <summary>Anthropic Claude bot.</summary>
    ClaudeBot,
    /// <summary>Anthropic API bot.</summary>
    Anthropic,
    /// <summary>Google Extended crawler.</summary>
    GoogleExtended
}
