# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Changed

- Switch `SeqConnection` DI registration from externally-created instance to factory delegate to ensure proper disposal on host shutdown
- Move `Uri.TryCreate` validation before `SeqConnection` instantiation in `Program.cs` to enforce validate-before-use principle
- Extract duplicated date-range parsing logic from `QueryLogsTool` and `SqlQueryTool` into shared `DateRangeHelper.ParseDateRange` helper
- Remove redundant `cancellationToken.ThrowIfCancellationRequested()` from synchronous `VersionTool.GetVersion()` method (dead code — no I/O, no suspension points)
- Add "Wzorzec lokalny (synchroniczny)" section to `copilot-instructions.md` documenting conventions for synchronous MCP tools
- Update `Seq.Api` version in `copilot-instructions.md` from 2024.3.0 to 2025.2.2

### Added

- Security comments in `SqlQueryTool` and `QueryLogsTool` documenting the transparent proxy design — SQL queries and filter expressions are forwarded to Seq API without server-side sanitization (accepted risk: Seq SQL is read-only, authorization enforced by Seq server)
- "Security Considerations" section in README documenting the transparent proxy model, read-only Seq SQL, API key authorization, and least-privilege recommendation
- Transparent proxy guideline in CONTRIBUTING.md Security section for contributors adding new tools
- Security review checkbox in Pull Request template and CONTRIBUTING.md checklist (OWASP Top 10, input validation, vulnerable dependencies)

### Fixed

- Improve date parsing error messages in `DateRangeHelper.ParseDateRange` — invalid dates now report the specific parameter name (`fromUtc`/`toUtc`) and the invalid value instead of a generic `FormatException`
- Add empty query validation in `SqlQueryTool` to return clear error instead of forwarding empty SQL to Seq engine
- Fix false-positive LIMIT detection in `SqlQueryTool` that skipped auto-injection when "limit" appeared as column name, comment, string literal, or alias

## [2.0.0] - 2026-03-22

_Released version — no further changes._

### Breaking Changes

- Minimum required Seq server version is now **2025.1+** (API media type v11). SeqMcpServer 1.x remains available for older Seq installations using API v10.
- `seq_get_alerts` output format changed from raw Seq API JSON to projected JSON with fields: `Id`, `OwnerId`, `NotificationLevel`, `NotificationAppInstanceIds`, `Activity`

### Changed

- Upgrade Seq.Api SDK from 2024.3.0 to 2025.2.2
- Migrate `seq_get_alerts` (AlertsTool) from HTTP pattern (`IHttpClientFactory`) to SDK pattern (`SeqConnection` + `AlertState.ListAsync()` + LINQ projection)
- Pin Docker image in integration tests from `datalust/seq:latest` to `datalust/seq:2025.2`

### Added

- Version Compatibility table in README mapping SeqMcpServer releases to Seq.Api SDK versions, minimum Seq server requirements, and API media type versions

## [1.1.0] - 2026-03-22

### Security

- Pin third-party GitHub Actions to full commit SHA: `softprops/action-gh-release` v2.6.1 in `release.yml`, `stefanbuck/github-issue-parser` v3.2.3 in `validate-issue.yml` (SonarCloud S7637)
- Move secrets (`NUGET_API_KEY`, `SONAR_TOKEN`) from inline `${{ secrets.* }}` expansion in `run` blocks to `env:` blocks in `release.yml` and `sonar.yml` (SonarCloud S7636)
- Move all permissions from workflow level to job level with deny-all default (`permissions: {}`) in `release.yml`, `sonar.yml`, and `validate-issue.yml` (SonarCloud S8264)

### Added

- SonarQube for IDE shared Connected Mode binding (`.sonarlint/connectedMode.json`) for automatic SonarCloud connection setup
- New `ensuring-code-quality` skill covering the full Sonar ecosystem (SonarQube for IDE, SonarQube MCP Server, SonarCloud CI) and local analyzers
- SonarQube for IDE awareness in `code-reviewer` and `software-engineer` agents with `ensuring-code-quality` skill reference
- SonarQube for IDE verification step in `code-reviewing` skill (Step 7) and `codebase-analysing` skill (Step 10)
- SonarQube for IDE verification step in `review` prompt workflow
- SonarQube for IDE installation and Connected Mode setup documentation in README
- Unit tests for error paths (catch blocks) in SDK-based tools: DashboardsTool, QueryLogsTool, RetentionPoliciesTool, SignalsTool, SqlQueryTool
- Unit tests for DateTime parameter validation in QueryLogsTool and SqlQueryTool
- Code coverage reporting (OpenCover via coverlet) in SonarCloud workflow — unit and integration tests
- Integration tests execution in SonarCloud workflow (`sonar.yml`)
- Dedicated Security section in `CONTRIBUTING.md` with contributor security guidelines and reference to `SECURITY.md`
- GitHub Actions workflow (`sonar.yml`) for SonarCloud CI-based analysis with SonarScanner for .NET — triggers on push to main/version branches and pull requests
- SonarCloud Quality Gate badge in README
- SonarQube MCP Server configuration documentation in README for local developer setup with AI agents
- SonarCloud integration in code-reviewing and codebase-analysing skills for querying analysis results via SonarQube MCP tools
- Security policy (`SECURITY.md`) with responsible disclosure process, response timeline, scope definition, and Safe Harbor clause via GitHub Security Advisories
- Security vulnerability reporting link in issue template chooser (`config.yml`)

## [1.0.0] - 2026-03-20

### Added

- GitHub Actions workflow (`validate-issue.yml`) for automatic issue template validation — labels and comments on issues not matching Bug Report or Feature Request templates
- `blank_issues_enabled: false` in template chooser configuration — blocks free-form issues in the GitHub UI
- `.editorconfig` with formatting and .NET code style rules (Allman braces, 4-space indent, file-scoped namespaces, `var` preference, explicit access modifiers) — enforced at editor level as suggestions
- GitHub Issue templates (Bug Report, Feature Request) with template chooser configuration
- Pull Request template with contribution checklist
- GitHub Discussions enabled for community Q&A
- Contributing guide (`CONTRIBUTING.md`) with development setup, coding conventions, MCP tool patterns, testing guidelines, and PR process
- Unit test project (`SeqMcpServer.Tests.Unit`) with xUnit and NSubstitute — 15 tests covering VersionTool, AlertsTool, DiagnosticsTool, JsonDefaults, and VersionInfo
- Integration test project (`SeqMcpServer.Tests.Integration`) with xUnit and Testcontainers — 19 tests running all 8 MCP tools against a real Seq instance in Docker
- `InternalsVisibleTo` for test projects to access internal helpers
- Contributor Covenant Code of Conduct (`CODE_OF_CONDUCT.md`)
- GitHub Actions release workflow for automated cross-platform builds and GitHub Release creation
- SHA256 checksum file for all release artifacts
- NuGet dotnet tool package for installation via dotnet tool install
- MCP tool `seq_query_logs` — search Seq log events using filter expressions
- MCP tool `seq_run_sql` — execute SQL queries against Seq log data
- MCP tool `seq_list_signals` — list shared signals defined in Seq
- MCP tool `seq_list_dashboards` — list shared dashboards with chart definitions
- MCP tool `seq_get_alerts` — get configured alerts and their current state
- MCP tool `seq_get_diagnostics` — get Seq server diagnostics
- MCP tool `seq_get_retention` — get retention policies configured in Seq
- MCP tool `seq_get_version` — get MCP server version and runtime information
- `VersionInfo` helper for assembly-based version resolution
- Startup diagnostics with server version on stderr
- Version defined as Single Source of Truth in `.csproj`
- MIT License

### Changed

- Split `invalid-template` label into two: `invalid-template` (issue not using any template) and `needs-info` (template used but required sections missing) — enables finer-grained issue triage
- Migrated Issue templates from Markdown to YAML Issue Forms with required field validation, dropdowns (OS, MCP client, deployment type, feature category), and code-rendered log output
