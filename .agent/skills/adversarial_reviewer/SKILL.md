---
name: Adversarial Reviewer
description: Generates a 'Red Team' critique of recent code or plans to identify weak assumptions and edge cases.
version: 1.0.0
author: Antigravity Skills Library
created: 2026-01-16
leverage_score: 5/5
---

# SKILL-019: Adversarial Reviewer

## Overview

Forces a context switch from "Builder" to "Attacker". Ideally used before finalizing any critical component (e.g. auth, payments, file I/O). It prepares a **structured prompt packet** that the agent then uses to critique its own work.

## Trigger Phrases

- `red team this`
- `adversarial review`
- `find bugs`
- `critique code`

## Inputs

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `--FilePath` | string | Yes | - | Path to the file to review |
| `--Mode` | string | No | `Security` | `Security`, `Performance`, or `Logic` |

## Outputs

### 1. Structured Prompt (Markdown)

A prompt template pre-filled with the code and specific attack vectors. **The Agent must then "simulate" the adversary by responding to this prompt.**

```markdown
# ⚔️ ADVERSARIAL REVIEW REQUEST
**TARGET:** src/Auth.cs
**MODE:** SECURITY

## INSTRUCTIONS
You are now the ADVERSARY. Break this code.
Look for:
1. Race Conditions
2. Replay Attacks
...
```

## Preconditions

1. Target file exists.
2. PowerShell 5.1+ or Core 7+.

## Safety/QA Checks

1. **Read-Only**: Does not modify the code; only reads it.
2. **Size Limit**: If file is massive (>10k lines), warns or truncates to prevent token overflow.

## Stop Conditions

| Condition | Action |
|-----------|--------|
| File not found | Return error |

## Implementation

See `scripts/prepare_review.ps1`.

## Integration with Other Skills

1. Call `prepare_review.ps1`.
2. **Agent Step**: Read the output text.
3. **Agent Step**: "Thinking" block -> Process the critique.
4. **Agent Step**: Generate list of fixes.
