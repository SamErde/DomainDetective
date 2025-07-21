using System.Collections.Generic;
using System.Net.NetworkInformation;
using DomainDetective.Network;
using System.Threading;
using System.Threading.Tasks;

namespace DomainDetective {
    public partial class DomainHealthCheck {
        /// <summary>
        /// Sends an ICMP echo request to a host.
        /// </summary>
        public async Task<PingReply> VerifyPing(string host, int timeout = 4000, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            return await PingTraceroute.PingAsync(host, timeout, _logger);
        }

        /// <summary>
        /// Performs a traceroute to the specified host.
        /// </summary>
        public async Task<IReadOnlyList<PingTraceroute.TracerouteHop>> VerifyTraceroute(string host, int maxHops = 30, int timeout = 4000, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            return await PingTraceroute.TracerouteAsync(host, maxHops, timeout, _logger);
        }
    }
}
