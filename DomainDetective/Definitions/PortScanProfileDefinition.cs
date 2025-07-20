using System;
using System.Collections.Generic;
namespace DomainDetective;

/// <summary>
/// Provides port lists for predefined scan profiles.
/// </summary>
public static class PortScanProfileDefinition
{
    /// <summary>Predefined service port profiles.</summary>
    public enum PortScanProfile
    {
        /// <summary>Standard scan of top ports.</summary>
        Default,
        /// <summary>Ports commonly used by SMB.</summary>
        SMB,
        /// <summary>Ports commonly used by NTP.</summary>
        NTP
    }

    private const string PortsCsv = "80,631,161,137,123,445,138,1434,135,53,139,67,23,443,21,22,500,1900,68,520,25,514,4500,111,49152,162,69,5353,49154,3389,110,1701,999,998,996,997,49153,3283,1723,1433,8888,53,2083,49155,161,445,7777,7,8080,443,9090,587,873,3306,5432,9091,1900,464,139,631,49156,123,81,589,554,500,49157,2,5222,113,664,69,27017,587,110,8000,995,88,8080,139,161,995,23,8008,389,2082,3306,11211,110,389,591,1025,543,22,1194,139,520,873,4379,8089,49158,3306,110,1521,3268,631,6001,69,53,901,5672,25,8009,54321,3283,3311,49159,123,5000,49160";

    private static readonly int[] _topPorts = Array.ConvertAll(PortsCsv.Split(','), int.Parse);

    private static readonly Dictionary<PortScanProfile, int[]> ProfilePorts = new()
    {
        [PortScanProfile.SMB] = new[] { 445, 139 },
        [PortScanProfile.NTP] = new[] { 123 },
        [PortScanProfile.Default] = _topPorts
    };

    /// <summary>Gets the ports for the given profile.</summary>
    public static IReadOnlyList<int> GetPorts(PortScanProfile profile) =>
        ProfilePorts.TryGetValue(profile, out var ports) ? ports : Array.Empty<int>();

    /// <summary>Overrides ports for a profile (tests only).</summary>
    internal static void OverrideProfilePorts(PortScanProfile profile, int[] ports) =>
        ProfilePorts[profile] = ports;

    /// <summary>Default port list.</summary>
    public static IReadOnlyList<int> DefaultPorts => _topPorts;
}
