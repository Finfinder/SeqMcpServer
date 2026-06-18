[CmdletBinding()]
param(
    [switch]$Apply,
    [string]$SeedPath = (Join-Path (Split-Path -Parent $PSScriptRoot) '.github/issue-seed.json'),
    [string]$Repo
)

. "$PSScriptRoot/GitHub-Common.ps1"

$repoRoot = Split-Path -Parent $PSScriptRoot
$mode = if ($Apply) { 'apply' } else { 'dry-run' }
$sourceIdPattern = 'SEQ-\d+'

function Get-RepoConfig {
    $configPath = Join-Path $repoRoot '.github/gh-sync.json'
    if (-not (Test-Path $configPath)) {
        throw "Default repo config not found: $configPath"
    }

    $config = Get-Content -Raw -Path $configPath | ConvertFrom-Json -Depth 100
    if (-not $config.repo.slug) {
        throw "Missing repo.slug in $configPath"
    }
    if (-not $config.labels) {
        throw "Missing labels section in $configPath"
    }
    if (-not $config.milestones) {
        throw "Missing milestones section in $configPath"
    }

    return $config
}

function Get-ConfiguredMilestoneTitles {
    param($RepoConfig)

    $titles = @()
    $seen = @{}

    foreach ($milestone in @($RepoConfig.milestones)) {
        $title = [string]$milestone.title
        if ([string]::IsNullOrWhiteSpace($title)) {
            throw "Each milestone in .github/gh-sync.json requires a title."
        }
        if ($seen.ContainsKey($title)) {
            throw "Duplicate milestone '$title' found in .github/gh-sync.json."
        }

        $seen[$title] = $true
        $titles += $title
    }

    return @($titles | Sort-Object)
}

function Get-IssueSeed {
    param(
        [string]$Path,
        [string[]]$AllowedLabels,
        [string[]]$AllowedMilestones
    )

    $resolvedPath = Resolve-PathFromRepoRoot -Path $Path
    if (-not (Test-Path $resolvedPath)) {
        throw "Seed file not found: $resolvedPath"
    }

    $seed = Get-Content -Raw -Path $resolvedPath | ConvertFrom-Json -Depth 100
    if (-not ($seed.PSObject.Properties.Name -contains 'issues')) {
        throw "Missing issues section in $resolvedPath"
    }
    if (-not $seed.issues -or $seed.issues.Count -eq 0) {
        throw "No issues found in $resolvedPath"
    }

    foreach ($issue in $seed.issues) {
        if ([string]::IsNullOrWhiteSpace([string]$issue.title)) {
            throw 'Every seed issue requires a title.'
        }
        if ([string]::IsNullOrWhiteSpace([string]$issue.body)) {
            throw "Seed issue '$([string]$issue.title)' is missing body."
        }
        if ([string]::IsNullOrWhiteSpace([string]$issue.milestone)) {
            throw "Seed issue '$([string]$issue.title)' is missing milestone."
        }
        if ($AllowedMilestones -notcontains [string]$issue.milestone) {
            throw "Seed issue '$([string]$issue.title)' references milestone '$([string]$issue.milestone)' that is not defined in .github/gh-sync.json."
        }
        if (-not $issue.labels -or $issue.labels.Count -eq 0) {
            throw "Seed issue '$([string]$issue.title)' requires at least one label."
        }
        if (-not $issue.sourceIds -or $issue.sourceIds.Count -eq 0) {
            throw "Seed issue '$([string]$issue.title)' requires at least one source id."
        }

        $declaredSourceIds = Get-NormalizedSourceIds -SourceIds @($issue.sourceIds | ForEach-Object { [string]$_ })
        $bodySourceIds = Get-SourceIdsFromBody -Body ([string]$issue.body)
        if ($bodySourceIds.Count -eq 0) {
            throw "Seed issue '$([string]$issue.title)' body must contain 'Original backlog IDs: ...'."
        }
        if ((Get-SourceIdKey -SourceIds $declaredSourceIds) -ne (Get-SourceIdKey -SourceIds $bodySourceIds)) {
            throw "Seed issue '$([string]$issue.title)' has mismatched source ids between sourceIds and body."
        }

        foreach ($label in @($issue.labels | ForEach-Object { [string]$_ })) {
            if ($AllowedLabels -notcontains $label) {
                throw "Seed issue '$([string]$issue.title)' uses unsupported label '$label'."
            }
        }
    }

    return [pscustomobject]@{
        Path = $resolvedPath
        Data = $seed
    }
}

function Get-ExistingIssueMaps {
    param([string]$Slug)

    $issues = Invoke-GhJson @('issue', 'list', '--repo', $Slug, '--state', 'all', '--limit', '500', '--json', 'number,title,body,labels,milestone')
    $titleMap = @{}
    $sourceIdMap = @{}
    if ($issues) {
        foreach ($issue in $issues) {
            $issueTitle = [string]$issue.title
            if ($titleMap.ContainsKey($issueTitle)) {
                $existingNumber = [string]$titleMap[$issueTitle].number
                throw "Duplicate remote issue title '$issueTitle' found on #$existingNumber and #$([string]$issue.number) in '$Slug'. Resolve the conflict before seeding."
            }

            $titleMap[$issueTitle] = $issue

            $sourceIdKey = Get-SourceIdKey -SourceIds (Get-SourceIdsFromBody -Body ([string]$issue.body))
            if (-not [string]::IsNullOrWhiteSpace($sourceIdKey)) {
                if ($sourceIdMap.ContainsKey($sourceIdKey)) {
                    $existingNumber = [string]$sourceIdMap[$sourceIdKey].number
                    throw "Duplicate remote issue sourceIds '$sourceIdKey' found on #$existingNumber and #$([string]$issue.number) in '$Slug'. Resolve the conflict before seeding."
                }

                $sourceIdMap[$sourceIdKey] = $issue
            }
        }
    }

    return [pscustomobject]@{
        ByTitle = $titleMap
        BySourceIds = $sourceIdMap
    }
}

function Get-ExistingMilestoneMap {
    param([string]$Slug)

    $milestones = Invoke-GhJson @('api', "repos/$Slug/milestones?state=all&per_page=100")
    $map = @{}
    if ($milestones) {
        foreach ($milestone in $milestones) {
            $map[[string]$milestone.title] = $milestone
        }
    }

    return $map
}

function Format-List {
    param([object[]]$Items)

    $rendered = @(
        $Items |
            ForEach-Object { [string]$_ } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )

    if ($rendered.Count -eq 0) {
        return 'none'
    }

    return ($rendered -join ', ')
}

function Get-LabelNamesFromIssue {
    param($Issue)

    if (-not $Issue -or -not $Issue.labels) {
        return @()
    }

    return @($Issue.labels | ForEach-Object { [string]$_.name } | Sort-Object -Unique)
}

function Get-MilestoneTitleFromIssue {
    param($Issue)

    if (-not $Issue -or -not $Issue.milestone) {
        return ''
    }

    return [string]$Issue.milestone.title
}

function Set-IssueInMaps {
    param(
        $Maps,
        [int]$Number,
        [string]$Title,
        [string]$Body,
        [string]$Milestone,
        [string[]]$Labels,
        [string]$PreviousTitle,
        [string[]]$PreviousSourceIds
    )

    if (-not [string]::IsNullOrWhiteSpace($PreviousTitle) -and $Maps.ByTitle.ContainsKey($PreviousTitle)) {
        [void]$Maps.ByTitle.Remove($PreviousTitle)
    }

    $previousSourceIdKey = Get-SourceIdKey -SourceIds $PreviousSourceIds
    if (-not [string]::IsNullOrWhiteSpace($previousSourceIdKey) -and $Maps.BySourceIds.ContainsKey($previousSourceIdKey)) {
        if ([string]$Maps.BySourceIds[$previousSourceIdKey].number -eq [string]$Number) {
            [void]$Maps.BySourceIds.Remove($previousSourceIdKey)
        }
    }

    $issueSnapshot = [pscustomobject]@{
        number = $Number
        title = $Title
        body = $Body
        milestone = if ([string]::IsNullOrWhiteSpace($Milestone)) { $null } else { [pscustomobject]@{ title = $Milestone } }
        labels = @($Labels | ForEach-Object { [pscustomobject]@{ name = $_ } })
    }

    $Maps.ByTitle[$Title] = $issueSnapshot

    $sourceIdKey = Get-SourceIdKey -SourceIds (Get-SourceIdsFromBody -Body $Body)
    if (-not [string]::IsNullOrWhiteSpace($sourceIdKey)) {
        $Maps.BySourceIds[$sourceIdKey] = $issueSnapshot
    }
}

$repoConfig = Get-RepoConfig
$allowedLabels = @($repoConfig.labels | ForEach-Object { [string]$_.name } | Sort-Object -Unique)
$allowedMilestones = Get-ConfiguredMilestoneTitles -RepoConfig $repoConfig
$seed = Get-IssueSeed -Path $SeedPath -AllowedLabels $allowedLabels -AllowedMilestones $allowedMilestones
$repoSlug = if ([string]::IsNullOrWhiteSpace($Repo)) { [string]$repoConfig.repo.slug } else { $Repo }

Write-Log -Prefix '[seed-github-issues]' "Using seed '$($seed.Path)'"
Write-Log -Prefix '[seed-github-issues]' "Target repo '$repoSlug'"

Invoke-GhReadyCheck

$existingIssues = Get-ExistingIssueMaps -Slug $repoSlug
$existingMilestones = Get-ExistingMilestoneMap -Slug $repoSlug

foreach ($issue in $seed.Data.issues) {
    $title = [string]$issue.title
    $body = [string]$issue.body
    $milestone = [string]$issue.milestone
    $labels = @($issue.labels | ForEach-Object { [string]$_ })
    $sourceIds = Get-NormalizedSourceIds -SourceIds @($issue.sourceIds | ForEach-Object { [string]$_ })

    if (-not $existingMilestones.ContainsKey($milestone)) {
        throw "Milestone '$milestone' does not exist in '$repoSlug'. Run 'scripts/sync-github-meta.ps1 -Apply' first."
    }

    $sourceIdKey = Get-SourceIdKey -SourceIds $sourceIds
    $titleMatch = $null
    $sourceIdMatch = $null

    if ($existingIssues.ByTitle.ContainsKey($title)) {
        $titleMatch = $existingIssues.ByTitle[$title]
    }
    if (-not [string]::IsNullOrWhiteSpace($sourceIdKey) -and $existingIssues.BySourceIds.ContainsKey($sourceIdKey)) {
        $sourceIdMatch = $existingIssues.BySourceIds[$sourceIdKey]
    }

    if ($titleMatch -and $sourceIdMatch -and ([string]$titleMatch.number -ne [string]$sourceIdMatch.number)) {
        throw "Conflicting remote matches for seed issue '$title': exact title matched #$([string]$titleMatch.number) but sourceIds matched #$([string]$sourceIdMatch.number). Resolve the conflict before seeding."
    }

    $matchedIssue = $null
    $matchReason = $null

    if ($titleMatch -and $sourceIdMatch) {
        $matchedIssue = $titleMatch
        $matchReason = 'exact title + sourceIds'
    }
    elseif ($titleMatch) {
        $matchedIssue = $titleMatch
        $matchReason = 'exact title'
    }
    elseif ($sourceIdMatch) {
        $matchedIssue = $sourceIdMatch
        $matchReason = 'sourceIds'
    }

    if ($matchedIssue) {
        $existingNumber = [string]$matchedIssue.number
        $existingLabels = Get-LabelNamesFromIssue -Issue $matchedIssue
        $existingMilestone = Get-MilestoneTitleFromIssue -Issue $matchedIssue
        $labelsToAdd = @($labels | Where-Object { $existingLabels -notcontains $_ })
        $labelsToRemove = @($existingLabels | Where-Object { $labels -notcontains $_ })
        $hasNoChanges =
            ([string]$matchedIssue.title -ceq $title) -and
            ([string]$matchedIssue.body -ceq $body) -and
            ($existingMilestone -ceq $milestone) -and
            ($labelsToAdd.Count -eq 0) -and
            ($labelsToRemove.Count -eq 0)

        if ($hasNoChanges) {
            if ($Apply) {
                Write-Log -Prefix '[seed-github-issues]' "Skipped issue #$existingNumber via ${matchReason}: no changes for '$title'"
            }
            else {
                    Write-Log -Prefix '[seed-github-issues]' "DRY-RUN: no changes for #$existingNumber via ${matchReason}: '$title'"
            }

            continue
        }

        if (-not $Apply) {
            Write-Log -Prefix '[seed-github-issues]' "DRY-RUN: would update #$existingNumber via $matchReason to '$title' [milestone: $milestone] [add labels: $(Format-List -Items $labelsToAdd)] [remove labels: $(Format-List -Items $labelsToRemove)] [sourceIds: $(Format-List -Items $sourceIds)]"
            continue
        }

        $arguments = @('issue', 'edit', $existingNumber, '--repo', $repoSlug, '--title', $title, '--body', $body, '--milestone', $milestone)
        foreach ($label in $labelsToAdd) {
            $arguments += @('--add-label', $label)
        }
        foreach ($label in $labelsToRemove) {
            $arguments += @('--remove-label', $label)
        }

        $previousTitle = [string]$matchedIssue.title
        $previousSourceIds = Get-SourceIdsFromBody -Body ([string]$matchedIssue.body)
        Invoke-GhText -Arguments $arguments | Out-Null
        Set-IssueInMaps -Maps $existingIssues -Number ([int]$matchedIssue.number) -Title $title -Body $body -Milestone $milestone -Labels $labels -PreviousTitle $previousTitle -PreviousSourceIds $previousSourceIds
        Write-Log -Prefix '[seed-github-issues]' "Updated issue #$existingNumber via ${matchReason}: '$title'"
        continue
    }

    if (-not $Apply) {
        Write-Log -Prefix '[seed-github-issues]' "DRY-RUN: would create '$title' [milestone: $milestone] [labels: $(Format-List -Items $labels)] [sourceIds: $(Format-List -Items $sourceIds)]"
        continue
    }

    $arguments = @('issue', 'create', '--repo', $repoSlug, '--title', $title, '--body', $body, '--milestone', $milestone)
    foreach ($label in $labels) {
        $arguments += @('--label', $label)
    }

    $result = Invoke-GhText -Arguments $arguments
    if ($result -match '/issues/(?<number>\d+)$') {
        Set-IssueInMaps -Maps $existingIssues -Number ([int]$Matches.number) -Title $title -Body $body -Milestone $milestone -Labels $labels -PreviousTitle '' -PreviousSourceIds @()
    }

    Write-Log -Prefix '[seed-github-issues]' "Created issue '$title'"
}

Write-Log -Prefix '[seed-github-issues]' "Completed in $mode mode."
if (-not $Apply) {
    Write-Log -Prefix '[seed-github-issues]' 'Use -Apply to create or update issues after milestones are synced.'
}
