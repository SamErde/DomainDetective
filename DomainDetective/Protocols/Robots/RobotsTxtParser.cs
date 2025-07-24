namespace DomainDetective;

using System;

/// <summary>
/// Provides a simple robots.txt parser.
/// </summary>
public static class RobotsTxtParser
{
    /// <summary>Parses robots.txt content into a <see cref="RobotsFile"/>.</summary>
    public static RobotsFile Parse(string content)
    {
        var file = new RobotsFile();
        RobotsGroup? group = null;
        var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            int idx = line.IndexOf(':');
            if (idx <= 0)
            {
                continue;
            }

            var field = line.Substring(0, idx).Trim();
            var value = line.Substring(idx + 1).Trim();

            switch (field.ToLowerInvariant())
            {
                case "user-agent":
                    if (group == null || group.Directives.Count > 0 || group.UserAgents.Count == 0)
                    {
                        group = new RobotsGroup();
                        file.Groups.Add(group);
                    }
                    group.UserAgents.Add(value);
                    break;
                case "allow":
                    group ??= new RobotsGroup();
                    if (!file.Groups.Contains(group))
                    {
                        file.Groups.Add(group);
                    }
                    group.Directives.Add(new RobotsDirective { Type = RobotsDirectiveType.Allow, Value = value });
                    break;
                case "disallow":
                    group ??= new RobotsGroup();
                    if (!file.Groups.Contains(group))
                    {
                        file.Groups.Add(group);
                    }
                    group.Directives.Add(new RobotsDirective { Type = RobotsDirectiveType.Disallow, Value = value });
                    break;
                case "crawl-delay":
                    group ??= new RobotsGroup();
                    if (!file.Groups.Contains(group))
                    {
                        file.Groups.Add(group);
                    }
                    group.Directives.Add(new RobotsDirective { Type = RobotsDirectiveType.CrawlDelay, Value = value });
                    break;
                case "sitemap":
                    file.Sitemaps.Add(value);
                    break;
                case "host":
                    file.Host = value;
                    break;
                default:
                    group ??= new RobotsGroup();
                    if (!file.Groups.Contains(group))
                    {
                        file.Groups.Add(group);
                    }
                    group.Directives.Add(new RobotsDirective { Type = RobotsDirectiveType.Unknown, Value = field + ":" + value });
                    break;
            }
        }

        return file;
    }
}

