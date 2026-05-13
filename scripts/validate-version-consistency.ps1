[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [Parameter(Mandatory = $true)]
    [string]$VersionTargetsPath,

    [Parameter(Mandatory = $true)]
    [string]$ReadmeTargetsPath,

    [string]$ExpectedVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path (Split-Path -Parent $PSCommandPath) 'release-script-common.ps1')
. (Join-Path (Split-Path -Parent $PSCommandPath) 'version-target-strategies.ps1')

function Read-TargetVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory = $true)]
        [pscustomobject]$Target,

        [Parameter(Mandatory = $true)]
        [string]$TargetLabel
    )

    if ([string]::IsNullOrWhiteSpace($Target.path)) {
        throw "$TargetLabel path must not be empty."
    }

    if ([string]::IsNullOrWhiteSpace($Target.strategy)) {
        throw "$TargetLabel strategy must not be empty for $($Target.path)."
    }

    $targetPath = Resolve-CanonicalPath -LiteralPath (Join-Path $RepositoryRoot $Target.path) -Label $TargetLabel
    Assert-PathInsideRepository -RepositoryRoot $RepositoryRoot -Path $targetPath -Label $TargetLabel

    $content = Get-Content -LiteralPath $targetPath -Raw -Encoding UTF8
    $rawVersion = Get-TargetVersionFromContent -Content $content -Strategy $Target.strategy

    if ([string]::IsNullOrWhiteSpace($rawVersion)) {
        throw "Could not read version using strategy $($Target.strategy): $($Target.path)"
    }

    return [pscustomobject]@{
        RelativePath = Get-RelativePath -BasePath $RepositoryRoot -TargetPath $targetPath
        Strategy = $Target.strategy
        Version = Normalize-Version -InputVersion $rawVersion -Label $Target.path
    }
}

function Assert-TargetCollectionNotEmpty {
    param(
        [object[]]$Targets,

        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    if ($null -eq $Targets -or @($Targets).Count -eq 0) {
        throw "$Label must not be empty."
    }
}

function Assert-AllVersionsMatch {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Targets,

        [Parameter(Mandatory = $true)]
        [string]$ExpectedVersion
    )

    foreach ($target in $Targets) {
        if ($target.Version -ne $ExpectedVersion) {
            throw "Inconsistent version in $($target.RelativePath): expected $ExpectedVersion, found $($target.Version)"
        }
    }
}

$resolvedRepositoryRoot = Resolve-CanonicalPath -LiteralPath $RepositoryRoot -Label 'Repository root'
$resolvedVersionTargetsPath = Resolve-CanonicalPath -LiteralPath $VersionTargetsPath -Label 'Version targets path'
$resolvedReadmeTargetsPath = Resolve-CanonicalPath -LiteralPath $ReadmeTargetsPath -Label 'Readme targets path'

Assert-PathInsideRepository -RepositoryRoot $resolvedRepositoryRoot -Path $resolvedVersionTargetsPath -Label 'Version targets path'
Assert-PathInsideRepository -RepositoryRoot $resolvedRepositoryRoot -Path $resolvedReadmeTargetsPath -Label 'Readme targets path'

$versionTargets = Read-JsonArray -Path $resolvedVersionTargetsPath -Label 'Version targets'
$readmeTargets = Read-JsonArray -Path $resolvedReadmeTargetsPath -Label 'Readme targets'

Assert-TargetCollectionNotEmpty -Targets $versionTargets -Label 'Version targets'
Assert-TargetCollectionNotEmpty -Targets $readmeTargets -Label 'Readme targets'

$resolvedVersionTargets = @($versionTargets | ForEach-Object {
    Read-TargetVersion -RepositoryRoot $resolvedRepositoryRoot -Target $_ -TargetLabel 'Target file'
})
$resolvedReadmeTargets = @($readmeTargets | ForEach-Object {
    Read-TargetVersion -RepositoryRoot $resolvedRepositoryRoot -Target $_ -TargetLabel 'Target file'
})

$canonicalVersion = $resolvedVersionTargets[0].Version
Assert-AllVersionsMatch -Targets $resolvedVersionTargets -ExpectedVersion $canonicalVersion
Assert-AllVersionsMatch -Targets $resolvedReadmeTargets -ExpectedVersion $canonicalVersion

if (-not [string]::IsNullOrWhiteSpace($ExpectedVersion)) {
    $normalizedExpectedVersion = Normalize-Version -InputVersion $ExpectedVersion -Label 'expected_version'
    if ($canonicalVersion -ne $normalizedExpectedVersion) {
        throw "Inconsistent version in expected_version: expected $normalizedExpectedVersion, found $canonicalVersion"
    }
}

return [pscustomobject]@{
    Version = $canonicalVersion
    VersionTargets = $resolvedVersionTargets
    ReadmeTargets = $resolvedReadmeTargets
    ValidatedTargetCount = $resolvedVersionTargets.Count + $resolvedReadmeTargets.Count
}
