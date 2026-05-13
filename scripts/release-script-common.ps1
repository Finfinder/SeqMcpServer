Set-StrictMode -Version Latest

function Resolve-CanonicalPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$LiteralPath,

        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    if (-not (Test-Path -LiteralPath $LiteralPath)) {
        throw "$Label does not exist: $LiteralPath"
    }

    $resolvedPath = Resolve-Path -LiteralPath $LiteralPath
    return [System.IO.Path]::GetFullPath($resolvedPath.ProviderPath)
}

function Test-IsChildPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ParentPath,

        [Parameter(Mandatory = $true)]
        [string]$ChildPath
    )

    $normalizedParent = [System.IO.Path]::GetFullPath($ParentPath)
    $normalizedChild = [System.IO.Path]::GetFullPath($ChildPath)

    if ($normalizedParent.Length -gt 3) {
        $normalizedParent = $normalizedParent.TrimEnd('\', '/')
    }

    if ($normalizedChild.Length -gt 3) {
        $normalizedChild = $normalizedChild.TrimEnd('\', '/')
    }

    if ($normalizedChild.Equals($normalizedParent, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    $parentWithSeparator = $normalizedParent + [System.IO.Path]::DirectorySeparatorChar
    return $normalizedChild.StartsWith($parentWithSeparator, [System.StringComparison]::OrdinalIgnoreCase)
}

function Assert-PathInsideRepository {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    if (-not (Test-IsChildPath -ParentPath $RepositoryRoot -ChildPath $Path)) {
        throw "$Label must be located inside repository root: $Path"
    }
}

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BasePath,

        [Parameter(Mandatory = $true)]
        [string]$TargetPath
    )

    $normalizedBasePath = [System.IO.Path]::GetFullPath($BasePath)
    if (-not $normalizedBasePath.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
        $normalizedBasePath += [System.IO.Path]::DirectorySeparatorChar
    }

    $baseUri = New-Object System.Uri($normalizedBasePath)
    $targetUri = New-Object System.Uri([System.IO.Path]::GetFullPath($TargetPath))
    $relativePath = [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString())

    return $relativePath
}

function Normalize-Version {
    param(
        [Parameter(Mandatory = $true)]
        [string]$InputVersion,

        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    $match = [System.Text.RegularExpressions.Regex]::Match(
        $InputVersion,
        '^(?:v)?(?<version>(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?)$'
    )

    if (-not $match.Success) {
        throw "$Label must be a semver value such as 1.2.3 or v1.2.3. Received: $InputVersion"
    }

    return $match.Groups['version'].Value
}

function Read-JsonObject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    $content = Get-Content -LiteralPath $Path -Raw -Encoding UTF8
    if ([string]::IsNullOrWhiteSpace($content)) {
        throw "$Label must not be empty: $Path"
    }

    return $content | ConvertFrom-Json
}

function Read-JsonArray {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    $parsedValue = Read-JsonObject -Path $Path -Label $Label
    if ($null -eq $parsedValue) {
        Write-Output -NoEnumerate -InputObject @()
        return
    }

    Write-Output -NoEnumerate -InputObject @($parsedValue)
}
