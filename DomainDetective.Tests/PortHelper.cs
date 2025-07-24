namespace DomainDetective.Tests;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

internal sealed class PortReservation {
    public required Mutex Mutex { get; init; }
    public SynchronizationContext? Context { get; init; }
    public int ThreadId { get; init; }
}

internal static class PortHelper {
    private static readonly object PortLock = new();
    private static readonly HashSet<int> UsedPorts = new();
    private static readonly Dictionary<int, PortReservation> Reservations = new();

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

                Reservations[port] = new PortReservation {
                    Mutex = mutex,
                    Context = SynchronizationContext.Current,
                    ThreadId = Environment.CurrentManagedThreadId
                };
                return port;
            }
        }
    }

    public static void ReleasePort(int port) {
        lock (PortLock) {
            if (Reservations.TryGetValue(port, out var reservation)) {
                if (reservation.ThreadId == Environment.CurrentManagedThreadId) {
                    reservation.Mutex.ReleaseMutex();
                } else if (reservation.Context != null && reservation.Context != SynchronizationContext.Current) {
                    reservation.Context.Send(_ => reservation.Mutex.ReleaseMutex(), null);
                } else {
                    reservation.Mutex.ReleaseMutex();
                }

                reservation.Mutex.Dispose();
                Reservations.Remove(port);
            }

            UsedPorts.Remove(port);
        }
    }
}