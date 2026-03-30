<#
.SYNOPSIS
    Detects reasoning impasses and loops in agent execution.
.DESCRIPTION
    Analyzes conversation transcripts or logs to identify repetitive patterns,
    circular reasoning, or lack of progress (idempotent loops).
.PARAMETER TranscriptPath
    Path to the conversation log/json file.
.PARAMETER Content
    Direct string content to analyze.
.PARAMETER Lookback
    Number of recent turns to analyze. Default 10.
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory = $false)]
    [string]$TranscriptPath,

    [Parameter(Mandatory = $false)]
    [string]$Content,

    [int]$Lookback = 10
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-ImpasseScore {
    param([string]$Text)

    $Score = 0
    $Reasons = @()

    # 1. Check for "Apology Loop"
    $ApologyMatches = [regex]::Matches($Text, "apologize|sorry|mistake|overlooked|confusion", "IgnoreCase")
    if ($ApologyMatches.Count -gt 2) {
        $Score += 40
        if ($ApologyMatches.Count -gt 4) { $Score += 20 } # Bonus for severe looping
        $Reasons += "Apology loop detected ($($ApologyMatches.Count) occurrences)"
    }

    # 2. Check for "Try Again" Loop / futile effort
    # Broader regex to catch "I will fix", "retrying", "attempt 2", etc.
    $RetryMatches = [regex]::Matches($Text, "try again|attempting|let me fix|will fix|correcting|retrying|unable to|failed to", "IgnoreCase")
    if ($RetryMatches.Count -gt 2) {
        $Score += 30
        if ($RetryMatches.Count -gt 4) { $Score += 10 }
        $Reasons += "Repetitive retry/failure pattern ($($RetryMatches.Count) occurrences)"
    }

    # 3. Check for repetitive file reads (Heuristic)
    $ReadMatches = [regex]::Matches($Text, "(view_file|list_dir|read_resource|run_command)", "IgnoreCase")
    if ($ReadMatches.Count -gt 6) { 
        $Score += 20
        $Reasons += "High frequency of tool operations ($($ReadMatches.Count) in window)"
    }

    return @{ Score = $Score; Reasons = $Reasons }
}

try {
    $AnalysisContent = ""

    if ($TranscriptPath -and (Test-Path $TranscriptPath)) {
        $AnalysisContent = Get-Content -Path $TranscriptPath -Raw
    }
    elseif ($Content) {
        $AnalysisContent = $Content
    }
    else {
        Write-Output (@{ 
                status     = "UNKNOWN" 
                confidence = 0
                reason     = "No input provided" 
            } | ConvertTo-Json)
        exit 0
    }

    # Simple slice for lookback (approx 20 lines per turn)
    $Lines = $AnalysisContent -split "`n"
    if ($Lines.Count -gt ($Lookback * 20)) {
        $StartIndex = $Lines.Count - ($Lookback * 20)
        $AnalysisContent = ($Lines[$StartIndex..($Lines.Count - 1)]) -join "`n"
    }

    $Result = Get-ImpasseScore -Text $AnalysisContent

    $Status = "CLEAR"
    $Recommendation = "CONTINUE"

    if ($Result.Score -ge 50) {
        $Status = "IMPASSE"
        $Recommendation = "ESCALATE_TO_USER"
        
        if ($Result.Score -lt 80) {
            $Recommendation = "PAUSE_AND_REFLECT"
        }
    }

    $Output = @{
        status         = $Status
        confidence     = [Math]::Min(1.0, $Result.Score / 100)
        reasons        = $Result.Reasons
        recommendation = $Recommendation
        score          = $Result.Score
        timestamp      = (Get-Date).ToString("o")
    }

    Write-Output ($Output | ConvertTo-Json -Depth 3)

}
catch {
    Write-Output (@{ 
            status = "ERROR" 
            error  = $_.Exception.Message 
        } | ConvertTo-Json)
    exit 1
}
