Set-StrictMode -Version Latest

. (Join-Path (Split-Path -Parent $PSCommandPath) 'release-script-common.ps1')

function Get-RequiredManifestValue {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Manifest,

        [Parameter(Mandatory = $true)]
        [string]$FieldName
    )

    $property = $Manifest.PSObject.Properties[$FieldName]
    if ($null -eq $property -or [string]::IsNullOrWhiteSpace([string]$property.Value)) {
        throw "Manifest must contain a non-empty $FieldName value."
    }

    return [string]$property.Value
}

function Read-NextVersionManifest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory = $true)]
        [string]$ManifestPath,

        [string]$ExpectedReleaseVersion
    )

    $resolvedRepositoryRoot = Resolve-CanonicalPath -LiteralPath $RepositoryRoot -Label 'Repository root'
    $resolvedManifestPath = Resolve-CanonicalPath -LiteralPath $ManifestPath -Label 'Manifest path'

    Assert-PathInsideRepository -RepositoryRoot $resolvedRepositoryRoot -Path $resolvedManifestPath -Label 'Manifest path'

    $manifest = Read-JsonObject -Path $resolvedManifestPath -Label 'Manifest'
    $releaseVersion = Normalize-Version -InputVersion (Get-RequiredManifestValue -Manifest $manifest -FieldName 'release_version') -Label 'release_version'
    $nextVersion = Normalize-Version -InputVersion (Get-RequiredManifestValue -Manifest $manifest -FieldName 'next_version') -Label 'next_version'

    if ($releaseVersion -eq $nextVersion) {
        throw 'next_version must differ from release_version'
    }

    if (-not [string]::IsNullOrWhiteSpace($ExpectedReleaseVersion)) {
        $normalizedExpectedReleaseVersion = Normalize-Version -InputVersion $ExpectedReleaseVersion -Label 'release_version'
        if ($releaseVersion -ne $normalizedExpectedReleaseVersion) {
            throw "release_version '$releaseVersion' does not match tag version '$normalizedExpectedReleaseVersion'"
        }
    }

    return [pscustomobject]@{
        RepositoryRoot = $resolvedRepositoryRoot
        ManifestPath = $resolvedManifestPath
        ReleaseVersion = $releaseVersion
        NextVersion = $nextVersion
    }
}

function Write-NextVersionRequestArtifact {
    param(
        [Parameter(Mandatory = $true)]
        [string]$OutputDirectory,

        [Parameter(Mandatory = $true)]
        [string]$ReleaseVersion,

        [Parameter(Mandatory = $true)]
        [string]$NextVersion
    )

    if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
        throw 'Output directory must not be empty.'
    }

    $resolvedOutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
    $null = New-Item -ItemType Directory -Path $resolvedOutputDirectory -Force

    $artifactPath = Join-Path $resolvedOutputDirectory 'next-version.json'
    $payload = @{
        release_version = $ReleaseVersion
        next_version = $NextVersion
    } | ConvertTo-Json

    Set-Content -LiteralPath $artifactPath -Value ($payload + [Environment]::NewLine) -Encoding UTF8
    return [System.IO.Path]::GetFullPath($artifactPath)
}
