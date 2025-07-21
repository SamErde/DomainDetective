using DnsClientX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective {
    public partial class DomainHealthCheck {
        /// <summary>
        /// Queries DNS and analyzes DMARC records for a domain.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyDMARC(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            if (IsPublicSuffix) {
                return;
            }
            var dmarc = await DnsConfiguration.QueryDNS("_dmarc." + domainName, DnsRecordType.TXT, "DMARC1", cancellationToken);
            await DmarcAnalysis.AnalyzeDmarcRecords(dmarc, _logger, domainName, _publicSuffixList.GetRegistrableDomain);
            DmarcAnalysis.EvaluatePolicyStrength(UseSubdomainPolicy);
        }

        /// <summary>
        /// Analyzes a raw DMARC record.
        /// </summary>
        /// <param name="dmarcRecord">DMARC record text.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckDMARC(string dmarcRecord, CancellationToken cancellationToken = default) {
            await DmarcAnalysis.AnalyzeDmarcRecords(new List<DnsAnswer> {
                new DnsAnswer {
                    DataRaw = dmarcRecord,
                    Type = DnsRecordType.TXT
                }
            }, _logger);
            DmarcAnalysis.EvaluatePolicyStrength(UseSubdomainPolicy);
        }
    }
}
