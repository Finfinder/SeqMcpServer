# Seq MCP Server

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MCP](https://img.shields.io/badge/MCP-Model%20Context%20Protocol-blue)](https://modelcontextprotocol.io/)
[![Seq](https://img.shields.io/badge/Seq-Centralized%20Logging-0A0A0A)](https://datalust.co/seq)
[![Version](https://img.shields.io/badge/version-1.1.0-green)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Finfinder_SeqMcpServer&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Finfinder_SeqMcpServer)

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

## Code Quality

This project is continuously analyzed by [SonarCloud](https://sonarcloud.io/summary/new_code?id=Finfinder_SeqMcpServer) for code quality and security.

- **CI-based analysis**: The [`sonar.yml`](.github/workflows/sonar.yml) workflow runs SonarScanner for .NET on every push to `main`, version branches, and pull requests.
- **Code coverage**: Unit and integration tests generate OpenCover coverage reports via [coverlet](https://github.com/coverlet-coverage/coverlet), which are uploaded to SonarCloud for coverage analysis and PR decoration.
- **PR decoration**: SonarCloud automatically posts analysis results as comments on pull requests, including new issues, quality gate status, and coverage changes.
- **Quality Gate**: The project uses the "Sonar way" quality gate — new code must pass all conditions before merging.

### SonarQube for IDE (Real-time Analysis)

[SonarQube for IDE](https://marketplace.visualstudio.com/items?itemName=SonarSource.sonarlint-vscode) (formerly SonarLint) provides real-time code analysis directly in VS Code. With **Connected Mode**, it synchronizes the Quality Profile from SonarCloud, ensuring the same rules are applied locally and in CI.

**Requirements:** Java 17+, SonarCloud account with access to the `finfinder` organization.

**Installation:**

1. Install the extension from VS Code Marketplace: search for "SonarQube for IDE" or run:
   ```
   code --install-extension SonarSource.sonarlint-vscode
   ```
2. Open this project in VS Code. The extension will detect the `.sonarlint/connectedMode.json` shared binding and prompt you to configure Connected Mode.
3. Click **"Use Configuration"** when prompted, then provide your **User Token**.
4. To generate a User Token, go to [SonarCloud Security](https://sonarcloud.io/account/security) and create a new token (type: **User Token**).

> **Important:** Your User Token is personal and must **not** be committed to the repository. Each developer generates their own token.

**What Connected Mode provides:**
- Synchronized Quality Profile rules from SonarCloud
- Suppression of issues marked as Accepted/False Positive on the server
- Focus on new code analysis
- Smart notifications about Quality Gate changes
- Branch awareness compatible with the project's versioned branch model

### SonarQube MCP Server (Optional)

The [SonarQube MCP Server](https://github.com/SonarSource/sonarqube-mcp-server) enables AI agents (GitHub Copilot) to query SonarCloud analysis results directly from VS Code. This integration is used by the `code-reviewer` and `software-engineer` agents.

**Requirements:** Docker

Add the following to your `.vscode/mcp.json`:

```json
{
  "servers": {
    "sonarqube": {
      "command": "docker",
      "args": ["run", "-i", "--rm",
        "-e", "SONAR_TOKEN",
        "-e", "SONAR_HOST_URL=https://sonarcloud.io",
        "-e", "SONAR_ORGANIZATION=finfinder",
        "mcp/sonarqube"
      ],
      "env": {
        "SONAR_TOKEN": "${input:sonarToken}"
      }
    }
  }
}
```

To generate a `SONAR_TOKEN`, go to [SonarCloud Security](https://sonarcloud.io/account/security) and create a new token with **Execute Analysis** scope.

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
