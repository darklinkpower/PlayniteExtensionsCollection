$ScriptPath = Join-Path $PSScriptRoot "..\scripts\detect_impasse.ps1"
$ReportPath = Join-Path $PSScriptRoot "..\VERIFICATION_REPORT.md"

# Case 1: Clean (Pass)
$CleanInput = @"
User: Check the file.
Agent: Checks file.
Agent: Everything looks good.
User: Great, move on.
"@

# Case 2: Mild Impasse (Trigger Warning)
# 3 apologies (Threshold > 2), 2 retries (Threshold > 2 not met) -> Score 40 (Clear/Pause)
$MildInput = @"
Agent: I apologize.
Agent: I will fix.
Agent: I apologize.
Agent: Let me try.
Agent: I apologize.
"@

# Case 3: Severe Impasse (Trigger Critical)
# 5 apologies (Score 40+20=60), 5 retries (Score 30+10=40) -> Total 100
$SevereInput = @"
Agent: I apologize for the mistake.
Agent: Unable to read file.
Agent: I apologize.
Agent: Let me fix this.
Agent: I apologize.
Agent: Retrying command.
Agent: I apologize.
Agent: Failed to execute.
Agent: I apologize, complete failure.
Agent: Will fix now.
"@

Write-Host "Running 100% Quality Gate Validation for SKILL-017..."

$CleanResult = & $ScriptPath -Content $CleanInput | ConvertFrom-Json
$MildResult = & $ScriptPath -Content $MildInput | ConvertFrom-Json
$SevereResult = & $ScriptPath -Content $SevereInput | ConvertFrom-Json

$PassClean = $CleanResult.status -eq "CLEAR"
# Mild might be CLEAR or IMPASSE depending on score, but let's check score logic.
# 3 apologies = 40 pts. < 50 threshold. Should be CLEAR.
$PassMild = $MildResult.status -eq "CLEAR" -and $MildResult.score -eq 40

$PassSevere = $SevereResult.status -eq "IMPASSE" -and $SevereResult.score -ge 100

$AllPassed = $PassClean -and $PassMild -and $PassSevere

$Report = @"
# 🧪 Verification Report: SKILL-017 (Impasse Detector)
**Date:** $(Get-Date)
**Quality Gate:** 100% Pass Required

## Test Cases

### 1. Clean Path
**Input:** Simple conversation.
**Expected:** CLEAR
**Actual:** $($CleanResult.status)
**Pass:** $(if($PassClean){"✅"}else{"❌"})

### 2. Mild Loop (Below Threshold)
**Input:** 3 apologies.
**Expected:** CLEAR (Score 40)
**Actual:** $($MildResult.status) (Score $($MildResult.score))
**Pass:** $(if($PassMild){"✅"}else{"❌"})

### 3. Severe Impasse (Critical)
**Input:** 5 apologies, 5 retries.
**Expected:** IMPASSE (Score 100)
**Actual:** $($SevereResult.status) (Score $($SevereResult.score))
**Pass:** $(if($PassSevere){"✅"}else{"❌"})

## Final Verdict
$(if($AllPassed){"**✅ PASSED (100% Coverage)**"}else{"**❌ FAILED**"})
"@

Set-Content -Path $ReportPath -Value $Report
Write-Host "Report saved to $ReportPath"
Write-Output $Report
