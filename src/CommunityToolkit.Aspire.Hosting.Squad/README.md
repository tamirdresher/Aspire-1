# CommunityToolkit.Aspire.Hosting.Squad

Aspire hosting integration for the Squad multi-agent CLI.

## Install

```shell
dotnet add package CommunityToolkit.Aspire.Hosting.Squad --prerelease
```

## Quick start

In your AppHost:

```csharp
var squad = builder.AddSquad("squad", teamRoot: "../my-team");

var api = builder.AddProject<Projects.MyApi>("api")
    .WithReference(squad);
```

In your `MyApi` (uses `Squad.Agents.AI` — Track A NuGet):

```csharp
builder.Services.AddSquadAgent();   // picks up ConnectionStrings__squad
```

That's it. The connection string is emitted in the Hybrid PATH+URI format
and parsed by `Squad.Agents.AI.SquadConnectionFactory`.

## Dashboard commands

The resource ships 4 dashboard commands:

- **refresh-agents** — re-scans `.squad/team.md`
- **open-team-root** — opens the team-root folder
- **open-copilot-cli** — opens a terminal with `copilot`
- **check-inbox** — lists `.squad/decisions/inbox/`

## Configuration

The `teamRoot` parameter is required when calling `AddSquad`. It must be an absolute path to the directory containing the `.squad/` folder.

## What v1.0 does NOT do

Process spawning — the Squad CLI must be started externally. `WithSquadCli()`
is present as a preview stub marked `[Experimental("SQUAD001")]`. It throws
`NotImplementedException` at runtime. To use it (for future-proofing your code):

```csharp
#pragma warning disable SQUAD001
var squad = builder.AddSquad("squad", teamRoot: "../my-team")
    .WithSquadCli();  // Preview stub - will work in v0.2
#pragma warning restore SQUAD001
```

## Connection-string wire format (Hybrid)

- **PATH form (default):** `"C:\\path\\to\\team-root"` (or `/Users/...` on Unix)
- **URI form (when extra knobs):** `squad://localhost?teamRoot=...&cliPath=...&protocol=maf-1.0`

Both are accepted by `Squad.Agents.AI`. The URI form is reserved for
future AFCP support; today the host portion is ignored.

## Status

Draft preview. Surface may change before 1.0.
