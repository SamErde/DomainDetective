namespace DomainDetective.Scanning;

/// <summary>Lightweight scan logger interface; CLI implements typewriter output.</summary>
public interface IScanLogger
{
    /// <summary>Enqueue a line for UI/log consumption. Accepts Spectre markup in CLI.</summary>
    void Enqueue(string line);
}

