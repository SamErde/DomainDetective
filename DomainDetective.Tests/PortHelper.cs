namespace DomainDetective.Tests;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

internal static class PortHelper {
    private static readonly object PortLock = new();
    private static readonly HashSet<int> UsedPorts = new();
    private static readonly Dictionary<int, Semaphore> Semaphores = new();

    public static int GetFreePort() {
        lock (PortLock) {
            while (true) {
                var listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();

                if (!UsedPorts.Add(port)) {
                    continue;
                }

                var semaphore = new Semaphore(1, 1, $"DomainDetective_{port}", out _);
                if (!semaphore.WaitOne(0)) {
                    semaphore.Dispose();
                    UsedPorts.Remove(port);
                    continue;
                }

                Semaphores[port] = semaphore;
                return port;
            }
        }
    }

    public static void ReleasePort(int port) {
        lock (PortLock) {
            if (Semaphores.TryGetValue(port, out var semaphore)) {
                semaphore.Release();
                semaphore.Dispose();
                Semaphores.Remove(port);
            }

            UsedPorts.Remove(port);
        }
    }
}