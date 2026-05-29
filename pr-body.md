> ⚠️ DRAFT — Tamir tests locally before this goes ready-for-review

## What this adds

`CommunityToolkit.Aspire.Hosting.Squad` — Aspire hosting package for the Squad multi-agent CLI. Emits a Hybrid PATH+URI connection string consumed by `Squad.Agents.AI` (Track A) in MAF apps.

## Design refs (in `tamirdresher_microsoft/tamresearch1`)

- `.squad/decisions.md` Decisions 441, 443, 445, 447, 448
- Decision 447 Q1 — Hybrid PATH+URI wire format with AFCP rationale
- Decision 448 Q3b Option C — metadata-only v1.0 + `.WithSquadCli()` stub for v1.1+

## What's in v1.0

- `SquadResource` — team-root + agent discovery
- `SquadBuilderExtensions` — `AddSquad` (with mandatory `teamRoot` parameter), 4 dashboard commands (refresh-agents, open-team-root, open-copilot-cli, check-inbox)
- `WithSquadCli()` — STUB throws `NotImplementedException`; reserved for v1.1+ process spawning
- Hybrid PATH+URI connection-string emission with AFCP comment
- README.md with Aspire-injected config pattern (Decision 384)

## What's NOT in v1.0

- Process spawning (`.WithSquadCli()` is the future-compat shim)
- Readiness probe (no process to probe)
- Multi-instance (one resource per AppHost in v1.0)
- Example AppHost (deferred to after Track A `Squad.Agents.AI` NuGet is published)

## How to test locally

1. Clone this branch:
   ```
   gh repo clone tamirdresher/Aspire-1
   cd Aspire-1
   gh pr checkout <PR-NUMBER>
   ```

2. Build:
   ```
   dotnet restore
   dotnet build src/CommunityToolkit.Aspire.Hosting.Squad
   ```

3. Verify build success (zero warnings).

4. (Full loop test — requires Track A `Squad.Agents.AI` NuGet):
   - Create a minimal AppHost that calls `builder.AddSquad("squad", teamRoot: "...")`
   - Reference the built DLL from the AppHost
   - Run the AppHost: `dotnet run --project path/to/AppHost`
   - Verify the Aspire dashboard shows the `squad` resource
   - Verify 4 dashboard commands appear and are clickable
   - Click **open-team-root** → opens folder
   - Click **refresh-agents** → logs agent roster
   - Verify the connection string resolves as a simple PATH (not URI) in the consumer

## Wire format verification

The Q1 Hybrid PATH+URI implementation emits:

- **Default (v1.0):** Simple PATH form: `C:\path\to\team-root`
- **Future (v1.1+ when `.WithSquadCli()` is implemented):** URI form with extra knobs: `squad://localhost?teamRoot=...&cliPath=...&protocol=maf-1.0`

The AFCP comment in `SquadResource.cs` lines 55-62 explains the rationale.

## Co-author

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
