using DnsClientX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective {
    public partial class DomainHealthCheck {

        /// <summary>
        /// Queries DNSSEC information for a domain.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyDNSSEC(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            if (IsPublicSuffix) {
                return;
            }
            DnsSecAnalysis = new DnsSecAnalysis();
            await DnsSecAnalysis.Analyze(domainName, _logger, DnsConfiguration);
        }



        /// <summary>
        /// Tests MX hosts for open relay configuration.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="port">SMTP port to test.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyOpenRelay(string domainName, int port = 25, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            if (IsPublicSuffix) {
                return;
            }
            ValidatePort(port);
            var mxRecords = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            IEnumerable<string> hosts = CertificateAnalysis.ExtractMxHosts(mxRecords);
            foreach (string host in hosts) {
                cancellationToken.ThrowIfCancellationRequested();
                await OpenRelayAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
            }
        }

        /// <summary>
        /// Tests name servers for open recursion.
        /// </summary>
        public async Task VerifyOpenResolver(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            if (IsPublicSuffix) {
                return;
            }
            var nsRecords = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.NS, cancellationToken: cancellationToken);
            foreach (var record in nsRecords) {
                var host = record.Data.Trim('.');
                cancellationToken.ThrowIfCancellationRequested();
                await OpenResolverAnalysis.AnalyzeServer(host, 53, _logger, cancellationToken);
            }
        }
    }
}
