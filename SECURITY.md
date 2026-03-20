# Security Policy

Thank you for helping keep **Seq MCP Server** and its users safe. We take security vulnerabilities seriously and appreciate responsible disclosure.

---

## Supported Versions

| Version | Supported          |
|---------|--------------------|
| 1.1.x   | :white_check_mark: |
| < 1.1   | :x:                |

Only the latest released version receives security updates. If you are using an older version, please upgrade before reporting.

---

## Reporting a Vulnerability

**Please do NOT report security vulnerabilities through public issues, discussions, or pull requests.**

Instead, use [GitHub Security Advisories](https://github.com/Finfinder/SeqMcpServer/security/advisories/new) to report vulnerabilities privately.

This ensures the issue is handled confidentially and a fix can be prepared before public disclosure.

---

## What to Include

To help us understand and resolve the issue quickly, please include as much of the following as possible:

- A clear description of the vulnerability
- Step-by-step instructions to reproduce the issue
- The potential impact (e.g., data exposure, denial of service)
- The affected version(s) of Seq MCP Server
- The affected source code location (file, function, or module)
- Any special configuration required to reproduce the issue
- A suggested fix or mitigation (optional, but appreciated)

---

## Response Timeline

| Stage           | Target Time   |
|-----------------|---------------|
| Acknowledgement | Within 48 hours |
| Assessment      | Within 14 days  |
| Fix             | Within 90 days  |

We will keep you informed of our progress throughout the process.

---

## Scope

### In-scope

- The SeqMcpServer codebase and its published artifacts (NuGet package, GitHub releases)

### Out-of-scope

The following are maintained by other organizations and should be reported to them directly:

- **Seq server** — report to [Datalust](https://datalust.co/support)
- **MCP protocol** — report to the [Model Context Protocol project](https://github.com/modelcontextprotocol)
- **.NET runtime** — report to [Microsoft](https://github.com/dotnet/runtime/blob/main/SECURITY.md)

---

## Disclosure Policy

We follow a **coordinated disclosure** model:

1. The reporter submits a vulnerability through [GitHub Security Advisories](https://github.com/Finfinder/SeqMcpServer/security/advisories/new).
2. We acknowledge receipt, investigate, and work on a fix within the timelines above.
3. Once a fix is available, we publish a GitHub Security Advisory with a CVE identifier.
4. The reporter receives credit in the advisory and release notes (unless they prefer to remain anonymous).
5. If no fix is released within **90 days**, the reporter may disclose the vulnerability publicly.

---

## Safe Harbor

We consider security research conducted in good faith to be authorized and welcome it. We will not pursue legal action against researchers who:

- Act in good faith to avoid harm to users and the project
- Report vulnerabilities through the designated channel described above
- Avoid accessing or modifying other users' data
- Do not exploit vulnerabilities beyond the minimum necessary to demonstrate the issue
- Follow the coordinated disclosure model outlined in this policy

If you are unsure whether your research complies with this policy, please reach out through a [GitHub Security Advisory](https://github.com/Finfinder/SeqMcpServer/security/advisories/new) before proceeding.
