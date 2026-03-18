# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

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
