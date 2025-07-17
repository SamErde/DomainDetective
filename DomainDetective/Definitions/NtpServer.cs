using System;

namespace DomainDetective;

/// <summary>
/// Commonly used public NTP servers.
/// </summary>
public enum NtpServer {
    /// <summary>Pool.ntp.org service.</summary>
    Pool,
    /// <summary>Google public NTP.</summary>
    Google,
    /// <summary>Cloudflare public NTP.</summary>
    Cloudflare,
    /// <summary>NIST time service.</summary>
    Nist,
    /// <summary>Windows time service.</summary>
    Windows
}

/// <summary>Extension methods for <see cref="NtpServer"/>.</summary>
public static class NtpServerExtensions {
    /// <summary>Gets the host name for the server.</summary>
    public static string ToHost(this NtpServer server) => server switch {
        NtpServer.Google => "time.google.com",
        NtpServer.Cloudflare => "time.cloudflare.com",
        NtpServer.Nist => "time.nist.gov",
        NtpServer.Windows => "time.windows.com",
        _ => "pool.ntp.org"
    };

    /// <summary>Tries to parse a server name.</summary>
    public static bool TryParse(string? value, out NtpServer server) {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<NtpServer>(value, true, out var result)) {
            server = result;
            return true;
        }
        server = default;
        return false;
    }
}
