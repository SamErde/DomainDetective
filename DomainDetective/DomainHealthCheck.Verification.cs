using DnsClientX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;
using System.Linq.Expressions;
using System.Globalization;
using DomainDetective.Network;
using DomainDetective.Helpers;

using PortScanProfile = DomainDetective.PortScanProfileDefinition.PortScanProfile;
namespace DomainDetective {
    /// <summary>
    /// Contains verification methods used by <see cref="DomainHealthCheck"/>.
    /// </summary>
    /// <para>Part of the DomainDetective project.</para>
    public partial class DomainHealthCheck {

        private static string NormalizeDomain(string input)
        {
            return DomainHelper.ValidateIdn(input).ToLowerInvariant();
        }

        private static string CreateServiceQuery(int port, string domain) {
#if NET6_0_OR_GREATER
            var portString = port.ToString(CultureInfo.InvariantCulture);
            return string.Create(portString.Length + domain.Length + 7, (portString, domain), static (span, state) => {
                var (digits, host) = state;
                var pos = 0;
                span[pos++] = '_';
                digits.AsSpan().CopyTo(span[pos..]);
                pos += digits.Length;
                "._tcp.".AsSpan().CopyTo(span[pos..]);
                pos += 6;
                host.AsSpan().CopyTo(span[pos..]);
            });
#else
            return $"_{port}._tcp.{domain}";
#endif
        }

        private static void ValidateServiceQueryProtocol(string query) {
            bool hasTcp = query.IndexOf("._tcp.", StringComparison.OrdinalIgnoreCase) >= 0;
            bool hasUdp = query.IndexOf("._udp.", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!hasTcp && !hasUdp) {
                throw new InvalidOperationException($"Invalid service query '{query}', expected _tcp or _udp suffix.");
            }
        }

        private void UpdateIsPublicSuffix(string domainName) {
            string host = domainName;
            if (Uri.TryCreate($"http://{domainName}", UriKind.Absolute, out var uri)) {
                host = uri.Host;
            } else {
                try {
                    host = DomainHelper.ValidateIdn(domainName);
                } catch (ArgumentException) {
                }
            }

            var ascii = NormalizeDomain(host);
            IsPublicSuffix = _publicSuffixList.IsPublicSuffix(ascii);
        }

        /// <summary>
        /// Runs the requested health checks against a domain.
        /// </summary>
        /// <param name="domainName">Domain to validate.</param>
        /// <param name="healthCheckTypes">Health checks to execute or <c>null</c> for defaults.</param>
        /// <param name="dkimSelectors">DKIM selectors to use when verifying DKIM.</param>
        /// <param name="daneServiceType">DANE service types to inspect. When <c>null</c>, SMTP and HTTPS (port 443) are queried.</param>
        /// <param name="danePorts">Custom ports to check for DANE. Overrides <paramref name="daneServiceType"/> when provided.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task Verify(string domainName, HealthCheckType[] healthCheckTypes = null, string[] dkimSelectors = null, ServiceType[] daneServiceType = null, int[] danePorts = null, PortScanProfile[] portScanProfiles = null, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            IsPublicSuffix = false;
            domainName = ValidateHostName(domainName);
            UpdateIsPublicSuffix(domainName);
            if (healthCheckTypes == null || healthCheckTypes.Length == 0) {
                healthCheckTypes = new[]                {
                    HealthCheckType.DMARC,
                    HealthCheckType.SPF,
                    HealthCheckType.DKIM,
                    HealthCheckType.MX,
                    HealthCheckType.CAA,
                    HealthCheckType.DANE,
                    HealthCheckType.DNSSEC,
                    HealthCheckType.DNSBL,
                    HealthCheckType.MESSAGEHEADER
                };
            }

            healthCheckTypes = healthCheckTypes.Distinct().ToArray();

            var totalChecks = healthCheckTypes.Length;
            var processedChecks = 0;

            var actions = new Dictionary<HealthCheckType, Func<Task>> {
                [HealthCheckType.DMARC] = () => VerifyDMARC(domainName, cancellationToken),
                [HealthCheckType.SPF] = () => VerifySPF(domainName, cancellationToken),
                [HealthCheckType.DKIM] = () => VerifyDKIM(domainName, dkimSelectors ?? Definitions.DKIMSelectors.GuessSelectors().ToArray(), cancellationToken),
                [HealthCheckType.MX] = () => VerifyMX(domainName, cancellationToken),
                [HealthCheckType.REVERSEDNS] = () => VerifyReverseDnsAsync(domainName, cancellationToken),
                [HealthCheckType.FCRDNS] = () => VerifyFcrDnsAsync(domainName, cancellationToken),
                [HealthCheckType.CAA] = () => VerifyCAA(domainName, cancellationToken),
                [HealthCheckType.NS] = () => VerifyNS(domainName, cancellationToken),
                [HealthCheckType.DELEGATION] = () => VerifyDelegation(domainName, cancellationToken),
                [HealthCheckType.ZONETRANSFER] = () => VerifyZoneTransfer(domainName, cancellationToken),
                [HealthCheckType.DANE] = () => VerifyDaneAsync(domainName, daneServiceType, danePorts, cancellationToken),
                [HealthCheckType.DNSSEC] = () => VerifyDNSSEC(domainName, cancellationToken),
                [HealthCheckType.DNSBL] = () => VerifyDNSBL(domainName, cancellationToken),
                [HealthCheckType.MTASTS] = () => VerifyMTASTS(domainName, cancellationToken),
                [HealthCheckType.TLSRPT] = () => VerifyTLSRPT(domainName, cancellationToken),
                [HealthCheckType.BIMI] = () => VerifyBIMI(domainName, cancellationToken),
                [HealthCheckType.AUTODISCOVER] = () => VerifyAutodiscover(domainName, cancellationToken),
                [HealthCheckType.CERT] = () => VerifyWebsiteCertificate(domainName, cancellationToken: cancellationToken),
                [HealthCheckType.SECURITYTXT] = () => VerifySecurityTxtAsync(domainName, cancellationToken),
                [HealthCheckType.SOA] = () => VerifySOA(domainName, cancellationToken),
                [HealthCheckType.OPENRELAY] = () => VerifyOpenRelay(domainName, 25, cancellationToken),
                [HealthCheckType.OPENRESOLVER] = () => VerifyOpenResolver(domainName, cancellationToken),
                [HealthCheckType.STARTTLS] = () => VerifySTARTTLS(domainName, 25, cancellationToken),
                [HealthCheckType.SMTPTLS] = () => VerifySMTPTLS(domainName, cancellationToken),
                [HealthCheckType.IMAPTLS] = () => VerifyIMAPTLS(domainName, cancellationToken),
                [HealthCheckType.POP3TLS] = () => VerifyPOP3TLS(domainName, cancellationToken),
                [HealthCheckType.SMTPBANNER] = () => VerifySMTPBanner(domainName, 25, cancellationToken),
                [HealthCheckType.SMTPAUTH] = () => VerifySmtpAuth(domainName, 25, cancellationToken),
                [HealthCheckType.HTTP] = () => VerifyPlainHttp(domainName, cancellationToken),
                [HealthCheckType.HPKP] = () => VerifyHpkpAsync(domainName, cancellationToken),
                [HealthCheckType.CONTACT] = () => VerifyContactInfo(domainName, cancellationToken),
                [HealthCheckType.MESSAGEHEADER] = () => VerifyMessageHeaderAsync(cancellationToken),
                [HealthCheckType.DANGLINGCNAME] = () => VerifyDanglingCname(domainName, cancellationToken),
                [HealthCheckType.TTL] = () => VerifyDnsTtlAsync(domainName, cancellationToken),
                [HealthCheckType.PORTAVAILABILITY] = () => CheckPortAvailability(domainName, null, cancellationToken),
                [HealthCheckType.PORTSCAN] = () => ScanPorts(domainName, null, portScanProfiles, cancellationToken),
                [HealthCheckType.SNMP] = () => CheckSnmpHost(domainName, 161, cancellationToken),
                [HealthCheckType.IPNEIGHBOR] = () => CheckIPNeighbors(domainName, cancellationToken),
                [HealthCheckType.RPKI] = () => VerifyRPKI(domainName, cancellationToken),
                [HealthCheckType.DNSTUNNELING] = () => CheckDnsTunnelingAsync(domainName, cancellationToken),
                [HealthCheckType.TYPOSQUATTING] = () => VerifyTyposquatting(domainName, cancellationToken),
                [HealthCheckType.WILDCARDDNS] = () => VerifyWildcardDns(domainName),
                [HealthCheckType.EDNSSUPPORT] = () => VerifyEdnsSupport(domainName, cancellationToken),
                [HealthCheckType.FLATTENINGSERVICE] = () => VerifyFlatteningServiceAsync(domainName, cancellationToken),
                [HealthCheckType.THREATINTEL] = () => VerifyThreatIntel(domainName, cancellationToken),
                [HealthCheckType.THREATFEED] = () => VerifyThreatFeed(domainName, cancellationToken),
                [HealthCheckType.DIRECTORYEXPOSURE] = () => VerifyDirectoryExposure(domainName, cancellationToken)
            };

            foreach (var healthCheckType in healthCheckTypes) {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.WriteProgress(
                    "HealthCheck",
                    healthCheckType.ToString(),
                    processedChecks * 100d / totalChecks,
                    processedChecks,
                    totalChecks);
                if (actions.TryGetValue(healthCheckType, out var action)) {
                    await action();
                } else {
                    _logger.WriteError("Unknown health check type: {0}", healthCheckType);
                    throw new NotSupportedException("Health check type not implemented.");
                }

                processedChecks++;
                _logger.WriteInformation("{0} check completed", healthCheckType);
                _logger.WriteProgress(
                    "HealthCheck",
                    healthCheckType.ToString(),
                    processedChecks * 100d / totalChecks,
                    processedChecks,
                    totalChecks);
            }
        }
        /// <summary>
        /// Analyzes a raw SMIMEA record.
        /// </summary>
        /// <param name="smimeaRecord">SMIMEA record text.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckSMIMEA(string smimeaRecord, CancellationToken cancellationToken = default) {
            await SmimeaAnalysis.AnalyzeSMIMEARecords(new List<DnsAnswer> {
                new DnsAnswer {
                    DataRaw = smimeaRecord
                }
            }, _logger);
        }


        /// <summary>
        /// Tests an SMTP server for open relay configuration.
        /// </summary>
        /// <param name="host">Target host name.</param>
        /// <param name="port">Port to connect to. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckOpenRelayHost(string host, int port = 25, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            await OpenRelayAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks if a DNS server allows recursive queries.
        /// </summary>
        /// <param name="host">Target server host name or IP.</param>
        /// <param name="port">DNS port number. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckOpenResolverHost(string host, int port = 53, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            await OpenResolverAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks a host for STARTTLS support.
        /// </summary>
        /// <param name="host">Target host name.</param>
        /// <param name="port">Port to connect to. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckStartTlsHost(string host, int port = 25, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            await StartTlsAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks a host for SMTP TLS capabilities.
        /// </summary>
        /// <param name="host">Target host name.</param>
        /// <param name="port">Port to connect to. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckSmtpTlsHost(string host, int port = 25, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            await SmtpTlsAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks a host for IMAP TLS capabilities.
        /// </summary>
        /// <param name="host">Target host name.</param>
        /// <param name="port">Port to connect to. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckImapTlsHost(string host, int port = 143, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            await ImapTlsAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks a host for POP3 TLS capabilities.
        /// </summary>
        /// <param name="host">Target host name.</param>
        /// <param name="port">Port to connect to. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckPop3TlsHost(string host, int port = 110, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            await Pop3TlsAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Retrieves the SMTP banner from a host.
        /// </summary>
        /// <param name="host">Target host name.</param>
        /// <param name="port">Port to connect to. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckSmtpBannerHost(string host, int port = 25, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            await SmtpBannerAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Measures mail server connection and banner latency.
        /// </summary>
        /// <param name="host">Target host name.</param>
        /// <param name="port">Port to connect to. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckMailLatency(string host, int port = 25, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            await MailLatencyAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Tests connectivity to common service ports on a host.
        /// </summary>
        /// <param name="host">Target host name.</param>
        /// <param name="ports">Ports to check. Defaults to common services.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckPortAvailability(string host, IEnumerable<int>? ports = null, CancellationToken cancellationToken = default) {
            var list = ports?.ToArray() ?? new[] { 25, 80, 443, 465, 587 };
            foreach (var p in list) {
                ValidatePort(p);
            }
            await PortAvailabilityAnalysis.AnalyzeServers(new[] { host }, list, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks a host for SNMP responses.
        /// </summary>
        /// <param name="host">Target host name.</param>
        /// <param name="port">SNMP port to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckSnmpHost(string host, int port = 161, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            await SnmpAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Scans a host for open TCP and UDP ports.
        /// </summary>
        /// <param name="host">Target host name.</param>
        /// <param name="ports">Ports to scan. Defaults to the top 1000 ports.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task ScanPorts(string host, IEnumerable<int>? ports = null, PortScanProfile[]? profiles = null, CancellationToken cancellationToken = default, bool showProgress = true) {
            IEnumerable<int> selected;
            if (ports != null && ports.Any()) {
                selected = ports;
            } else if (profiles != null && profiles.Length > 0) {
                selected = profiles.SelectMany(PortScanProfileDefinition.GetPorts).Distinct();
            } else {
                selected = PortScanProfileDefinition.DefaultPorts;
            }
            var list = selected.ToArray();
            foreach (var p in list) {
                ValidatePort(p);
            }
            await PortScanAnalysis.Scan(host, list, _logger, cancellationToken, showProgress);
        }

        /// <summary>Queries neighbors sharing the same IP as <paramref name="domainName"/>.</summary>
        public async Task CheckIPNeighbors(string domainName, CancellationToken cancellationToken = default) {
            await IPNeighborAnalysis.Analyze(domainName, _logger, cancellationToken);
        }

        /// <summary>Analyzes DNS logs for tunneling patterns.</summary>
        public void CheckDnsTunneling(string domainName, CancellationToken ct = default) {
            CheckDnsTunnelingAsync(domainName, ct).GetAwaiter().GetResult();
        }

        public async Task CheckDnsTunnelingAsync(string domainName, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();
            var lines = DnsTunnelingLogs ?? Array.Empty<string>();
            await Task.Run(() => DnsTunnelingAnalysis.Analyze(domainName, lines), ct);
        }

        /// <summary>Queries an NTP server for clock information.</summary>
        /// <param name="host">Target server host name or IP.</param>
        /// <param name="port">NTP port number.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task TestNtpServer(string host, int port = 123, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            await NtpAnalysis.AnalyzeServer(host, port, _logger, cancellationToken);
        }

        /// <summary>Queries a predefined NTP server.</summary>
        /// <param name="server">Built-in server enumeration.</param>
        /// <param name="port">NTP port number.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public Task TestNtpServer(NtpServer server, int port = 123, CancellationToken cancellationToken = default) =>
            TestNtpServer(server.ToHost(), port, cancellationToken);


        /// <summary>
        /// Analyzes a raw BIMI record.
        /// </summary>
        /// <param name="bimiRecord">BIMI record text.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckBIMI(string bimiRecord, CancellationToken cancellationToken = default) {
            await BimiAnalysis.AnalyzeBimiRecords(new List<DnsAnswer> {
                new DnsAnswer {
                    DataRaw = bimiRecord,
                    Type = DnsRecordType.TXT
                }
            }, _logger, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Analyzes a raw contact TXT record.
        /// </summary>
        /// <param name="contactRecord">Contact record text.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckContactInfo(string contactRecord, CancellationToken cancellationToken = default) {
            await ContactInfoAnalysis.AnalyzeContactRecords(new List<DnsAnswer> {
                new DnsAnswer {
                    DataRaw = contactRecord,
                    Type = DnsRecordType.TXT
                }
            }, _logger);
        }

        /// <summary>
        /// Queries random subdomains to detect wildcard DNS behavior.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="sampleCount">Number of names to test.</param>
        public async Task VerifyWildcardDns(string domainName, int sampleCount = 3) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            WildcardDnsAnalysis.DnsConfiguration = DnsConfiguration;
            await WildcardDnsAnalysis.Analyze(domainName, _logger, sampleCount);
        }

        /// <summary>
        /// Parses raw message headers.
        /// </summary>
        /// <param name="rawHeaders">Unparsed header text.</param>
        /// <param name="ct">Token to cancel the operation.</param>
        /// <returns>Populated <see cref="MessageHeaderAnalysis"/> instance.</returns>
        public MessageHeaderAnalysis CheckMessageHeaders(string rawHeaders, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();

            var analysis = new MessageHeaderAnalysis();
            analysis.Parse(rawHeaders, _logger);
            return analysis;
        }

        /// <summary>
        /// Validates ARC headers contained in <paramref name="rawHeaders"/>.
        /// </summary>
        /// <param name="rawHeaders">Raw message headers.</param>
        /// <param name="ct">Token to cancel the operation.</param>
        /// <returns>Populated <see cref="ARCAnalysis"/> instance.</returns>
        public ARCAnalysis VerifyARC(string rawHeaders, CancellationToken ct = default) {
            return VerifyARCAsync(rawHeaders, ct).GetAwaiter().GetResult();
        }

        public async Task<ARCAnalysis> VerifyARCAsync(string rawHeaders, CancellationToken ct = default) {
            ct.ThrowIfCancellationRequested();
            return await Task.Run(() => {
                ArcAnalysis = new ARCAnalysis();
                ArcAnalysis.Analyze(rawHeaders, _logger);
                return ArcAnalysis;
            }, ct);
        }


        /// <summary>
        /// Verifies MTA-STS policy for a domain.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyMTASTS(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            MTASTSAnalysis = new MTASTSAnalysis {
                PolicyUrlOverride = MtaStsPolicyUrlOverride,
                DnsConfiguration = DnsConfiguration
            };
            await MTASTSAnalysis.AnalyzePolicy(domainName, _logger);
        }

        /// <summary>
        /// Checks all MX hosts for STARTTLS support.
        /// </summary>
        /// <param name="domainName">Domain whose MX records are queried.</param>
        /// <param name="port">SMTP port to connect to. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifySTARTTLS(string domainName, int port = 25, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            ValidatePort(port);
            var mxRecordsForTls = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            IEnumerable<string> tlsHosts = CertificateAnalysis.ExtractMxHosts(mxRecordsForTls);
            await StartTlsAnalysis.AnalyzeServers(tlsHosts, new[] { port }, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks all MX hosts for SMTP TLS configuration.
        /// </summary>
        /// <param name="domainName">Domain whose MX records are queried.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifySMTPTLS(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            var mxRecordsForTls = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            IEnumerable<string> tlsHosts = CertificateAnalysis.ExtractMxHosts(mxRecordsForTls);
            await SmtpTlsAnalysis.AnalyzeServers(tlsHosts, 25, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks all MX hosts for IMAP TLS configuration.
        /// </summary>
        /// <param name="domainName">Domain whose MX records are queried.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyIMAPTLS(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            var mxRecordsForTls = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            IEnumerable<string> tlsHosts = CertificateAnalysis.ExtractMxHosts(mxRecordsForTls);
            await ImapTlsAnalysis.AnalyzeServers(tlsHosts, 143, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks all MX hosts for POP3 TLS configuration.
        /// </summary>
        /// <param name="domainName">Domain whose MX records are queried.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyPOP3TLS(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            var mxRecordsForTls = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            IEnumerable<string> tlsHosts = CertificateAnalysis.ExtractMxHosts(mxRecordsForTls);
            await Pop3TlsAnalysis.AnalyzeServers(tlsHosts, 110, _logger, cancellationToken);
        }

        /// <summary>
        /// Collects SMTP banners from all MX hosts.
        /// </summary>
        /// <param name="domainName">Domain whose MX records are queried.</param>
        /// <param name="port">SMTP port to connect to. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
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
        /// <param name="domainName">Domain whose MX records are queried.</param>
        /// <param name="port">SMTP port to connect to. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
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

        /// <summary>
        /// Queries and analyzes BIMI records for a domain.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyBIMI(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            BimiAnalysis = new BimiAnalysis();
            var bimi = await DnsConfiguration.QueryDNS($"default._bimi.{domainName}", DnsRecordType.TXT, cancellationToken: cancellationToken);
            await BimiAnalysis.AnalyzeBimiRecords(bimi, _logger, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Queries contact TXT records for a domain.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyContactInfo(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            ContactInfoAnalysis = new ContactInfoAnalysis();
            var contact = await DnsConfiguration.QueryDNS("contact." + domainName, DnsRecordType.TXT, cancellationToken: cancellationToken);
            await ContactInfoAnalysis.AnalyzeContactRecords(contact, _logger);
        }

        /// Attempts zone transfers against authoritative name servers.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyZoneTransfer(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            var nsRecords = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.NS, cancellationToken: cancellationToken);
            var servers = nsRecords.Select(r => r.Data.Trim('.'));
            await ZoneTransferAnalysis.AnalyzeServers(domainName, servers, _logger, cancellationToken);
        }

        /// <summary>
        /// Validates delegation information against the parent zone.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyDelegation(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = NormalizeDomain(domainName);
            UpdateIsPublicSuffix(domainName);
            var ns = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.NS, cancellationToken: cancellationToken);
            await NSAnalysis.AnalyzeNsRecords(ns, _logger);
            await NSAnalysis.AnalyzeParentDelegation(domainName, _logger);
        }

        /// <summary>
        /// Detects dangling CNAME records for the domain.
        /// </summary>
        public async Task VerifyDanglingCname(string domainName, CancellationToken cancellationToken = default) {
            domainName = NormalizeDomain(domainName);
            DanglingCnameAnalysis = new DanglingCnameAnalysis { DnsConfiguration = DnsConfiguration };
            await DanglingCnameAnalysis.Analyze(domainName, _logger, cancellationToken);
        }

        /// <summary>
        /// Checks for CNAMEs pointing to takeover prone providers.
        /// </summary>
        public async Task VerifyTakeoverCname(string domainName, CancellationToken cancellationToken = default) {
            domainName = NormalizeDomain(domainName);
            TakeoverCnameAnalysis = new TakeoverCnameAnalysis { DnsConfiguration = DnsConfiguration };
            await TakeoverCnameAnalysis.Analyze(domainName, _logger, cancellationToken);
        }

        /// <summary>
        /// Scans common directories for public exposure.
        /// </summary>
        public async Task VerifyDirectoryExposure(string domainName, CancellationToken cancellationToken = default) {
            domainName = ValidateHostName(domainName);
            UpdateIsPublicSuffix(domainName);
            DirectoryExposureAnalysis = new DirectoryExposureAnalysis();
            await DirectoryExposureAnalysis.Analyze($"http://{domainName}", _logger, cancellationToken);
        }

        /// Queries Autodiscover related records for a domain.
        /// </summary>
        /// <param name="domainName">Domain to verify.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyAutodiscover(string domainName, CancellationToken cancellationToken = default) {
            domainName = NormalizeDomain(domainName);
            AutodiscoverAnalysis = new AutodiscoverAnalysis();
            await AutodiscoverAnalysis.Analyze(domainName, DnsConfiguration, _logger, cancellationToken);
        }

        private async Task VerifyReverseDnsAsync(string domainName, CancellationToken cancellationToken) {
            var mxRecords = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            var rdnsHosts = CertificateAnalysis.ExtractMxHosts(mxRecords);
            await ReverseDnsAnalysis.AnalyzeHosts(rdnsHosts, _logger);
        }

        private async Task VerifyFcrDnsAsync(string domainName, CancellationToken cancellationToken) {
            var mxRecords = await DnsConfiguration.QueryDNS(domainName, DnsRecordType.MX, cancellationToken: cancellationToken);
            var rdnsHosts = CertificateAnalysis.ExtractMxHosts(mxRecords);
            await ReverseDnsAnalysis.AnalyzeHosts(rdnsHosts, _logger);
            await FcrDnsAnalysis.Analyze(ReverseDnsAnalysis.Results, _logger);
        }

        private async Task VerifyDaneAsync(string domainName, ServiceType[]? serviceTypes, int[]? ports, CancellationToken cancellationToken) {
            if (ports != null && ports.Length > 0) {
                await VerifyDANE(domainName, ports, cancellationToken);
            } else {
                await VerifyDANE(domainName, serviceTypes, cancellationToken);
            }
        }

        private async Task VerifySecurityTxtAsync(string domainName, CancellationToken cancellationToken) {
            SecurityTXTAnalysis = new SecurityTXTAnalysis();
            await SecurityTXTAnalysis.AnalyzeSecurityTxtRecord(domainName, _logger);
        }

        private Task VerifyHpkpAsync(string domainName, CancellationToken cancellationToken) {
            return HPKPAnalysis.AnalyzeUrl($"http://{domainName}", _logger);
        }

        private Task VerifyMessageHeaderAsync(CancellationToken cancellationToken) {
            MessageHeaderAnalysis = CheckMessageHeaders(string.Empty, cancellationToken);
            return Task.CompletedTask;
        }

        private Task VerifyDnsTtlAsync(string domainName, CancellationToken cancellationToken) {
            return DnsTtlAnalysis.Analyze(domainName, _logger);
        }

        private Task VerifyFlatteningServiceAsync(string domainName, CancellationToken cancellationToken) {
            FlatteningServiceAnalysis = new FlatteningServiceAnalysis { DnsConfiguration = DnsConfiguration };
            return FlatteningServiceAnalysis.Analyze(domainName, _logger, cancellationToken);
        }

        private async Task<DnsAnswer[]> QueryDaneDns(string name, CancellationToken cancellationToken) {
            if (DaneAnalysis.QueryDnsOverride != null) {
                return await DaneAnalysis.QueryDnsOverride(name, DnsRecordType.TLSA);
            }

            return await DnsConfiguration.QueryDNS(name, DnsRecordType.TLSA, cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Queries SMIMEA records for an email address.
        /// <param name="emailAddress">Email address to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifySMIMEA(string emailAddress, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(emailAddress)) {
                throw new ArgumentNullException(nameof(emailAddress));
            }

            var name = SMIMEAAnalysis.GetQueryName(emailAddress);
            SmimeaAnalysis = new SMIMEAAnalysis();
            var records = await DnsConfiguration.QueryDNS(name, DnsRecordType.SMIMEA, cancellationToken: cancellationToken);
            if (records.Any()) {
                await SmimeaAnalysis.AnalyzeSMIMEARecords(records, _logger);
            } else {
                _logger.WriteWarning("No SMIMEA records found.");
            }
        }

        /// <summary>
        /// Verifies the certificate for a website. If no scheme is provided in <paramref name="url"/>, "https://" is assumed.
        /// </summary>
        /// <param name="url">Website address. If missing a scheme, "https://" will be prepended.</param>
        /// <param name="port">Port to use for the connection. Must be between 1 and 65535.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        public async Task VerifyWebsiteCertificate(string url, int port = 443, CancellationToken cancellationToken = default) {
            ValidatePort(port);
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                url = $"https://{url}";
            }
            await CertificateAnalysis.AnalyzeUrl(url, port, _logger, cancellationToken);
        }

        /// <summary>
        /// Performs a basic HTTP check without enforcing HTTPS.
        /// </summary>
        /// <param name="domainName">Domain or host to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task VerifyPlainHttp(string domainName, CancellationToken cancellationToken = default) {
            if (string.IsNullOrWhiteSpace(domainName)) {
                throw new ArgumentNullException(nameof(domainName));
            }
            domainName = ValidateHostName(domainName);
            UpdateIsPublicSuffix(domainName);
            await HttpAnalysis.AnalyzeUrl($"http://{domainName}", false, _logger, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sends an ICMP echo request to a host.
        /// </summary>
        /// <param name="host">Target host name or address.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task<PingReply> VerifyPing(string host, int timeout = 4000, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            return await PingTraceroute.PingAsync(host, timeout, _logger);
        }

        /// <summary>
        /// Performs a traceroute to the specified host.
        /// </summary>
        /// <param name="host">Target host name or address.</param>
        /// <param name="maxHops">Maximum number of hops to probe.</param>
        /// <param name="timeout">Timeout per hop in milliseconds.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task<IReadOnlyList<PingTraceroute.TracerouteHop>> VerifyTraceroute(string host, int maxHops = 30, int timeout = 4000, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            return await PingTraceroute.TracerouteAsync(host, maxHops, timeout, _logger);
        }

        /// <summary>
        /// Checks an IP address against configured DNS block lists.
        /// </summary>
        /// <param name="ipAddress">IP address to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckDNSBL(string ipAddress, CancellationToken cancellationToken = default) {
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
            foreach (var ip in ipAddresses) {
                cancellationToken.ThrowIfCancellationRequested();
                await foreach (var _ in DNSBLAnalysis.AnalyzeDNSBLRecords(ip, _logger)) {
                    cancellationToken.ThrowIfCancellationRequested();
                    // enumeration triggers processing
                }
            }
        }

        /// <summary>
        /// Queries WHOIS information and IANA RDAP for a domain.
        /// </summary>
        /// <param name="domain">Domain name to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task CheckWHOIS(string domain, CancellationToken cancellationToken = default) {
            var timeout = WhoisAnalysis.Timeout;
            WhoisAnalysis = new WhoisAnalysis { Timeout = timeout };
            domain = NormalizeDomain(domain);
            UpdateIsPublicSuffix(domain);
            await WhoisAnalysis.QueryWhoisServer(domain, cancellationToken);
            await WhoisAnalysis.QueryIana(domain, cancellationToken);
        }

        /// <summary>
        /// Queries RDAP information for a domain.
        /// </summary>
        /// <param name="domain">Domain name to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public async Task QueryRDAP(string domain, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(domain)) {
                throw new ArgumentNullException(nameof(domain));
            }

            domain = NormalizeDomain(domain);
            UpdateIsPublicSuffix(domain);
            await RdapAnalysis.Analyze(domain, _logger, cancellationToken);
        }

        /// <summary>
        /// Creates a high level summary of key analyses.
        /// </summary>
        /// <returns>A populated <see cref="DomainSummary"/>.</returns>
        public DomainSummary BuildSummary() {
            var spfValid = SpfAnalysis.SpfRecordExists && SpfAnalysis.StartsCorrectly &&
                            !SpfAnalysis.ExceedsDnsLookups && !SpfAnalysis.MultipleSpfRecords;

            var dmarcValid = DmarcAnalysis.DmarcRecordExists && DmarcAnalysis.StartsCorrectly &&
                             DmarcAnalysis.HasMandatoryTags && DmarcAnalysis.IsPolicyValid &&
                             DmarcAnalysis.IsPctValid && !DmarcAnalysis.MultipleRecords &&
                             !DmarcAnalysis.ExceedsCharacterLimit && DmarcAnalysis.ValidDkimAlignment &&
                             DmarcAnalysis.ValidSpfAlignment;

            var dkimValid = DKIMAnalysis.AnalysisResults.Values.Any(a =>
                a.DkimRecordExists && a.StartsCorrectly && a.PublicKeyExists &&
                a.ValidPublicKey && a.KeyTypeExists && a.ValidKeyType && a.ValidFlags);

            var hints = new List<string>();

            static void AddHint(List<string> list, HealthCheckType type) {
                var hint = CheckDescriptions.Get(type)?.Remediation;
                if (!string.IsNullOrWhiteSpace(hint)) {
                    list.Add(hint);
                }
            }

            if (!spfValid) {
                AddHint(hints, HealthCheckType.SPF);
            }
            if (!dmarcValid) {
                AddHint(hints, HealthCheckType.DMARC);
            }
            if (!dkimValid) {
                AddHint(hints, HealthCheckType.DKIM);
            }
            if (MXAnalysis is { MxRecordExists: false }) {
                AddHint(hints, HealthCheckType.MX);
            }
            if (!(DnsSecAnalysis?.ChainValid ?? false)) {
                AddHint(hints, HealthCheckType.DNSSEC);
            }
            if (WhoisAnalysis.IsExpired || WhoisAnalysis.ExpiresSoon) {
                hints.Add("Renew the domain registration.");
            }

            return new DomainSummary {
                HasSpfRecord = SpfAnalysis.SpfRecordExists,
                SpfValid = spfValid,
                HasDmarcRecord = DmarcAnalysis.DmarcRecordExists,
                DmarcPolicy = DmarcAnalysis.Policy,
                DmarcValid = dmarcValid,
                HasDkimRecord = DKIMAnalysis.AnalysisResults.Values.Any(a => a.DkimRecordExists),
                DkimValid = dkimValid,
                HasMxRecord = MXAnalysis?.MxRecordExists ?? false,
                DnsSecValid = DnsSecAnalysis?.ChainValid ?? false,
                IsPublicSuffix = IsPublicSuffix,
                ExpiryDate = WhoisAnalysis.ExpiryDate,
                ExpiresSoon = WhoisAnalysis.ExpiresSoon,
                IsExpired = WhoisAnalysis.IsExpired,
                RegistrarLocked = WhoisAnalysis.RegistrarLocked,
                PrivacyProtected = WhoisAnalysis.PrivacyProtected,
                Hints = hints.ToArray()
            };
        }

        /// <summary>Serializes this instance to a JSON string.</summary>
        /// <param name="options">
        /// <para>Optional serializer options. If not provided,</para>
        /// <para><see cref="JsonSerializerOptions.WriteIndented"/> is enabled.</para>
        /// </param>
        /// <returns>
        /// <para>A JSON representation of the current
        /// <see cref="DomainHealthCheck"/>.</para>
        /// </returns>
        public string ToJson(JsonSerializerOptions options = null) {
            options ??= JsonOptions;
            if (UnicodeOutput && options.Converters.All(c => c is not IdnStringConverter)) {
                var local = new JsonSerializerOptions(options);
                local.Converters.Add(new IdnStringConverter(true));
                return JsonSerializer.Serialize(this, local);
            }
            return JsonSerializer.Serialize(this, options);
        }

        private static void ValidatePort(int port) {
            if (port <= 0 || port > 65535) {
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");
            }
        }

        private static string ValidateHostName(string domainName) {
            var trimmed = domainName?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) {
                throw new ArgumentNullException(nameof(domainName));
            }

            if (!Uri.TryCreate($"http://{trimmed}", UriKind.Absolute, out var uri)) {
                // older frameworks may not handle IDN automatically
                var hostName = trimmed;
                var portIndex = trimmed.LastIndexOf(':');
                if (portIndex > 0 && trimmed.IndexOf(':') == portIndex &&
                    int.TryParse(trimmed.Substring(portIndex + 1), out _)) {
                    hostName = trimmed.Substring(0, portIndex);
                }

                try {
                    hostName = DomainHelper.ValidateIdn(hostName);
                } catch (ArgumentException) {
                    throw new ArgumentException("Invalid host name.", nameof(domainName));
                }

                var rebuilt = portIndex > 0 && trimmed.IndexOf(':') == portIndex
                    ? hostName + trimmed.Substring(portIndex)
                    : hostName;

                if (!Uri.TryCreate($"http://{rebuilt}", UriKind.Absolute, out uri)) {
                    throw new ArgumentException("Invalid host name.", nameof(domainName));
                }
            }

            if (!string.IsNullOrEmpty(uri.PathAndQuery) && uri.PathAndQuery != "/" ||
                !string.IsNullOrEmpty(uri.Fragment)) {
                throw new ArgumentException("Invalid host name.", nameof(domainName));
            }

            var host = uri.IdnHost;
            if (uri.HostNameType == UriHostNameType.Dns) {
                var labels = host.Split('.');
                if (labels.Length == 0 ||
                    !Helpers.DomainHelper.IsValidTld(labels[labels.Length - 1])) {
                    throw new ArgumentException(
                        "Invalid host name.",
                        nameof(domainName));
                }
            }

            if (!uri.IsDefaultPort) {
                if (uri.Port <= 0 || uri.Port > 65535) {
                    throw new ArgumentException("Invalid port.", nameof(domainName));
                }
                return $"{NormalizeDomain(host)}:{uri.Port}";
            }

            return NormalizeDomain(host);
        }

        /// <summary>Creates a copy with only the specified analyses included.</summary>
        /// <param name="healthCheckTypes">
        /// <para>Health checks that should remain in the returned
        /// <see cref="DomainHealthCheck"/>.</para>
        /// </param>
        /// <returns>
        /// <para>A clone of this object with non-selected analyses removed.</para>
        /// </returns>
        public DomainHealthCheck FilterAnalyses(IEnumerable<HealthCheckType> healthCheckTypes) {
            var active = healthCheckTypes != null
                ? new HashSet<HealthCheckType>(healthCheckTypes)
                : new HashSet<HealthCheckType>();

            var filtered = new DomainHealthCheck(DnsEndpoint, _logger) {
                DnsSelectionStrategy = DnsSelectionStrategy,
                DnsConfiguration = DnsConfiguration,
                MtaStsPolicyUrlOverride = MtaStsPolicyUrlOverride
            };

            filtered.DmarcAnalysis = active.Contains(HealthCheckType.DMARC) ? CloneAnalysis(DmarcAnalysis) : null;
            filtered.SpfAnalysis = active.Contains(HealthCheckType.SPF) ? CloneAnalysis(SpfAnalysis) : null;
            filtered.DKIMAnalysis = active.Contains(HealthCheckType.DKIM) ? CloneAnalysis(DKIMAnalysis) : null;
            filtered.MXAnalysis = active.Contains(HealthCheckType.MX) ? CloneAnalysis(MXAnalysis) : null;
            filtered.ReverseDnsAnalysis = active.Contains(HealthCheckType.REVERSEDNS) ? CloneAnalysis(ReverseDnsAnalysis) : null;
            filtered.FcrDnsAnalysis = active.Contains(HealthCheckType.FCRDNS) ? CloneAnalysis(FcrDnsAnalysis) : null;
            filtered.CAAAnalysis = active.Contains(HealthCheckType.CAA) ? CloneAnalysis(CAAAnalysis) : null;
            filtered.NSAnalysis =
                active.Contains(HealthCheckType.NS) || active.Contains(HealthCheckType.DELEGATION)
                    ? CloneAnalysis(NSAnalysis)
                    : null;
            filtered.ZoneTransferAnalysis = active.Contains(HealthCheckType.ZONETRANSFER) ? CloneAnalysis(ZoneTransferAnalysis) : null;
            filtered.DaneAnalysis = active.Contains(HealthCheckType.DANE) ? CloneAnalysis(DaneAnalysis) : null;
            filtered.DNSBLAnalysis = active.Contains(HealthCheckType.DNSBL) ? CloneAnalysis(DNSBLAnalysis) : null;
            filtered.DnsSecAnalysis = active.Contains(HealthCheckType.DNSSEC) ? CloneAnalysis(DnsSecAnalysis) : null;
            filtered.MTASTSAnalysis = active.Contains(HealthCheckType.MTASTS) ? CloneAnalysis(MTASTSAnalysis) : null;
            filtered.TLSRPTAnalysis = active.Contains(HealthCheckType.TLSRPT) ? CloneAnalysis(TLSRPTAnalysis) : null;
            filtered.BimiAnalysis = active.Contains(HealthCheckType.BIMI) ? CloneAnalysis(BimiAnalysis) : null;
            filtered.AutodiscoverAnalysis = active.Contains(HealthCheckType.AUTODISCOVER) ? CloneAnalysis(AutodiscoverAnalysis) : null;
            filtered.CertificateAnalysis = active.Contains(HealthCheckType.CERT) ? CloneAnalysis(CertificateAnalysis) : null;
            filtered.SecurityTXTAnalysis = active.Contains(HealthCheckType.SECURITYTXT) ? CloneAnalysis(SecurityTXTAnalysis) : null;
            filtered.SOAAnalysis = active.Contains(HealthCheckType.SOA) ? CloneAnalysis(SOAAnalysis) : null;
            filtered.OpenRelayAnalysis = active.Contains(HealthCheckType.OPENRELAY) ? CloneAnalysis(OpenRelayAnalysis) : null;
            filtered.OpenResolverAnalysis = active.Contains(HealthCheckType.OPENRESOLVER) ? CloneAnalysis(OpenResolverAnalysis) : null;
            filtered.StartTlsAnalysis = active.Contains(HealthCheckType.STARTTLS) ? CloneAnalysis(StartTlsAnalysis) : null;
            filtered.SmtpTlsAnalysis = active.Contains(HealthCheckType.SMTPTLS) ? CloneAnalysis(SmtpTlsAnalysis) : null;
            filtered.ImapTlsAnalysis = active.Contains(HealthCheckType.IMAPTLS) ? CloneAnalysis(ImapTlsAnalysis) : null;
            filtered.Pop3TlsAnalysis = active.Contains(HealthCheckType.POP3TLS) ? CloneAnalysis(Pop3TlsAnalysis) : null;
            filtered.SmtpBannerAnalysis = active.Contains(HealthCheckType.SMTPBANNER) ? CloneAnalysis(SmtpBannerAnalysis) : null;
            filtered.SmtpAuthAnalysis = active.Contains(HealthCheckType.SMTPAUTH) ? CloneAnalysis(SmtpAuthAnalysis) : null;
            filtered.HttpAnalysis = active.Contains(HealthCheckType.HTTP) ? CloneAnalysis(HttpAnalysis) : null;
            filtered.HPKPAnalysis = active.Contains(HealthCheckType.HPKP) ? CloneAnalysis(HPKPAnalysis) : null;
            filtered.ContactInfoAnalysis = active.Contains(HealthCheckType.CONTACT) ? CloneAnalysis(ContactInfoAnalysis) : null;
            filtered.MessageHeaderAnalysis = active.Contains(HealthCheckType.MESSAGEHEADER) ? CloneAnalysis(MessageHeaderAnalysis) : null;
            filtered.ArcAnalysis = active.Contains(HealthCheckType.ARC) ? CloneAnalysis(ArcAnalysis) : null;
            filtered.DanglingCnameAnalysis = active.Contains(HealthCheckType.DANGLINGCNAME) ? CloneAnalysis(DanglingCnameAnalysis) : null;
            filtered.DnsTtlAnalysis = active.Contains(HealthCheckType.TTL) ? CloneAnalysis(DnsTtlAnalysis) : null;
            filtered.PortAvailabilityAnalysis = active.Contains(HealthCheckType.PORTAVAILABILITY) ? CloneAnalysis(PortAvailabilityAnalysis) : null;
            filtered.PortScanAnalysis = active.Contains(HealthCheckType.PORTSCAN) ? CloneAnalysis(PortScanAnalysis) : null;
            filtered.SnmpAnalysis = active.Contains(HealthCheckType.SNMP) ? CloneAnalysis(SnmpAnalysis) : null;
            filtered.IPNeighborAnalysis = active.Contains(HealthCheckType.IPNEIGHBOR) ? CloneAnalysis(IPNeighborAnalysis) : null;
            filtered.DnsTunnelingAnalysis = active.Contains(HealthCheckType.DNSTUNNELING) ? CloneAnalysis(DnsTunnelingAnalysis) : null;
            filtered.TyposquattingAnalysis = active.Contains(HealthCheckType.TYPOSQUATTING) ? CloneAnalysis(TyposquattingAnalysis) : null;
            filtered.ThreatIntelAnalysis = active.Contains(HealthCheckType.THREATINTEL) ? CloneAnalysis(ThreatIntelAnalysis) : null;
            filtered.ThreatFeedAnalysis = active.Contains(HealthCheckType.THREATFEED) ? CloneAnalysis(ThreatFeedAnalysis) : null;
            filtered.WildcardDnsAnalysis = active.Contains(HealthCheckType.WILDCARDDNS) ? CloneAnalysis(WildcardDnsAnalysis) : null;
            filtered.EdnsSupportAnalysis = active.Contains(HealthCheckType.EDNSSUPPORT) ? CloneAnalysis(EdnsSupportAnalysis) : null;
            filtered.FlatteningServiceAnalysis = active.Contains(HealthCheckType.FLATTENINGSERVICE) ? CloneAnalysis(FlatteningServiceAnalysis) : null;
            filtered.DirectoryExposureAnalysis = active.Contains(HealthCheckType.DIRECTORYEXPOSURE) ? CloneAnalysis(DirectoryExposureAnalysis) : null;
            filtered.NtpAnalysis = active.Contains(HealthCheckType.NTP) ? CloneAnalysis(NtpAnalysis) : null;

            return filtered;
        }

        private static readonly MethodInfo _cloneMethod = typeof(object).GetMethod(
            "MemberwiseClone",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static class Cloner<T> where T : class {
            internal static readonly Func<T, T> Delegate = CreateDelegate();

            private static Func<T, T> CreateDelegate() {
                ParameterExpression param = Expression.Parameter(typeof(T), "source");
                UnaryExpression body = Expression.Convert(Expression.Call(param, _cloneMethod), typeof(T));
                return Expression.Lambda<Func<T, T>>(body, param).Compile();
            }
        }

        private static T CloneAnalysis<T>(T analysis) where T : class {
            return analysis == null ? null : Cloner<T>.Delegate(analysis);
        }
    }
}
