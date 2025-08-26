using System.Management.Automation;
using System.Threading.Tasks;

namespace DomainDetective.PowerShell
{

/// <summary>Queries search engines for information.</summary>
/// <para>Part of the DomainDetective project.</para>
/// <example>
///   <summary>Query Google search engine.</summary>
///   <code>Get-DDSearchEngineInfo -Query 'example'</code>
/// </example>
[Cmdlet(VerbsCommon.Get, "DDSearchEngineInfo")]
[Alias("Get-SearchEngineInfo")]
public sealed class CmdletGetSearchEngineInfo : AsyncPSCmdlet
{
    /// <para>Search query.</para>
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNullOrEmpty]
    public string Query { get; set; } = string.Empty;

    /// <para>Google API key.</para>
    [Parameter]
    public string? GoogleApiKey { get; set; }

    /// <para>Google search engine identifier.</para>
    [Parameter]
    public string? GoogleCx { get; set; }

    /// <para>Google API endpoint.</para>
    [Parameter]
    public string GoogleEndpoint { get; set; } = "https://www.googleapis.com/customsearch/v1";

    /// <para>Bing API key.</para>
    [Parameter]
    public string? BingApiKey { get; set; }

    /// <para>Bing API endpoint.</para>
    [Parameter]
    public string BingEndpoint { get; set; } = "https://api.bing.microsoft.com/v7.0/search";

    /// <para>Search engine to use: google or bing.</para>
    [Parameter]
    public string Engine { get; set; } = "google";

    private SearchEngineAnalysis _analysis = null!;

    /// <summary>Initializes analysis instance.</summary>
    protected override Task BeginProcessingAsync()
    {
        _analysis = new SearchEngineAnalysis
        {
            GoogleApiKey = GoogleApiKey,
            GoogleCx = GoogleCx,
            GoogleEndpoint = GoogleEndpoint,
            BingApiKey = BingApiKey,
            BingEndpoint = BingEndpoint
        };
        return Task.CompletedTask;
    }

    /// <summary>Executes the request.</summary>
    protected override async Task ProcessRecordAsync()
    {
        object result = Engine.ToLowerInvariant() switch
        {
            "bing" => await _analysis.SearchBing(Query, CancelToken),
            "google" => await _analysis.SearchGoogle(Query, CancelToken),
            _ => throw new PSArgumentException("Engine must be 'google' or 'bing'.")
        };

        WriteObject(result);
    }
}
}
