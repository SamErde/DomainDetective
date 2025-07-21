using DnsClientX;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective {
    public partial class DomainHealthCheck {
        /// <summary>
        /// Checks all MX hosts for STARTTLS support.
        /// </summary>
        public async Task VerifySTARTTLS(string domainName, int port = 25, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            ValidatePort(port);
            var mxRecordsForTls = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            var tlsHosts = CertificateAnalysis.ExtractMxHosts(mxRecordsForTls);
            await StartTlsAnalysis.AnalyzeServers(tlsHosts, new[] { port }, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks all MX hosts for SMTP TLS configuration.
        /// </summary>
        public async Task VerifySMTPTLS(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            var mxRecordsForTls = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            var tlsHosts = CertificateAnalysis.ExtractMxHosts(mxRecordsForTls);
            await SmtpTlsAnalysis.AnalyzeServers(tlsHosts, 25, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks all MX hosts for IMAP TLS configuration.
        /// </summary>
        public async Task VerifyIMAPTLS(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            var mxRecordsForTls = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            var tlsHosts = CertificateAnalysis.ExtractMxHosts(mxRecordsForTls);
            await ImapTlsAnalysis.AnalyzeServers(tlsHosts, 143, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks all MX hosts for POP3 TLS configuration.
        /// </summary>
        public async Task VerifyPOP3TLS(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            var mxRecordsForTls = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            var tlsHosts = CertificateAnalysis.ExtractMxHosts(mxRecordsForTls);
            await Pop3TlsAnalysis.AnalyzeServers(tlsHosts, 110, _logger, cancellationToken);
        }

        /// <summary>
        /// Collects SMTP banners from all MX hosts.
        /// </summary>
        public async Task VerifySMTPBanner(string domainName, int port = 25, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            ValidatePort(port);
            var mx = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            var hosts = CertificateAnalysis.ExtractMxHosts(mx);
            await SmtpBannerAnalysis.AnalyzeServers(hosts, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Retrieves SMTP AUTH capabilities from all MX hosts.
        /// </summary>
        public async Task VerifySmtpAuth(string domainName, int port = 25, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            ValidatePort(port);
            var mx = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            var hosts = CertificateAnalysis.ExtractMxHosts(mx);
            await SmtpAuthAnalysis.AnalyzeServers(hosts, port, _logger, cancellationToken);
        }
    }
}
