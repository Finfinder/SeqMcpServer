# Seq MCP Server

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MCP](https://img.shields.io/badge/MCP-Model%20Context%20Protocol-blue)](https://modelcontextprotocol.io/)
[![Seq](https://img.shields.io/badge/Seq-Centralized%20Logging-0A0A0A)](https://datalust.co/seq)
[![Version](https://img.shields.io/badge/version-1.1.0-green)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) server that connects AI agents to [Seq](https://datalust.co/seq) — a centralized structured logging platform. This server enables LLMs to search log events, execute SQL queries, inspect dashboards, alerts, signals, and more through natural language interactions.

---

## Use Cases

- **Log Investigation**: Search and filter log events with Seq filter expressions. Let AI help you find errors, trace issues, and analyze patterns across your log data.
- **SQL Analytics**: Execute SQL queries against your Seq event stream. Aggregate, group, and analyze log data using natural language that translates to Seq SQL.
- **Dashboard Inspection**: Browse shared dashboards and their chart definitions to understand monitoring configurations.
- **Alert Monitoring**: Check configured alerts and their current state to quickly assess system health.
- **Signal Discovery**: List shared signals (saved filters/views) to understand how your team categorizes log data.
- **Infrastructure Insights**: Retrieve server diagnostics, retention policies, and system metrics for operational awareness.

---

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- A running [Seq](https://datalust.co/seq) instance (local or remote)
- An MCP-compatible client (VS Code, Claude Desktop, Cursor, Windsurf, or any other MCP host)

---

## Getting Started

### 1. Build the server

```bash
git clone <repository-url>
cd src/SeqMcpServer
dotnet build
```

### 2. Configure your MCP client

The server communicates over **stdio** and is configured entirely through environment variables.

<details>
<summary><b>VS Code</b></summary>

Add the following to your VS Code MCP settings (`.vscode/mcp.json` or user settings):

```json
{
  "servers": {
    "seq": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "/absolute/path/to/SeqMcpServer"],
      "env": {
        "SEQ_URL": "http://localhost:5341",
        "SEQ_API_KEY": "your-seq-api-key"
      }
    }
  }
}
```

</details>

<details>
<summary><b>Claude Desktop</b></summary>

Edit your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "seq": {
      "command": "dotnet",
      "args": ["run", "--project", "/absolute/path/to/SeqMcpServer"],
      "env": {
        "SEQ_URL": "http://localhost:5341",
        "SEQ_API_KEY": "your-seq-api-key"
      }
    }
  }
}
```

</details>

<details>
<summary><b>Cursor</b></summary>

Add to your Cursor MCP configuration:

```json
{
  "mcpServers": {
    "seq": {
      "command": "dotnet",
      "args": ["run", "--project", "/absolute/path/to/SeqMcpServer"],
      "env": {
        "SEQ_URL": "http://localhost:5341",
        "SEQ_API_KEY": "your-seq-api-key"
      }
    }
  }
}
```

</details>

<details>
<summary><b>Using a compiled binary</b></summary>

For better startup performance, publish the server first:

```bash
dotnet publish -c Release -o ./publish
```

Then reference the binary directly in your MCP client configuration:

```json
{
  "servers": {
    "seq": {
      "type": "stdio",
      "command": "/absolute/path/to/publish/SeqMcpServer.exe",
      "env": {
        "SEQ_URL": "http://localhost:5341",
        "SEQ_API_KEY": "your-seq-api-key"
      }
    }
  }
}
```

</details>

---

## Configuration

The server is configured exclusively through environment variables — no configuration files are needed.

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `SEQ_URL` | No | `http://localhost:5341` | Base URL of your Seq instance |
| `SEQ_API_KEY` | No | *(none)* | Seq API key for authentication. If omitted, connects without authentication |

> **Note:** Startup diagnostics (including the server version and authentication status) are written to `stderr` for visibility without interfering with the MCP stdio transport.

---

## Available Tools

The server exposes **8 MCP tools** that provide comprehensive access to Seq functionality:

### Log Querying

| Tool | Description |
|------|-------------|
| [`seq_query_logs`](#seq_query_logs) | Search Seq log events using filter expressions |
| [`seq_run_sql`](#seq_run_sql) | Execute SQL queries against Seq log data |

### Configuration & Monitoring

| Tool | Description |
|------|-------------|
| [`seq_list_signals`](#seq_list_signals) | List shared signals (saved filters/views) |
| [`seq_list_dashboards`](#seq_list_dashboards) | List shared dashboards with chart definitions |
| [`seq_get_alerts`](#seq_get_alerts) | Get configured alerts and their current state |
| [`seq_get_retention`](#seq_get_retention) | Get retention policies configured in Seq |

### Server Information

| Tool | Description |
|------|-------------|
| [`seq_get_diagnostics`](#seq_get_diagnostics) | Get Seq server diagnostics and system metrics |
| [`seq_get_version`](#seq_get_version) | Get MCP server version and runtime information |

---

## Tool Reference

### `seq_query_logs`

Search Seq log events using a filter expression. Returns matching events with timestamps, levels, messages, and properties.

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `filter` | `string` | No | `""` | Seq filter expression (e.g., `@Level = "Error"`, `Application = "MyApp"`). Leave empty for all events. |
| `count` | `int` | No | `50` | Maximum number of events to return (1–500) |
| `fromUtc` | `string` | No | Last 24 hours | ISO 8601 start time (e.g., `2025-01-15T00:00:00Z`) |
| `toUtc` | `string` | No | Now | ISO 8601 end time |

**Example prompts:**
- *"Show me the last 10 error logs"*
- *"Find all logs from the PaymentService in the last hour"*
- *"Search for logs containing 'timeout' with level Warning or Error"*

---

### `seq_run_sql`

Execute a SQL query against Seq log data using standard Seq SQL syntax.

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `query` | `string` | **Yes** | — | SQL query to execute (e.g., `select count(*) from stream group by @Level`) |
| `fromUtc` | `string` | No | Last 24 hours | ISO 8601 range start |
| `toUtc` | `string` | No | Now | ISO 8601 range end |

> **Safety:** A `LIMIT 1000` clause is automatically appended if no limit is specified to prevent excessive data retrieval.

**Example prompts:**
- *"Count log events grouped by level for the last 24 hours"*
- *"Show the top 10 most frequent error messages this week"*
- *"What is the average response time per endpoint?"*

---

### `seq_list_signals`

List all shared signals (saved log filters/views) defined in Seq.

*No parameters required.*

**Returns:** Signal ID, title, description, and filter expressions for each shared signal.

---

### `seq_list_dashboards`

List all shared dashboards configured in Seq with their chart definitions.

*No parameters required.*

**Returns:** Dashboard ID, title, and nested chart definitions including queries with filter and group-by clauses.

---

### `seq_get_alerts`

Get all configured alerts and their current state from Seq.

*No parameters required.*

**Returns:** Alert state data including configuration and current trigger status.

---

### `seq_get_retention`

Get all retention policies configured in Seq, showing how long log data is kept.

*No parameters required.*

**Returns:** Policy ID, retention period (in days), and associated signal expressions.

---

### `seq_get_diagnostics`

Get Seq server diagnostics including ingestion status, storage usage, and system metrics.

*No parameters required.*

**Returns:** Comprehensive diagnostics report with ingestion, storage, and system health metrics.

---

### `seq_get_version`

Get the MCP server version, name, and runtime information.

*No parameters required.*

**Returns:** Server name, version number, and .NET runtime description.

---

## Architecture

```
SeqMcpServer/
├── Program.cs              # Entry point, DI registration, MCP server setup
├── VersionInfo.cs          # Assembly-based version resolution
└── Tools/
    ├── JsonDefaults.cs     # Shared JSON serialization options
    ├── QueryLogsTool.cs    # seq_query_logs  — event search
    ├── SqlQueryTool.cs     # seq_run_sql     — SQL queries
    ├── SignalsTool.cs      # seq_list_signals
    ├── DashboardsTool.cs   # seq_list_dashboards
    ├── AlertsTool.cs       # seq_get_alerts
    ├── RetentionPoliciesTool.cs  # seq_get_retention
    ├── DiagnosticsTool.cs  # seq_get_diagnostics
    └── VersionTool.cs      # seq_get_version
```

- **Transport:** stdio (standard input/output)
- **Tool discovery:** Automatic via `[McpServerToolType]` and `[McpServerTool]` attributes
- **Seq integration:** Dual pattern — [Seq.Api SDK](https://github.com/datalust/seq-api) for typed access, named `HttpClient` for raw API endpoints
- **Error handling:** All tools return JSON-serialized errors — no exceptions propagate to the MCP host

---

## Tech Stack

| Component | Version |
|-----------|---------|
| .NET | 9.0 |
| [ModelContextProtocol](https://github.com/modelcontextprotocol/csharp-sdk) | 0.1.0-preview.9 |
| [Seq.Api](https://github.com/datalust/seq-api) | 2024.3.0 |
| Microsoft.Extensions.Hosting | 9.0.0 |
| Microsoft.Extensions.Http | 9.0.0 |

---

## Testing

The project includes unit tests and integration tests.

### Unit tests

```bash
dotnet test SeqMcpServer.Tests.Unit
```

No external dependencies required — runs entirely in-process with mocked HTTP handlers.

### Integration tests

```bash
dotnet test SeqMcpServer.Tests.Integration
```

Requires **Docker** — [Testcontainers](https://dotnet.testcontainers.org/) automatically starts a `datalust/seq:latest` container, seeds test data, runs all tests, and cleans up.

### All tests

```bash
dotnet test
```

---

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to get started.

---

## Security

To report a security vulnerability, please see [SECURITY.md](SECURITY.md) for instructions.

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes.

---

## License

This project is licensed under the [MIT License](LICENSE).
