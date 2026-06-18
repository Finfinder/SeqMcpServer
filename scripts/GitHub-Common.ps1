Set-StrictMode -Version Latest

# Common helper functions for GitHub CLI automation scripts in this repository.
# This file is dot-sourced by individual scripts (e.g. seed-github-issues.ps1,
# sync-github-meta.ps1) and must not contain business logic specific to a
# single script.

function Write-Log {
    param(
        [string]$Message,
        [string]$Prefix = '[common]'
    )

    Write-Host "$Prefix $Message"
}

function Invoke-GhText {
    param([string[]]$Arguments)

    $output = & gh @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        $rendered = ($output | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
        throw "gh command failed: gh $($Arguments -join ' ')$([Environment]::NewLine)$rendered"
    }

    return (($output | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine).Trim()
}

function Invoke-GhJson {
    param([string[]]$Arguments)

    $text = Invoke-GhText -Arguments $Arguments
    if ([string]::IsNullOrWhiteSpace($text)) {
        return $null
    }

    return $text | ConvertFrom-Json -Depth 100
}

function Invoke-GhReadyCheck {
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
        throw "GitHub CLI ('gh') was not found in PATH."
    }

    & gh --version *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub CLI ('gh') could not be started."
    }

    & gh auth status *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub CLI is not authenticated. Run 'gh auth login' first."
    }
}

function Resolve-PathFromRepoRoot {
    param(
        [string]$Path,
        [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot)
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path $RepoRoot $Path
}

function Get-NormalizedSourceIds {
    param([string[]]$SourceIds)

    return @(
        $SourceIds |
            Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) } |
            ForEach-Object { ([string]$_).Trim().ToUpperInvariant() } |
            Sort-Object -Unique
    )
}

function Get-SourceIdKey {
    param([string[]]$SourceIds)

    return (Get-NormalizedSourceIds -SourceIds $SourceIds) -join '|'
}

function Get-SourceIdsFromBody {
    param(
        [AllowNull()][string]$Body,
        [string]$SourceIdPattern = 'SEQ-\d+'
    )

    if ([string]::IsNullOrWhiteSpace($Body)) {
        return @()
    }

    $match = [regex]::Match($Body, '(?im)^\s*Original backlog IDs:\s*(?<ids>.+?)\s*$')
    if (-not $match.Success) {
        return @()
    }

    $matches = [regex]::Matches($match.Groups['ids'].Value, $SourceIdPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if (-not $matches -or $matches.Count -eq 0) {
        return @()
    }

    return Get-NormalizedSourceIds -SourceIds @($matches | ForEach-Object { $_.Value })
}
