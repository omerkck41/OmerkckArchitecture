#!/usr/bin/env bash
# Generates a solution from the packed Kck.Templates and builds (and tests) it against the packages of this commit.
# Usage: template-smoke.sh <feed-dir> <work-dir> <name> [dotnet new options...]
#   feed-dir: folder with every Kck *.nupkg of this commit (their prerelease versions are not on nuget.org)
#   work-dir: folder outside the repository, so the generated solution does not inherit the repo's
#             Directory.Build.props, Directory.Packages.props or global.json
#   name:     the -n value, e.g. Smoke, My.Shop, my-shop
# SMOKE_RUN_TESTS=0 skips `dotnet test` (databases other than SQLite need a server).
# SMOKE_RUN_APP=1 also starts the API and waits for /health/ready (plan G1.6); timings go to stdout and, in CI, to the
# job summary. SMOKE_TARGET_SECONDS (default 60) is the target for `dotnet new` -> healthy; exceeding it only warns.
set -euo pipefail

now() { date +%s.%N; }
elapsed() { awk -v a="$1" -v b="$2" 'BEGIN { printf "%.1f", b - a }'; }

# Absolute, native paths: NuGet on Windows reads a Git Bash path such as /tmp/feed as C:\tmp\feed.
native_path() {
  local dir
  dir="$(cd "$1" && pwd)"
  if command -v cygpath >/dev/null 2>&1; then
    cygpath -m "$dir"
  else
    echo "$dir"
  fi
}

feed="$(native_path "$1")"
mkdir -p "$2"
work="$(native_path "$2")"
name="$3"
shift 3
hive="$work/.template-hive"   # isolated template cache; leaves the user's `dotnet new` installs untouched
output="$work/$name"

shopt -s nullglob
templates=("$feed"/Kck.Templates.*.nupkg)
if [[ ${#templates[@]} -ne 1 ]]; then
  echo "::error::expected exactly one Kck.Templates package in $feed, found ${#templates[@]}"
  exit 1
fi

cat > "$work/nuget.config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-kck" value="$feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF

dotnet new install "${templates[0]}" --force --debug:custom-hive "$hive" >/dev/null
t_start="$(now)"
dotnet new kck-webapi -n "$name" -o "$output" --debug:custom-hive "$hive" "$@"   # includes the restore post action
t_generated="$(now)"

if grep -rn "KCK_PACKAGE_VERSION\|KCK_VERSION(\|TEMPLATE_RAW_NAME" "$output"; then
  echo "::error::template placeholder left in the generated solution"
  exit 1
fi

dotnet build "$output" --configuration Release -warnaserror
t_built="$(now)"

if [[ "${SMOKE_RUN_APP:-0}" == "1" ]]; then
  # The first launch profile ("http"): Development (creates the schema) on http://localhost:5080.
  dotnet run --no-build --configuration Release --project "$output/src/$name.Api" > "$work/app.log" 2>&1 &
  app_pid=$!
  trap 'kill "$app_pid" 2>/dev/null || true' EXIT
  healthy=0
  for _ in $(seq 1 240); do
    if curl -fsS http://localhost:5080/health/ready -o "$work/health.json" 2>/dev/null; then
      healthy=1
      break
    fi
    sleep 0.5
  done
  t_ready="$(now)"
  kill "$app_pid" 2>/dev/null || true
  if [[ "$healthy" != "1" ]]; then
    echo "::error::the API did not answer /health/ready within 120 s"
    cat "$work/app.log"
    exit 1
  fi
  grep -q '"status":"Healthy"' "$work/health.json" || { echo "::error::/health/ready is not Healthy"; cat "$work/health.json"; exit 1; }

  total="$(elapsed "$t_start" "$t_ready")"
  report="| Phase | Seconds |
|---|---|
| dotnet new + restore | $(elapsed "$t_start" "$t_generated") |
| build | $(elapsed "$t_generated" "$t_built") |
| start -> /health/ready 200 | $(elapsed "$t_built" "$t_ready") |
| **total** | **$total** (target ${SMOKE_TARGET_SECONDS:-60}) |"
  echo "$report"
  if [[ -n "${GITHUB_STEP_SUMMARY:-}" ]]; then
    printf '### kck-webapi: dotnet new -> healthy\n\n%s\n' "$report" >> "$GITHUB_STEP_SUMMARY"
  fi
  if awk -v t="$total" -v max="${SMOKE_TARGET_SECONDS:-60}" 'BEGIN { exit !(t > max) }'; then
    echo "::warning::dotnet new -> healthy took ${total} s, above the ${SMOKE_TARGET_SECONDS:-60} s target"
  fi
fi

if [[ -d "$output/tests" && "${SMOKE_RUN_TESTS:-1}" != "0" ]]; then
  dotnet test "$output" --configuration Release --no-build
fi
echo "template smoke: $name ($*) generated, built$([[ -d "$output/tests" && "${SMOKE_RUN_TESTS:-1}" != "0" ]] && echo ' and tested') from $(basename "${templates[0]}")"
