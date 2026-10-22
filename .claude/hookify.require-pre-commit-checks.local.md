---
name: require-pre-commit-checks
enabled: true
event: bash
pattern: \bgit\s+commit\b|\bjj(\s+--?\S+)*\s+(commit|ci|describe|desc|squash)\b
action: warn
---

**Pre-commit check required.**

Before proceeding, list the files in the change:

- jj: `jj diff --name-only` (for `jj describe -r <rev>` or `jj squash --into <rev>`, also check `-r <rev>`)
- git: `git diff --cached --name-only`

A `jj describe` that does not finalise a change (a working note on an in-progress change) needs no checks.

**If all changed files are documentation only** (`.md` files, `docs/`, `README`):
- No build or test checks required. Proceed.

**If any .NET source files are changed** (`.cs`, `.csproj`, `.sln`, `.editorconfig`, `.props`, `.targets`):
- You must run `scripts/pre-commit.sh` before committing. Under git, the installed hook runs it.

It must pass with zero errors. Stop and fix on any failure — never skip with `--no-verify`.

If you have already completed all checks in this session and they passed, you may proceed.
