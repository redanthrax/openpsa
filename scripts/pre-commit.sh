#!/usr/bin/env bash
# OpenPSA pre-commit hook: format staged C# files.
# Install:  ln -sf ../../scripts/pre-commit.sh .git/hooks/pre-commit && chmod +x scripts/pre-commit.sh
set -euo pipefail

staged_cs=$(git diff --cached --name-only --diff-filter=ACM | grep -E '\.(cs|razor)$' || true)
if [[ -z "$staged_cs" ]]; then
    exit 0
fi

echo "pre-commit: dotnet format on staged files"
# Format the whole solution (fast on incremental) and re-add anything format touched.
dotnet format OpenPsa.slnx --no-restore --verbosity quiet
echo "$staged_cs" | xargs git add
