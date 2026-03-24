# Contributing to Seq MCP Server

Thank you for your interest in contributing to **Seq MCP Server**! Whether you're fixing a bug, adding a new MCP tool, improving documentation, or writing tests — every contribution is appreciated.

---

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you agree to uphold a welcoming and respectful environment.

---

## Security

Please review the [Security Policy](SECURITY.md) for the full details on how we handle security in this project.

When contributing, keep the following security guidelines in mind:

- **Never commit credentials, API keys, or secrets** to the repository — use environment variables exclusively
- **Report vulnerabilities privately** through [GitHub Security Advisories](https://github.com/Finfinder/SeqMcpServer/security/advisories/new) — never through public issues or pull requests
- **Do not introduce dependencies with known vulnerabilities** — vet any new packages before adding them
- **Validate all user inputs** (MCP tool parameters) — use input clamping (`Math.Clamp`) and enforce query limits (e.g., automatic `LIMIT` for SQL) to protect against injection and excessive load on Seq

---

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- [Git](https://git-scm.com/)
- *(Optional)* [Docker](https://www.docker.com/) — required only for integration tests

### Clone and build

```bash
git clone <repository-url>
cd src/SeqMcpServer
dotnet build
```

### Local Seq instance

To test against a real Seq instance, start one with Docker:

```bash
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```

Seq will be available at `http://localhost:5341`.

### Environment variables

The server is configured exclusively through environment variables — no configuration files are used.

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `SEQ_URL` | No | `http://localhost:5341` | Base URL of your Seq instance |
| `SEQ_API_KEY` | No | *(none)* | Seq API key for authentication |

> **Important:** Never commit credentials or API keys to the repository.

---

## Project Structure

```
SeqMcpServer/
├── Program.cs                      # Entry point, DI registration, MCP server setup
├── VersionInfo.cs                  # Assembly-based version resolution
├── Tools/
│   ├── JsonDefaults.cs             # Shared JSON serialization options
│   ├── QueryLogsTool.cs            # seq_query_logs
│   ├── SqlQueryTool.cs             # seq_run_sql
│   ├── SignalsTool.cs              # seq_list_signals
│   ├── DashboardsTool.cs           # seq_list_dashboards
│   ├── AlertsTool.cs               # seq_get_alerts
│   ├── RetentionPoliciesTool.cs    # seq_get_retention
│   ├── DiagnosticsTool.cs          # seq_get_diagnostics
│   └── VersionTool.cs              # seq_get_version
├── SeqMcpServer.Tests.Unit/        # Unit tests (xUnit + NSubstitute)
└── SeqMcpServer.Tests.Integration/ # Integration tests (xUnit + Testcontainers)
```

---

## Coding Conventions

### Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Classes | PascalCase with `Tool` suffix | `SignalsTool` |
| Methods | PascalCase | `ListSignals` |
| MCP tool names | `seq_` prefix + snake_case | `seq_list_signals` |
| Variables | camelCase | `httpClient` |
| Namespace | `SeqMcpServer.Tools` | — |

### Style

Formatting and code style conventions are enforced automatically by the [`.editorconfig`](.editorconfig) file at the repository root. Your editor will apply these settings when you open the project — no manual configuration needed.

- **Allman style braces** (opening brace on its own line)
- **4 spaces** indentation (no tabs)
- `nullable enable` — respect nullable reference types
- `implicit usings enable` — standard .NET implicit usings are available

### Serialization

- Use **only** `System.Text.Json` — never `Newtonsoft.Json`
- Use `JsonDefaults.Indented` for all serialized output (defined in `Tools/JsonDefaults.cs`)
- HTTP pattern tools return raw JSON from the response — no additional serialization

### What NOT to do

- Do **not** use `appsettings.json` — configuration comes from environment variables only
- Do **not** create instance classes with constructors — tools are static classes
- Do **not** add `Newtonsoft.Json`, `MediatR`, or other unnecessary packages
- Do **not** skip `[Description]` attributes on parameters — LLMs need them to generate correct tool calls

---

## Adding a New MCP Tool

Adding a new tool is the most common type of contribution. Each tool lives in its own file under `Tools/` and follows a strict pattern.

### SDK Pattern (preferred)

Use when the `Seq.Api` SDK supports the endpoint:

```csharp
using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Seq.Api;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class ExampleTool
{
    [McpServerTool(Name = "seq_list_examples"), Description("Description of what this tool does.")]
    public static async Task<string> ListExamples(
        SeqConnection connection,
        [Description("Description of this parameter.")] string filter = "",
        [Description("Max items to return (1-100)")] int count = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            count = Math.Clamp(count, 1, 100);

            cancellationToken.ThrowIfCancellationRequested();

            var items = await connection.Examples.ListAsync();

            // ExampleEntity has: Id, Title, Description, ...
            var result = items.Select(x => new
            {
                x.Id,
                x.Title,
                x.Description
            });

            return JsonSerializer.Serialize(result, JsonDefaults.Indented);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Failed to list examples: {ex.Message}" });
        }
    }
}
```

### HTTP Pattern (fallback)

Use when the SDK does not support the endpoint:

```csharp
using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class ExampleTool
{
    [McpServerTool(Name = "seq_get_example"), Description("Description of what this tool does.")]
    public static async Task<string> GetExample(
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient("Seq");
            var response = await httpClient.GetAsync("/api/example", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Failed to get example: {ex.Message}" });
        }
    }
}
```

### Key rules

- **One file = one tool** in `Tools/`
- Class must be `public static` with `[McpServerToolType]`
- Method must have `[McpServerTool(Name = "seq_...")]` — the `seq_` prefix is mandatory
- `[Description("...")]` is required on the method **and every parameter**
- Return type is always `Task<string>` (serialized JSON)
- First parameter: DI dependency — `SeqConnection` (SDK) or `IHttpClientFactory` (HTTP)
- Last parameter: `CancellationToken cancellationToken = default`
- **Always** wrap the method body in `try/catch` — tools must never throw exceptions
- Error responses must be JSON: `new { Error = $"Failed to <operation>: {ex.Message}" }`
- Validate numeric inputs with `Math.Clamp`; auto-append `LIMIT` for SQL queries
- SDK pattern: call `cancellationToken.ThrowIfCancellationRequested()` before SDK calls (SDK methods don't accept `CancellationToken`)
- HTTP pattern: pass `cancellationToken` directly to `GetAsync`/`PostAsync`

### New tool checklist

- [ ] File created in `Tools/` with `Tool` suffix (e.g., `MyFeatureTool.cs`)
- [ ] `[McpServerToolType]` on the class
- [ ] `[McpServerTool(Name = "seq_...")]` and `[Description]` on the method
- [ ] `[Description]` on every parameter
- [ ] `CancellationToken` as the last parameter
- [ ] `try/catch` wrapping the entire method body
- [ ] Input validation where applicable
- [ ] Unit tests added
- [ ] `README.md` updated if the new tool should be documented
- [ ] `CHANGELOG.md` entry added under `[Unreleased]`

---

## Testing

### Unit tests (required)

All contributions must pass existing unit tests and include new ones for new functionality.

```bash
dotnet test SeqMcpServer.Tests.Unit
```

- Framework: **xUnit** with **NSubstitute** for mocking
- No external dependencies — runs entirely in-process with mocked HTTP handlers
- Test files are in `SeqMcpServer.Tests.Unit/`

### Integration tests (optional)

Integration tests run against a real Seq instance in Docker via [Testcontainers](https://dotnet.testcontainers.org/). They are encouraged but not required for pull requests.

```bash
dotnet test SeqMcpServer.Tests.Integration
```

- Requires **Docker** to be running
- Testcontainers automatically starts a `datalust/seq:latest` container, seeds test data, and cleans up

### All tests

```bash
dotnet test
```

---

## Git Workflow

### Commit messages

- Write in **English**
- Use **imperative mood** (e.g., `Add query timeout parameter`, `Fix signal filtering`)
- Keep it short and descriptive
- Do **not** use prefixes like `feat:`, `fix:`, `chore:` — keep it simple

### Branch strategy

This project uses **version branches** (e.g., `1.0.0`, `1.0.1`, `1.1.0`). Before starting work:

1. **Open an issue** or comment on an existing one to discuss the change
2. **Target the highest version branch** (e.g., `1.1.0`) — this is the default for all contributions. If your change specifically targets an earlier version, discuss with the repository owner in the issue first.
3. Create your feature branch from the target version branch

### Before committing

- Update **`CHANGELOG.md`** — add an entry in the `[Unreleased]` section following the [Keep a Changelog](https://keepachangelog.com/) format
- Update **`README.md`** if your change affects user-facing documentation (e.g., new tools, configuration changes)

---

## Submitting a Pull Request

1. Fork the repository and create your branch from the target version branch
2. Implement your changes following the coding conventions above
3. Run unit tests locally and ensure they pass
4. Commit with a clear, imperative message
5. Open a pull request with a description of *what* and *why*

### Pull request checklist

- [ ] Unit tests pass (`dotnet test SeqMcpServer.Tests.Unit`)
- [ ] New functionality has unit tests
- [ ] `CHANGELOG.md` updated under `[Unreleased]`
- [ ] `README.md` updated (if applicable)
- [ ] All `[Description]` attributes present on new tool methods and parameters
- [ ] No hardcoded credentials or API keys
- [ ] Security review completed (no OWASP Top 10 issues, inputs validated, no vulnerable dependencies)
- [ ] Code follows project conventions (naming, style, error handling)

---

## Reporting Issues

Use [GitHub Issues](../../issues) to report bugs or suggest features. The repository provides **Issue forms** — please use [Bug Report](../../issues/new?template=bug-report.yml) for bugs and [Feature Request](../../issues/new?template=feature-request.yml) for new ideas. When filing a bug report, please include:

- Server version (`seq_get_version` tool output or `VersionInfo.Current`)
- Steps to reproduce
- Expected vs. actual behavior
- Relevant log output (if any)

> **Automated validation**: A GitHub Actions workflow automatically checks whether issues are created from the provided templates and contain all required sections. Issues that don’t match a template or have missing sections will be labeled `invalid-template` and receive a comment with guidance. You can edit the issue to fix it — the validation runs again automatically.

---

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE) that covers this project.
