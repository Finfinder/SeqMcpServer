# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

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

### Changed

- Migrated Issue templates from Markdown to YAML Issue Forms with required field validation, dropdowns (OS, MCP client, deployment type, feature category), and code-rendered log output

## [1.0.0] - 2026-03-18

### Added

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
