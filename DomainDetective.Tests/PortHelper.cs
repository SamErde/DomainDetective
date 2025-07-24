namespace DomainDetective.Tests;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

internal static class PortHelper {
    private static readonly object PortLock = new();
    private static readonly HashSet<int> UsedPorts = new();
    private static readonly Dictionary<int, Mutex> Mutexes = new();

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

                var mutex = new Mutex(false, $"DomainDetective_{port}");
                if (!mutex.WaitOne(0)) {
                    mutex.Dispose();
                    UsedPorts.Remove(port);
                    continue;
                }

                Mutexes[port] = mutex;
                return port;
            }
        }
    }

    public static void ReleasePort(int port) {
        lock (PortLock) {
            if (Mutexes.TryGetValue(port, out var mutex)) {
                mutex.ReleaseMutex();
                mutex.Dispose();
                Mutexes.Remove(port);
            }

            UsedPorts.Remove(port);
        }
    }
}