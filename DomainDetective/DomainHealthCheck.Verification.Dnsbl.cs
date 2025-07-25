using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace DomainDetective {
    public partial class DomainHealthCheck {
        /// <summary>
        /// Checks domain MX hosts against configured DNS block lists.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyDNSBL(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            if (IsPublicSuffix) {
                return;
            }
            await DNSBLAnalysis.AnalyzeDNSBLRecordsMX(domainName, _logger);
        }

        /// <summary>
        /// Checks an IP address against configured DNS block lists.
        /// </summary>
        /// <param name="ipAddress">IP address to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckDNSBL(string ipAddress, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(ipAddress)) {
                throw new ArgumentNullException(nameof(ipAddress));
            }

            if (!IPAddress.TryParse(ipAddress, out _)) {
                throw new ArgumentException("Invalid IP address", nameof(ipAddress));
            }

            await foreach (var _ in DNSBLAnalysis.AnalyzeDNSBLRecords(ipAddress, _logger)) {
                cancellationToken.ThrowIfCancellationRequested();
                // enumeration triggers processing
            }
        }

        /// <summary>
        /// Checks multiple IP addresses against DNS block lists.
        /// </summary>
        /// <param name="ipAddresses">IPs to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckDNSBL(string[] ipAddresses, CancellationToken cancellationToken = default) {
            if (ipAddresses == null) {
                throw new ArgumentNullException(nameof(ipAddresses));
            }

            foreach (var ip in ipAddresses) {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IPAddress.TryParse(ip, out _)) {
                    throw new ArgumentException("Invalid IP address", nameof(ipAddresses));
                }

                await foreach (var _ in DNSBLAnalysis.AnalyzeDNSBLRecords(ip, _logger)) {
                    cancellationToken.ThrowIfCancellationRequested();
                    // enumeration triggers processing
                }
            }
        }
    }
}
