#!/usr/bin/env bash
# Generates a project from the packed Kck.Templates and builds it against the packages of this commit.
# Usage: template-smoke.sh <feed-dir> <work-dir>
#   feed-dir: folder with every Kck *.nupkg of this commit (their prerelease versions are not on nuget.org)
#   work-dir: empty folder outside the repository, so the generated project does not inherit the repo's
#             Directory.Build.props, Directory.Packages.props or global.json
set -euo pipefail

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
hive="$work/.template-hive"   # isolated template cache; leaves the user's `dotnet new` installs untouched

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

dotnet new install "${templates[0]}" --debug:custom-hive "$hive"
dotnet new kck-webapi -n Smoke -o "$work/Smoke" --debug:custom-hive "$hive"

if grep -rn "KCK_PACKAGE_VERSION" "$work/Smoke"; then
  echo "::error::template version placeholder left in the generated project"
  exit 1
fi

dotnet build "$work/Smoke" --configuration Release -warnaserror
echo "template smoke: generated and built $(basename "${templates[0]}")"
