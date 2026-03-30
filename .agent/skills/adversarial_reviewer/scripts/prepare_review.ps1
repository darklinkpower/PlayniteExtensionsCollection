<#
.SYNOPSIS
    Prepares a context packet for an adversarial code review.
#>
[CmdletBinding()]
Param(
    [Parameter(Mandatory = $true)][string]$FilePath,
    [Parameter(Mandatory = $false)][string]$Mode = "Security"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $FilePath)) { Write-Error "File not found"; exit 1 }

$Content = Get-Content -Path $FilePath -Raw

$Focus = "General Quality"
if ($Mode -eq "Security") { $Focus = "1. Injection/Sanitization`n2. Auth Bypasses`n3. Data Leaks`n4. DOS Vectors" }
elseif ($Mode -eq "Performance") { $Focus = "1. O(n^2) loops`n2. Memory Leaks`n3. Blocking I/O`n4. Allocations" }
elseif ($Mode -eq "Logic") { $Focus = "1. Off-by-one`n2. Null Refs`n3. Race Conditions" }

$Template = @"
# ⚔️ ADVERSARIAL REVIEW REQUEST
**TARGET:** $(Split-Path $FilePath -Leaf)
**MODE:** $Mode

### 🎯 FOCUS AREAS:
$Focus

## 📄 CODE ARTIFACT
```
$Content
```
"@

Write-Output $Template
