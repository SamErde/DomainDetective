using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DomainDetective.Helpers;

public static class DomainHelper {
    private static readonly IdnMapping _idn = new();
    private static readonly Regex _tldRegex = new(
        "^[A-Za-z](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?$",
        RegexOptions.Compiled);

    public static string ValidateIdn(string domain) {
        if (string.IsNullOrWhiteSpace(domain)) {
            throw new ArgumentNullException(nameof(domain));
        }

        try {
            var ascii = _idn.GetAscii(domain.Trim().Trim('.'));
            if (Uri.CheckHostName(ascii) != UriHostNameType.Dns) {
                throw new ArgumentException("Invalid domain name.", nameof(domain));
            }
            return ascii;
        } catch (ArgumentException e) {
            throw new ArgumentException("Invalid domain name.", nameof(domain), e);
        }
    }

    public static bool IsValidTld(string tld) =>
        _tldRegex.IsMatch(tld ?? string.Empty);
}