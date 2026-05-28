using System.Text.RegularExpressions;

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents a Squad AI-agent team as a first-class .NET Aspire resource.
/// Implements <see cref="IResourceWithConnectionString"/> so that downstream services can
/// reference the team with <c>.WithReference(squad)</c> and receive a Hybrid PATH+URI
/// connection string consumable by <c>Squad.Agents.AI.SquadConnectionFactory</c>.
/// The resource is a logical Aspire resource; it does not start a listener by itself.
/// </summary>
public sealed class SquadResource : Resource, IResourceWithConnectionString
{
    // Regexes that match agent names in supported .squad/team.md formats:
    //   | Ralph | Work Monitor | ...
    //   - **Ralph** (Work Monitor)
    private static readonly Regex AgentTableRowRegex =
        new(@"^\s*\|\s*([^|\r\n]+?)\s*\|", RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex AgentLineRegex =
        new(@"^\s*-\s+\*\*([^*\r\n]+?)\*\*", RegexOptions.Compiled | RegexOptions.Multiline);

    private readonly List<string> _agents;

    /// <summary>
    /// Gets the absolute path to the directory that contains the <c>.squad/</c> folder
    /// (i.e., the workspace root, not the <c>.squad/</c> sub-directory itself).
    /// </summary>
    public string TeamRoot { get; }

    /// <summary>Gets the list of agent names discovered from <c>.squad/team.md</c>.</summary>
    public IReadOnlyList<string> Agents => _agents;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Emits a Hybrid PATH+URI connection string:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><b>PATH form (default):</b> <c>C:\path\to\team-root</c> (or <c>/Users/...</c> on Unix)</description></item>
    ///   <item><description><b>URI form (when extra knobs):</b> <c>squad://localhost?teamRoot=...&amp;cliPath=...&amp;protocol=maf-1.0</c></description></item>
    /// </list>
    /// <para>
    /// Both forms are accepted by <c>Squad.Agents.AI.SquadConnectionFactory</c>.
    /// The URI form is reserved for future AFCP (Agent-to-Agent Framework Communication Protocol) support;
    /// today the host portion is ignored by the consumer.
    /// </para>
    /// </remarks>
    public ReferenceExpression ConnectionStringExpression
    {
        get
        {
            // AFCP (Agent-to-Agent Framework Communication Protocol) future-compat:
            // the URI form's host:port is reserved for future remote-spawn scenarios.
            // Today host is always "localhost" and ignored by the consumer
            // (Squad.Agents.AI / SquadConnectionFactory). We emit URI only when
            // extra knobs require structured key-value carriage; otherwise prefer
            // the simpler PATH form for tooling and human readability.

            // For now, we only emit the PATH form in v1.0 (metadata-only).
            // The URI escape hatch is reserved for future versions when process-spawning
            // via .WithSquadCli() is implemented and requires extra parameters.
            return ReferenceExpression.Create($"{TeamRoot}");
        }
    }

    /// <summary>
    /// Initialises a new <see cref="SquadResource"/>, auto-discovering agents from <c>.squad/team.md</c>.
    /// </summary>
    /// <param name="name">The Aspire resource name.</param>
    /// <param name="teamRoot">
    /// Absolute path to the workspace root (the directory that contains the <c>.squad/</c> folder).
    /// </param>
    public SquadResource(string name, string teamRoot)
        : base(name)
    {
        ArgumentException.ThrowIfNullOrEmpty(teamRoot);

        TeamRoot = teamRoot;
        _agents = DiscoverAgents(teamRoot);
    }

    // Agent discovery.

    /// <summary>
    /// Parses <c>.squad/team.md</c> and returns lowercase agent names.
    /// Falls back to a default roster when the file does not exist.
    /// </summary>
    internal static List<string> DiscoverAgents(string teamRoot)
    {
        var rosterPath = Path.Combine(teamRoot, ".squad", "team.md");

        if (!File.Exists(rosterPath))
        {
            // Return a sensible default roster so the resource is still useful without a team.md.
            return ["ralph", "seven", "picard", "belanna", "data", "worf", "kes", "neelix", "scribe", "troi"];
        }

        var content = File.ReadAllText(rosterPath);
        var agents = AgentTableRowRegex.Matches(content)
            .Concat(AgentLineRegex.Matches(content))
            .Select(m => NormalizeAgentName(m.Groups[1].Value))
            .Where(agent => IsKnownAgent(teamRoot, agent))
            .Distinct()
            .ToList();

        return agents.Count > 0
            ? agents
            : ["ralph", "seven", "picard", "belanna", "data", "worf", "kes", "neelix", "scribe", "troi"];
    }

    private static bool IsKnownAgent(string teamRoot, string agentName)
    {
        if (agentName.Length == 0)
        {
            return false;
        }

        var agentsDirectory = Path.Combine(teamRoot, ".squad", "agents");
        if (!Directory.Exists(agentsDirectory))
        {
            return true;
        }

        return File.Exists(Path.Combine(agentsDirectory, agentName, "charter.md"));
    }

    private static string NormalizeAgentName(string value)
    {
        var cleaned = value.Trim().ToLowerInvariant().Replace("'", string.Empty);
        return Regex.Replace(cleaned, @"[^a-z0-9]+", "-").Trim('-');
    }
}
