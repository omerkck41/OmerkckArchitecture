window.BENCHMARK_DATA = {
  "lastUpdate": 1793128941131,
  "repoUrl": "https://github.com/omerkck41/OmerkckArchitecture",
  "entries": {
    "Benchmark": [
      {
        "commit": {
          "author": {
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41",
            "email": "106805727+omerkck41@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "a2f51b52cf499df80e486265fd327ff2a491fb68",
          "message": "fix(ci): benchmark JSON pattern + GitHub Pages setup (#91)\n\n## Summary\n- **benchmark**: `find` pattern `*-report.json` → `*-report*.json` —\n`JsonExporter.Brief` produces `*-report-brief.json` files which the old\npattern missed\n- **docs**: GitHub Pages enabled via API (`build_type: workflow`) —\ndeploy step was returning 404\n\n## Context\nPR #90 fixed the root causes for mutation/benchmark/docs workflows.\nPost-merge CI revealed two remaining issues:\n1. Benchmark JSON output uses `-report-brief.json` suffix (not\n`-report.json`)\n2. GitHub Pages was not enabled on the repository\n\nmutation workflow's last failure was from the pre-merge scheduled run\n(2026-05-24 06:01 UTC). The config fix is already in main.\n\n## Test plan\n- [ ] benchmark workflow: verify JSON files found and merged after push\nto main\n- [ ] docs workflow: verify DocFX build + GitHub Pages deploy succeeds\n- [ ] mutation workflow: trigger via workflow_dispatch or wait for next\nSunday schedule\n\n:robot: Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 4.7 <noreply@anthropic.com>",
          "timestamp": "2026-05-24T21:53:57Z",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/a2f51b52cf499df80e486265fd327ff2a491fb68"
        },
        "date": 1779660611435,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.6209645328613425,
            "unit": "ns",
            "range": "± 0.02813012774235135"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.921574579675992,
            "unit": "ns",
            "range": "± 0.20585484304667284"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.2272621789574623,
            "unit": "ns",
            "range": "± 0.18143684900631435"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.13417629400889078,
            "unit": "ns",
            "range": "± 0.003340584737591309"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 404.98516060755804,
            "unit": "ns",
            "range": "± 0.8712106153446443"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 712.658324877421,
            "unit": "ns",
            "range": "± 2.694332594459767"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.6209645328613425,
            "unit": "ns",
            "range": "± 0.02813012774235135"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.921574579675992,
            "unit": "ns",
            "range": "± 0.20585484304667284"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.2272621789574623,
            "unit": "ns",
            "range": "± 0.18143684900631435"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.13417629400889078,
            "unit": "ns",
            "range": "± 0.003340584737591309"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.993403466542562,
            "unit": "ns",
            "range": "± 0.13035684728176783"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.634690840542316,
            "unit": "ns",
            "range": "± 0.21229088479900454"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.197282794330802,
            "unit": "ns",
            "range": "± 0.17388972566723188"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.354369775702557,
            "unit": "ns",
            "range": "± 0.019990523199366877"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.912277054328184,
            "unit": "ns",
            "range": "± 0.029664011412338737"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 12.374415291043428,
            "unit": "ns",
            "range": "± 0.02427523887077533"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.993403466542562,
            "unit": "ns",
            "range": "± 0.13035684728176783"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.634690840542316,
            "unit": "ns",
            "range": "± 0.21229088479900454"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.197282794330802,
            "unit": "ns",
            "range": "± 0.17388972566723188"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.354369775702557,
            "unit": "ns",
            "range": "± 0.019990523199366877"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.912277054328184,
            "unit": "ns",
            "range": "± 0.029664011412338737"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 12.374415291043428,
            "unit": "ns",
            "range": "± 0.02427523887077533"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 404.98516060755804,
            "unit": "ns",
            "range": "± 0.8712106153446443"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 712.658324877421,
            "unit": "ns",
            "range": "± 2.694332594459767"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "02627083e18138de49579a4b474b19a4569f978c",
          "message": "fix(ci): move stryker output dir to CLI flag (#92)\n\n## Summary\n- Stryker.NET 4.8.0 does not accept `output` or `output-path` in config\nfiles — only as `--output` CLI argument\n- Remove unsupported key from all 3 stryker config files\n- Pass `--output mutation-report/<module>` via CLI in mutation workflow\n\n## Context\nPR #90 renamed `output-path` to `output` but neither key is valid in\nStryker 4.8.0's config schema. The allowed config keys are:\n`additional-timeout`, `mutation-level`, `project`, `project-info`,\n`report-file-name`, `reporters`, `since`, `solution`,\n`target-framework`, `test-case-filter`, `test-projects`, `thresholds`,\n`verbosity`.\n\n## Test plan\n- [ ] Trigger mutation workflow via workflow_dispatch after merge\n- [ ] Verify all 3 matrix jobs (core, caching, security) pass\n\n:robot: Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 4.7 <noreply@anthropic.com>",
          "timestamp": "2026-05-24T22:25:37Z",
          "tree_id": "d5fd2c4a8915fa82e3bcabe8a561c346318b57d2",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/02627083e18138de49579a4b474b19a4569f978c"
        },
        "date": 1779661955851,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.89529705620729,
            "unit": "ns",
            "range": "± 0.07621054382408847"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.161376205086707,
            "unit": "ns",
            "range": "± 0.21202000725186632"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6376991724738708,
            "unit": "ns",
            "range": "± 0.00433452793100092"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.683873155287334,
            "unit": "ns",
            "range": "± 0.004878552190412419"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 465.7116725921631,
            "unit": "ns",
            "range": "± 2.403164317913461"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 793.6127172470093,
            "unit": "ns",
            "range": "± 5.554332794775759"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.89529705620729,
            "unit": "ns",
            "range": "± 0.07621054382408847"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.161376205086707,
            "unit": "ns",
            "range": "± 0.21202000725186632"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6376991724738708,
            "unit": "ns",
            "range": "± 0.00433452793100092"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.683873155287334,
            "unit": "ns",
            "range": "± 0.004878552190412419"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.427820303610392,
            "unit": "ns",
            "range": "± 0.19302510022605987"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.688490637038884,
            "unit": "ns",
            "range": "± 0.3023845671679136"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.538749978939693,
            "unit": "ns",
            "range": "± 0.11938704561663634"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.429306491145066,
            "unit": "ns",
            "range": "± 0.3773916854494944"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.150506963332495,
            "unit": "ns",
            "range": "± 0.15979253045801764"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.454216781258584,
            "unit": "ns",
            "range": "± 0.25300172587645964"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.427820303610392,
            "unit": "ns",
            "range": "± 0.19302510022605987"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.688490637038884,
            "unit": "ns",
            "range": "± 0.3023845671679136"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.538749978939693,
            "unit": "ns",
            "range": "± 0.11938704561663634"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.429306491145066,
            "unit": "ns",
            "range": "± 0.3773916854494944"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.150506963332495,
            "unit": "ns",
            "range": "± 0.15979253045801764"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.454216781258584,
            "unit": "ns",
            "range": "± 0.25300172587645964"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 465.7116725921631,
            "unit": "ns",
            "range": "± 2.403164317913461"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 793.6127172470093,
            "unit": "ns",
            "range": "± 5.554332794775759"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "8ad66178532adf8b5f1a49c245911f359e3aada6",
          "message": "fix(ci): upgrade Stryker.NET 4.8.0 to 4.14.2 (#93)\n\n## Summary\n- Upgrade Stryker.NET from 4.8.0 to 4.14.2 (latest, 2026-05-17)\n- Fixes `Commandline could not be parsed` error during .NET 10 project\nanalysis\n\n## Context\nStryker 4.8.0's Buildalyzer cannot handle .NET 10 SDK project files. All\n3 matrix jobs (core, caching, security) fail at project analysis before\nany mutation testing begins.\n\n## Test plan\n- [ ] After merge, trigger mutation workflow via `workflow_dispatch`\n- [ ] Verify all 3 matrix jobs (core, caching, security) pass\n\n:robot: Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 4.7 <noreply@anthropic.com>",
          "timestamp": "2026-05-24T22:39:35Z",
          "tree_id": "07dccfa0203ce4e11dad820053ef4429b3970359",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/8ad66178532adf8b5f1a49c245911f359e3aada6"
        },
        "date": 1779662795787,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.901240392029285,
            "unit": "ns",
            "range": "± 0.15105742993862278"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.106119423173368,
            "unit": "ns",
            "range": "± 0.21956476618444304"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.094843325515588,
            "unit": "ns",
            "range": "± 0.015195440230820104"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6603163066320121,
            "unit": "ns",
            "range": "± 0.01197473690554203"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 392.07673870722454,
            "unit": "ns",
            "range": "± 3.2469568040454124"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 712.3711468378702,
            "unit": "ns",
            "range": "± 3.263049223521756"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.901240392029285,
            "unit": "ns",
            "range": "± 0.15105742993862278"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.106119423173368,
            "unit": "ns",
            "range": "± 0.21956476618444304"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.094843325515588,
            "unit": "ns",
            "range": "± 0.015195440230820104"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6603163066320121,
            "unit": "ns",
            "range": "± 0.01197473690554203"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.684393734178123,
            "unit": "ns",
            "range": "± 0.441723426600153"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.909015343657561,
            "unit": "ns",
            "range": "± 0.07553928913807192"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.51288623043469,
            "unit": "ns",
            "range": "± 0.03968293550186866"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.11372957165752,
            "unit": "ns",
            "range": "± 0.12062997315906393"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.579885724399771,
            "unit": "ns",
            "range": "± 0.02381086761723503"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.695568438867728,
            "unit": "ns",
            "range": "± 0.23453212666751905"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.684393734178123,
            "unit": "ns",
            "range": "± 0.441723426600153"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.909015343657561,
            "unit": "ns",
            "range": "± 0.07553928913807192"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.51288623043469,
            "unit": "ns",
            "range": "± 0.03968293550186866"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.11372957165752,
            "unit": "ns",
            "range": "± 0.12062997315906393"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.579885724399771,
            "unit": "ns",
            "range": "± 0.02381086761723503"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.695568438867728,
            "unit": "ns",
            "range": "± 0.23453212666751905"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 392.07673870722454,
            "unit": "ns",
            "range": "± 3.2469568040454124"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 712.3711468378702,
            "unit": "ns",
            "range": "± 3.263049223521756"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "abf7a6dfe1b5cdd4da3f56fc6384ef224a4b5a17",
          "message": "fix(ci): upgrade Stryker 4.8→4.14.2 + caching mutation coverage (#94)\n\n## Summary\n- Upgrade Stryker.NET from 4.8.0 to 4.14.2 — fixes .NET 10 project\nanalysis failure\n- Add 3 unit tests for InMemory caching: DI registration (with/without\nconfigure) + eviction callback coverage\n- Raises caching mutation score above the 60% break threshold\n\n## Context\nAfter PR #93 upgraded Stryker, core and security passed but caching\nscored 58.33% (threshold: 60%). The 5 NoCoverage mutants were in the DI\nextension (null configure branch) and the eviction callback.\n\n## Test plan\n- [ ] After merge, trigger mutation workflow via workflow_dispatch\n- [ ] Verify all 3 matrix jobs pass (core ✓, security ✓, caching should\nnow pass)\n\n:robot: Generated with [Claude Code](https://claude.com/claude-code)\n\n---------\n\nCo-authored-by: Claude Opus 4.7 <noreply@anthropic.com>",
          "timestamp": "2026-05-24T23:00:05Z",
          "tree_id": "9b69f8f11f4e364af01f4e966249a6b37cf12cdc",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/abf7a6dfe1b5cdd4da3f56fc6384ef224a4b5a17"
        },
        "date": 1779663993348,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.864802915851275,
            "unit": "ns",
            "range": "± 0.039631248383652694"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.81752171768592,
            "unit": "ns",
            "range": "± 0.0464743444211485"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6325825240749579,
            "unit": "ns",
            "range": "± 0.004231973283055455"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5901334489385287,
            "unit": "ns",
            "range": "± 0.007007349095635074"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 492.66363220214845,
            "unit": "ns",
            "range": "± 2.1686757325261556"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 776.9673926035563,
            "unit": "ns",
            "range": "± 5.379929219547074"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.864802915851275,
            "unit": "ns",
            "range": "± 0.039631248383652694"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.81752171768592,
            "unit": "ns",
            "range": "± 0.0464743444211485"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6325825240749579,
            "unit": "ns",
            "range": "± 0.004231973283055455"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5901334489385287,
            "unit": "ns",
            "range": "± 0.007007349095635074"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.107404699921608,
            "unit": "ns",
            "range": "± 0.17068257580904841"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.433878296986222,
            "unit": "ns",
            "range": "± 0.2689252588486214"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 14.601257981856664,
            "unit": "ns",
            "range": "± 0.31135536840340805"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.082612984379132,
            "unit": "ns",
            "range": "± 0.11100339565802354"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.247642021377882,
            "unit": "ns",
            "range": "± 0.18537054122919408"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.099587266643843,
            "unit": "ns",
            "range": "± 0.23836459779429045"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.107404699921608,
            "unit": "ns",
            "range": "± 0.17068257580904841"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.433878296986222,
            "unit": "ns",
            "range": "± 0.2689252588486214"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 14.601257981856664,
            "unit": "ns",
            "range": "± 0.31135536840340805"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.082612984379132,
            "unit": "ns",
            "range": "± 0.11100339565802354"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.247642021377882,
            "unit": "ns",
            "range": "± 0.18537054122919408"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.099587266643843,
            "unit": "ns",
            "range": "± 0.23836459779429045"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 492.66363220214845,
            "unit": "ns",
            "range": "± 2.1686757325261556"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 776.9673926035563,
            "unit": "ns",
            "range": "± 5.379929219547074"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "49699333+dependabot[bot]@users.noreply.github.com",
            "name": "dependabot[bot]",
            "username": "dependabot[bot]"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "d7c476a389463d7cccbb2b314e523f08be9557d7",
          "message": "chore(ci): Bump dependabot/fetch-metadata from 2 to 3 (#95)\n\nBumps\n[dependabot/fetch-metadata](https://github.com/dependabot/fetch-metadata)\nfrom 2 to 3.\n<details>\n<summary>Release notes</summary>\n<p><em>Sourced from <a\nhref=\"https://github.com/dependabot/fetch-metadata/releases\">dependabot/fetch-metadata's\nreleases</a>.</em></p>\n<blockquote>\n<h2>v3.0.0</h2>\n<p>The breaking change is requiring Node.js version v24 as the Actions\nruntime.</p>\n<h2>What's Changed</h2>\n<ul>\n<li>feat: Parse versions from metadata links by <a\nhref=\"https://github.com/ppkarwasz\"><code>@​ppkarwasz</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/632\">dependabot/fetch-metadata#632</a></li>\n<li>Upgrade actions core and actions github packages by <a\nhref=\"https://github.com/truggeri\"><code>@​truggeri</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/649\">dependabot/fetch-metadata#649</a></li>\n<li>docs: Add notes for using <code>alert-lookup</code> with App Token\nby <a href=\"https://github.com/sue445\"><code>@​sue445</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/656\">dependabot/fetch-metadata#656</a></li>\n<li>feat!: update Node.js version to v24 by <a\nhref=\"https://github.com/sturman\"><code>@​sturman</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/671\">dependabot/fetch-metadata#671</a></li>\n<li>Switch build tooling from ncc to esbuild by <a\nhref=\"https://github.com/truggeri\"><code>@​truggeri</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/676\">dependabot/fetch-metadata#676</a></li>\n<li>Add --legal-comments=none to esbuild build commands by <a\nhref=\"https://github.com/jeffwidman\"><code>@​jeffwidman</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/679\">dependabot/fetch-metadata#679</a></li>\n<li>Bump tsconfig target from es2022 to es2024 by <a\nhref=\"https://github.com/jeffwidman\"><code>@​jeffwidman</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/680\">dependabot/fetch-metadata#680</a></li>\n<li>Remove vestigial outDir from tsconfig.json by <a\nhref=\"https://github.com/jeffwidman\"><code>@​jeffwidman</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/681\">dependabot/fetch-metadata#681</a></li>\n<li>Switch tsconfig module resolution to bundler by <a\nhref=\"https://github.com/jeffwidman\"><code>@​jeffwidman</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/682\">dependabot/fetch-metadata#682</a></li>\n<li>Remove skipLibCheck from tsconfig.json by <a\nhref=\"https://github.com/jeffwidman\"><code>@​jeffwidman</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/683\">dependabot/fetch-metadata#683</a></li>\n<li>Add typecheck step to CI by <a\nhref=\"https://github.com/jeffwidman\"><code>@​jeffwidman</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/685\">dependabot/fetch-metadata#685</a></li>\n<li>Enable noImplicitAny in tsconfig.json by <a\nhref=\"https://github.com/jeffwidman\"><code>@​jeffwidman</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/684\">dependabot/fetch-metadata#684</a></li>\n<li>Upgrade <code>@​actions/core</code> to ^3.0.0 by <a\nhref=\"https://github.com/truggeri\"><code>@​truggeri</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/677\">dependabot/fetch-metadata#677</a></li>\n<li>Upgrade <code>@​actions/github</code> to ^9.0.0 and\n<code>@​octokit/request-error</code> to ^7.1.0 by <a\nhref=\"https://github.com/truggeri\"><code>@​truggeri</code></a> in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/678\">dependabot/fetch-metadata#678</a></li>\n<li>Bump qs from 6.14.0 to 6.14.1 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/651\">dependabot/fetch-metadata#651</a></li>\n<li>Bump hono from 4.11.1 to 4.11.4 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/652\">dependabot/fetch-metadata#652</a></li>\n<li>Bump hono from 4.11.4 to 4.11.7 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/653\">dependabot/fetch-metadata#653</a></li>\n<li>Bump hono from 4.11.7 to 4.12.0 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/657\">dependabot/fetch-metadata#657</a></li>\n<li>Bump qs from 6.14.1 to 6.14.2 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/655\">dependabot/fetch-metadata#655</a></li>\n<li>Bump <code>@​modelcontextprotocol/sdk</code> from 1.25.1 to 1.26.0\nby <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/654\">dependabot/fetch-metadata#654</a></li>\n<li>Bump <code>@​hono/node-server</code> from 1.19.9 to 1.19.10 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/665\">dependabot/fetch-metadata#665</a></li>\n<li>Bump hono from 4.12.2 to 4.12.5 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/664\">dependabot/fetch-metadata#664</a></li>\n<li>Bump minimatch from 3.1.2 to 3.1.5 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/667\">dependabot/fetch-metadata#667</a></li>\n<li>Bump hono from 4.12.5 to 4.12.7 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/668\">dependabot/fetch-metadata#668</a></li>\n<li>Bump actions/create-github-app-token from 2.2.1 to 3.0.0 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/669\">dependabot/fetch-metadata#669</a></li>\n<li>Bump flatted from 3.3.3 to 3.4.2 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/670\">dependabot/fetch-metadata#670</a></li>\n<li>build(deps-dev): bump picomatch from 2.3.1 to 2.3.2 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/674\">dependabot/fetch-metadata#674</a></li>\n</ul>\n<h2>New Contributors</h2>\n<ul>\n<li><a href=\"https://github.com/ppkarwasz\"><code>@​ppkarwasz</code></a>\nmade their first contribution in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/632\">dependabot/fetch-metadata#632</a></li>\n<li><a href=\"https://github.com/truggeri\"><code>@​truggeri</code></a>\nmade their first contribution in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/649\">dependabot/fetch-metadata#649</a></li>\n<li><a href=\"https://github.com/sue445\"><code>@​sue445</code></a> made\ntheir first contribution in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/656\">dependabot/fetch-metadata#656</a></li>\n<li><a href=\"https://github.com/sturman\"><code>@​sturman</code></a> made\ntheir first contribution in <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/671\">dependabot/fetch-metadata#671</a></li>\n</ul>\n<p><strong>Full Changelog</strong>: <a\nhref=\"https://github.com/dependabot/fetch-metadata/compare/v2...v3.0.0\">https://github.com/dependabot/fetch-metadata/compare/v2...v3.0.0</a></p>\n<h2>v2.5.0</h2>\n<h2>What's Changed</h2>\n<ul>\n<li>Bump actions/publish-immutable-action from 0.0.3 to 0.0.4 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/628\">dependabot/fetch-metadata#628</a></li>\n<li>Bump the dev-dependencies group with 11 updates by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/629\">dependabot/fetch-metadata#629</a></li>\n<li>Bump actions/create-github-app-token from 2.0.6 to 2.1.1 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/635\">dependabot/fetch-metadata#635</a></li>\n<li>Bump actions/create-github-app-token from 2.1.1 to 2.1.4 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/638\">dependabot/fetch-metadata#638</a></li>\n<li>Bump actions/checkout from 4 to 5 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/636\">dependabot/fetch-metadata#636</a></li>\n<li>Bump actions/setup-node from 4 to 5 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/637\">dependabot/fetch-metadata#637</a></li>\n<li>Bump actions/setup-node from 5 to 6 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/639\">dependabot/fetch-metadata#639</a></li>\n<li>Bump actions/create-github-app-token from 2.1.4 to 2.2.0 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/pull/643\">dependabot/fetch-metadata#643</a></li>\n</ul>\n<!-- raw HTML omitted -->\n</blockquote>\n<p>... (truncated)</p>\n</details>\n<details>\n<summary>Commits</summary>\n<ul>\n<li><a\nhref=\"https://github.com/dependabot/fetch-metadata/commit/25dd0e34f4fe68f24cc83900b1fe3fe149efef98\"><code>25dd0e3</code></a>\nv3.1.0 (<a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/issues/692\">#692</a>)</li>\n<li><a\nhref=\"https://github.com/dependabot/fetch-metadata/commit/e073f50d732cb48d48fb80afedb4fa61361626e9\"><code>e073f50</code></a>\nMerge pull request <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/issues/705\">#705</a>\nfrom dependabot/dependabot/npm_and_yarn/hono-4.12.14</li>\n<li><a\nhref=\"https://github.com/dependabot/fetch-metadata/commit/0670e167df1fbee1b0d07121de6a182ddebdd674\"><code>0670e16</code></a>\nbuild(deps-dev): bump hono from 4.12.12 to 4.12.14</li>\n<li><a\nhref=\"https://github.com/dependabot/fetch-metadata/commit/7a7fe10a42310e65df80af6c771e9aa5d59842d1\"><code>7a7fe10</code></a>\nMerge pull request <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/issues/702\">#702</a>\nfrom dependabot/dependabot/npm_and_yarn/dependencies-...</li>\n<li><a\nhref=\"https://github.com/dependabot/fetch-metadata/commit/5168191cea3d4daa635bff6c796b4f0faeba522d\"><code>5168191</code></a>\nUpdating dist build</li>\n<li><a\nhref=\"https://github.com/dependabot/fetch-metadata/commit/23882e175b2f16bc495c89aa50940399c6a17504\"><code>23882e1</code></a>\nbuild(deps): bump <code>@​actions/github</code> in the dependencies\ngroup</li>\n<li><a\nhref=\"https://github.com/dependabot/fetch-metadata/commit/1072469591c13fda1d8dba1d1ac2e80187e247d7\"><code>1072469</code></a>\nMerge pull request <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/issues/701\">#701</a>\nfrom dependabot/dependabot/github_actions/actions/cre...</li>\n<li><a\nhref=\"https://github.com/dependabot/fetch-metadata/commit/43f8a0055c8e32587be67e097dff89a6823c9752\"><code>43f8a00</code></a>\nbuild(deps): bump actions/create-github-app-token from 3.0.0 to\n3.1.1</li>\n<li><a\nhref=\"https://github.com/dependabot/fetch-metadata/commit/b4d904a50935c8ebe744da148ea8a18a43fe72e1\"><code>b4d904a</code></a>\nMerge pull request <a\nhref=\"https://redirect.github.com/dependabot/fetch-metadata/issues/703\">#703</a>\nfrom dependabot/dependabot/npm_and_yarn/globals-17.5.0</li>\n<li><a\nhref=\"https://github.com/dependabot/fetch-metadata/commit/c8046bb877d9989cc848797de1b944bc3e93ef82\"><code>c8046bb</code></a>\nbuild(deps-dev): bump globals from 17.4.0 to 17.5.0</li>\n<li>Additional commits viewable in <a\nhref=\"https://github.com/dependabot/fetch-metadata/compare/v2...v3\">compare\nview</a></li>\n</ul>\n</details>\n<br />\n\n\n[![Dependabot compatibility\nscore](https://dependabot-badges.githubapp.com/badges/compatibility_score?dependency-name=dependabot/fetch-metadata&package-manager=github_actions&previous-version=2&new-version=3)](https://docs.github.com/en/github/managing-security-vulnerabilities/about-dependabot-security-updates#about-compatibility-scores)\n\nDependabot will resolve any conflicts with this PR as long as you don't\nalter it yourself. You can also trigger a rebase manually by commenting\n`@dependabot rebase`.\n\n[//]: # (dependabot-automerge-start)\n[//]: # (dependabot-automerge-end)\n\n---\n\n<details>\n<summary>Dependabot commands and options</summary>\n<br />\n\nYou can trigger Dependabot actions by commenting on this PR:\n- `@dependabot rebase` will rebase this PR\n- `@dependabot recreate` will recreate this PR, overwriting any edits\nthat have been made to it\n- `@dependabot show <dependency name> ignore conditions` will show all\nof the ignore conditions of the specified dependency\n- `@dependabot ignore this major version` will close this PR and stop\nDependabot creating any more for this major version (unless you reopen\nthe PR or upgrade to it yourself)\n- `@dependabot ignore this minor version` will close this PR and stop\nDependabot creating any more for this minor version (unless you reopen\nthe PR or upgrade to it yourself)\n- `@dependabot ignore this dependency` will close this PR and stop\nDependabot creating any more for this dependency (unless you reopen the\nPR or upgrade to it yourself)\n\n\n</details>\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>",
          "timestamp": "2026-05-25T22:59:04+03:00",
          "tree_id": "5931bb10e54bac1f71cd4b48caf9f94f0aa908e6",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/d7c476a389463d7cccbb2b314e523f08be9557d7"
        },
        "date": 1779739571363,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.336708342532317,
            "unit": "ns",
            "range": "± 0.16451231530367072"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.67909507950147,
            "unit": "ns",
            "range": "± 0.12636954255390034"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.3818934476003051,
            "unit": "ns",
            "range": "± 0.00461097509204806"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.4427786705394586,
            "unit": "ns",
            "range": "± 0.01697688086655852"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 413.14197874069214,
            "unit": "ns",
            "range": "± 6.901272169628525"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 719.7809429168701,
            "unit": "ns",
            "range": "± 3.994171061060732"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.336708342532317,
            "unit": "ns",
            "range": "± 0.16451231530367072"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.67909507950147,
            "unit": "ns",
            "range": "± 0.12636954255390034"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.3818934476003051,
            "unit": "ns",
            "range": "± 0.00461097509204806"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.4427786705394586,
            "unit": "ns",
            "range": "± 0.01697688086655852"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.143211845556895,
            "unit": "ns",
            "range": "± 0.1703194606214709"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.708134164909522,
            "unit": "ns",
            "range": "± 0.1230788135509901"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.259154737989109,
            "unit": "ns",
            "range": "± 0.12020704075754765"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.560923622250558,
            "unit": "ns",
            "range": "± 0.36135366377319633"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.729980401121653,
            "unit": "ns",
            "range": "± 0.028903188240089583"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.0408218735829,
            "unit": "ns",
            "range": "± 0.2575825662839742"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.143211845556895,
            "unit": "ns",
            "range": "± 0.1703194606214709"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.708134164909522,
            "unit": "ns",
            "range": "± 0.1230788135509901"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.259154737989109,
            "unit": "ns",
            "range": "± 0.12020704075754765"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.560923622250558,
            "unit": "ns",
            "range": "± 0.36135366377319633"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.729980401121653,
            "unit": "ns",
            "range": "± 0.028903188240089583"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.0408218735829,
            "unit": "ns",
            "range": "± 0.2575825662839742"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 413.14197874069214,
            "unit": "ns",
            "range": "± 6.901272169628525"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 719.7809429168701,
            "unit": "ns",
            "range": "± 3.994171061060732"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "5a3ccc2de56d3fcfef7e3aad82386172448588a6",
          "message": "fix(ci): upgrade fetch-metadata to v3, tolerate approval failure (#98)\n\n## Summary\n\n- `dependabot/fetch-metadata@v2` → `@v3`: Node.js 20 → 24 uyumluluğu\n(deprecation warning giderildi, June 2 deadline)\n- `continue-on-error: true` approval adımına eklendi: `GITHUB_TOKEN` PR\napprove edemediğinde workflow artık FAILURE yerine SUCCESS döner\n\n## Root Cause\n\nPR #96 (`testcontainers` minor bump) auto-merge job'ı `failed to create\nreview: GraphQL: GitHub Actions is not permitted to approve pull\nrequests` hatasıyla düşüyordu. `require_code_owner_reviews: true` branch\nprotection kuralı approval gerektiriyor, ancak GitHub Actions kendi\nPR'ını approve edemiyor.\n\n## Test plan\n\n- [ ] `dependabot-auto-merge` workflow'u artık approval adımı başarısız\nolsa bile SUCCESS döner\n- [ ] `fetch-metadata@v3` Node.js 24 ile çalışır\n- [ ] Patch/minor Dependabot PR'larında auto-merge etkinleştirilir\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-05-25T23:07:59+03:00",
          "tree_id": "2c492a2cb83bb69c673f72c63f1c6727f7b19dc4",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/5a3ccc2de56d3fcfef7e3aad82386172448588a6"
        },
        "date": 1779740087922,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.602053350458543,
            "unit": "ns",
            "range": "± 0.03400726292803792"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.637333159645398,
            "unit": "ns",
            "range": "± 0.11860269993786843"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6364389014031206,
            "unit": "ns",
            "range": "± 0.005546393702232064"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6780036842184407,
            "unit": "ns",
            "range": "± 0.00452578705295474"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 462.7044857910701,
            "unit": "ns",
            "range": "± 3.476853044212949"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 766.7855336849506,
            "unit": "ns",
            "range": "± 3.6123763895450787"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.602053350458543,
            "unit": "ns",
            "range": "± 0.03400726292803792"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.637333159645398,
            "unit": "ns",
            "range": "± 0.11860269993786843"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6364389014031206,
            "unit": "ns",
            "range": "± 0.005546393702232064"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6780036842184407,
            "unit": "ns",
            "range": "± 0.00452578705295474"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.746667918104391,
            "unit": "ns",
            "range": "± 0.10598276051683524"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.762876759283245,
            "unit": "ns",
            "range": "± 0.22161239674791722"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.78895947150886,
            "unit": "ns",
            "range": "± 0.24580981969071464"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.560477757683167,
            "unit": "ns",
            "range": "± 0.08221825945535939"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.662124073505401,
            "unit": "ns",
            "range": "± 0.21207216497770112"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.92866807480653,
            "unit": "ns",
            "range": "± 0.15694972813558558"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.746667918104391,
            "unit": "ns",
            "range": "± 0.10598276051683524"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.762876759283245,
            "unit": "ns",
            "range": "± 0.22161239674791722"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.78895947150886,
            "unit": "ns",
            "range": "± 0.24580981969071464"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.560477757683167,
            "unit": "ns",
            "range": "± 0.08221825945535939"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.662124073505401,
            "unit": "ns",
            "range": "± 0.21207216497770112"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.92866807480653,
            "unit": "ns",
            "range": "± 0.15694972813558558"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 462.7044857910701,
            "unit": "ns",
            "range": "± 3.476853044212949"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 766.7855336849506,
            "unit": "ns",
            "range": "± 3.6123763895450787"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "49699333+dependabot[bot]@users.noreply.github.com",
            "name": "dependabot[bot]",
            "username": "dependabot[bot]"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "eb54166a1debe91fc5b2226a0967243835458cb5",
          "message": "chore(deps): Bump the testcontainers group with 4 updates (#96)\n\nUpdated\n[Testcontainers.Elasticsearch](https://github.com/testcontainers/testcontainers-dotnet)\nfrom 4.11.0 to 4.12.0.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [Testcontainers.Elasticsearch's\nreleases](https://github.com/testcontainers/testcontainers-dotnet/releases)._\n\n## 4.12.0\n\n# What's Changed\n\nThanks to all contributors 👏.\n\nThe NuGet packages for this release have been attested for supply chain\nsecurity using [`actions/attest`](https://github.com/actions/attest).\nThis confirms the integrity and provenance of the artifacts and helps\nensure they can be trusted:\n[#​21198535](https://github.com/testcontainers/testcontainers-dotnet/attestations/28009236).\n\n## ⚠️ Breaking Changes\n\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n## 🚀 Features\n\n* feat: Add Floci module (#​1690) @​object\n* feat: Ignore port-forwarding extra host in reuse hash (#​1689)\n@​HofmeisterAn\n* feat: Allow devs to override the reuse hash calculation (#​1688)\n@​HofmeisterAn\n* feat: Add connect to network API (#​1672) @​HofmeisterAn\n* feat(LocalStack): Require auth token for 4.15 and onwards (#​1667)\n@​HofmeisterAn\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n## 🐛 Bug Fixes\n\n* fix: Trim tar record padding to avoid broken-pipe failure on Podman\n(#​1684) @​artiomchi\n* fix(Nats): Use healthz API for readiness probe (#​1679) @​eriblo01\n* fix: Remove KeepAlive socket option (#​1671) @​Angelinsky7\n\n## 📖 Documentation\n\n* docs: Extend WithCommand(params string[]) documentation (#​1685)\n@​HofmeisterAn\n\n## 🧹 Housekeeping\n\n* feat: Prepare next release cycle (4.12.0) (#​1664) @​HofmeisterAn\n\n## 📦 Dependency Updates\n\n* chore(deps): Bump the actions group with 5 updates (#​1687)\n@[dependabot[bot]](https://github.com/apps/dependabot)\n* chore(deps): Bump Docker.DotNet from 4.1.0 to 4.2.0 (#​1686)\n@​HofmeisterAn\n* chore(deps): Bump the actions group with 5 updates (#​1676)\n@[dependabot[bot]](https://github.com/apps/dependabot)\n* chore(deps): Bump Docker.DotNet from 4.0.2 to 4.1.0 (#​1674)\n@​HofmeisterAn\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n\nCommits viewable in [compare\nview](https://github.com/testcontainers/testcontainers-dotnet/compare/4.11.0...4.12.0).\n</details>\n\nUpdated\n[Testcontainers.PostgreSql](https://github.com/testcontainers/testcontainers-dotnet)\nfrom 4.11.0 to 4.12.0.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [Testcontainers.PostgreSql's\nreleases](https://github.com/testcontainers/testcontainers-dotnet/releases)._\n\n## 4.12.0\n\n# What's Changed\n\nThanks to all contributors 👏.\n\nThe NuGet packages for this release have been attested for supply chain\nsecurity using [`actions/attest`](https://github.com/actions/attest).\nThis confirms the integrity and provenance of the artifacts and helps\nensure they can be trusted:\n[#​21198535](https://github.com/testcontainers/testcontainers-dotnet/attestations/28009236).\n\n## ⚠️ Breaking Changes\n\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n## 🚀 Features\n\n* feat: Add Floci module (#​1690) @​object\n* feat: Ignore port-forwarding extra host in reuse hash (#​1689)\n@​HofmeisterAn\n* feat: Allow devs to override the reuse hash calculation (#​1688)\n@​HofmeisterAn\n* feat: Add connect to network API (#​1672) @​HofmeisterAn\n* feat(LocalStack): Require auth token for 4.15 and onwards (#​1667)\n@​HofmeisterAn\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n## 🐛 Bug Fixes\n\n* fix: Trim tar record padding to avoid broken-pipe failure on Podman\n(#​1684) @​artiomchi\n* fix(Nats): Use healthz API for readiness probe (#​1679) @​eriblo01\n* fix: Remove KeepAlive socket option (#​1671) @​Angelinsky7\n\n## 📖 Documentation\n\n* docs: Extend WithCommand(params string[]) documentation (#​1685)\n@​HofmeisterAn\n\n## 🧹 Housekeeping\n\n* feat: Prepare next release cycle (4.12.0) (#​1664) @​HofmeisterAn\n\n## 📦 Dependency Updates\n\n* chore(deps): Bump the actions group with 5 updates (#​1687)\n@[dependabot[bot]](https://github.com/apps/dependabot)\n* chore(deps): Bump Docker.DotNet from 4.1.0 to 4.2.0 (#​1686)\n@​HofmeisterAn\n* chore(deps): Bump the actions group with 5 updates (#​1676)\n@[dependabot[bot]](https://github.com/apps/dependabot)\n* chore(deps): Bump Docker.DotNet from 4.0.2 to 4.1.0 (#​1674)\n@​HofmeisterAn\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n\nCommits viewable in [compare\nview](https://github.com/testcontainers/testcontainers-dotnet/compare/4.11.0...4.12.0).\n</details>\n\nUpdated\n[Testcontainers.RabbitMq](https://github.com/testcontainers/testcontainers-dotnet)\nfrom 4.11.0 to 4.12.0.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [Testcontainers.RabbitMq's\nreleases](https://github.com/testcontainers/testcontainers-dotnet/releases)._\n\n## 4.12.0\n\n# What's Changed\n\nThanks to all contributors 👏.\n\nThe NuGet packages for this release have been attested for supply chain\nsecurity using [`actions/attest`](https://github.com/actions/attest).\nThis confirms the integrity and provenance of the artifacts and helps\nensure they can be trusted:\n[#​21198535](https://github.com/testcontainers/testcontainers-dotnet/attestations/28009236).\n\n## ⚠️ Breaking Changes\n\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n## 🚀 Features\n\n* feat: Add Floci module (#​1690) @​object\n* feat: Ignore port-forwarding extra host in reuse hash (#​1689)\n@​HofmeisterAn\n* feat: Allow devs to override the reuse hash calculation (#​1688)\n@​HofmeisterAn\n* feat: Add connect to network API (#​1672) @​HofmeisterAn\n* feat(LocalStack): Require auth token for 4.15 and onwards (#​1667)\n@​HofmeisterAn\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n## 🐛 Bug Fixes\n\n* fix: Trim tar record padding to avoid broken-pipe failure on Podman\n(#​1684) @​artiomchi\n* fix(Nats): Use healthz API for readiness probe (#​1679) @​eriblo01\n* fix: Remove KeepAlive socket option (#​1671) @​Angelinsky7\n\n## 📖 Documentation\n\n* docs: Extend WithCommand(params string[]) documentation (#​1685)\n@​HofmeisterAn\n\n## 🧹 Housekeeping\n\n* feat: Prepare next release cycle (4.12.0) (#​1664) @​HofmeisterAn\n\n## 📦 Dependency Updates\n\n* chore(deps): Bump the actions group with 5 updates (#​1687)\n@[dependabot[bot]](https://github.com/apps/dependabot)\n* chore(deps): Bump Docker.DotNet from 4.1.0 to 4.2.0 (#​1686)\n@​HofmeisterAn\n* chore(deps): Bump the actions group with 5 updates (#​1676)\n@[dependabot[bot]](https://github.com/apps/dependabot)\n* chore(deps): Bump Docker.DotNet from 4.0.2 to 4.1.0 (#​1674)\n@​HofmeisterAn\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n\nCommits viewable in [compare\nview](https://github.com/testcontainers/testcontainers-dotnet/compare/4.11.0...4.12.0).\n</details>\n\nUpdated\n[Testcontainers.Redis](https://github.com/testcontainers/testcontainers-dotnet)\nfrom 4.11.0 to 4.12.0.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [Testcontainers.Redis's\nreleases](https://github.com/testcontainers/testcontainers-dotnet/releases)._\n\n## 4.12.0\n\n# What's Changed\n\nThanks to all contributors 👏.\n\nThe NuGet packages for this release have been attested for supply chain\nsecurity using [`actions/attest`](https://github.com/actions/attest).\nThis confirms the integrity and provenance of the artifacts and helps\nensure they can be trusted:\n[#​21198535](https://github.com/testcontainers/testcontainers-dotnet/attestations/28009236).\n\n## ⚠️ Breaking Changes\n\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n## 🚀 Features\n\n* feat: Add Floci module (#​1690) @​object\n* feat: Ignore port-forwarding extra host in reuse hash (#​1689)\n@​HofmeisterAn\n* feat: Allow devs to override the reuse hash calculation (#​1688)\n@​HofmeisterAn\n* feat: Add connect to network API (#​1672) @​HofmeisterAn\n* feat(LocalStack): Require auth token for 4.15 and onwards (#​1667)\n@​HofmeisterAn\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n## 🐛 Bug Fixes\n\n* fix: Trim tar record padding to avoid broken-pipe failure on Podman\n(#​1684) @​artiomchi\n* fix(Nats): Use healthz API for readiness probe (#​1679) @​eriblo01\n* fix: Remove KeepAlive socket option (#​1671) @​Angelinsky7\n\n## 📖 Documentation\n\n* docs: Extend WithCommand(params string[]) documentation (#​1685)\n@​HofmeisterAn\n\n## 🧹 Housekeeping\n\n* feat: Prepare next release cycle (4.12.0) (#​1664) @​HofmeisterAn\n\n## 📦 Dependency Updates\n\n* chore(deps): Bump the actions group with 5 updates (#​1687)\n@[dependabot[bot]](https://github.com/apps/dependabot)\n* chore(deps): Bump Docker.DotNet from 4.1.0 to 4.2.0 (#​1686)\n@​HofmeisterAn\n* chore(deps): Bump the actions group with 5 updates (#​1676)\n@[dependabot[bot]](https://github.com/apps/dependabot)\n* chore(deps): Bump Docker.DotNet from 4.0.2 to 4.1.0 (#​1674)\n@​HofmeisterAn\n* chore(deps): Bump Docker.DotNet from 3.131.1 to 4.0.2 (#​1665)\n@​HofmeisterAn\n\n\nCommits viewable in [compare\nview](https://github.com/testcontainers/testcontainers-dotnet/compare/4.11.0...4.12.0).\n</details>\n\nDependabot will resolve any conflicts with this PR as long as you don't\nalter it yourself. You can also trigger a rebase manually by commenting\n`@dependabot rebase`.\n\n[//]: # (dependabot-automerge-start)\n[//]: # (dependabot-automerge-end)\n\n---\n\n<details>\n<summary>Dependabot commands and options</summary>\n<br />\n\nYou can trigger Dependabot actions by commenting on this PR:\n- `@dependabot rebase` will rebase this PR\n- `@dependabot recreate` will recreate this PR, overwriting any edits\nthat have been made to it\n- `@dependabot show <dependency name> ignore conditions` will show all\nof the ignore conditions of the specified dependency\n- `@dependabot ignore <dependency name> major version` will close this\ngroup update PR and stop Dependabot creating any more for the specific\ndependency's major version (unless you unignore this specific\ndependency's major version or upgrade to it yourself)\n- `@dependabot ignore <dependency name> minor version` will close this\ngroup update PR and stop Dependabot creating any more for the specific\ndependency's minor version (unless you unignore this specific\ndependency's minor version or upgrade to it yourself)\n- `@dependabot ignore <dependency name>` will close this group update PR\nand stop Dependabot creating any more for the specific dependency\n(unless you unignore this specific dependency or upgrade to it yourself)\n- `@dependabot unignore <dependency name>` will remove all of the ignore\nconditions of the specified dependency\n- `@dependabot unignore <dependency name> <ignore condition>` will\nremove the ignore condition of the specified dependency and ignore\nconditions\n\n\n</details>\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>\nCo-authored-by: Ömer KÜÇÜK <106805727+omerkck41@users.noreply.github.com>",
          "timestamp": "2026-05-25T20:22:50Z",
          "tree_id": "339cc0d386c3de8874efe8d92529b6fd1f4eb5bb",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/eb54166a1debe91fc5b2226a0967243835458cb5"
        },
        "date": 1779740959636,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.537870632914396,
            "unit": "ns",
            "range": "± 0.009873755319162202"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.450665117374489,
            "unit": "ns",
            "range": "± 0.015564815172137998"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.641233510695971,
            "unit": "ns",
            "range": "± 0.004659209946027079"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5841957181692123,
            "unit": "ns",
            "range": "± 0.0022232621508410724"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 445.97821170943126,
            "unit": "ns",
            "range": "± 0.6072129183493944"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 746.7622771944318,
            "unit": "ns",
            "range": "± 2.5336177877764094"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.537870632914396,
            "unit": "ns",
            "range": "± 0.009873755319162202"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.450665117374489,
            "unit": "ns",
            "range": "± 0.015564815172137998"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.641233510695971,
            "unit": "ns",
            "range": "± 0.004659209946027079"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5841957181692123,
            "unit": "ns",
            "range": "± 0.0022232621508410724"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.677633268492562,
            "unit": "ns",
            "range": "± 0.029970951722523495"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.673403078956264,
            "unit": "ns",
            "range": "± 0.027419219015448366"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.058917865157127,
            "unit": "ns",
            "range": "± 0.017481744244455356"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.52429406940937,
            "unit": "ns",
            "range": "± 0.05217053950242512"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.74664232134819,
            "unit": "ns",
            "range": "± 0.00994707663440198"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.529386507471402,
            "unit": "ns",
            "range": "± 0.027770022753725313"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.677633268492562,
            "unit": "ns",
            "range": "± 0.029970951722523495"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.673403078956264,
            "unit": "ns",
            "range": "± 0.027419219015448366"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.058917865157127,
            "unit": "ns",
            "range": "± 0.017481744244455356"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.52429406940937,
            "unit": "ns",
            "range": "± 0.05217053950242512"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.74664232134819,
            "unit": "ns",
            "range": "± 0.00994707663440198"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.529386507471402,
            "unit": "ns",
            "range": "± 0.027770022753725313"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 445.97821170943126,
            "unit": "ns",
            "range": "± 0.6072129183493944"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 746.7622771944318,
            "unit": "ns",
            "range": "± 2.5336177877764094"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "49699333+dependabot[bot]@users.noreply.github.com",
            "name": "dependabot[bot]",
            "username": "dependabot[bot]"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "c1519e7cde905145fabbf2f729769bc8fbd72121",
          "message": "chore(deps): Bump the elastic group with 1 update (#97)\n\nUpdated\n[Elastic.Clients.Elasticsearch](https://github.com/elastic/elasticsearch-net)\nfrom 9.4.0 to 9.4.1.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [Elastic.Clients.Elasticsearch's\nreleases](https://github.com/elastic/elasticsearch-net/releases)._\n\n## 9.4.1\n\n## What's Changed\n\n* Regenerate client by @​flobernd in\nhttps://github.com/elastic/elasticsearch-net/pull/8899 and\nhttps://github.com/elastic/elasticsearch-net/pull/8907\n* Fixes (de-)serialization of unions with three or more variants —\npreviously, hand-rolled union converters could fail to select the\ncorrect variant\n* Search response shape fix in `InnerHits`: the `Fields` property is\nsplit into `Field` (single-field selector using `Fields`) and a new\n`Fields` collection of `FieldAndFormat` — existing usages of\n`InnerHits.Fields = …` may need to be retargeted to `Field`\n* `DataStreamLifecycle` gains `EffectiveRetention` and\n`RetentionDeterminedBy`; new `GlobalRetention` and `RetentionSource`\ntypes; `GetDataLifecycleResponse` now exposes `GlobalRetention`\n* Reindex rethrottle response now models parent task progress via the\nnew `ParentReindexStatus` type\n* Inference and `_mvt` content-type alignment: `chat_completion_unified`\n/ `stream_completion` send `Content-Type: application/json`; `_mvt` uses\nthe versioned `application/vnd.elasticsearch+vnd.mapbox-vector-tile`\nAccept header\n\n\n**Full Changelog**:\nhttps://github.com/elastic/elasticsearch-net/compare/9.4.0...9.4.1\n\n\nCommits viewable in [compare\nview](https://github.com/elastic/elasticsearch-net/compare/9.4.0...9.4.1).\n</details>\n\n[![Dependabot compatibility\nscore](https://dependabot-badges.githubapp.com/badges/compatibility_score?dependency-name=Elastic.Clients.Elasticsearch&package-manager=nuget&previous-version=9.4.0&new-version=9.4.1)](https://docs.github.com/en/github/managing-security-vulnerabilities/about-dependabot-security-updates#about-compatibility-scores)\n\nDependabot will resolve any conflicts with this PR as long as you don't\nalter it yourself. You can also trigger a rebase manually by commenting\n`@dependabot rebase`.\n\n[//]: # (dependabot-automerge-start)\n[//]: # (dependabot-automerge-end)\n\n---\n\n<details>\n<summary>Dependabot commands and options</summary>\n<br />\n\nYou can trigger Dependabot actions by commenting on this PR:\n- `@dependabot rebase` will rebase this PR\n- `@dependabot recreate` will recreate this PR, overwriting any edits\nthat have been made to it\n- `@dependabot show <dependency name> ignore conditions` will show all\nof the ignore conditions of the specified dependency\n- `@dependabot ignore <dependency name> major version` will close this\ngroup update PR and stop Dependabot creating any more for the specific\ndependency's major version (unless you unignore this specific\ndependency's major version or upgrade to it yourself)\n- `@dependabot ignore <dependency name> minor version` will close this\ngroup update PR and stop Dependabot creating any more for the specific\ndependency's minor version (unless you unignore this specific\ndependency's minor version or upgrade to it yourself)\n- `@dependabot ignore <dependency name>` will close this group update PR\nand stop Dependabot creating any more for the specific dependency\n(unless you unignore this specific dependency or upgrade to it yourself)\n- `@dependabot unignore <dependency name>` will remove all of the ignore\nconditions of the specified dependency\n- `@dependabot unignore <dependency name> <ignore condition>` will\nremove the ignore condition of the specified dependency and ignore\nconditions\n\n\n</details>\n\n---------\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>\nCo-authored-by: github-actions[bot] <github-actions[bot]@users.noreply.github.com>\nCo-authored-by: Ömer KÜÇÜK <106805727+omerkck41@users.noreply.github.com>",
          "timestamp": "2026-05-25T20:33:13Z",
          "tree_id": "4cee17694eeb3d19922ee3a2d04ef0afb851f099",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/c1519e7cde905145fabbf2f729769bc8fbd72121"
        },
        "date": 1779741608508,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.9740594037705,
            "unit": "ns",
            "range": "± 0.19700912780614224"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.211712436874707,
            "unit": "ns",
            "range": "± 0.08980978732812074"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6370156700057643,
            "unit": "ns",
            "range": "± 0.004224285233472326"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5880113730827967,
            "unit": "ns",
            "range": "± 0.006731503992806245"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 490.5513591032762,
            "unit": "ns",
            "range": "± 1.3182434064361646"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 808.0983128229777,
            "unit": "ns",
            "range": "± 5.093639210940195"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.9740594037705,
            "unit": "ns",
            "range": "± 0.19700912780614224"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.211712436874707,
            "unit": "ns",
            "range": "± 0.08980978732812074"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6370156700057643,
            "unit": "ns",
            "range": "± 0.004224285233472326"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5880113730827967,
            "unit": "ns",
            "range": "± 0.006731503992806245"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 12.184993183070963,
            "unit": "ns",
            "range": "± 0.5774652997884422"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.158581158315593,
            "unit": "ns",
            "range": "± 0.49858364035888497"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.839767710438796,
            "unit": "ns",
            "range": "± 0.16590142452906678"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 11.530541492501895,
            "unit": "ns",
            "range": "± 0.22841956803086624"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.886449022934986,
            "unit": "ns",
            "range": "± 0.1632666642704924"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.118633821606636,
            "unit": "ns",
            "range": "± 0.19368296413632297"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 12.184993183070963,
            "unit": "ns",
            "range": "± 0.5774652997884422"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.158581158315593,
            "unit": "ns",
            "range": "± 0.49858364035888497"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.839767710438796,
            "unit": "ns",
            "range": "± 0.16590142452906678"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 11.530541492501895,
            "unit": "ns",
            "range": "± 0.22841956803086624"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.886449022934986,
            "unit": "ns",
            "range": "± 0.1632666642704924"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.118633821606636,
            "unit": "ns",
            "range": "± 0.19368296413632297"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 490.5513591032762,
            "unit": "ns",
            "range": "± 1.3182434064361646"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 808.0983128229777,
            "unit": "ns",
            "range": "± 5.093639210940195"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "41a2d84de5dc4100060619e94b0310ecd6be8a7c",
          "message": "fix(codeql): resolve 3 open code scanning alerts (#99)\n\n## Summary\n\n- **#75 `cs/missed-using-statement`** `JwtTokenService.cs:163` —\n`succeeded` flag replaced with `rsa = null` ownership-transfer pattern.\n`RsaSecurityKey` takes ownership of the `RSA` instance on the happy\npath; `finally` only disposes on failure. `using` cannot be used here\nbecause the lifetime extends beyond the method.\n- **#74 `cs/local-not-disposed`** `ElasticsearchIntegrationTests.cs:58`\n— `((IDisposable?)x)?.Dispose()` changed to `if (x is IDisposable d)\nd.Dispose()`. CodeQL tracks the `is`-pattern as a recognised disposal\ncall; the nullable-cast pattern was not.\n- **#51 `cs/useless-cast-to-self`**\n`ConfigurationSecretsManagerTests.cs:12` — Redundant `(string?)` cast\nremoved; explicit generic type parameters `<(string,string), string,\nstring?>` passed to `ToDictionary` instead.\n\n## Test plan\n\n- [ ] Build passes (0 errors)\n- [ ] CodeQL scan clears all 3 alerts on next run\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\n---------\n\nCo-authored-by: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-05-26T00:10:47+03:00",
          "tree_id": "55b7b6898d8d8994ebc79af6b399358cff427cdd",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/41a2d84de5dc4100060619e94b0310ecd6be8a7c"
        },
        "date": 1779743932497,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 7.037697954103351,
            "unit": "ns",
            "range": "± 0.4026074715481392"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.934837990999222,
            "unit": "ns",
            "range": "± 0.21445377900351237"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.1974855861626565,
            "unit": "ns",
            "range": "± 0.18014235275447127"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6619001882416862,
            "unit": "ns",
            "range": "± 0.0032842351298936526"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 395.30489257665783,
            "unit": "ns",
            "range": "± 0.9610454722484718"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 704.6183833394732,
            "unit": "ns",
            "range": "± 2.0093171969663133"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 7.037697954103351,
            "unit": "ns",
            "range": "± 0.4026074715481392"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.934837990999222,
            "unit": "ns",
            "range": "± 0.21445377900351237"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.1974855861626565,
            "unit": "ns",
            "range": "± 0.18014235275447127"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6619001882416862,
            "unit": "ns",
            "range": "± 0.0032842351298936526"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 11.021381616592407,
            "unit": "ns",
            "range": "± 0.022093420038220554"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.248248507236612,
            "unit": "ns",
            "range": "± 0.39406032074841374"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.704832086196312,
            "unit": "ns",
            "range": "± 0.0386453204453277"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.989413575402327,
            "unit": "ns",
            "range": "± 0.09337332509690321"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.077385627664626,
            "unit": "ns",
            "range": "± 0.24471539123341665"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.9246988962094,
            "unit": "ns",
            "range": "± 0.04407717768996744"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 11.021381616592407,
            "unit": "ns",
            "range": "± 0.022093420038220554"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.248248507236612,
            "unit": "ns",
            "range": "± 0.39406032074841374"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.704832086196312,
            "unit": "ns",
            "range": "± 0.0386453204453277"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.989413575402327,
            "unit": "ns",
            "range": "± 0.09337332509690321"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.077385627664626,
            "unit": "ns",
            "range": "± 0.24471539123341665"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.9246988962094,
            "unit": "ns",
            "range": "± 0.04407717768996744"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 395.30489257665783,
            "unit": "ns",
            "range": "± 0.9610454722484718"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 704.6183833394732,
            "unit": "ns",
            "range": "± 2.0093171969663133"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "c9f6df7beb640456f5a7cd46cbfa009833eb11a2",
          "message": "fix(sonarcloud): resolve 11 open BLOCKER/MAJOR issues (#100)\n\n## Summary\n\n| Rule | Severity | Count | Fix |\n|---|---|---|---|\n| `S2699` No assertion in test | BLOCKER | 1 | Made async + added\n`NotThrowAsync` assertion |\n| `S107` Too many parameters | MAJOR | 4 | `// NOSONAR S107` on\ndeprecated backwards-compat APIs |\n| `S2326` Unused type parameter | MAJOR | 1 | `// NOSONAR S2326` — `T`\nis a DI discriminator by design |\n| `CA1716` Keyword conflict | MAJOR | 5 | Extended existing `// NOSONAR\nS1700` to also cover `CA1716` |\n\n## Details\n\n- **S2699**\n`InMemoryEventBusTests.Subscribe_DuplicateHandler_DoesNotAddTwice` — was\nvoid with no assertion; now async with `await\nact.Should().NotThrowAsync()`\n- **S107** `IReadRepository` / `ReadRepositoryExtensions` `GetListAsync`\n+ `GetListByDynamic` — deprecated APIs kept for backwards compatibility,\nparameter count cannot change\n- **S2326** `IFilterPropertyWhitelist<T>` — `T` appears in XML docs but\nnot in member signatures; intentional DI discriminator pattern\n(`IFilterPropertyWhitelist<Product>` vs\n`IFilterPropertyWhitelist<Order>`)\n- **CA1716** `Set`/`Get` on `ICookieManager`, `ISessionManager`,\n`IGauge` — standard domain verbs; VB.NET callers can use fully-qualified\nnames\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-05-26T00:36:41+03:00",
          "tree_id": "66f089546e86791aca4a5bc39dd9d0756af1eeb7",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/c9f6df7beb640456f5a7cd46cbfa009833eb11a2"
        },
        "date": 1779745418627,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.984584033489227,
            "unit": "ns",
            "range": "± 0.09905356825469686"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.073489542802175,
            "unit": "ns",
            "range": "± 0.1259559994464653"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6383068992623262,
            "unit": "ns",
            "range": "± 0.0030462769201172776"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5807511955499649,
            "unit": "ns",
            "range": "± 0.00392194576528674"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 456.46107333047047,
            "unit": "ns",
            "range": "± 2.8812554921813773"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 779.8137670516968,
            "unit": "ns",
            "range": "± 5.404861848108346"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.984584033489227,
            "unit": "ns",
            "range": "± 0.09905356825469686"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.073489542802175,
            "unit": "ns",
            "range": "± 0.1259559994464653"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6383068992623262,
            "unit": "ns",
            "range": "± 0.0030462769201172776"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5807511955499649,
            "unit": "ns",
            "range": "± 0.00392194576528674"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.80631032132584,
            "unit": "ns",
            "range": "± 0.3345589470851898"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.814598672588666,
            "unit": "ns",
            "range": "± 0.20947107058654896"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.478079333475657,
            "unit": "ns",
            "range": "± 0.47684059089961417"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.495391523838043,
            "unit": "ns",
            "range": "± 0.16682622890913662"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.42440182289907,
            "unit": "ns",
            "range": "± 0.12927858289907496"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.145206543115469,
            "unit": "ns",
            "range": "± 0.10761503943095052"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.80631032132584,
            "unit": "ns",
            "range": "± 0.3345589470851898"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.814598672588666,
            "unit": "ns",
            "range": "± 0.20947107058654896"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.478079333475657,
            "unit": "ns",
            "range": "± 0.47684059089961417"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.495391523838043,
            "unit": "ns",
            "range": "± 0.16682622890913662"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.42440182289907,
            "unit": "ns",
            "range": "± 0.12927858289907496"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.145206543115469,
            "unit": "ns",
            "range": "± 0.10761503943095052"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 456.46107333047047,
            "unit": "ns",
            "range": "± 2.8812554921813773"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 779.8137670516968,
            "unit": "ns",
            "range": "± 5.404861848108346"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "e89e884201a7fbeaac57b7e38675975e0e59c825",
          "message": "fix(sonarcloud): resolve 22 MINOR/INFO issues (#101)\n\n## Summary\n\n- **S1075** — `AwsS3StorageService`: NOSONAR on S3 key delimiter `/`\n(protocol requirement, not a hardcoded URI)\n- **S1133** (×10) — NOSONAR on 10 `[Obsolete]` members retained for\nbackwards compatibility: `IReadRepository` (×6), `JwtTokenService`,\n`ITokenService`, `InMemoryEventBus DI` (×2)\n- **S2292** — `Entity<TId>`: convert `_id` backing field + manual\nproperty to auto-property\n- **S3220** (×2) — `PathHelper`: use `['/', '\\']` collection-expression\nsyntax to make params array call unambiguous\n- **S4136** (×4) — `IWriteRepository` + `EfRepository`: reorder\noverloads so all `DeleteAsync` are adjacent and all\n`RevertSoftDeleteAsync` are adjacent\n- **ASP0015** — `SecurityHeadersMiddleware`:\n`headers[\"Content-Security-Policy\"]` → `headers.ContentSecurityPolicy`\ntyped property\n- **S3236** (×2) — `AzureServiceBus` + `RabbitMq` DI: remove explicit\n`nameof(...)` arg from `ThrowIfNullOrWhiteSpace` (compiler fills it via\n`[CallerArgumentExpression]`)\n- **S3267** — `SoftDeleteHelper`: NOSONAR on side-effect `foreach` (LINQ\n`Select()` for mutations would obscure intent)\n\n## Test plan\n\n- [ ] `dotnet build` passes with 0 errors ✅\n- [ ] No behavioural changes — all fixes are suppressions, cosmetic\nreorderings, or trivially equivalent refactors\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\n---------\n\nCo-authored-by: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-05-26T02:08:04+03:00",
          "tree_id": "ca4a236b036f5b1725dc34fa81d3285e60a0f1f7",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/e89e884201a7fbeaac57b7e38675975e0e59c825"
        },
        "date": 1779750880628,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.065770743290583,
            "unit": "ns",
            "range": "± 0.11816212264865542"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.873815484841665,
            "unit": "ns",
            "range": "± 0.11506764802207355"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6395506388865984,
            "unit": "ns",
            "range": "± 0.006663021194056043"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5881975659957299,
            "unit": "ns",
            "range": "± 0.009103158015605685"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 483.0217970530192,
            "unit": "ns",
            "range": "± 2.709054447232808"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 787.6655876159668,
            "unit": "ns",
            "range": "± 6.989282580704805"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.065770743290583,
            "unit": "ns",
            "range": "± 0.11816212264865542"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.873815484841665,
            "unit": "ns",
            "range": "± 0.11506764802207355"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6395506388865984,
            "unit": "ns",
            "range": "± 0.006663021194056043"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5881975659957299,
            "unit": "ns",
            "range": "± 0.009103158015605685"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.15605040533202,
            "unit": "ns",
            "range": "± 0.16830850006836198"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.23201808532079,
            "unit": "ns",
            "range": "± 0.22709871257286302"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.099117001252514,
            "unit": "ns",
            "range": "± 0.10201934580825332"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.684834970037143,
            "unit": "ns",
            "range": "± 0.15742281183416135"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.037054288600173,
            "unit": "ns",
            "range": "± 0.16520668508512448"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.943987525999546,
            "unit": "ns",
            "range": "± 0.10260540855948706"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.15605040533202,
            "unit": "ns",
            "range": "± 0.16830850006836198"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.23201808532079,
            "unit": "ns",
            "range": "± 0.22709871257286302"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.099117001252514,
            "unit": "ns",
            "range": "± 0.10201934580825332"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.684834970037143,
            "unit": "ns",
            "range": "± 0.15742281183416135"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.037054288600173,
            "unit": "ns",
            "range": "± 0.16520668508512448"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.943987525999546,
            "unit": "ns",
            "range": "± 0.10260540855948706"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 483.0217970530192,
            "unit": "ns",
            "range": "± 2.709054447232808"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 787.6655876159668,
            "unit": "ns",
            "range": "± 6.989282580704805"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "0a9d48607646f0ee9f724c2643fd34efcd52a105",
          "message": "test(mutation): kill 9 survived Stryker mutants — 76.60% → 95.74% (#102)\n\n## Summary\n\n- Stryker mutation score: **76.60% → 95.74%** (high threshold ≥80% now\nexceeded)\n- 9 survived mutants eliminated across `Paginate.cs` and\n`ResultExtensions.cs`\n\n### Root causes fixed\n\n**`Paginate.cs` (4 mutations)**\n| Line | Mutation | Fix |\n|------|----------|-----|\n| 68 | `size > 0` → `true` / `size >= 0` | Added\n`Create_ZeroSize_ReturnsZeroPages` (size=0 → Pages=0) |\n| 79 | `index - from` → `index + from` in HasNext | Added\n`from=2/index=2` case asserting HasNext=true |\n| 81 | `index - from` → `index + from` in IsLastPage | Same test\nasserting IsLastPage=false |\n\n**`ResultExtensions.cs` (5 mutations)**\nAll 5 `ArgumentNullException.ThrowIfNull` calls had statement mutations\n(replaced with `;`) that survived because no null inputs were ever\npassed. Added one null-guard test per method:\n- `Map_NullMapper_ThrowsArgumentNullException`\n- `Bind_NullBinder_ThrowsArgumentNullException`\n- `Tap_NullAction_ThrowsArgumentNullException`\n- `Ensure_NullPredicate_ThrowsArgumentNullException`\n- `Ensure_NullError_ThrowsArgumentNullException`\n\n## Test plan\n- [x] `dotnet test tests/Kck.Core.Abstractions.Tests` — 65/65 passed\n(was 58)\n- [x] `dotnet stryker` — 95.74% (was 76.60%)\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-05-26T02:42:23+03:00",
          "tree_id": "c55769dc6a9a2b1ef5b2b9395f154c0a66706994",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/0a9d48607646f0ee9f724c2643fd34efcd52a105"
        },
        "date": 1779752902707,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 7.0402863730986915,
            "unit": "ns",
            "range": "± 0.12189741243768769"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.357592630386352,
            "unit": "ns",
            "range": "± 0.19417272751541237"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.5089810703481947,
            "unit": "ns",
            "range": "± 0.005770735150612179"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 1.106851310034593,
            "unit": "ns",
            "range": "± 0.018672926299510798"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 481.91876646188587,
            "unit": "ns",
            "range": "± 2.7279413771134187"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 841.9221125920614,
            "unit": "ns",
            "range": "± 2.6074978597844924"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 7.0402863730986915,
            "unit": "ns",
            "range": "± 0.12189741243768769"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.357592630386352,
            "unit": "ns",
            "range": "± 0.19417272751541237"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.5089810703481947,
            "unit": "ns",
            "range": "± 0.005770735150612179"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 1.106851310034593,
            "unit": "ns",
            "range": "± 0.018672926299510798"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 11.089449081155989,
            "unit": "ns",
            "range": "± 0.3188809635960203"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 11.207186601559322,
            "unit": "ns",
            "range": "± 0.2644693939523313"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.903464946307635,
            "unit": "ns",
            "range": "± 0.3357697289130015"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 11.883538355429968,
            "unit": "ns",
            "range": "± 0.23974219086537363"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.80285450390407,
            "unit": "ns",
            "range": "± 0.19268888643070567"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 15.500485358635585,
            "unit": "ns",
            "range": "± 0.2182205716257834"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 11.089449081155989,
            "unit": "ns",
            "range": "± 0.3188809635960203"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 11.207186601559322,
            "unit": "ns",
            "range": "± 0.2644693939523313"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.903464946307635,
            "unit": "ns",
            "range": "± 0.3357697289130015"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 11.883538355429968,
            "unit": "ns",
            "range": "± 0.23974219086537363"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.80285450390407,
            "unit": "ns",
            "range": "± 0.19268888643070567"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 15.500485358635585,
            "unit": "ns",
            "range": "± 0.2182205716257834"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 481.91876646188587,
            "unit": "ns",
            "range": "± 2.7279413771134187"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 841.9221125920614,
            "unit": "ns",
            "range": "± 2.6074978597844924"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "5b246e556ffd67339c06193b8dfd4501e4c37552",
          "message": "test(mutation): add Stryker mutation testing for Security.Totp module (#103)\n\n## Özet\n`Kck.Security.Totp` modülü için Stryker mutasyon testi eklendi — mevcut\ncore/caching/security desenini takip ederek.\n\n## Değişiklikler\n- **`stryker-totp.json`** — yeni config (module: \"Security (TOTP)\"),\n`Kck.Security.Totp` projesini hedefler\n- **`ServiceCollectionExtensionsTests.cs`** — `AddKckTotp` için 4 DI\ntesti (configure'lu/configure'suz kayıt, IMemoryCache kaydı, chaining)\n- **`mutation.yml`** — `totp` modülü CI matrix'ine ve workflow_dispatch\naçıklamasına eklendi\n\n## Sonuç\nMutation score: **%62.50 -> %81.25** (high eşiği 80 üstünde). NoCoverage\nmutantları 5 -> 0. Test suite: 8/8 geçti.\n\n## Kalan survived mutantlar (öldürülmedi — gerekçeli)\n| Konum | Tür | Neden |\n|---|---|---|\n| `ServiceCollectionExtensions.cs:22` | boş `Configure` else dalı |\nEşdeğer mutant — `AddMemoryCache()` zaten `AddOptions()` çağırıyor,\ndefaultlar property initializer'dan geliyor |\n| `TotpMfaProvider.cs:61` | `true`->`false` | Eşdeğer mutant — replay\ndeğeri okunmuyor, sadece anahtar varlığı kontrol ediliyor |\n| `TotpMfaProvider.cs:60` | TTL aritmetiği `*`->`/` | Öldürmek için\nproduction koduna TimeProvider/sahte saat enjeksiyonu gerekir — kapsam\ndışı |\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\n---------\n\nCo-authored-by: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-05-29T21:29:29+03:00",
          "tree_id": "b933a3d6fa4886c0e3a2f773097b5f303cffcf97",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/5b246e556ffd67339c06193b8dfd4501e4c37552"
        },
        "date": 1780079789893,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.743706328670184,
            "unit": "ns",
            "range": "± 0.0749239674608767"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.376118379831315,
            "unit": "ns",
            "range": "± 0.1526552434548682"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.582627626260122,
            "unit": "ns",
            "range": "± 0.16388323085466797"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.359035449226697,
            "unit": "ns",
            "range": "± 0.1459851840862995"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.333014617363611,
            "unit": "ns",
            "range": "± 0.21603957857150338"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.358275211354096,
            "unit": "ns",
            "range": "± 0.19632844043115616"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 404.50019823710124,
            "unit": "ns",
            "range": "± 7.209795215990172"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 725.0506522314889,
            "unit": "ns",
            "range": "± 11.652356035127614"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 404.50019823710124,
            "unit": "ns",
            "range": "± 7.209795215990172"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 725.0506522314889,
            "unit": "ns",
            "range": "± 11.652356035127614"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.897519650427919,
            "unit": "ns",
            "range": "± 0.21062828312161186"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.9415905475616455,
            "unit": "ns",
            "range": "± 0.22896783098841167"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.195636679668252,
            "unit": "ns",
            "range": "± 0.162174260990728"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6619330965555631,
            "unit": "ns",
            "range": "± 0.005247990733419363"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.743706328670184,
            "unit": "ns",
            "range": "± 0.0749239674608767"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.376118379831315,
            "unit": "ns",
            "range": "± 0.1526552434548682"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.582627626260122,
            "unit": "ns",
            "range": "± 0.16388323085466797"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.359035449226697,
            "unit": "ns",
            "range": "± 0.1459851840862995"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.333014617363611,
            "unit": "ns",
            "range": "± 0.21603957857150338"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.358275211354096,
            "unit": "ns",
            "range": "± 0.19632844043115616"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.897519650427919,
            "unit": "ns",
            "range": "± 0.21062828312161186"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.9415905475616455,
            "unit": "ns",
            "range": "± 0.22896783098841167"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.195636679668252,
            "unit": "ns",
            "range": "± 0.162174260990728"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6619330965555631,
            "unit": "ns",
            "range": "± 0.005247990733419363"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "d814d142443e35fa2a720dadf55be3ad60ba1cfa",
          "message": "test(mutation): add Stryker mutation testing for Localization module (#104)\n\n## Özet\n`Kck.Localization` modülü için Stryker mutasyon testi eklendi ve mevcut\ntestlerin zayıf noktaları kapatıldı. Mevcut core/caching/security/totp\ndesenini takip eder.\n\n## Değişiklikler\n- **`stryker-localization.json`** — yeni config (module: \"Localization\")\n- **`LocalizationServiceMutationTests.cs`** — davranış-ayırt edici\ntestler: provider öncelik sıralaması (OrderBy/OrderByDescending), ayrık\nexact/parent/fallback culture aşamaları, argsız ham-değer formatlama,\nplural \"other\" fallback'i, dynamic-reload filtreleme, eksik-key loglama,\ngeçersiz culture catch'i\n- **`ServiceCollectionExtensionsTests.cs`** — DI registration testleri\n(9 NoCoverage mutantını kapatır)\n- **`FormatterServiceTests.cs`** — özel format vs N2 default, geçersiz\nculture invariant fallback testleri güçlendirildi\n- **`DefaultPluralizerTests.cs`** — Polonya çoğul üst sınır vakası\n(count=14)\n- **`mutation.yml`** — `localization` modülü CI matrix'ine eklendi\n\n## Sonuç\nMutation score: **%63.00 -> %86.00** (high eşiği 80 üstünde). NoCoverage\n14 -> 0. Test suite: 89/89 geçti (önceden 69).\n\n## Kalan 14 survivor — tamamı eşdeğer mutant (öldürülemez)\n| Tür | Adet | Neden |\n|---|---|---|\n| `ConfigureAwait(false)` -> `(true)` | 11 | Test bağlamında\nSynchronizationContext yok -> davranış aynı |\n| `DefaultPluralizer:26` `> 0` -> `>= 0` | 1 | Base-language çıkarımı;\nplural kategori çıktısı değişmiyor |\n| `LocalizationService:158` conditional | 1 | parent.Name=\"\" vs null ->\ngözlemlenebilir fark yok |\n| `LocalizationService:161` catch block removal | 1 | catch null-return\nvs swallow -> eşdeğer |\n\nEfektif mutation skoru (eşdeğerler hariç) **%100**.\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 4.8 <noreply@anthropic.com>",
          "timestamp": "2026-05-29T21:59:16+03:00",
          "tree_id": "801d32e48b1a160efef80dfb19ac2408c7e5d98f",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/d814d142443e35fa2a720dadf55be3ad60ba1cfa"
        },
        "date": 1780081577959,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 8.361247065663338,
            "unit": "ns",
            "range": "± 0.15629174580813193"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 8.043197610974312,
            "unit": "ns",
            "range": "± 0.09780537927280958"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 7.789140386240823,
            "unit": "ns",
            "range": "± 0.08839886892426223"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 8.413842936356863,
            "unit": "ns",
            "range": "± 0.08533084071415709"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 8.29845294257005,
            "unit": "ns",
            "range": "± 0.1076784904785954"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 8.423838099198681,
            "unit": "ns",
            "range": "± 0.074695482331102"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 318.5509204864502,
            "unit": "ns",
            "range": "± 1.1832473765158085"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 584.6867731639317,
            "unit": "ns",
            "range": "± 3.6316713071178928"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 318.5509204864502,
            "unit": "ns",
            "range": "± 1.1832473765158085"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 584.6867731639317,
            "unit": "ns",
            "range": "± 3.6316713071178928"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.431226593195175,
            "unit": "ns",
            "range": "± 0.3246639647638664"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 6.3294446115692455,
            "unit": "ns",
            "range": "± 0.12660107627639056"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.7276344762996156,
            "unit": "ns",
            "range": "± 0.13934192228921727"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5147255951395402,
            "unit": "ns",
            "range": "± 0.0018687781191442203"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 8.361247065663338,
            "unit": "ns",
            "range": "± 0.15629174580813193"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 8.043197610974312,
            "unit": "ns",
            "range": "± 0.09780537927280958"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 7.789140386240823,
            "unit": "ns",
            "range": "± 0.08839886892426223"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 8.413842936356863,
            "unit": "ns",
            "range": "± 0.08533084071415709"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 8.29845294257005,
            "unit": "ns",
            "range": "± 0.1076784904785954"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 8.423838099198681,
            "unit": "ns",
            "range": "± 0.074695482331102"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.431226593195175,
            "unit": "ns",
            "range": "± 0.3246639647638664"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 6.3294446115692455,
            "unit": "ns",
            "range": "± 0.12660107627639056"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.7276344762996156,
            "unit": "ns",
            "range": "± 0.13934192228921727"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5147255951395402,
            "unit": "ns",
            "range": "± 0.0018687781191442203"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "b09e69ed81e3625d2da03097e36092a0462132f6",
          "message": "test(mutation): add Stryker mutation testing for Security.Jwt module (#105)\n\n## Özet\n`Kck.Security.Jwt` modülü için Stryker mutasyon testi eklendi. Bu modül\nen zayıf kapsamaya sahipti (%50.88, break eşiği 60'ın altında) —\nözellikle token doğrulama ve RSA key yükleme yolları test edilmemişti\n(güvenlik-kritik).\n\n## Değişiklikler\n- **`stryker-jwt.json`** — yeni config (module: \"Security (JWT)\")\n- **`JwtTokenServiceMutationTests.cs`** — claim üretimi\n(rol/name/custom), yanlış issuer/audience/imza-key reddi, hata-sonucu\nmesajları, imza-key yükleme hatasıyla exception catch yolu, File\nkey-source PEM import (çapraz doğrulamalı), key-load hata yönetimi, log\nassertion'ları\n- **`ServiceCollectionExtensionsTests.cs`** — DI registration testleri\n- **`JwtOptionsValidatorTests.cs`** — RefreshTokenTtlDays sınır vakaları\n(0 fail, 1 pass)\n- **`mutation.yml`** — `jwt` modülü CI matrix'ine eklendi\n\n## Sonuç\nMutation score: **%50.88 -> %94.74** (break eşiğinin altından high\neşiğinin üstüne). NoCoverage 17 -> 0, Survived 11 -> 3. Test suite:\n32/32 geçti (önceden 13).\n\n## Kalan 3 survivor — eşdeğer / host-only mutant\n| Konum | Neden |\n|---|---|\n| `ServiceCollectionExtensions:18` `ValidateOnStart()` | Yalnızca host\nbaşlangıcında çalışır; saf DI testinde tetiklenemez |\n| `JwtTokenService:97` `ValidateIssuerSigningKey` | İmza zaten\nIssuerSigningKey'e karşı doğrulanıyor -> true/false farkı gözlemlenemez\n|\n| `JwtTokenService:198` `rsa.Dispose()` (catch) | Kaynak temizliği;\ngözlemlenebilir davranış değişikliği yok |\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\n---------\n\nCo-authored-by: Claude Opus 4.8 <noreply@anthropic.com>",
          "timestamp": "2026-05-29T22:47:18+03:00",
          "tree_id": "60a7d49b40c906f06d00c2428edc7c45cebcf119",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/b09e69ed81e3625d2da03097e36092a0462132f6"
        },
        "date": 1780084473201,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.139469081163407,
            "unit": "ns",
            "range": "± 0.16205673871891996"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.047248630722363,
            "unit": "ns",
            "range": "± 0.18663358349971287"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.223083766301473,
            "unit": "ns",
            "range": "± 0.21946317136742272"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.923413493235905,
            "unit": "ns",
            "range": "± 0.14363815659546977"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.57560814122359,
            "unit": "ns",
            "range": "± 0.12612866253142938"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.191739018474307,
            "unit": "ns",
            "range": "± 0.22443302979777585"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 393.8074289652017,
            "unit": "ns",
            "range": "± 0.48862431271750906"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 715.5526440484183,
            "unit": "ns",
            "range": "± 2.458361933581984"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 393.8074289652017,
            "unit": "ns",
            "range": "± 0.48862431271750906"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 715.5526440484183,
            "unit": "ns",
            "range": "± 2.458361933581984"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.222135220136908,
            "unit": "ns",
            "range": "± 0.316481499138205"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.925534961124261,
            "unit": "ns",
            "range": "± 0.20928366909545001"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.3047296028998163,
            "unit": "ns",
            "range": "± 0.17526660638751002"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6612096801400185,
            "unit": "ns",
            "range": "± 0.0027531681756293343"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.139469081163407,
            "unit": "ns",
            "range": "± 0.16205673871891996"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.047248630722363,
            "unit": "ns",
            "range": "± 0.18663358349971287"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.223083766301473,
            "unit": "ns",
            "range": "± 0.21946317136742272"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.923413493235905,
            "unit": "ns",
            "range": "± 0.14363815659546977"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.57560814122359,
            "unit": "ns",
            "range": "± 0.12612866253142938"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.191739018474307,
            "unit": "ns",
            "range": "± 0.22443302979777585"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.222135220136908,
            "unit": "ns",
            "range": "± 0.316481499138205"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.925534961124261,
            "unit": "ns",
            "range": "± 0.20928366909545001"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.3047296028998163,
            "unit": "ns",
            "range": "± 0.17526660638751002"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6612096801400185,
            "unit": "ns",
            "range": "± 0.0027531681756293343"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "2ccdcbb013dbc86d243fd08a26f10ec1fb120ef8",
          "message": "test(mutation): add Stryker mutation testing for Pipeline.Mediator module (#106)\n\n## Özet\n`Kck.Pipeline.Mediator` modülü için Stryker mutasyon testi eklendi. En\nzayıf kapsamaya sahipti (%28.81, break eşiği 60'ın çok altında) — 5\nbehavior'dan ikisi (Transaction, Logging) tamamen, Caching kısmen test\nedilmemişti.\n\n## Değişiklikler\n- **`stryker-pipeline.json`** — yeni config; `ignore-methods:\n[\"ConfigureAwait\"]` ile öldürülemez `ConfigureAwait(false)` eşdeğer\nmutantları denominator'dan çıkarıldı (test bağlamında\nSynchronizationContext yok)\n- **`TransactionBehaviorTests.cs`** — begin/save/commit başarı yolu +\nrollback/rethrow hata yolu; UoW çağrı sırası ve log assertion'ları\n- **`LoggingBehaviorTests.cs`** — hızlı (Handled) vs yavaş >500ms\n(LongRunning) yolları\n- **`CachingBehaviorMutationTests.cs`** — hit/set loglama +\nsliding-expiration null-coalescing (mesaj değeri vs 5dk default)\ncapturing cache ile\n- **`PipelineBuilderTests.cs`** — 5 Use* behavior'ın doğru open-generic\nIPipelineBehavior kaydı; AddKckMediatorPipeline null-guard ve chaining\n- **`CapturingLogger.cs`** — paylaşılan test helper\n- **`mutation.yml`** — `pipeline` modülü CI matrix'ine eklendi\n\n## Sonuç\nMutation score: **%28.81 -> %95.00** (break eşiğinin çok altından high\neşiğinin üstüne). NoCoverage 29 -> 0. Test suite: 26/26 geçti (önceden\n13).\n\n## Kalan 2 survivor — eşdeğer mutant\n| Konum | Neden |\n|---|---|\n| `LoggingBehavior:28` `sw.Stop()` kaldırma | Elapsed okuması\netkilenmiyor (Stop sadece dondurur) |\n| `LoggingBehavior:30` `>= 500` sınırı | Tam 500ms deterministik olarak\nyakalanamaz |\n\nNot: `ignore-methods: [\"ConfigureAwait\"]` diğer modül config'lerine de\nuygulanabilir (orada görece daha az logic/async oranı olduğu için skoru\nzaten >80'di) — kapsam disiplini için bu PR'da yalnızca pipeline'a\neklendi.\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 4.8 <noreply@anthropic.com>",
          "timestamp": "2026-05-29T23:23:33+03:00",
          "tree_id": "cd0568edb5a93e9775f3008ec7f7817549b2a737",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/2ccdcbb013dbc86d243fd08a26f10ec1fb120ef8"
        },
        "date": 1780086694261,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.151450030008952,
            "unit": "ns",
            "range": "± 0.09038524205145887"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.866285343964895,
            "unit": "ns",
            "range": "± 0.15931769799883055"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.501496880673445,
            "unit": "ns",
            "range": "± 0.05048299721211377"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.329371369590884,
            "unit": "ns",
            "range": "± 0.2898272952215353"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.03054211338361,
            "unit": "ns",
            "range": "± 0.15990886754996128"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.485780842105548,
            "unit": "ns",
            "range": "± 0.16102970554603674"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 401.00289473166833,
            "unit": "ns",
            "range": "± 2.492074809214646"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 727.3682648585393,
            "unit": "ns",
            "range": "± 2.0285712080687714"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 401.00289473166833,
            "unit": "ns",
            "range": "± 2.492074809214646"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 727.3682648585393,
            "unit": "ns",
            "range": "± 2.0285712080687714"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.093264148785518,
            "unit": "ns",
            "range": "± 0.07782612071318011"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.05069835989603,
            "unit": "ns",
            "range": "± 0.5051932131184974"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.2997262493468995,
            "unit": "ns",
            "range": "± 0.1715260829933788"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.09971374731797439,
            "unit": "ns",
            "range": "± 0.005445957961121988"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.151450030008952,
            "unit": "ns",
            "range": "± 0.09038524205145887"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.866285343964895,
            "unit": "ns",
            "range": "± 0.15931769799883055"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.501496880673445,
            "unit": "ns",
            "range": "± 0.05048299721211377"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.329371369590884,
            "unit": "ns",
            "range": "± 0.2898272952215353"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.03054211338361,
            "unit": "ns",
            "range": "± 0.15990886754996128"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.485780842105548,
            "unit": "ns",
            "range": "± 0.16102970554603674"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.093264148785518,
            "unit": "ns",
            "range": "± 0.07782612071318011"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.05069835989603,
            "unit": "ns",
            "range": "± 0.5051932131184974"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.2997262493468995,
            "unit": "ns",
            "range": "± 0.1715260829933788"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.09971374731797439,
            "unit": "ns",
            "range": "± 0.005445957961121988"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "f8827e51605a90db63f74e1803a4cfbee63d0646",
          "message": "build(deps): replace commercial FluentAssertions v8 with AwesomeAssertions 9.4.0 (#107)\n\n## Özet\nFluentAssertions v8 (Ocak 2025) Xceed ticari lisansına geçti\n($130/geliştirici/yıl, ticari kullanımda zorunlu) — projenin \"ücretsiz\naraç\" kuralıyla çelişiyor. Tüm test projeleri **AwesomeAssertions\n9.4.0**'a (Apache 2.0, topluluk fork'u, birebir aynı assertion API)\ntaşındı.\n\nBu değişiklik `/currency-check` protokolü (Anayasa #1–#6) ile\ndoğrulandı.\n\n## Değişiklikler\n- **`Directory.Packages.props`**: FluentAssertions 8.3.0 →\nAwesomeAssertions 9.4.0\n- **39 test `.csproj`**: `PackageReference Include=\"FluentAssertions\"` →\n`AwesomeAssertions`\n- **94 test `.cs`**: `using FluentAssertions;` → `using\nAwesomeAssertions;` (v9 namespace'i yeniden adlandırdı; assertion API ve\ntest mantığı değişmedi)\n- **packages.lock.json** test projelerinde yeniden üretildi\n- **Karar artefaktı**:\n`decisions/2026-05-29-fluentassertions-license.md`\n\n## Alternatif değerlendirme (currency-check)\n| Aday | Lisans | Drop-in | Karar |\n|---|---|---|---|\n| **AwesomeAssertions 9.4.0** | Apache 2.0 (kalıcı) | ✅ aynı API |\n**SEÇİLDİ** |\n| Shouldly 4.3.0 | free | ❌ farklı API (`.ShouldBe()`) | ele |\n| FA v7'ye pin | Apache 2.0 (donmuş) | ✅ | ele (teknik borç) |\n\n## Doğrulama\n- Build: **0 hata**\n- Tüm assertion testleri **geçti**\n- Tek başarısızlık: Docker gerektiren Testcontainers entegrasyon\ntestleri (Persistence EF, Redis) — lokalde Docker kapalı olduğu için; bu\ndeğişiklikle **ilgisiz**, CI'da Docker mevcut\n\n## Not\n- v9 namespace rename'i nedeniyle ilk \"zero-code-change\" varsayımı\ndüzeltildi (artefaktta post-mortem, Doktrin #7).\n- `xunit 2.9.3` (deprecated/Legacy → xunit.v3) ayrı/daha büyük bir\nmigration olarak **ertelendi**.\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 4.8 <noreply@anthropic.com>",
          "timestamp": "2026-05-30T01:58:25+03:00",
          "tree_id": "a654597dc3b9a0dc39221b89aa63e8a8f4f3a3c7",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/f8827e51605a90db63f74e1803a4cfbee63d0646"
        },
        "date": 1780095941770,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.018386072346143,
            "unit": "ns",
            "range": "± 0.09046182350936324"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.907864084201199,
            "unit": "ns",
            "range": "± 0.17835988862025298"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.32831098722375,
            "unit": "ns",
            "range": "± 0.3406915483394398"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.754594592750072,
            "unit": "ns",
            "range": "± 0.31237527377839"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.978664043274792,
            "unit": "ns",
            "range": "± 0.34773347022932305"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.316444062417553,
            "unit": "ns",
            "range": "± 0.4054680030250694"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 466.82695066928864,
            "unit": "ns",
            "range": "± 8.687868239400425"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 782.6515290578207,
            "unit": "ns",
            "range": "± 10.56037786116533"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 466.82695066928864,
            "unit": "ns",
            "range": "± 8.687868239400425"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 782.6515290578207,
            "unit": "ns",
            "range": "± 10.56037786116533"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.462916008631388,
            "unit": "ns",
            "range": "± 0.18189611615284199"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.236726060509682,
            "unit": "ns",
            "range": "± 0.14707006083428403"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6386957346246793,
            "unit": "ns",
            "range": "± 0.004280589219732356"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6766207590699196,
            "unit": "ns",
            "range": "± 0.004024964730230141"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.018386072346143,
            "unit": "ns",
            "range": "± 0.09046182350936324"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.907864084201199,
            "unit": "ns",
            "range": "± 0.17835988862025298"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.32831098722375,
            "unit": "ns",
            "range": "± 0.3406915483394398"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.754594592750072,
            "unit": "ns",
            "range": "± 0.31237527377839"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.978664043274792,
            "unit": "ns",
            "range": "± 0.34773347022932305"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.316444062417553,
            "unit": "ns",
            "range": "± 0.4054680030250694"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.462916008631388,
            "unit": "ns",
            "range": "± 0.18189611615284199"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.236726060509682,
            "unit": "ns",
            "range": "± 0.14707006083428403"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6386957346246793,
            "unit": "ns",
            "range": "± 0.004280589219732356"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6766207590699196,
            "unit": "ns",
            "range": "± 0.004024964730230141"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "c06b5f8932e7d8a0a101fa70f14308145b5f495c",
          "message": "build(deps): bump OpenTelemetry EF instrumentation to 1.15.1-beta.1 (#108)\n\n## Özet\nSağlık/güncellik taramasında\n`OpenTelemetry.Instrumentation.EntityFrameworkCore` `--outdated`\nçıktısında \"Kaynaklarda bulunamadı\" göründü. Araştırmada paketin\n**tasarım gereği stable sürümü olmadığı** (deneysel semantic conventions\n→ kalıcı beta) doğrulandı; \"eksik paket\" değil. Ancak sürümümüz\n(`1.12.0-beta.2`) OTel stack'in geri kalanından (`1.15.x`) 3 beta geride\nkalıyordu.\n\n## Değişiklik\n- `Directory.Packages.props`: `1.12.0-beta.2` → `1.15.1-beta.1`\n(2026-04-21) — stack hizalama\n\n## Breaking change (ele alındı)\n1.13+ `EntityFrameworkInstrumentationOptions.SetDbStatementForText`\nözelliğini **kaldırdı** — `db.query.text` artık her zaman yakalanıyor ve\n**sanitize ediliyor** (literal'ler `?` ile). `KckOpenTelemetryBuilder`:\n```diff\n- .AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = true);\n+ .AddEntityFrameworkCoreInstrumentation();\n```\nHer-zaman-sanitize davranışı `rules/observability.md` (hassas\n`db.statement` sanitize) ile daha uyumlu.\n\n## Doğrulama\n- Build **0 hata**; Observability testleri **10/10** geçti\n- Karar artefaktı:\n`decisions/2026-05-30-otel-ef-instrumentation-bump.md`\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\n---------\n\nCo-authored-by: Claude Opus 4.8 <noreply@anthropic.com>",
          "timestamp": "2026-05-30T02:56:00+03:00",
          "tree_id": "09b30f0b432d22d6bfd23354c73a4074f13efa8c",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/c06b5f8932e7d8a0a101fa70f14308145b5f495c"
        },
        "date": 1780099368496,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 8.127353216089853,
            "unit": "ns",
            "range": "± 0.38303929976311013"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 8.13613155608376,
            "unit": "ns",
            "range": "± 0.11514209782188752"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 7.5634020606676735,
            "unit": "ns",
            "range": "± 0.12683094550103585"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 7.406453670064608,
            "unit": "ns",
            "range": "± 0.06323945361984583"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 7.632999972082102,
            "unit": "ns",
            "range": "± 0.017861746634903176"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 7.513796145362513,
            "unit": "ns",
            "range": "± 0.13839582839798226"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 298.90124320983887,
            "unit": "ns",
            "range": "± 0.43481884679472854"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 555.2093426631047,
            "unit": "ns",
            "range": "± 0.5434132578217994"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 298.90124320983887,
            "unit": "ns",
            "range": "± 0.43481884679472854"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 555.2093426631047,
            "unit": "ns",
            "range": "± 0.5434132578217994"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 4.606752247363329,
            "unit": "ns",
            "range": "± 0.17025351598980162"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 6.064026150320258,
            "unit": "ns",
            "range": "± 0.06183420996305575"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.7198164542516072,
            "unit": "ns",
            "range": "± 0.13542515694779042"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.7873284650536684,
            "unit": "ns",
            "range": "± 0.002301972234338232"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 8.127353216089853,
            "unit": "ns",
            "range": "± 0.38303929976311013"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 8.13613155608376,
            "unit": "ns",
            "range": "± 0.11514209782188752"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 7.5634020606676735,
            "unit": "ns",
            "range": "± 0.12683094550103585"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 7.406453670064608,
            "unit": "ns",
            "range": "± 0.06323945361984583"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 7.632999972082102,
            "unit": "ns",
            "range": "± 0.017861746634903176"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 7.513796145362513,
            "unit": "ns",
            "range": "± 0.13839582839798226"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 4.606752247363329,
            "unit": "ns",
            "range": "± 0.17025351598980162"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 6.064026150320258,
            "unit": "ns",
            "range": "± 0.06183420996305575"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.7198164542516072,
            "unit": "ns",
            "range": "± 0.13542515694779042"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.7873284650536684,
            "unit": "ns",
            "range": "± 0.002301972234338232"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "0a3341701600c4c00246324a80ad31ead53fd080",
          "message": "build(deps): bump Redis, MailKit and AWS SDK to latest minor/patch (#109)\n\n## Özet\nSağlık/güncellik taramasının düşük-riskli minor/patch bump grubu. Hepsi\nNuGet feed'inden doğrulandı (2026-05-30), semver-güvenli, breaking\nchange yok.\n\n| Paket | Eski | Yeni | Tür |\n|---|---|---|---|\n| StackExchange.Redis | 2.12.14 | 2.13.17 | minor |\n| MailKit | 4.16.0 | 4.17.0 | minor (MimeKit transitive izler) |\n| AWSSDK.SimpleEmailV2 | 4.0.12.13 | 4.0.14 | patch |\n| AWSSDK.Core | 4.0.7.1 | 4.0.7.4 | patch (AWS SDK ile hizalama) |\n\n## Bilinçli olarak hariç tutulan\n**Microsoft.IdentityModel.JsonWebTokens** 8.17.0'da bırakıldı —\n`Microsoft.AspNetCore.Authentication.JwtBearer 10.0.5`'in transitive\nçektiği sürümle eşleşmesi için kasıtlı pinli (diamond-conflict guard;\n`Directory.Packages.props` yorumu / LS-FAZ-2 Bölüm 2.3). 8.18'e bump\nçakışma yaratırdı.\n\n## Doğrulama (push öncesi, CI-eşdeğeri)\n- **solution-wide** `dotnet restore` + `dotnet restore --locked-mode` →\n**0 hata** (13 lock dosyası tutarlı)\n- Build (Release): **0 hata**\n- MailKit **15/15**, AmazonSes **11/11** testleri geçti\n- Redis testleri Docker gerektirir → CI'da doğrulanır\n\n> Not: Önceki PR'daki NU1004 dersini uyguladım — sürüm değişiminde\nsolution-wide restore + locked-mode lokal doğrulaması yapıldı.\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 4.8 <noreply@anthropic.com>",
          "timestamp": "2026-05-30T03:34:03+03:00",
          "tree_id": "cdb3986074aa46523dcc9b3f51af8f4aefd9af40",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/0a3341701600c4c00246324a80ad31ead53fd080"
        },
        "date": 1780101693803,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.136432519058387,
            "unit": "ns",
            "range": "± 0.08668789560136854"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.690948041776815,
            "unit": "ns",
            "range": "± 0.2754127849637499"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.241048114640373,
            "unit": "ns",
            "range": "± 0.04429294772161723"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.923365518450737,
            "unit": "ns",
            "range": "± 0.049275827618785795"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.039166097839674,
            "unit": "ns",
            "range": "± 0.14544022593173145"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.672375284135342,
            "unit": "ns",
            "range": "± 0.022091836342401104"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 399.62028755460466,
            "unit": "ns",
            "range": "± 0.5391653316384241"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 735.7018334706625,
            "unit": "ns",
            "range": "± 3.5041727612503943"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 399.62028755460466,
            "unit": "ns",
            "range": "± 0.5391653316384241"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 735.7018334706625,
            "unit": "ns",
            "range": "± 3.5041727612503943"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.93268758803606,
            "unit": "ns",
            "range": "± 0.19101688310190165"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.087114492058754,
            "unit": "ns",
            "range": "± 0.14273792757722784"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.2416177640887016,
            "unit": "ns",
            "range": "± 0.17685070958353669"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6623668063145417,
            "unit": "ns",
            "range": "± 0.002282144709963941"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.136432519058387,
            "unit": "ns",
            "range": "± 0.08668789560136854"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.690948041776815,
            "unit": "ns",
            "range": "± 0.2754127849637499"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.241048114640373,
            "unit": "ns",
            "range": "± 0.04429294772161723"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.923365518450737,
            "unit": "ns",
            "range": "± 0.049275827618785795"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.039166097839674,
            "unit": "ns",
            "range": "± 0.14544022593173145"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.672375284135342,
            "unit": "ns",
            "range": "± 0.022091836342401104"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.93268758803606,
            "unit": "ns",
            "range": "± 0.19101688310190165"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.087114492058754,
            "unit": "ns",
            "range": "± 0.14273792757722784"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.2416177640887016,
            "unit": "ns",
            "range": "± 0.17685070958353669"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6623668063145417,
            "unit": "ns",
            "range": "± 0.002282144709963941"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "856bb3364dd28a4e034a5ff52da3406816faa245",
          "message": "build(deps): bump SourceLink to 10.0.300 and Test.SDK to 18.6.0 (#110)\n\n## Özet\nDüşük-riskli build/test tooling güncellik bump'ları. NuGet feed'inden\ndoğrulandı (2026-05-30), build-only / test-only.\n\n| Paket | Eski | Yeni | Not |\n|---|---|---|---|\n| Microsoft.SourceLink.GitHub | 8.0.0 | 10.0.300 | Build-only\n(PrivateAssets=All); son stable 2026-05-12, versiyonlama SDK ile hizalı\n|\n| Microsoft.NET.Test.Sdk | 18.5.1 | 18.6.0 | minor |\n\n## Hariç tutulan\n`xunit` / `xunit.runner.visualstudio` / `coverlet` major'ları kasıtlı\nbırakıldı — bunlar ayrı **xunit v2→v3 migration**'ına bağlı (breaking;\npremortem + ADR ile ayrıca planlanacak). `Microsoft.IdentityModel` da\npinli kalıyor (diamond-conflict guard).\n\n## Doğrulama (push öncesi, CI-eşdeğeri — NU1004 dersi uygulandı)\n- **solution-wide** `dotnet restore` + `dotnet restore --locked-mode` →\n**0 hata** (102 lock dosyası tutarlı)\n- Build (Release): **0 hata**\n- Core.Abstractions testleri **65/65** geçti\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 4.8 <noreply@anthropic.com>",
          "timestamp": "2026-05-30T03:52:25+03:00",
          "tree_id": "ff7b92c168550f036f69f12f320347600fa21cc3",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/856bb3364dd28a4e034a5ff52da3406816faa245"
        },
        "date": 1780102799641,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.381223324934641,
            "unit": "ns",
            "range": "± 0.22496301763585547"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.436227877552692,
            "unit": "ns",
            "range": "± 0.01897414515273972"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.86548140992721,
            "unit": "ns",
            "range": "± 0.19695429607780107"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.829262622765132,
            "unit": "ns",
            "range": "± 0.08099435634789136"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.895966976881027,
            "unit": "ns",
            "range": "± 0.017109439338712788"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.727771766483784,
            "unit": "ns",
            "range": "± 0.198773176446651"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 404.9935948167528,
            "unit": "ns",
            "range": "± 1.0771867438375644"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 712.1453598658244,
            "unit": "ns",
            "range": "± 3.496260382661377"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 404.9935948167528,
            "unit": "ns",
            "range": "± 1.0771867438375644"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 712.1453598658244,
            "unit": "ns",
            "range": "± 3.496260382661377"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.481524576311526,
            "unit": "ns",
            "range": "± 0.24474114237966016"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.481083711179403,
            "unit": "ns",
            "range": "± 0.0274737082210853"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.9393025448217112,
            "unit": "ns",
            "range": "± 0.18713821948052323"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.4381260889417985,
            "unit": "ns",
            "range": "± 0.16825297989855115"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.381223324934641,
            "unit": "ns",
            "range": "± 0.22496301763585547"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.436227877552692,
            "unit": "ns",
            "range": "± 0.01897414515273972"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.86548140992721,
            "unit": "ns",
            "range": "± 0.19695429607780107"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.829262622765132,
            "unit": "ns",
            "range": "± 0.08099435634789136"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.895966976881027,
            "unit": "ns",
            "range": "± 0.017109439338712788"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.727771766483784,
            "unit": "ns",
            "range": "± 0.198773176446651"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.481524576311526,
            "unit": "ns",
            "range": "± 0.24474114237966016"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.481083711179403,
            "unit": "ns",
            "range": "± 0.0274737082210853"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.9393025448217112,
            "unit": "ns",
            "range": "± 0.18713821948052323"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.4381260889417985,
            "unit": "ns",
            "range": "± 0.16825297989855115"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "c610d00d3120fff5d7e9b2c30698a7bd87909b23",
          "message": "docs(adr): defer xunit v3 migration — blocked by Stryker.NET incompatibility (#111)\n\n## Özet\nxunit v3 migration **değerlendirildi ve ertelendi (NO-GO)**. Karar,\npremortem + tek-proje pilot ile kanıta dayandırıldı; bu PR sadece karar\nartefaktını ekler (kod değişikliği yok).\n\n## Pilot bulguları (Core.Abstractions.Tests, VSTest-uyumlu mod)\n- ✅ `xunit.v3` 3.2.2 + `OutputType=Exe` + runner 3.1.5 → build + 65 test\n- ✅ coverage collection (coverlet/cobertura) çalıştı — VSTest EXE'yi\nkeşfetti\n- ✅ CsCheck v3 ile çalıştı\n- ✅ Kaynak breaking change YOK (repo'da 0 `Xunit.Abstractions`, 0 `async\nvoid`)\n- ❌ **Stryker.NET 4.14.2 + xunit.v3 → mutation skoru %0** (58 mutant, 0\nkilled; \"Crashing...\")\n\n## Blocker\nStryker.NET 4.x, xunit.v3 testlerini mutant başına koşamıyor — bilinen\naçık sorun\n([#3117](https://github.com/stryker-mutator/stryker-net/issues/3117),\n[#3094](https://github.com/stryker-mutator/stryker-net/issues/3094)).\nKök neden xunit.v3'ün Microsoft Testing Platform mimarisi; sürüm bump'ı\nçözmez.\n\n## Gerekçe\nRepodaki **7 mutation config** (bu döngüde %80–95'e çıkarıldı) kritik\nkalite altyapısı. v3 geçişi hepsini %0'a düşürür + mutation CI'ı\nçökertir. xunit v2.9.3 hâlâ güvenlik fix'i alıyor. **Yeniden\ndeğerlendirme:** Stryker MTP/v3 desteği gelince.\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 4.8 <noreply@anthropic.com>",
          "timestamp": "2026-05-30T04:27:42+03:00",
          "tree_id": "e97140376fe8e00e395593d40ad7379470d75d75",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/c610d00d3120fff5d7e9b2c30698a7bd87909b23"
        },
        "date": 1780104860132,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.91517257805054,
            "unit": "ns",
            "range": "± 0.08403364691833637"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.716107617815336,
            "unit": "ns",
            "range": "± 0.07514076838042039"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.747436241461681,
            "unit": "ns",
            "range": "± 0.05434419205952313"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.904722971717517,
            "unit": "ns",
            "range": "± 0.10771028321877718"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.823429396748542,
            "unit": "ns",
            "range": "± 0.06154947826684014"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 11.511322349309921,
            "unit": "ns",
            "range": "± 0.09534208953192835"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 462.89649403889973,
            "unit": "ns",
            "range": "± 3.924259117956213"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 766.0349203745524,
            "unit": "ns",
            "range": "± 2.5882982963775447"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 462.89649403889973,
            "unit": "ns",
            "range": "± 3.924259117956213"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 766.0349203745524,
            "unit": "ns",
            "range": "± 2.5882982963775447"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.8984786208186835,
            "unit": "ns",
            "range": "± 0.04758350882202171"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.851520754835185,
            "unit": "ns",
            "range": "± 0.18474107830655817"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6420126526306074,
            "unit": "ns",
            "range": "± 0.0046401484924123906"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5822651418192046,
            "unit": "ns",
            "range": "± 0.003869006592758199"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.91517257805054,
            "unit": "ns",
            "range": "± 0.08403364691833637"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.716107617815336,
            "unit": "ns",
            "range": "± 0.07514076838042039"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.747436241461681,
            "unit": "ns",
            "range": "± 0.05434419205952313"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.904722971717517,
            "unit": "ns",
            "range": "± 0.10771028321877718"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.823429396748542,
            "unit": "ns",
            "range": "± 0.06154947826684014"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 11.511322349309921,
            "unit": "ns",
            "range": "± 0.09534208953192835"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.8984786208186835,
            "unit": "ns",
            "range": "± 0.04758350882202171"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.851520754835185,
            "unit": "ns",
            "range": "± 0.18474107830655817"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6420126526306074,
            "unit": "ns",
            "range": "± 0.0046401484924123906"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5822651418192046,
            "unit": "ns",
            "range": "± 0.003869006592758199"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "359bc6a095e4109b139f05a797e09de9fc069a9a",
          "message": "test(persistence): raise EF mutation score 13.49% -> 80.95% (#112)\n\n## Özet\n`Kck.Persistence.EntityFramework` mutation kapsamasını **%13.49 →\n%80.95** seviyesine çıkarır (break %60 ✓, high %80 ✓). En büyük\nkapsamasız moduldü.\n\n## Yapılanlar\n- **+92 unit test** (17 → 109), hepsi yeşil:\n- `Security/DynamicFilterWhitelistGuardTests` — recursion / sort / null\ndalları + non-whitelisted alan reddi\n- `Configuration/RegistrationTests` — builder + DI +\n`DefaultEfRepositoryFactory`\n- `HealthChecks/EfCoreHealthCheckTests` — name/tag default +\nHealthy/Unhealthy\n- `Interceptors/{Audit,DomainEventDispatch}…` — sync/async, soft-delete\ndönüşümü, sync warn log\n- `UnitOfWork/EfUnitOfWorkTests` — gerçek transaction\n(begin/commit/rollback/dispose/cache)\n- `Repositories/{EfRepositoryMutation,HelperAndFilter}Tests` — tracking,\ndetached state, not-found guard'ları, range delete, cascade soft-delete,\nglobal filter, paging\n- **SQLite in-memory test provider** eklendi\n(`Microsoft.EntityFrameworkCore.Sqlite` 10.0.8) — transaction,\nquery-filter ve detached-entity yollarını EF InMemory kaldıramadığı\niçin.\n- `stryker-persistence.json` **haftalık mutation CI matrix'ine** eklendi\n(artık break üstünde, CI'ı kırmaz).\n\n## Kalan survivor'lar (~%19)\nÇoğunluğu **eşdeğer mutant**: navigasyonsuz entity için cascade çağrısı,\ntracked entity için redundant `EnsureAttached`, `as IList<T> ??\nToList()` (gözlemlenemez), `_disposed` bayrağı, fall-through\n`return`'ler. Ayrıca `BulkUpdateAsync` (SQLite boxed-object `@value`\ntype mapping desteklemiyor — PostgreSQL Testcontainers entegrasyon\nsuite'i kapsar).\n\n## Not — takip edilebilecek bulgu\n`BulkOperationHelper.BulkUpdateAsync`, `SetProperty(property, _ =>\nvalue!)` ile boxed `object` değer kullandığından SQLite'da `@value`\nparametresine type mapping atayamıyor. Provider taşınabilirliği için\nkırılgan; ayrı bir issue/ADR değerlendirilebilir.\n\n## Doğrulama\n- `dotnet test --filter Category!=Integration` → 109/109 ✓\n- `dotnet stryker -f stryker-persistence.json` → **80.95%** ✓\n- `dotnet restore --locked-mode` (solution-wide) → temiz ✓\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\n\nRelated: #113\n\n---------\n\nCo-authored-by: Claude Opus 4.8 <noreply@anthropic.com>",
          "timestamp": "2026-05-30T22:02:17+03:00",
          "tree_id": "28ccc71c62bd4a6c05cb5c6ef2ebfdf8e9ffa5cf",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/359bc6a095e4109b139f05a797e09de9fc069a9a"
        },
        "date": 1780168162937,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.785291495422522,
            "unit": "ns",
            "range": "± 0.11865650524595324"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.143874407878943,
            "unit": "ns",
            "range": "± 0.16030765371232086"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.652435054572729,
            "unit": "ns",
            "range": "± 0.11839709050081537"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.679632140199343,
            "unit": "ns",
            "range": "± 0.10919624622385074"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.305705378452936,
            "unit": "ns",
            "range": "± 0.11952573621950859"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.680989671912458,
            "unit": "ns",
            "range": "± 0.27567128048417916"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 395.52998198781694,
            "unit": "ns",
            "range": "± 3.923997119723031"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 708.9188318252563,
            "unit": "ns",
            "range": "± 3.7745807868030057"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 395.52998198781694,
            "unit": "ns",
            "range": "± 3.923997119723031"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 708.9188318252563,
            "unit": "ns",
            "range": "± 3.7745807868030057"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.802300640443961,
            "unit": "ns",
            "range": "± 0.17732457346701447"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.729360010226568,
            "unit": "ns",
            "range": "± 0.10575992416672401"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.7269852224024053,
            "unit": "ns",
            "range": "± 0.17885926374296024"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.31553717656061053,
            "unit": "ns",
            "range": "± 0.004851665254132148"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.785291495422522,
            "unit": "ns",
            "range": "± 0.11865650524595324"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.143874407878943,
            "unit": "ns",
            "range": "± 0.16030765371232086"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.652435054572729,
            "unit": "ns",
            "range": "± 0.11839709050081537"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.679632140199343,
            "unit": "ns",
            "range": "± 0.10919624622385074"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.305705378452936,
            "unit": "ns",
            "range": "± 0.11952573621950859"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.680989671912458,
            "unit": "ns",
            "range": "± 0.27567128048417916"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.802300640443961,
            "unit": "ns",
            "range": "± 0.17732457346701447"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.729360010226568,
            "unit": "ns",
            "range": "± 0.10575992416672401"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.7269852224024053,
            "unit": "ns",
            "range": "± 0.17885926374296024"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.31553717656061053,
            "unit": "ns",
            "range": "± 0.004851665254132148"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "eee76fbe2c184f59347db3744adcc362ce33f7e8",
          "message": "security: dependency vulnerability sweep (SDK 10.0.402, Pkcs, SourceLink, test deps) (#140)\n\nClears advisories published after the last green main run (2026-05-30).\nRecord: decisions/2026-10-14-sdk-10.0.402-imagesharp-4.md\n\n- build(sdk): .NET SDK 10.0.203 -> 10.0.402 (security release, runtime\n  10.0.13); global.json + 9 workflows; ILLink.Tasks lock entries only\n- security(deps): SourceLink 10.0.300 -> 10.0.402 (GHSA-23fw-v26w-5fgq)\n- security(messaging): Pkcs >= 10.0.13 floor in Kck.Messaging.MailKit\n  (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7) — consumer-facing\n- security(test): Testcontainers 4.16.0 -> SSH.NET 2026.0.0\n  (GHSA-q939-rpr3-3284, GHSA-mggc-4xg6-vcxf)\n- security(test): SQLitePCLRaw 2.1.14 floor in EF tests (GHSA-2m69-gcr7-jv3q)\n- security(imagesharp): temporary documented suppression of ImageSharp\n  3.1.12 advisories (ImageSharp 4 needs a Six Labors key; review by\n  2026-11-14) + README security notice\n- security(ci): audit gate honours only *.AuditSuppress.props, parsed as XML\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-14T20:21:29+03:00",
          "tree_id": "1493d6dfa73ffcb1787a2a2039b9dc6780b1bc85",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/eee76fbe2c184f59347db3744adcc362ce33f7e8"
        },
        "date": 1791998967730,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 3.8594025348623595,
            "unit": "ns",
            "range": "± 0.13652879385261643"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 4.584585611793128,
            "unit": "ns",
            "range": "± 0.16139097925517604"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.4572328233293125,
            "unit": "ns",
            "range": "± 0.04446011195846666"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.46656381645623374,
            "unit": "ns",
            "range": "± 0.05004096658458749"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 3.8594025348623595,
            "unit": "ns",
            "range": "± 0.13652879385261643"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 4.584585611793128,
            "unit": "ns",
            "range": "± 0.16139097925517604"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.4572328233293125,
            "unit": "ns",
            "range": "± 0.04446011195846666"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.46656381645623374,
            "unit": "ns",
            "range": "± 0.05004096658458749"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 250.7445604642232,
            "unit": "ns",
            "range": "± 4.393923323468416"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 429.6585330327352,
            "unit": "ns",
            "range": "± 6.798928715698478"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 250.7445604642232,
            "unit": "ns",
            "range": "± 4.393923323468416"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 429.6585330327352,
            "unit": "ns",
            "range": "± 6.798928715698478"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 5.464775825540225,
            "unit": "ns",
            "range": "± 0.15854431596944793"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 5.845278336604436,
            "unit": "ns",
            "range": "± 0.23065886088714327"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 6.256876989454031,
            "unit": "ns",
            "range": "± 0.5982149576016105"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 6.921480934674802,
            "unit": "ns",
            "range": "± 0.4590781643204865"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 6.584034954347918,
            "unit": "ns",
            "range": "± 0.2578772934758698"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 5.842785108645083,
            "unit": "ns",
            "range": "± 0.32023415198977606"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 5.464775825540225,
            "unit": "ns",
            "range": "± 0.15854431596944793"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 5.845278336604436,
            "unit": "ns",
            "range": "± 0.23065886088714327"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 6.256876989454031,
            "unit": "ns",
            "range": "± 0.5982149576016105"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 6.921480934674802,
            "unit": "ns",
            "range": "± 0.4590781643204865"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 6.584034954347918,
            "unit": "ns",
            "range": "± 0.2578772934758698"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 5.842785108645083,
            "unit": "ns",
            "range": "± 0.32023415198977606"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "a71053040fe42d76fb868a012de493135252f075",
          "message": "ci(release): gate NuGet publish behind protected environment (#139)\n\n- nuget-publish.yml: publish job runs in the protected `nuget` environment\n  (required reviewer, environment-scoped NUGET_API_KEY) — ADR-0027\n- CLAUDE.md: TIER 3 process profile + team conventions for agent sessions\n  (SDK 10.0.402 pin, audit-suppression rule)\n- decisions/2026-10-14-claude-process-profile.md: TIER rationale\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-14T20:40:42+03:00",
          "tree_id": "b87dbd819acafe2a3fb9df92ca6a9013664dc439",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/a71053040fe42d76fb868a012de493135252f075"
        },
        "date": 1792000048203,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 3.8590554387441705,
            "unit": "ns",
            "range": "± 0.09372835738503948"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 5.100631491414139,
            "unit": "ns",
            "range": "± 0.11431234138430564"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.5396471793896386,
            "unit": "ns",
            "range": "± 0.07442624004082045"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.2034105695784092,
            "unit": "ns",
            "range": "± 0.022154137030021465"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 3.8590554387441705,
            "unit": "ns",
            "range": "± 0.09372835738503948"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 5.100631491414139,
            "unit": "ns",
            "range": "± 0.11431234138430564"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.5396471793896386,
            "unit": "ns",
            "range": "± 0.07442624004082045"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.2034105695784092,
            "unit": "ns",
            "range": "± 0.022154137030021465"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 234.98911985984216,
            "unit": "ns",
            "range": "± 1.7287034569227095"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 443.4634053366525,
            "unit": "ns",
            "range": "± 1.7057943566554499"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 234.98911985984216,
            "unit": "ns",
            "range": "± 1.7287034569227095"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 443.4634053366525,
            "unit": "ns",
            "range": "± 1.7057943566554499"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 6.298456077190006,
            "unit": "ns",
            "range": "± 0.3404071892910995"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 6.294410610023667,
            "unit": "ns",
            "range": "± 0.17325408067532339"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 6.120294180053931,
            "unit": "ns",
            "range": "± 0.1310131114077238"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 6.128168374300003,
            "unit": "ns",
            "range": "± 0.15360784695134247"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 6.468842745520348,
            "unit": "ns",
            "range": "± 0.31873995265930494"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 6.549274655488821,
            "unit": "ns",
            "range": "± 0.13101684316852819"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 6.298456077190006,
            "unit": "ns",
            "range": "± 0.3404071892910995"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 6.294410610023667,
            "unit": "ns",
            "range": "± 0.17325408067532339"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 6.120294180053931,
            "unit": "ns",
            "range": "± 0.1310131114077238"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 6.128168374300003,
            "unit": "ns",
            "range": "± 0.15360784695134247"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 6.468842745520348,
            "unit": "ns",
            "range": "± 0.31873995265930494"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 6.549274655488821,
            "unit": "ns",
            "range": "± 0.13101684316852819"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "f443a7b898d3ca20fe1b051e636d2091f6658f5e",
          "message": "security(imagesharp): upgrade ImageSharp 3.1.12 -> 4.1.3, drop suppression (#141)\n\nFixes GHSA-j3p4-wp97-rph4, GHSA-j9gm-c75j-xc9q, GHSA-jjfr-hcj7-qf5w (high),\nGHSA-gwg2-r3hj-4w44 and GHSA-wmxv-xphr-5c9g in Kck.Documents.ImageSharp.\n\n- SixLabors.ImageSharp 3.1.12 -> 4.1.3; temporary audit suppression removed\n- ImageSharp 4 Release builds validate an assembly-scoped Six Labors\n  Community license (Kck.Documents.ImageSharp, Kck.Documents.ImageSharp.Tests):\n  CI reads SIXLABORS_LICENSE_KEY via the SixLaborsLicenseKey env var in the\n  7 Release-building workflows; locally a git-ignored repo-root\n  sixlabors.lic is picked up by Directory.Build.props\n- README licensing note; CONTRIBUTING: Release needs a key, Debug warns\n- decisions/: license scope, storage (secret only) and renewal (2028-01-12)\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-14T21:33:51+03:00",
          "tree_id": "2ecb313f960fc569532541f800f62c7688c36bf3",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/f443a7b898d3ca20fe1b051e636d2091f6658f5e"
        },
        "date": 1792003284096,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.743998820290846,
            "unit": "ns",
            "range": "± 0.1976372074908813"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 12.109270149634945,
            "unit": "ns",
            "range": "± 0.5046970437020103"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6397160520920386,
            "unit": "ns",
            "range": "± 0.00628236520081958"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.693374232522079,
            "unit": "ns",
            "range": "± 0.0038539409415599244"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.743998820290846,
            "unit": "ns",
            "range": "± 0.1976372074908813"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 12.109270149634945,
            "unit": "ns",
            "range": "± 0.5046970437020103"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6397160520920386,
            "unit": "ns",
            "range": "± 0.00628236520081958"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.693374232522079,
            "unit": "ns",
            "range": "± 0.0038539409415599244"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 470.45111586252847,
            "unit": "ns",
            "range": "± 3.4541323761559726"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 797.9266832351684,
            "unit": "ns",
            "range": "± 7.950784163814436"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 470.45111586252847,
            "unit": "ns",
            "range": "± 3.4541323761559726"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 797.9266832351684,
            "unit": "ns",
            "range": "± 7.950784163814436"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.2329620718956,
            "unit": "ns",
            "range": "± 0.10763931243464249"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.46692310592958,
            "unit": "ns",
            "range": "± 0.2400267240020151"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.872107190304789,
            "unit": "ns",
            "range": "± 0.41563202576605246"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 12.959466211001079,
            "unit": "ns",
            "range": "± 0.2702566873427991"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.44112353026867,
            "unit": "ns",
            "range": "± 0.24564387565213505"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.2916907998068,
            "unit": "ns",
            "range": "± 0.1851595635058826"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.2329620718956,
            "unit": "ns",
            "range": "± 0.10763931243464249"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.46692310592958,
            "unit": "ns",
            "range": "± 0.2400267240020151"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.872107190304789,
            "unit": "ns",
            "range": "± 0.41563202576605246"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 12.959466211001079,
            "unit": "ns",
            "range": "± 0.2702566873427991"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.44112353026867,
            "unit": "ns",
            "range": "± 0.24564387565213505"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.2916907998068,
            "unit": "ns",
            "range": "± 0.1851595635058826"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "73bc1146c788494e99e50adf673ede3508203f35",
          "message": "chore(deps): update grouped dependencies (replaces stale Dependabot PRs) (#142)\n\n## Summary\n\nGrouped same-major NuGet updates (versions checked on nuget.org,\n2026-10-14). Replaces the stale Dependabot PRs\nlisted below, which were opened before the 2026-10 security sweep (#140)\nand target older versions.\n\n## Changed packages (`Directory.Packages.props`)\n\n| Group | Package | From | To |\n|---|---|---|---|\n| EF Core | Microsoft.EntityFrameworkCore / .InMemory / .Sqlite | 10.0.8\n| 10.0.13 |\n| EF Core | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.1 | 10.0.3 |\n| Microsoft.Extensions | Caching.StackExchangeRedis,\nDiagnostics.HealthChecks, DependencyInjection.Abstractions, Options,\nHosting.Abstractions, Logging.Abstractions | 10.0.8 | 10.0.13 |\n| Microsoft.Extensions | Caching.Hybrid, Resilience, Http.Resilience,\nServiceDiscovery | 10.6.0 | 10.11.0 |\n| Microsoft.Extensions | Configuration.UserSecrets | 10.0.5 | 10.0.13 |\n| Background jobs | Hangfire | 1.8.23 | 1.8.25 |\n| Background jobs | Quartz, Quartz.Extensions.DependencyInjection,\nQuartz.Extensions.Hosting | 3.18.1 | 3.22.5 |\n| Azure | Azure.Messaging.ServiceBus | 7.20.1 | 7.21.0 |\n| Azure | Azure.Data.AppConfiguration | 1.9.0 | 1.11.1 |\n| Azure | Azure.Storage.Blobs | 12.28.0 | 12.30.1 |\n| Azure | Azure.Security.KeyVault.Secrets | 4.11.0 | 4.11.2 |\n\nRemoved: the test-only `SQLitePCLRaw.bundle_e_sqlite3` 2.1.14 floor (and\nits reference in\n`Kck.Persistence.EntityFramework.Tests.csproj`). EF Core Sqlite 10.0.13\npulls SQLitePCLRaw 2.1.13 itself, which is\npatched for GHSA-2m69-gcr7-jv3q.\n\nOut of scope: Quartz 4.x (major), OpenTelemetry (#137), GitHub Actions\nmajors (#119, #127).\n\nLock files regenerated solution-wide (`dotnet restore --force-evaluate`,\nthen `--locked-mode`).\n\n## Supersedes (close after merge)\n\n- #133 chore(deps): Bump the quartz group with 3 updates\n- #134 chore(deps): Bump the microsoft-extensions group with 10 updates\n- #135 chore(deps): Bump the entityframework group with 4 updates\n- #138 chore(deps): Bump the azure group with 3 updates\n\n(#131 hangfire was already closed; Hangfire 1.8.25 is included here.)\n\n## Verification (local, from the commit)\n\n- NuGet audit clean; `dotnet restore --locked-mode` passes\n- `dotnet build -c Release`: 0 errors\n- `dotnet test -c Release`: 39/39 test projects, 680 passed, 0 failed\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-14T22:48:27+03:00",
          "tree_id": "2cdc848d68433b5d5db381018c1dd57916527a49",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/73bc1146c788494e99e50adf673ede3508203f35"
        },
        "date": 1792007816541,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.469662334989099,
            "unit": "ns",
            "range": "± 0.17337043034164903"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 11.366800716850493,
            "unit": "ns",
            "range": "± 0.8084969552799457"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6301054789551668,
            "unit": "ns",
            "range": "± 0.0029534159352752024"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5853146399770465,
            "unit": "ns",
            "range": "± 0.003315018281517086"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.469662334989099,
            "unit": "ns",
            "range": "± 0.17337043034164903"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 11.366800716850493,
            "unit": "ns",
            "range": "± 0.8084969552799457"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6301054789551668,
            "unit": "ns",
            "range": "± 0.0029534159352752024"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5853146399770465,
            "unit": "ns",
            "range": "± 0.003315018281517086"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 490.51301043374195,
            "unit": "ns",
            "range": "± 4.934307633202752"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 847.9299933569772,
            "unit": "ns",
            "range": "± 2.2805897403118043"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 490.51301043374195,
            "unit": "ns",
            "range": "± 4.934307633202752"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 847.9299933569772,
            "unit": "ns",
            "range": "± 2.2805897403118043"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 11.433259334634332,
            "unit": "ns",
            "range": "± 0.2847022655635914"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.873110167243901,
            "unit": "ns",
            "range": "± 0.28981366826681093"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.98542302966118,
            "unit": "ns",
            "range": "± 0.3687808628728211"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 11.036497688293457,
            "unit": "ns",
            "range": "± 0.4638371475591007"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.367356280711563,
            "unit": "ns",
            "range": "± 0.34890636265671066"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 11.513718961653383,
            "unit": "ns",
            "range": "± 0.5511670316604338"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 11.433259334634332,
            "unit": "ns",
            "range": "± 0.2847022655635914"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.873110167243901,
            "unit": "ns",
            "range": "± 0.28981366826681093"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.98542302966118,
            "unit": "ns",
            "range": "± 0.3687808628728211"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 11.036497688293457,
            "unit": "ns",
            "range": "± 0.4638371475591007"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.367356280711563,
            "unit": "ns",
            "range": "± 0.34890636265671066"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 11.513718961653383,
            "unit": "ns",
            "range": "± 0.5511670316604338"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "58aa876ba7b51234a58ab5169df8d51f30c3b2e7",
          "message": "ci(benchmark): make regression alerts informational, dedupe reports (#143)\n\n## Summary\nThe `benchmark` workflow has been red on every main push since May.\nDiagnosis: measurement noise, not a regression.\n\n- Benchmarked code (`Result.cs`, `Paginate.cs`) is unchanged since #69,\nbefore the whole benchmark history.\n- `ubuntu-latest` runs on different CPUs (AMD EPYC 9V45 / AVX-512 vs\nEPYC 7763 / AVX2). At f443a7b all 12 benchmarks \"regressed\" 1.6x-3.4x at\nonce.\n- Replaying gh-pages history (25 comparisons): 110% -> 17 red runs, 150%\n-> 8, even 300% -> 4.\n- The job runs post-merge, so failing it never blocked anything.\n\n## Changes\n- `fail-on-alert: false`, `alert-threshold: '150%'` — commit comment +\njob summary + gh-pages chart remain.\n- Merge step reads only `*-report-full-compressed.json` (format\nrecommended by the action's BenchmarkDotNet example). Previously both\nthe brief and full JSON were merged, so every benchmark was stored\ntwice.\n- Decision record: `decisions/2026-10-14-benchmark-regression-gate.md`\n(alternatives: higher threshold, more iterations, paid runners —\nrejected; same-runner A/B on PRs — deferred).\n\n## Test plan\n- [x] Workflow YAML parses\n- [ ] `benchmark` run green after merge to main (workflow only triggers\non `push: main` / `workflow_dispatch`)\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-15T01:01:38+03:00",
          "tree_id": "e56ea3fb22643283e5311ec5681f6ad60527374e",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/58aa876ba7b51234a58ab5169df8d51f30c3b2e7"
        },
        "date": 1792015716212,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.660527956398094,
            "unit": "ns",
            "range": "± 0.2750848809068953"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.697325607140859,
            "unit": "ns",
            "range": "± 0.17932837183744968"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.0148319127754524,
            "unit": "ns",
            "range": "± 0.16530339798061613"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.4745886417726676,
            "unit": "ns",
            "range": "± 0.04562741141972155"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 364.7924154145377,
            "unit": "ns",
            "range": "± 5.200996371386596"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 650.8094680786132,
            "unit": "ns",
            "range": "± 8.459346480999283"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.040344510423509,
            "unit": "ns",
            "range": "± 0.2626846004320764"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.46489370082106,
            "unit": "ns",
            "range": "± 0.14745001087326867"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 8.985733824116844,
            "unit": "ns",
            "range": "± 0.2827950627342844"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.138076498040132,
            "unit": "ns",
            "range": "± 0.21280664964746007"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.233771263062954,
            "unit": "ns",
            "range": "± 0.17720609704474305"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.042303320239572,
            "unit": "ns",
            "range": "± 0.25453981233158324"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "dffae45ea834842343a0705b84b7e642b89f4257",
          "message": "fix(persistence): type BulkUpdateAsync setters for provider portability (#144)\n\n## Summary\n`BulkOperationHelper.BulkUpdateAsync` passed `object`-typed\n`SetProperty` calls to `ExecuteUpdateAsync`. Providers with no type\nmapping for `System.Object` (SQLite) failed with `Expression '@value' in\nthe SQL tree does not have a type mapping assigned` — reproduced on EF\nCore 10.0.13 for both `int` and `string` columns.\n\nEach update now unwraps the boxed `Convert(e.Prop, object)` lambda and\ncalls the typed `UpdateSettersBuilder<T>.SetProperty<TProperty>` via\nreflection (the dynamic-setter pattern from the [EF Core 10 release\nnotes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew)).\nValues are sent as typed parameters (`SET \"Value\" = @p`).\n\n**Public API unchanged** (`IWriteRepository.BulkUpdateAsync` signature\nintact) — no ADR needed.\n\n### Behaviour change\nValues must match the property CLR type exactly. A mismatch (e.g. `99L`\nfor an `int` column) or `null` for a non-nullable value type throws\n`ArgumentException` **before** any SQL runs, so no partial update.\n\n### Alternatives considered\n- **Typed generic public API** — breaking change to `IWriteRepository`,\nneeds an EF-free setter abstraction in Abstractions; disproportionate\nfor this bug.\n- **Typed constant inside `Convert`** — works, but inlines literals\n(`CAST(99 AS INTEGER)`) → per-value SQL text and plan-cache churn.\n- **Docs only** — leaves SQLite broken and the method untestable without\nPostgres.\n\n## Tests\n- New `BulkUpdateTests` (SQLite in-memory): value type, multiple\nproperties, null → nullable, wrong type (rows unchanged), null →\nnon-nullable. RED 5/5 before the fix, GREEN after.\n- `Kck.Persistence.EntityFramework.Tests` Release: 116/116.\n- Stryker (`stryker-persistence.json`): 80.95% → **82.09%**;\n`BulkOperationHelper` 8/9 killed (survivor: `Single`→`SingleOrDefault`,\nequivalent).\n- Not run locally: full-solution Release build (ImageSharp 4 licence\nfile absent in the worktree; CI uses the secret).\n\nFixes #113\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-14T22:20:32Z",
          "tree_id": "00e1d08a474f60e5c5d0e6056b8e492757177c03",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/dffae45ea834842343a0705b84b7e642b89f4257"
        },
        "date": 1792016831289,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.171889091531436,
            "unit": "ns",
            "range": "± 0.061215566807182174"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.807987760504087,
            "unit": "ns",
            "range": "± 0.1362654144337476"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.639981316526731,
            "unit": "ns",
            "range": "± 0.009596508242512899"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5839680896273681,
            "unit": "ns",
            "range": "± 0.0016233587266588708"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 465.74748137791954,
            "unit": "ns",
            "range": "± 4.014323859345954"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 801.3593625386555,
            "unit": "ns",
            "range": "± 2.636915147298171"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.564589336514473,
            "unit": "ns",
            "range": "± 0.07782657295715406"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.08529561261336,
            "unit": "ns",
            "range": "± 0.11432575111910673"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.309979783800932,
            "unit": "ns",
            "range": "± 0.07463490267182234"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.055617789427439,
            "unit": "ns",
            "range": "± 0.09340294242673652"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.24668529033661,
            "unit": "ns",
            "range": "± 0.06495982657601217"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.104663982987404,
            "unit": "ns",
            "range": "± 0.07360105503242784"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "b4fed20844c2f4aad811634bc23a56daa0b449a9",
          "message": "security(ci): pin all GitHub Actions to full commit SHAs (#145)\n\n32 uses: lines in 11 workflows pinned to owner/repo@<sha> # vX.Y.Z; each SHA verified against its release tag (latest of its major). zizmor unpinned-uses 32 -> 0. Record: decisions/2026-10-14-github-actions-sha-pinning.md\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-15T01:49:10+03:00",
          "tree_id": "ecaa91b1c19b6798c30d287e9b05a4d2b027fa94",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/b4fed20844c2f4aad811634bc23a56daa0b449a9"
        },
        "date": 1792018604711,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 7.462098620335261,
            "unit": "ns",
            "range": "± 0.11701685433952491"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 11.355632548681413,
            "unit": "ns",
            "range": "± 1.0113347209658559"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6408415700380619,
            "unit": "ns",
            "range": "± 0.008828382443017641"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5856415417331916,
            "unit": "ns",
            "range": "± 0.005235111046645264"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 502.7914215496608,
            "unit": "ns",
            "range": "± 2.684292688381234"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 766.0125188191731,
            "unit": "ns",
            "range": "± 2.5557803167426454"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.317797189785374,
            "unit": "ns",
            "range": "± 0.2757715989993018"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.503422579169273,
            "unit": "ns",
            "range": "± 0.25408204054661876"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.850562026395517,
            "unit": "ns",
            "range": "± 0.2855255095522728"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 12.320972188313801,
            "unit": "ns",
            "range": "± 0.16499819509560246"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.884868828455607,
            "unit": "ns",
            "range": "± 0.15863966574680569"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 12.143517847855886,
            "unit": "ns",
            "range": "± 0.09980383592575344"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "3360b68ec662e18bd5824768e5768a1beb71a15c",
          "message": "security(ci): fix zizmor findings (bot conditions, injection, credentials, permissions) (#146)\n\nsecurity(ci): fix zizmor findings (bot conditions, injection, credentials, permissions)\n\nzizmor 1.30.1: 19 findings -> 1 informational (use-trusted-publishing,\ndeferred to a separate ADR).\n\n- bot-conditions: Dependabot jobs check\n  github.event.pull_request.user.login instead of the spoofable\n  github.actor.\n- template-injection (nuget-publish): the tag input reaches the shell only\n  via an env var, is validated against ^v<semver>(-pre)?$ before checkout,\n  never echoed, and flows as $VERSION; the API key is read from the env.\n- artipacked: persist-credentials: false on every checkout except\n  dependabot-lockfile, which pushes regenerated lock files (explicit true).\n  benchmark pushes through its own github-token; docs deploys via\n  deploy-pages.\n- permissions: benchmark and dependabot-auto-merge have no workflow-level\n  permissions; each job declares only what it uses.\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-15T02:07:24+03:00",
          "tree_id": "573794ade0634ed97dd83871d36e485015e6f3a2",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/3360b68ec662e18bd5824768e5768a1beb71a15c"
        },
        "date": 1792019749965,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.773132257439472,
            "unit": "ns",
            "range": "± 0.27225142313613465"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 11.298251159023494,
            "unit": "ns",
            "range": "± 0.840052981396369"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6369267538189889,
            "unit": "ns",
            "range": "± 0.0031969103544613312"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5809156309281077,
            "unit": "ns",
            "range": "± 0.0028829701477380352"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 462.50970772334506,
            "unit": "ns",
            "range": "± 2.6754005908731693"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 833.5189517339071,
            "unit": "ns",
            "range": "± 4.934220096768734"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 12.034155638380485,
            "unit": "ns",
            "range": "± 0.36104203864736495"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 11.653456337749958,
            "unit": "ns",
            "range": "± 0.28911839906931197"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.632012088100115,
            "unit": "ns",
            "range": "± 0.2521828861187324"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.979088741044203,
            "unit": "ns",
            "range": "± 0.47683625123032347"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 13.519741597175598,
            "unit": "ns",
            "range": "± 0.43474601535119656"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 11.25235235487873,
            "unit": "ns",
            "range": "± 0.34411399046091945"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "e2955976231c98110d76094ec95dbe028640d00a",
          "message": "ci(actions): bump checkout v7.0.1, setup-dotnet v6.0.0, setup-java v6.1.0 (pinned) (#147)\n\nReplaces #119 and #127. Release notes reviewed; SHAs verified against release tags.\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-15T02:15:47+03:00",
          "tree_id": "09fce764c8c52df5563cb630d2de179ddc102a74",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/e2955976231c98110d76094ec95dbe028640d00a"
        },
        "date": 1792020147971,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 3.6044863084597245,
            "unit": "ns",
            "range": "± 0.08787009057698611"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 4.543575842465673,
            "unit": "ns",
            "range": "± 0.08712220508418818"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.47725561757882434,
            "unit": "ns",
            "range": "± 0.016371735476996545"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.2505786550732759,
            "unit": "ns",
            "range": "± 0.017215706561074254"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 243.50347274541855,
            "unit": "ns",
            "range": "± 1.0036102294237308"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 415.67742988467216,
            "unit": "ns",
            "range": "± 8.088806544200565"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 5.268095700868538,
            "unit": "ns",
            "range": "± 0.09944787212593865"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 5.42689127315368,
            "unit": "ns",
            "range": "± 0.12422210623511538"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 5.445689529180527,
            "unit": "ns",
            "range": "± 0.09162389044175842"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 5.639681125991046,
            "unit": "ns",
            "range": "± 0.15080737256007484"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 5.4152656508528665,
            "unit": "ns",
            "range": "± 0.18020240265711523"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 5.503245426341891,
            "unit": "ns",
            "range": "± 0.1464750252088643"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "681e95204eaa1daf53631fb50672beb60d2fa588",
          "message": "ci(release): NuGet Trusted Publishing, split build/publish, keep only newest version listed (#148)\n\nPublish via NuGet Trusted Publishing (OIDC, 1-hour single-use key; no stored API key). build (no OIDC, no environment) and publish (needs build, nuget environment + required reviewer, id-token only, runs no repo code). After a successful stable push, older listed versions are unlisted. ADR-0027 updated.\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-15T03:14:39+03:00",
          "tree_id": "352e6ffa1d17158f210e7dc9b7553de532a74892",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/681e95204eaa1daf53631fb50672beb60d2fa588"
        },
        "date": 1792023701697,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.340878809491794,
            "unit": "ns",
            "range": "± 0.11646358809660369"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.412896730005741,
            "unit": "ns",
            "range": "± 0.11878327323336721"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6437078743026807,
            "unit": "ns",
            "range": "± 0.0062483798959722826"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 1.097352991004785,
            "unit": "ns",
            "range": "± 0.0038022923955399242"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 488.22767972946167,
            "unit": "ns",
            "range": "± 3.916580559649722"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 795.7228502860436,
            "unit": "ns",
            "range": "± 2.4431288837906786"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.732886924346287,
            "unit": "ns",
            "range": "± 0.23502048415300594"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.322615806944668,
            "unit": "ns",
            "range": "± 0.25813713143437184"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.419992408901454,
            "unit": "ns",
            "range": "± 0.30892891680933066"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.316247940529138,
            "unit": "ns",
            "range": "± 0.41561374890621605"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.392335059387344,
            "unit": "ns",
            "range": "± 0.12390525457498108"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 11.210864979454449,
            "unit": "ns",
            "range": "± 0.46711108863625417"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "6489b6d1dbb9adbca31c4cf3d42b4c58c71aa04e",
          "message": "security(ci): fail-closed license gate; no paid-license dependencies (#164)\n\n## Why\n\nProject rule: **no dependency may require a paid license.** The existing\n`license-audit` job could not enforce it:\n\n- it grepped `nuget-license` output for `GPL|LGPL|SSPL|BUSL` — packages\nthat ship their license as a **file** never matched;\n- tool errors were swallowed with `|| true` (fail-open) and the tool was\ninstalled unpinned.\n\nResult: the job reported \"OK: no copyleft contamination detected\" (run\n40324570972) while `Kck.Pipeline.MediatR`\nshipped **MediatR 14.1.0** (RPL-1.5 or paid Lucky Penny license), and\nHangfire (LGPL-3.0) and ImageSharp (Split\nLicense) were never reviewed.\n\n## What\n\n- **Fail-closed license gate** — `.github/scripts/license_gate.py` (+ 17\n`unittest` tests, stdlib only): reads every\n`packages.lock.json`, checks each nuspec license against\n`.github/license-policy.json`. File licenses are pinned by\nthe SHA-256 of their text, so a vendor relicensing a new version fails\nthe gate. Unknown / changed / expired-exception\n  licenses and any gate error fail the job.\n- **Policy** — permissive allowlist; documented exceptions: `Hangfire*`\n(LGPL-3.0), `SixLabors.ImageSharp` (Apache-2.0\nfor Kck consumers via the transitive-dependency clause); `MediatR`\ntemporary until **2027-01-31** — `Kck.Pipeline.MediatR`\n  is removed in v4.0.0.\n- `docs/licensing.md`, CONTRIBUTING rule, decision record\n`decisions/2026-10-26-license-gate.md`.\n- Coverage threshold **43/38 → 55/53** (main measured 56 / 54.5).\n- Removed unreferenced `PackageVersion` entries (NetAnalyzers preview,\nSystem.Drawing.Common); Dependabot testing\n  group now matches AwesomeAssertions.\n- Deprecation table lists every pending removal (KCK0001 was due in\nv1.0).\n- `Kck.Documents.Abstractions` README: Quick Start had 6 compile errors\n— rewritten against the real API.\nImageSharp package description no longer claims AVIF (GIF/BMP are\nsupported); MediatR package carries a license warning.\n\nNo runtime code changed.\n\n## Verification (local)\n\n| Check | Result |\n|---|---|\n| Gate unit tests | 17/17 pass (RED seen first) |\n| Gate on repo, real policy | PASS — 224 packages, 0 violations, 8\ndocumented exceptions |\n| Gate without policy entries | FAIL — names MediatR, ImageSharp,\nHangfire |\n| Mutation probes (drop MediatR entry, change hash, expiry passed, wrong\nNuGet root, missing policy) | all FAIL |\n| `dotnet restore --force-evaluate` / `--locked-mode` | lock files\nunchanged / pass |\n| `dotnet build -c Release` | 0 errors (5 known CA1716 warnings) |\n| README sample compiled in a scratch project | 0 errors (previous\nsample: 5 errors) |\n| actionlint / zizmor | not run locally (tools not installed) — CI\nchecks apply |\n\n## Follow-ups (not in this PR)\n\n- Gate does not cover `.config/dotnet-tools.json` tools yet (all\npermissive today).\n- `dotnet new` template pins `Kck.Bundle.WebApi` 3.0.0.\n\nRoadmap: F0 of the license / quality plan (v3.6 → v4.0 cleanup).\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\n---------\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-26T19:52:01+03:00",
          "tree_id": "87b1d27be64873d4013542abf64aa013a7e24ce8",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/6489b6d1dbb9adbca31c4cf3d42b4c58c71aa04e"
        },
        "date": 1793033971583,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.697232168700014,
            "unit": "ns",
            "range": "± 0.0783519665187231"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 13.288111194372178,
            "unit": "ns",
            "range": "± 1.1989086603966157"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6407104457418125,
            "unit": "ns",
            "range": "± 0.0039385086047506075"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5865904273731368,
            "unit": "ns",
            "range": "± 0.005674854518317917"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 487.3698136989887,
            "unit": "ns",
            "range": "± 2.642065044660497"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 799.7131769816081,
            "unit": "ns",
            "range": "± 5.947475327489924"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.944368349654335,
            "unit": "ns",
            "range": "± 0.17538802848513776"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 11.629280901948611,
            "unit": "ns",
            "range": "± 0.18369732612429904"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.204306958119075,
            "unit": "ns",
            "range": "± 0.22054313180314006"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.783415228128433,
            "unit": "ns",
            "range": "± 0.20769467679818437"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.036250662058592,
            "unit": "ns",
            "range": "± 0.3144130391419601"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 11.465319571395716,
            "unit": "ns",
            "range": "± 0.18123434778276765"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "19b7d05c7cd344ea056561e8b92fe73ac9fac8ef",
          "message": "fix(ci): run required checks after Dependabot lock file pushes (#169)\n\n## Why\n\n`dependabot-lockfile` regenerates `packages.lock.json` and pushes it to\nthe Dependabot branch with `GITHUB_TOKEN`.\nGitHub does not start `pull_request` runs for pushes made with\n`GITHUB_TOKEN` — they sit in **`action_required`**\n(e.g. ClosedXML branch, head `cca2c00`: build-test, codeql,\nlicense-audit, sonar all `action_required`). Result:\nevery Dependabot PR whose lock files changed (#153, #158, #159, #160)\nhad **no checks** on its head commit and could\nnever satisfy branch protection.\n\n## What\n\n- After it pushes, `dependabot-lockfile` dispatches `build-test.yml`,\n`codeql.yml` and `license-audit.yml` on the PR\nbranch (`gh workflow run --ref <head_ref>`). Per GitHub docs,\n`workflow_dispatch` and `repository_dispatch` \"always\n  create workflow runs\", so **no PAT / GitHub App secret** is needed.\n- `codeql.yml`: add a `workflow_dispatch` trigger. Job permission\n`actions: write` for the dispatch.\n- `head_ref` reaches the shell through an env var (no expression\ninterpolation in `run`).\n\n## Security note\n\nDispatched runs receive Actions secrets instead of Dependabot secrets.\n`SIXLABORS_LICENSE_KEY` is already defined in\nboth stores, so no new secret becomes reachable by dependency-update\nbuilds.\n\n## Verification\n\n- YAML parses; change is CI-only.\n- GitHub docs do not state whether dispatched runs satisfy required\nchecks on the PR head SHA. After merge this is\nverified on a real Dependabot PR (`@dependabot recreate` on #160) before\nrelying on it.\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\n---------\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-26T20:40:08+03:00",
          "tree_id": "818b76fb6bbfd763d51d400c70e1ca1b6b57de59",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/19b7d05c7cd344ea056561e8b92fe73ac9fac8ef"
        },
        "date": 1793036784943,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 7.649604165554047,
            "unit": "ns",
            "range": "± 0.04684003764719666"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 9.212676854005881,
            "unit": "ns",
            "range": "± 0.05286418845713103"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6134668673078219,
            "unit": "ns",
            "range": "± 0.06637154044075108"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.575343345957143,
            "unit": "ns",
            "range": "± 0.018748821701419175"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 405.6850063250615,
            "unit": "ns",
            "range": "± 0.9446868929696054"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 734.6977419172015,
            "unit": "ns",
            "range": "± 2.315836941934119"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 11.614817093525614,
            "unit": "ns",
            "range": "± 0.09861774088702088"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 12.624170593420665,
            "unit": "ns",
            "range": "± 0.1304900930549523"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.726283715168636,
            "unit": "ns",
            "range": "± 0.09783810177111205"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 12.592416183153789,
            "unit": "ns",
            "range": "± 0.13493169262368176"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 12.765398238102595,
            "unit": "ns",
            "range": "± 0.06639000371918989"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 12.825285057226816,
            "unit": "ns",
            "range": "± 0.1533940500057783"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "856c84f402316bddb8847ced7093935dc982e523",
          "message": "ci(coverage): branch threshold 51 for coverlet 10 measurement (#172)\n\nPrerequisite for #154 (coverlet.collector 6 -> 10).\n\n| | coverlet 6 (main) | coverlet 10 (#154) |\n|---|---|---|\n| Line | 56% | 56% |\n| Branch | 54.5% | 52.3% |\n\nSame code, different branch counting -> the 53% gate fails on a tool\nchange, not a test regression. Branch threshold follows the ratchet rule\n(~1.5 points below the measurement): **51**. Line threshold stays 55.\nMerge this before #154.\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-26T23:48:03+03:00",
          "tree_id": "01849d45e79152555b1773dba8027972c17cf51c",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/856c84f402316bddb8847ced7093935dc982e523"
        },
        "date": 1793048115021,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.504911472400029,
            "unit": "ns",
            "range": "± 0.08915870255001923"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 9.55888244509697,
            "unit": "ns",
            "range": "± 0.22200055389469098"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6358931672114592,
            "unit": "ns",
            "range": "± 0.007561618500626475"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.2748482728806826,
            "unit": "ns",
            "range": "± 0.003505072579967182"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 468.817018362192,
            "unit": "ns",
            "range": "± 1.7624241171585084"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 794.505000931876,
            "unit": "ns",
            "range": "± 5.114172564601969"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.072299381451947,
            "unit": "ns",
            "range": "± 0.08547430152490679"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.208847365776698,
            "unit": "ns",
            "range": "± 0.20613692190844524"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.081477663914363,
            "unit": "ns",
            "range": "± 0.13192116904031698"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.090983968514662,
            "unit": "ns",
            "range": "± 0.0639250805101028"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 12.560250137249628,
            "unit": "ns",
            "range": "± 0.0959009735403373"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 13.376375842094422,
            "unit": "ns",
            "range": "± 0.18634197486873846"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "49699333+dependabot[bot]@users.noreply.github.com",
            "name": "dependabot[bot]",
            "username": "dependabot[bot]"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "5eacf4a9022fa9e9931fcf945e39c1eafab0692a",
          "message": "chore(deps): Bump Microsoft.EntityFrameworkCore and 2 others (#168)\n\nPinned [Microsoft.EntityFrameworkCore](https://github.com/dotnet/dotnet)\nat 10.0.14.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [Microsoft.EntityFrameworkCore's\nreleases](https://github.com/dotnet/dotnet/releases)._\n\nNo release notes found for this version range.\n\nCommits viewable in [compare\nview](https://github.com/dotnet/dotnet/commits).\n</details>\n\nUpdated\n[Microsoft.EntityFrameworkCore.InMemory](https://github.com/dotnet/dotnet)\nfrom 10.0.13 to 10.0.14.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [Microsoft.EntityFrameworkCore.InMemory's\nreleases](https://github.com/dotnet/dotnet/releases)._\n\nNo release notes found for this version range.\n\nCommits viewable in [compare\nview](https://github.com/dotnet/dotnet/commits).\n</details>\n\nUpdated\n[Microsoft.EntityFrameworkCore.Sqlite](https://github.com/dotnet/dotnet)\nfrom 10.0.13 to 10.0.14.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [Microsoft.EntityFrameworkCore.Sqlite's\nreleases](https://github.com/dotnet/dotnet/releases)._\n\nNo release notes found for this version range.\n\nCommits viewable in [compare\nview](https://github.com/dotnet/dotnet/commits).\n</details>\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>",
          "timestamp": "2026-10-27T00:27:46+03:00",
          "tree_id": "e0297b6364af85144991da905cedb0093242e3c5",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/5eacf4a9022fa9e9931fcf945e39c1eafab0692a"
        },
        "date": 1793050465743,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.147086146085159,
            "unit": "ns",
            "range": "± 0.20714216347285935"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.013340070843697,
            "unit": "ns",
            "range": "± 0.21634022065496183"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.8399366121739149,
            "unit": "ns",
            "range": "± 0.0088329033845069"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.519995927500228,
            "unit": "ns",
            "range": "± 0.0048911040176093405"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 315.94625990731373,
            "unit": "ns",
            "range": "± 0.6055950769396745"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 577.337892595927,
            "unit": "ns",
            "range": "± 3.11767605330819"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 8.44349725716389,
            "unit": "ns",
            "range": "± 0.02701255654991451"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 8.248338179519543,
            "unit": "ns",
            "range": "± 0.0274735855412158"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 8.686712258628436,
            "unit": "ns",
            "range": "± 0.11750274446931852"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 8.757740025009427,
            "unit": "ns",
            "range": "± 0.06190105923869185"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 8.727454543113708,
            "unit": "ns",
            "range": "± 0.15050612896012522"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 8.66123183319966,
            "unit": "ns",
            "range": "± 0.11585219021540644"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "49699333+dependabot[bot]@users.noreply.github.com",
            "name": "dependabot[bot]",
            "username": "dependabot[bot]"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "aee4d84c0e9e4873a505496d72fcb535a0101015",
          "message": "chore(deps): Bump Microsoft.CodeAnalysis.PublicApiAnalyzers from 4.14.0 to 5.6.0 (#155)\n\nUpdated\n[Microsoft.CodeAnalysis.PublicApiAnalyzers](https://github.com/dotnet/roslyn)\nfrom 4.14.0 to 5.6.0.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [Microsoft.CodeAnalysis.PublicApiAnalyzers's\nreleases](https://github.com/dotnet/roslyn/releases)._\n\n## 5.0.4\n\n[Release](https://github.com/dotnet/core/releases/tag/v5.0.4)\n\n## 5.0.2\n\n[Release\nNotes](https://github.com/dotnet/core/blob/master/release-notes/5.0/5.0.2/5.0.2.md)\n[Install\nInstructions](https://github.com/dotnet/core/blob/master/release-notes/5.0/5.0.2/5.0.2-install-instructions.md)\n\n# Repos\n\n* [Core](https://github.com/dotnet/core/releases/tag/v5.0.2)\n\n## 5.0.1\n\n[Release\nNotes](https://github.com/dotnet/core/blob/master/release-notes/5.0/5.0.1/5.0.1.md)\n[Install\nInstructions](https://github.com/dotnet/core/blob/master/release-notes/5.0/5.0.1/5.0.1-install-instructions.md)\n\n# Repo\n\n* [Core](https://github.com/dotnet/core/releases/tag/v5.0.1)\n\nCommits viewable in [compare\nview](https://github.com/dotnet/roslyn/commits).\n</details>\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>",
          "timestamp": "2026-10-27T00:57:28+03:00",
          "tree_id": "a3f27d30635745df0c160b8e2aa5431cbeedd830",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/aee4d84c0e9e4873a505496d72fcb535a0101015"
        },
        "date": 1793052278927,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 7.43010327120622,
            "unit": "ns",
            "range": "± 0.09044989783787355"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 12.064739057822013,
            "unit": "ns",
            "range": "± 0.725722860898711"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.634211876644538,
            "unit": "ns",
            "range": "± 0.0059886650783418415"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5819037083004203,
            "unit": "ns",
            "range": "± 0.002734258418718073"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 456.7523194721767,
            "unit": "ns",
            "range": "± 3.0493923804841923"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 776.7524291992188,
            "unit": "ns",
            "range": "± 5.97500264539623"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.905535638332367,
            "unit": "ns",
            "range": "± 0.22168664302485647"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.910272725423177,
            "unit": "ns",
            "range": "± 0.20630916721301654"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.066045153141022,
            "unit": "ns",
            "range": "± 0.16696954712653364"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.448958125896752,
            "unit": "ns",
            "range": "± 0.24994824546790737"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.705955591797828,
            "unit": "ns",
            "range": "± 0.13584522565386337"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.611010035233837,
            "unit": "ns",
            "range": "± 0.12081423940405099"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "13c60c5804b829cf8ba5fc1b8df7f68cb5213ba6",
          "message": "ci(actions): bump codeql-action init + analyze to v4.39.1 together (#165)\n\n## Why\n\nDependabot opened the codeql-action bump as two PRs: #162 (`init`) and\n#163 (`analyze`). Merged separately, CodeQL\nfails — run 40262728588: *\"Loaded a configuration file for version\n'4.38.3', but running version '4.39.1'. All steps\nin a workflow that use `github/codeql-action` must use the same\nversion\"*.\n\n## What\n\n- `codeql.yml`: `init` and `analyze` → v4.39.1\n(`9b3254085494b55a0f7c4c2b3d2d52cc712ce370`, verified against the\n  `v4.39.1` tag via the GitHub API).\n- `dependabot.yml`: `codeql-action` group for the github-actions\necosystem so future bumps arrive in one PR.\n\nSupersedes #162 and #163 (close them after this merges).\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T01:05:45+03:00",
          "tree_id": "27ab4271cc444654d47ab004986a05ea71369e25",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/13c60c5804b829cf8ba5fc1b8df7f68cb5213ba6"
        },
        "date": 1793052787449,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 4.003278424342473,
            "unit": "ns",
            "range": "± 0.11252667669505274"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 5.168849542344871,
            "unit": "ns",
            "range": "± 0.15607779970419886"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.5098448425531388,
            "unit": "ns",
            "range": "± 0.030034033906625172"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.2658907480251331,
            "unit": "ns",
            "range": "± 0.06756286441451058"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 260.4365055220468,
            "unit": "ns",
            "range": "± 2.7753170637629556"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 486.8651698185847,
            "unit": "ns",
            "range": "± 5.077895020149904"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 6.909130792084493,
            "unit": "ns",
            "range": "± 0.1864202998791587"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 6.740250077098608,
            "unit": "ns",
            "range": "± 0.2764219182339659"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 6.597016218083876,
            "unit": "ns",
            "range": "± 0.24347063840190508"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 6.655114811224242,
            "unit": "ns",
            "range": "± 0.35013129065688886"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 6.207590944589453,
            "unit": "ns",
            "range": "± 0.3252303832335683"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 6.149017333257489,
            "unit": "ns",
            "range": "± 0.2888392159300867"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "49699333+dependabot[bot]@users.noreply.github.com",
            "name": "dependabot[bot]",
            "username": "dependabot[bot]"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "860bedc90833c3c3204b4ecd63042cc705dc08f4",
          "message": "chore(deps): Bump coverlet.collector and 3 others (#154)\n\nUpdated\n[coverlet.collector](https://github.com/coverlet-coverage/coverlet) from\n6.0.2 to 10.1.0.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [coverlet.collector's\nreleases](https://github.com/coverlet-coverage/coverlet/releases)._\n\n## 10.1.0\n\n### Improvements\n\n- Publish Microsoft.Testing.Platform coverage messages from coverlet.MTP\n[#​2019](https://github.com/coverlet-coverage/coverlet/pull/2019)\n- Implement dynamic exclusion filters for assemblies (Coverlet.MTP)\n[#​1946](https://github.com/coverlet-coverage/coverlet/pull/1946)\n- Replace legacy .sln files with modern .slnx format\n[#​1966](https://github.com/coverlet-coverage/coverlet/pull/1966)\n- coverlet.console: add trace diagnostics and actionable warnings for\ninstrumentation/hit/empty-result failures\n[#​2005](https://github.com/coverlet-coverage/coverlet/pull/2005)\n- Relax auto-property skip logic and improve coverage for records\n[#​1941](https://github.com/coverlet-coverage/coverlet/pull/1941)\n\n### Fixed\n\n- Fix coverlet.MTP does not collect coverage on the .NET Framework\nportion of a large project\n[#​1980](https://github.com/coverlet-coverage/coverlet/pull/1980)\n[#​1967](https://github.com/coverlet-coverage/coverlet/pull/1967)\n- Fix Regression in branch coverage for lambda expressions\n[#​1938](https://github.com/coverlet-coverage/coverlet/pull/1938)\n- Fix When using \"is\" with \"or\" in pattern matching, branch coverage is\nlower than normal\n[#​1979](https://github.com/coverlet-coverage/coverlet/pull/1979)\n- Fix silent zero coverage on .NET Framework since 8.0.0\n[#​1985](https://github.com/coverlet-coverage/coverlet/pull/1985) by\n@​tobiwae\n- Fix Race condition between ProcessExit hit-file write and out-of-proc\ncoverage read causes EndOfStreamException\n[#​1987](https://github.com/coverlet-coverage/coverlet/pull/1987)\n[#​1988](https://github.com/coverlet-coverage/coverlet/pull/1988) by\n@​bkoelman\n- Fix Regression TypeInitializationException when targeting .NET\nFramework - Could not load type\n'System.Collections.Concurrent.ConcurrentBag\n[#​2010](https://github.com/coverlet-coverage/coverlet/pull/2010)\n- Fix use --config-file CLI arg in coverlet.MTP\n[#​2030](https://github.com/coverlet-coverage/coverlet/pull/2030) by\nalexthornton1\n- Fix silently empty coverage for shared-framework assemblies missing\nfrom compileLibraries\n[#​2032](https://github.com/coverlet-coverage/coverlet/pull/2032) by\n@​Eljees\n\n[Diff between 10.0.1 and\n10.1.0](https://github.com/coverlet-coverage/coverlet/compare/v10.0.1...v10.1.0)\n\n## 10.0.1\n\n### Improvements\n\n- Coverlet with MTP 2 doesn't show test coverage statistic in console\n[#​1907](https://github.com/coverlet-coverage/coverlet/issues/1907)\n- Avoid unnecessary testhost restarts\n[#​1912](https://github.com/coverlet-coverage/coverlet/issues/1912) by\n<https://github.com/mawosoft>\n\n### Fixed\n\n- Fix inconsistent paths in cobertura reports\n[#​1723](https://github.com/coverlet-coverage/coverlet/issues/1723)\n- Fix when using \"is\" with \"and\" in pattern matching, branch coverage is\nlower than normal\n[#​1313](https://github.com/coverlet-coverage/coverlet/issues/1313)\n- Fix Coverlet flagging a branch for an async functions finally block\nwhere none exists\n[#​1337](https://github.com/coverlet-coverage/coverlet/issues/1337)\n- Fix Coverlet Tracker Missing CompilerGeneratedAttribute\n[#​1828](https://github.com/coverlet-coverage/coverlet/issues/1828)\n\n### Maintenance\n\n- Add architecture docs and diagrams for all integrations\n[#​1927](https://github.com/coverlet-coverage/coverlet/pull/1927)\n- Update NuGet packages and .NET SDK versions\n[#​1933](https://github.com/coverlet-coverage/coverlet/pull/1933)\n\n[Diff between 10.0.0 and\n10.0.1](https://github.com/coverlet-coverage/coverlet/compare/v10.0.0...v10.0.1)\n\n## 10.0.0\n\n## Improvements\n\n- Unique Report Filenames (coverlet.MTP and AzDO)\n[#​1866](https://github.com/coverlet-coverage/coverlet/issues/1866)\n- Add `--coverlet-file-prefix` option for unique report files\n[#​1869](https://github.com/coverlet-coverage/coverlet/pull/1869)\n- Introduce .NET 10 support\n[#​1823](https://github.com/coverlet-coverage/coverlet/pull/1823)\n\n## Fixed\n\n- Fix [BUG] Wrong branch rate on IAsyncEnumerable for generic type\n[#​1836](https://github.com/coverlet-coverage/coverlet/issues/1836)\n- Fix [BUG] Missing Coverage after moving to MTP\n[#​1843](https://github.com/coverlet-coverage/coverlet/issues/1843)\n- Fix [BUG] No coverage reported when targeting .NET Framework with\n8.0.1\n[#​1842](https://github.com/coverlet-coverage/coverlet/issues/1842)\n- Fix [BUG] Behavior changes between MTP and Legacy (msbuild)\n[#​1878](https://github.com/coverlet-coverage/coverlet/issues/1878)\n- Fix [BUG] Coverlet.MTP - Unable to load coverlet.mtp.appsettings.json\n[#​1880](https://github.com/coverlet-coverage/coverlet/issues/1880)\n- Fix [BUG] Coverlet.Collector produces empty report when\nMediator.SourceGenerator is referenced\n[#​1718](https://github.com/coverlet-coverage/coverlet/issues/1718) by\n<https://github.com/yusyd>\n- Fix [BUG] Crash during instrumentation (Methods using\nLibraryImport/DllImport have no body)\n[#​1762](https://github.com/coverlet-coverage/coverlet/issues/1762)\n\n## Maintenance\n\n- Add comprehensive async method tests and documentation for issue\n[#​1864](https://github.com/coverlet-coverage/coverlet/pull/1864)\n- Replace Tmds.ExecFunction Package in coverlet.core.coverage.tests\n[#​1833](https://github.com/coverlet-coverage/coverlet/issues/1833)\n- Add net9.0 and net10.0 targets\n[#​1822](https://github.com/coverlet-coverage/coverlet/issues/1822)\n\n[Diff between 8.0.1 and\n10.0.0](https://github.com/coverlet-coverage/coverlet/compare/v8.0.1...v10.0.0)\n\n## 8.0.1\n\n### Fixed\n- Fix [BUG] TypeInitializationException when targeting .NET Framework\n[#​1818](https://github.com/coverlet-coverage/coverlet/issues/1818)\n- Fix [BUG] coverlet.MTP build fails with CS0400 due to\ndevelopmentDependency=true\n[#​1827](https://github.com/coverlet-coverage/coverlet/issues/1827)\n\n### Improvements\n- Additional improvements needed for .NET Framework instrumentation type\nimport\n[#​1825](https://github.com/coverlet-coverage/coverlet/issues/1825)\n\n[Diff between 8.0.0 and\n8.0.1](https://github.com/coverlet-coverage/coverlet/compare/v8.0.0...v8.0.1)\n\n\n## 8.0.0\n\n**Special Thanks:** A huge thank you to\n[@​Bertk](https://github.com/Bertk) for driving the majority of the work\nin this release! 🎉\n\n### Fixed\n- Fix System.CommandLine 2.0 release is available\n[#​1776](https://github.com/coverlet-coverage/coverlet/issues/1776)\n- Fix Excluding From Coverage bad defaults from given example\n[#​1764](https://github.com/coverlet-coverage/coverlet/issues/1764)\n- Fix branchpoint exclusion for sdk 8.0.407\n[#​1741](https://github.com/coverlet-coverage/coverlet/issues/1741)\n- Fix missing copyright information in NuGet\n[#​1794](https://github.com/coverlet-coverage/coverlet/issues/1794)\n- Fix bad default values in documentation\n[#​1764](https://github.com/coverlet-coverage/coverlet/issues/1764) by\n<https://github.com/cboudereau>\n\n### Improvements\n\n- Coverlet MTP extension feature\n[#​1788](https://github.com/coverlet-coverage/coverlet/pull/1788)\n- Generate SBOM for nuget packages\n[#​1752](https://github.com/coverlet-coverage/coverlet/pull/1752)\n- Use multi targets projects for coverlet.collector,\ncoverlet.msbuild.tasks packages\n[#​1742](https://github.com/coverlet-coverage/coverlet/pull/1742)\n- Use .NET 8.0 target framework for coverlet.core and remove\nNewtonsoft.Json\n[#​1733](https://github.com/coverlet-coverage/coverlet/pull/1733)\n- Use latest System.CommandLine version\n[#​1660](https://github.com/coverlet-coverage/coverlet/pull/1660)\n- Upgraded minimum required .NET SDK and runtime to .NET 8.0 LTS (Long\nTerm Support) (**Breaking Change**)\n- Use [xunit.v3](https://xunit.net/docs/getting-started/v3/whats-new)\nfor tests and example code\n\n[Diff between 6.0.4 and\n8.0.0](https://github.com/coverlet-coverage/coverlet/compare/v6.0.4...v8.0.0)\n\n## 6.0.4\n\n### Fixed\n- Fix empty coverage report when using include and exclude filters\n[#​1726](https://github.com/coverlet-coverage/coverlet/issues/1726)\n\n[Diff between 6.0.3 and\n6.0.4](https://github.com/coverlet-coverage/coverlet/compare/v6.0.3...v6.0.4)\n\n## 6.0.3\n\n### Fixed\n- Fix RuntimeConfigurationReader to support self-contained builds\n[#​1705](https://github.com/coverlet-coverage/coverlet/pull/1705) by\nhttps://github.com/pfeigl\n- Fix inconsistent filenames with UseSourceLink after .NET 8\n[#​1679](https://github.com/coverlet-coverage/coverlet/issues/1679)\n- Fix hanging tests\n[#​989](https://github.com/coverlet-coverage/coverlet/issues/989)\n- Fix coverlet instrumentation becomes slow after installing dotnet sdk\n8.0.200\n[#​1620](https://github.com/coverlet-coverage/coverlet/issues/1620)\n- Fix upgrading v6.0.1 to v6.0.2 increases instrumentation time\n[#​1649](https://github.com/coverlet-coverage/coverlet/issues/1649)\n- Fix Unable to instrument module - NET 8\n[#​1631](https://github.com/coverlet-coverage/coverlet/issues/1631)\n- Fix slow modules filtering process\n[#​1646](https://github.com/coverlet-coverage/coverlet/issues/1646) by\nhttps://github.com/BlackGad\n- Fix incorrect coverage await using in generic method\n[#​1490](https://github.com/coverlet-coverage/coverlet/issues/1490)\n\n### Improvements\n- Cache the regex used in InstrumentationHelper\n[#​1693](https://github.com/coverlet-coverage/coverlet/issues/1693)\n- Enable dotnetTool integration tests for linux\n[#​660](https://github.com/coverlet-coverage/coverlet/issues/660)\n\n[Diff between 6.0.2 and\n6.0.3](https://github.com/coverlet-coverage/coverlet/compare/v6.0.2...v6.0.3)\n\nCommits viewable in [compare\nview](https://github.com/coverlet-coverage/coverlet/compare/v6.0.2...v10.1.0).\n</details>\n\nUpdated [Microsoft.NET.Test.Sdk](https://github.com/microsoft/vstest)\nfrom 18.6.0 to 18.11.0.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [Microsoft.NET.Test.Sdk's\nreleases](https://github.com/microsoft/vstest/releases)._\n\n## 18.11.0\n\n<!-- Release notes generated using configuration in .github/release.yml\nat rel/18.11 -->\n\n## What's Changed\n* Fix TRX logger mangling astral-plane (non-BMP) characters by\n@​azat-msft in https://github.com/microsoft/vstest/pull/16318\n* Disable the MTP testhost by default by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16337\n* Fix OS-blind apphost probe in MTP bridge on Unix (regression from\n#​16201) by @​azat-msft in\nhttps://github.com/microsoft/vstest/pull/16336\n* Retarget the MTP client onto\nMicrosoft.Testing.Platform.ServerMode.Client.Sources by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16300\n* Pass arguments to PowerShell with -File instead of -command by\n@​nohwnd in https://github.com/microsoft/vstest/pull/16363\n* Respect console logger verbosity from a settings file by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16044\n* Fix backslash normalization in VSTestCLIRunSettings on Unix by\n@​nohwnd in https://github.com/microsoft/vstest/pull/15795\n* Docs: fix typo explicitely -> explicitly by @​vaibhav8a in\nhttps://github.com/microsoft/vstest/pull/16397\n* Copy acceptance test diagnostic logs into the test results directory\nby @​nohwnd in https://github.com/microsoft/vstest/pull/16403\n\n## New Contributors\n* @​vaibhav8a made their first contribution in\nhttps://github.com/microsoft/vstest/pull/16397\n\n**Full Changelog**:\nhttps://github.com/microsoft/vstest/compare/v18.10.1...v18.11.0\n\n## 18.10.1\n\n\n## What's Changed\n* Isolate code coverage files in published output by @​azat-msft in\nhttps://github.com/microsoft/vstest/pull/16476\n\n\n**Full Changelog**:\nhttps://github.com/microsoft/vstest/compare/v18.10.0...v18.10.1\n\n## 18.10.0\n\n\n## What's Changed\n* Drop Mono fallback, run .NET Framework tests on Windows only by\n@​nohwnd in https://github.com/microsoft/vstest/pull/16158\n* Run Microsoft.Testing.Platform test apps under `vstest.console` and\ndatacollector by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16201\n* Fix test output eaten by MSBuild terminal logger by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16223\n* Remove the experimental test session feature by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16231\n* Skip a single bad executor instead of failing all executor loading by\n@​nohwnd in https://github.com/microsoft/vstest/pull/16239\n* Assert apartment state instead of using Clipboard in UI tests by\n@​nohwnd in https://github.com/microsoft/vstest/pull/16270\n* Surface test host crashes during protocol negotiation by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16285\n* Pass the inferred target platform to the host in run settings by\n@​nohwnd in https://github.com/microsoft/vstest/pull/16271\n* Report raw invalid IsTargetPlatformInferred value, cover host x64\nforcing by @​nohwnd in https://github.com/microsoft/vstest/pull/16295\n* Disable the MTP testhost by default (#​16337) by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16341\n\n\n**Full Changelog**:\nhttps://github.com/microsoft/vstest/compare/v18.9.0...v18.10.0\n\n## 18.9.0\n\n## What's Changed\n* Fix tilde/exclamation characters corrupted in TerminalLogger test\noutput by @​nohwnd in https://github.com/microsoft/vstest/pull/16046\n* Make TranslationLayer Native AOT-compatible by @​drewnoakes in\nhttps://github.com/microsoft/vstest/pull/16045\n* Guard GenerateProgramFile target against UseWinUI/UseUwpTools\nevaluation order by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16072\n* Add RequestingAssembly to AssemblyResolveEventArgs for binary compat\nby @​nohwnd in https://github.com/microsoft/vstest/pull/16076\n* Remove stale Microsoft.Extensions.FileSystemGlobbing binding redirect\nfrom testhost.x86 and datacollector by @​Evangelink in\nhttps://github.com/microsoft/vstest/pull/16082\n* Fix TRX attachment paths when LogFileName contains a subdirectory by\n@​nohwnd in https://github.com/microsoft/vstest/pull/15791\n* Fix missing dumps for .NET Framework child processes in\nNetClientHangDumper by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16098\n* Fix data collection channels to use negotiated protocol version\ninstead of V1 by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16096\n* Fix race condition in BlameCollector: skip hang dump when testhost\nhasn't launched yet by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16065\n* Replace TestSDKAutoGeneratedCode with ExcludeFromCodeCoverage in\nauto-generated Program files by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16101\n* Include testhost process path in crash error messages by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16108\n* Fix DataDriven test results being double-counted in TRX logger totals\nby @​nohwnd in https://github.com/microsoft/vstest/pull/15766\n* Fix datacollector crash visibility: replace Assert with throwable\nexceptions by @​nohwnd in https://github.com/microsoft/vstest/pull/16048\n* Add TreatErrorMessagesAsWarnings parameter to TRX logger by @​nohwnd\nin https://github.com/microsoft/vstest/pull/16106\n* Wait for testhost stderr to drain before reading its crash output by\n@​nohwnd in https://github.com/microsoft/vstest/pull/16128\n* Handle runtimeconfig.dev.json without additionalProbingPaths by @​tmat\nin https://github.com/microsoft/vstest/pull/16166\n* Suggest Microsoft.NET.Test.Sdk when a managed test project brings no\ntesthost by @​nohwnd in https://github.com/microsoft/vstest/pull/16169\n* Fix x86 testhost loading mismatched x64 hostfxr (0x800700C1) when run\nvia vstest.console.exe directly (#​16151) by @​azat-msft in\nhttps://github.com/microsoft/vstest/pull/16156\n* Preserve the real exception (type + stack trace) when a test run\naborts in BaseRunTests by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16167\n\n## New Contributors\n* @​drewnoakes made their first contribution in\nhttps://github.com/microsoft/vstest/pull/16045\n\n**Full Changelog**:\nhttps://github.com/microsoft/vstest/compare/v18.8.0...v18.9.0\n\n## 18.8.1\n\n## What's Changed\n* Fix protocol negotiation timeout when STJ reflection is disabled\n(18.8.1) by @​nohwnd in https://github.com/microsoft/vstest/pull/16281\n\n\n**Full Changelog**:\nhttps://github.com/microsoft/vstest/compare/v18.8.0...v18.8.1\n\n## 18.8.0\n\n## What's Changed\n* Migrate from Newtonsoft.Json to System.Text.Json / Jsonite (merge to\nmain) by @​nohwnd in https://github.com/microsoft/vstest/pull/15687\n- For more detail refer to\nhttps://devblogs.microsoft.com/dotnet/vs-test-is-removing-its-newtonsoft-json-dependency/\n* Create source-only filter package by @​Youssef1313 in\nhttps://github.com/microsoft/vstest/pull/15638\n* Add ARM64 msdia140.dll support to test platform packages by @​nohwnd\nin https://github.com/microsoft/vstest/pull/15692\n* Fix mutex cleanup crash on macOS/Linux by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/15684\n* Restrict artifact temp directory permissions on Unix by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/15729\n* Add support for filtering uncategorized tests with TestCategory=None\nby @​Evangelink in https://github.com/microsoft/vstest/pull/15727\n* Fix SCI binding failure in DTA hosts (main) by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/15724\n* Fix HTML logger parallel file collision by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/15435\n* Improve error message when testhost cannot be found by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/16053\n* Fix HTML logger exception on invalid XML chars in test display names\nby @​nohwnd in https://github.com/microsoft/vstest/pull/16051\n\n**Full Changelog**:\nhttps://github.com/microsoft/vstest/compare/v18.7.0...v18.8.0\n\n## 18.7.0\n\n## What's Changed\n* Add ARM64 msdia140.dll support to test platform packages by\n@​jamesmcroft in https://github.com/microsoft/vstest/pull/15689\n* Update System.Memory from 4.5.5 to 4.6.3 by @​nohwnd in\nhttps://github.com/microsoft/vstest/pull/15706\n\n## New Contributors\n* @​jamesmcroft made their first contribution in\nhttps://github.com/microsoft/vstest/pull/15689\n\n**Full Changelog**:\nhttps://github.com/microsoft/vstest/compare/v18.6.0...v18.7.0\n\nCommits viewable in [compare\nview](https://github.com/microsoft/vstest/compare/v18.6.0...v18.11.0).\n</details>\n\nUpdated [NSubstitute](https://github.com/nsubstitute/NSubstitute) from\n5.3.0 to 6.2.0.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [NSubstitute's\nreleases](https://github.com/nsubstitute/NSubstitute/releases)._\n\n## 6.2.0\n\n# NSubstitute v6.2.0\n\nThis release improves generic call matching (#​989, #​974, #​990).\nThanks to @​zvirja and @​JMolenkamp for fixes this.\nWe've also switched to using trusted nuget publishing linked to GitHub\nReleases. (#​987, @​zvirja)\n\n## What's Changed\n\n* Add trusted nuget publish by @​zvirja in\nhttps://github.com/nsubstitute/NSubstitute/pull/987\n* fix: Stop using return-type assignability for generic call matching by\n@​zvirja in https://github.com/nsubstitute/NSubstitute/pull/989\n* Remove skip-duplicate option from NuGet push command by @​zvirja in\nhttps://github.com/nsubstitute/NSubstitute/pull/988\n* Add tag trigger for release pipeline by @​zvirja in\nhttps://github.com/nsubstitute/NSubstitute/pull/991\n* Fix broken comparison of the generic methods by @​zvirja in\nhttps://github.com/nsubstitute/NSubstitute/pull/990\n* Support C# 13 params collections in argument matching by @​zvirja in\nhttps://github.com/nsubstitute/NSubstitute/pull/993\n* Add tests for nullable Task by @​zvirja in\nhttps://github.com/nsubstitute/NSubstitute/pull/994\n\n**Full Changelog**:\nhttps://github.com/nsubstitute/NSubstitute/compare/v6.1.0...v6.2.0\n\n## 6.1.0\n\n# NSubstitute v6.1.0\n\nThis release reverts nullability support introduced in 6.0. This change\ncaused a number of problems for callers (#​973, #​976), and trying to\nresolve these (#​976) revealed the need for more fundamental changes to\nNSubstitute to effectively support this. As a result, nullability is\nagain disabled for public API. Huge thanks to @​jdb0123, @​Romfos, and\n@​Moha-sami for their PRs addressing this, and thanks for all those who\nraised issues. We also humbly award @​zvirja with one unlimited \"I told\nyou so\" pass to use as he sees fit.\n\n## What's Changed\n\n* Add new argument matcher for comparing objects by reference by\n@​Wer-Wolf in https://github.com/nsubstitute/NSubstitute/pull/963\n* Revert nullability changes in\nhttps://github.com/nsubstitute/NSubstitute/pull/985\n\n## New Contributors\n* @​Wer-Wolf made their first contribution in\nhttps://github.com/nsubstitute/NSubstitute/pull/963\n\n**Full Changelog**:\nhttps://github.com/nsubstitute/NSubstitute/compare/v6.0.0...v6.1.0\n\n## 6.0.0\n\n:information_source: No changes from [Release Candidate\n1](https://github.com/nsubstitute/NSubstitute/releases/tag/v6.0.0-rc.1).\n\n# NSubstitute v6.0.0 \n\nFrom\n[RC1](https://github.com/nsubstitute/NSubstitute/releases/tag/v6.0.0-rc.1)\nnotes:\n\n* [NEW] `ArgMatchers.Matching` predicate matcher as an alternative to\n`Is(Expression<Predicate<T>>`. (.NET6 and above.)\n* [UPDATE] Improved support for custom argument matchers. `Arg.Is` now\naccepts arg matchers.\n* [UPDATE][BREAKING] Update target frameworks: .NET8, .NET Standard 2.0\n* [UPDATE][BREAKING] Remove legacy obsolete API\n* [UPDATE][BREAKING] Mark as obsolete api CompatArg with pre c# 7.0\nsupport\n* [UPDATE][BREAKING] Nullability is enabled for public api for .NET 8+\nTFMs\n* [UPDATE] Migrate documentation to [docfx\nplatform](https://github.com/dotnet/docfx) and update samples to NUnit 4\n* [NEW] Added NuGet Package README file.\n\n\n## Full change list\n\n* Update target frameworks and other infrastructure changes by @​Romfos\nin https://github.com/nsubstitute/NSubstitute/pull/831\n* Remove Google Groups hyperlinks by @​304NotModified in\nhttps://github.com/nsubstitute/NSubstitute/pull/804\n* Improve output for expected argument matchers in\nhttps://github.com/nsubstitute/NSubstitute/pull/806\n* Mark Substitute.For<T> method as Pure by @​Dzliera in\nhttps://github.com/nsubstitute/NSubstitute/pull/844\n* Move package creating from build.fsproj to github actions by @​Romfos\nin https://github.com/nsubstitute/NSubstitute/pull/838\n* Added .NET 9 to test matrix by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/848\n* Update dependencies by @​Saibamen in\nhttps://github.com/nsubstitute/NSubstitute/pull/843\n* Migrate documentation to docfx by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/850\n* Added test for issue #​716 by @​rbeurskens in\nhttps://github.com/nsubstitute/NSubstitute/pull/846\n* feat: add dependabot for this project for minor and patch updates for\nnuget packages and github actions by @​wmundev in\nhttps://github.com/nsubstitute/NSubstitute/pull/792\n* #​853 Fix matching with multiple generic arguments of the same type by\n@​rholek in https://github.com/nsubstitute/NSubstitute/pull/858\n* Ability to mock protected methods with and without return value by\n@​Jason31569 in https://github.com/nsubstitute/NSubstitute/pull/845\n* Enable nullability for public api by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/856\n* Bugfix/async event handlers return instantly by @​jmartschinke in\nhttps://github.com/nsubstitute/NSubstitute/pull/808\n* Fix matching generic calls with AnyType when the generic argument is\nalso a generic argument in return type, out or ref parameter by\n@​JMolenkamp in https://github.com/nsubstitute/NSubstitute/pull/862\n* Feature: allow interception of any generic method call when using\nArg.AnyType by @​JMolenkamp in\nhttps://github.com/nsubstitute/NSubstitute/pull/855\n* Params arg unit test by @​Jason31569 in\nhttps://github.com/nsubstitute/NSubstitute/pull/874\n* Added exception extensions for ValueTask without return type (rebase)\nin https://github.com/nsubstitute/NSubstitute/pull/873\n* Migrate to slnx by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/882\n* Migrate documentation validation from build.fsproj to Roslyn code\ngenerator by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/883\n* Fix doc links (#​884) in\nhttps://github.com/nsubstitute/NSubstitute/pull/886\n* Add PackageReadmeFile. by @​peymanr34 in\nhttps://github.com/nsubstitute/NSubstitute/pull/888\n* Make public api and tests the same for all TFMs by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/885\n* Migrate documentation samples to NUnit4 by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/889\n* Run unit tests in Microsoft.Testing.Platform mode by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/896\n* Bump actions/checkout from 4 to 5 by @​dependabot[bot] in\nhttps://github.com/nsubstitute/NSubstitute/pull/902\n* Fix typo in return value documentation by @​ericmutta in\nhttps://github.com/nsubstitute/NSubstitute/pull/903\n* Bump actions/setup-dotnet from 4 to 5 by @​dependabot[bot] in\nhttps://github.com/nsubstitute/NSubstitute/pull/907\n* Fix typo in received calls documentation by @​ericmutta in\nhttps://github.com/nsubstitute/NSubstitute/pull/904\n* Simplify github actions to use less jobs by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/911\n* Bump actions/upload-artifact from 4 to 5 by @​dependabot[bot] in\nhttps://github.com/nsubstitute/NSubstitute/pull/916\n* Bump NUnit3TestAdapter from 5.0.0 to 5.2.0 by @​dependabot[bot] in\nhttps://github.com/nsubstitute/NSubstitute/pull/922\n ... (truncated)\n\n## 6.0.0-rc.1\n\n# NSubstitute v6.0.0 Release Candidate 1\n\nDue to the large number of changes in this release, we wanted to start\nwith a release candidate to ensure we've correctly captured breaking\nchanges.\n\n* [NEW] `ArgMatchers.Matching` predicate matcher as an alternative to\n`Is(Expression<Predicate<T>>`. (.NET6 and above.)\n* [UPDATE] Improved support for custom argument matchers. `Arg.Is` now\naccepts arg matchers.\n* [UPDATE][BREAKING] Update target frameworks: .NET8, .NET Standard 2.0\n* [UPDATE][BREAKING] Remove legacy obsolete API\n* [UPDATE][BREAKING] Mark as obsolete api CompatArg with pre c# 7.0\nsupport\n* [UPDATE][BREAKING] Nullability is enabled for public api for .NET 8+\nTFMs\n* [UPDATE] Migrate documentation to [docfx\nplatform](https://github.com/dotnet/docfx) and update samples to NUnit 4\n* [NEW] Added NuGet Package README file.\n\n\n## Full change list\n\n* Update target frameworks and other infrastructure changes by @​Romfos\nin https://github.com/nsubstitute/NSubstitute/pull/831\n* Remove Google Groups hyperlinks by @​304NotModified in\nhttps://github.com/nsubstitute/NSubstitute/pull/804\n* Improve output for expected argument matchers in\nhttps://github.com/nsubstitute/NSubstitute/pull/806\n* Mark Substitute.For<T> method as Pure by @​Dzliera in\nhttps://github.com/nsubstitute/NSubstitute/pull/844\n* Move package creating from build.fsproj to github actions by @​Romfos\nin https://github.com/nsubstitute/NSubstitute/pull/838\n* Added .NET 9 to test matrix by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/848\n* Update dependencies by @​Saibamen in\nhttps://github.com/nsubstitute/NSubstitute/pull/843\n* Migrate documentation to docfx by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/850\n* Added test for issue #​716 by @​rbeurskens in\nhttps://github.com/nsubstitute/NSubstitute/pull/846\n* feat: add dependabot for this project for minor and patch updates for\nnuget packages and github actions by @​wmundev in\nhttps://github.com/nsubstitute/NSubstitute/pull/792\n* #​853 Fix matching with multiple generic arguments of the same type by\n@​rholek in https://github.com/nsubstitute/NSubstitute/pull/858\n* Ability to mock protected methods with and without return value by\n@​Jason31569 in https://github.com/nsubstitute/NSubstitute/pull/845\n* Enable nullability for public api by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/856\n* Bugfix/async event handlers return instantly by @​jmartschinke in\nhttps://github.com/nsubstitute/NSubstitute/pull/808\n* Fix matching generic calls with AnyType when the generic argument is\nalso a generic argument in return type, out or ref parameter by\n@​JMolenkamp in https://github.com/nsubstitute/NSubstitute/pull/862\n* Feature: allow interception of any generic method call when using\nArg.AnyType by @​JMolenkamp in\nhttps://github.com/nsubstitute/NSubstitute/pull/855\n* Params arg unit test by @​Jason31569 in\nhttps://github.com/nsubstitute/NSubstitute/pull/874\n* Added exception extensions for ValueTask without return type (rebase)\nin https://github.com/nsubstitute/NSubstitute/pull/873\n* Migrate to slnx by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/882\n* Migrate documentation validation from build.fsproj to Roslyn code\ngenerator by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/883\n* Fix doc links (#​884) in\nhttps://github.com/nsubstitute/NSubstitute/pull/886\n* Add PackageReadmeFile. by @​peymanr34 in\nhttps://github.com/nsubstitute/NSubstitute/pull/888\n* Make public api and tests the same for all TFMs by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/885\n* Migrate documentation samples to NUnit4 by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/889\n* Run unit tests in Microsoft.Testing.Platform mode by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/896\n* Bump actions/checkout from 4 to 5 by @​dependabot[bot] in\nhttps://github.com/nsubstitute/NSubstitute/pull/902\n* Fix typo in return value documentation by @​ericmutta in\nhttps://github.com/nsubstitute/NSubstitute/pull/903\n* Bump actions/setup-dotnet from 4 to 5 by @​dependabot[bot] in\nhttps://github.com/nsubstitute/NSubstitute/pull/907\n* Fix typo in received calls documentation by @​ericmutta in\nhttps://github.com/nsubstitute/NSubstitute/pull/904\n* Simplify github actions to use less jobs by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/911\n* Bump actions/upload-artifact from 4 to 5 by @​dependabot[bot] in\nhttps://github.com/nsubstitute/NSubstitute/pull/916\n* Bump NUnit3TestAdapter from 5.0.0 to 5.2.0 by @​dependabot[bot] in\nhttps://github.com/nsubstitute/NSubstitute/pull/922\n* Bump BenchmarkDotNet from 0.15.2 to 0.15.5 by @​dependabot[bot] in\nhttps://github.com/nsubstitute/NSubstitute/pull/921\n* Add .NET 10 to test matrix by @​Romfos in\nhttps://github.com/nsubstitute/NSubstitute/pull/913\n ... (truncated)\n\nCommits viewable in [compare\nview](https://github.com/nsubstitute/NSubstitute/compare/v5.3.0...v6.2.0).\n</details>\n\nUpdated\n[xunit.runner.visualstudio](https://github.com/xunit/visualstudio.xunit)\nfrom 2.8.2 to 4.0.1.\n\n<details>\n<summary>Release notes</summary>\n\n_Sourced from [xunit.runner.visualstudio's\nreleases](https://github.com/xunit/visualstudio.xunit/releases)._\n\n## 4.0.1\n\nRelease notes: https://xunit.net/releases/visualstudio/4.0.1\n\n## 4.0.0\n\nRelease notes: https://xunit.net/releases/visualstudio/4.0.0\n\n## 4.0.0-pre.5\n\nRelease notes: https://xunit.net/releases/visualstudio/4.0.0-pre.5\n\n## 4.0.0-pre.4\n\nRelease notes: https://xunit.net/releases/visualstudio/4.0.0-pre.4\n\n## 4.0.0-pre.3\n\nRelease notes: https://xunit.net/releases/visualstudio/4.0.0-pre.3\n\n## 3.1.5\n\nRelease notes: https://xunit.net/releases/visualstudio/3.1.5\n\nCommits viewable in [compare\nview](https://github.com/xunit/visualstudio.xunit/commits/4.0.1).\n</details>\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>",
          "timestamp": "2026-10-27T16:43:32+03:00",
          "tree_id": "e2d2b8ee8b00102c13aa6778614fd14154a430cd",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/860bedc90833c3c3204b4ecd63042cc705dc08f4"
        },
        "date": 1793109032259,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.023958463221788,
            "unit": "ns",
            "range": "± 0.19526495938965274"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 6.94823699593544,
            "unit": "ns",
            "range": "± 0.1494998144069134"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.374054958919684,
            "unit": "ns",
            "range": "± 0.029173521794838616"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.761786666053992,
            "unit": "ns",
            "range": "± 0.010054543775079767"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 350.02984663645424,
            "unit": "ns",
            "range": "± 1.42622660617833"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 621.1770654405866,
            "unit": "ns",
            "range": "± 9.867331463710412"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.63840849591153,
            "unit": "ns",
            "range": "± 0.202600798187195"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 8.544793080290159,
            "unit": "ns",
            "range": "± 0.20854972372446248"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 8.706047854785409,
            "unit": "ns",
            "range": "± 0.35718764212937415"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.453711853495665,
            "unit": "ns",
            "range": "± 0.20239079240330857"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.594052989449766,
            "unit": "ns",
            "range": "± 0.2788814932029128"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 8.188802146485873,
            "unit": "ns",
            "range": "± 0.14192249310884322"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "9bfe9ed5d4056b25c3b9c51a8813a7aed9eb0e22",
          "message": "docs: track roadmap, plans and known issues in git (#174)\n\n## Summary\nDecision D6 of the 2026-10-26 roadmap: keep the project's working\ndocuments in git so they are not lost.\n\n- `docs/plans/2026-10-26-dis-inceleme-yol-haritasi.md` — roadmap F0–F3;\ndecisions D0–D6 recorded as approved (2026-10-27)\n- `docs/plans/2026-10-14-omerkck-v2-fixes.md` and\n`docs/archive/2026-10-15-omerkck-v2-fixes.md` — completed v2 fixes plan\nand checkpoint\n- `docs/memory/known-issues.md` — K1 Six Labors licence renewal\n(2027-10-14), K2 MediatR licence exception expiry (2027-01-31)\n- `docfx.json` — excludes `docs/plans`, `docs/archive`, `docs/memory` so\ninternal (Turkish) working notes are not published on the documentation\nsite\n\nNo code or package changes. Files were scanned for secrets before commit\n(none).\n\n## Test plan\n- [x] `docfx.json` parses as valid JSON\n- [ ] CI green (docs workflow builds the site without the excluded\nfolders)\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T16:59:03+03:00",
          "tree_id": "811d048af8300964a7d430899a3da22ee674622d",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/9bfe9ed5d4056b25c3b9c51a8813a7aed9eb0e22"
        },
        "date": 1793109901632,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 7.371169749895731,
            "unit": "ns",
            "range": "± 0.09004634376628497"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 10.83068301660173,
            "unit": "ns",
            "range": "± 0.2815360126610268"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6405572603855814,
            "unit": "ns",
            "range": "± 0.004183045068058839"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5913106615344683,
            "unit": "ns",
            "range": "± 0.006975475554379602"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 493.6807131767273,
            "unit": "ns",
            "range": "± 1.5809209061037346"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 831.9061549050467,
            "unit": "ns",
            "range": "± 1.5467195037020713"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 14.649244207143784,
            "unit": "ns",
            "range": "± 0.21680737226889502"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 11.828547393282255,
            "unit": "ns",
            "range": "± 0.2232550582781262"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 12.2428808649381,
            "unit": "ns",
            "range": "± 0.1987916907442213"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 12.28936870296796,
            "unit": "ns",
            "range": "± 0.25158669147883156"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.972624754533172,
            "unit": "ns",
            "range": "± 0.2938194107718805"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 12.477676658829052,
            "unit": "ns",
            "range": "± 0.2583741763269381"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "81780fce51c351d98fdcf72d5f6248d20d02de06",
          "message": "ci(release): keep newest version of previous major listed (#175)\n\n## Summary\nRoadmap H8 (must land before v4.0.0): the \"Unlist older versions\" step\nunlisted every lower stable version after a release. Publishing 4.0.0\nwould have hidden the newest 3.x from consumers who cannot upgrade yet.\n\nNow only lower **stable versions of the same major** are unlisted:\n\n| Publish | Listed before | Unlisted |\n|---|---|---|\n| 3.6.0 | 3.5.0 | 3.5.0 |\n| 4.0.0 | 3.6.0 | – |\n| 4.0.1 | 4.0.0, 3.6.0 | 4.0.0 |\n| 3.6.1 | 4.0.1, 3.6.0 | 3.6.0 |\n| 4.1.0-rc.1 | 4.0.1, 3.6.1 | – |\n\n- Selection moved into a pure inline function `to_unlist` above a `# ---\nmain ---` marker; the privileged publish job still runs no repository\ncode.\n- `.github/scripts/test_unlist_selection.py` (8 tests) executes that\nfunction straight from the workflow file; License Audit already runs all\n`.github/scripts/test_*.py`.\n\n## Test plan\n- [x] RED before the change (function missing), GREEN after: 31/31\nscript tests\n- [x] Mutation probe: removing the same-major condition fails 3 tests\n- [x] Workflow YAML parses; the full inline script compiles\n- [ ] CI: License Audit runs the new tests; actionlint/zizmor not run\nlocally\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T17:11:11+03:00",
          "tree_id": "08d73b73b11c758e7364e63c072845d1a08ec767",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/81780fce51c351d98fdcf72d5f6248d20d02de06"
        },
        "date": 1793110668560,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.682926452503755,
            "unit": "ns",
            "range": "± 0.29370847745271117"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.760923997163773,
            "unit": "ns",
            "range": "± 0.31533889239333357"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.3827716251835227,
            "unit": "ns",
            "range": "± 0.10626461765146995"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.35260788289209205,
            "unit": "ns",
            "range": "± 0.03754297896870975"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 379.2026532377516,
            "unit": "ns",
            "range": "± 2.715804553403429"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 674.8902694066365,
            "unit": "ns",
            "range": "± 5.36531149451071"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.965151729005756,
            "unit": "ns",
            "range": "± 0.47184648836004417"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 11.82482129619235,
            "unit": "ns",
            "range": "± 0.3715671133657088"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.557825482704423,
            "unit": "ns",
            "range": "± 0.33075244850182095"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 11.906405889987946,
            "unit": "ns",
            "range": "± 0.21794536673702147"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.95262475418193,
            "unit": "ns",
            "range": "± 0.45496298943341934"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 11.15768210310489,
            "unit": "ns",
            "range": "± 0.4725761418395412"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "324bf49061890d2f9372fc2d729ec1a20f3848ce",
          "message": "build: validate packages against 3.5.0 on every PR (#176)\n\n## Summary\nRoadmap H7: catch accidental public API breaks before they ship.\n\n- **Package validation** (`Directory.Build.props`):\n`EnablePackageValidation` + `PackageValidationBaselineVersion` 3.5.0 for\n`src/` projects only (all 59 packages exist at 3.5.0 on nuget.org; test\nprojects never try to download a baseline). Intentional breaks in v4.0\ngo into a per-project `CompatibilitySuppressions.xml`; bump the baseline\nafter each release.\n- **`MinVerTagPrefix=v`**: release tags are `v1.2.3`, so MinVer ignored\nthem and every non-release build was `0.0.0-alpha`, which package\nvalidation rejects (CP0003 on all packages). Builds now version as\n`3.5.1-alpha.0.N`. Releases still pass `MinVerVersionOverride`, so\npublished versions are unchanged.\n- **build-test (ubuntu)**: new `Pack (package validation)` step, so a\nbreak fails the PR instead of the release tag.\n\nDocs:\nhttps://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator\n(updated 2025-08-21).\n\n## Test plan\n- [x] Release build + pack, whole solution: 59 packages, 0 errors\nagainst 3.5.0\n- [x] RED probe: making `AddKckArgon2` internal fails pack with\n`CP0002`; reverted\n- [x] `dotnet restore --locked-mode` passes, no lock file changes\n- [x] Unit tests (Release): 0 failures\n- [ ] CI: Build & Test (ubuntu) runs the new pack step\n\nKnown local issue (CI not affected): on a Turkish-culture machine SDK\n10.0.402 lowercases package ids culture-sensitively\n(`kck.caching.ınmemory`) and pack fails with MSB4018 for 4 packages;\nworkaround `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1`.\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T17:21:31+03:00",
          "tree_id": "5e20ca2ac792f07704293764b68c3417cfdd5c19",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/324bf49061890d2f9372fc2d729ec1a20f3848ce"
        },
        "date": 1793111336519,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.931199199954668,
            "unit": "ns",
            "range": "± 0.033836327637748284"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 14.870815576016902,
            "unit": "ns",
            "range": "± 1.4696922543463042"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6447794964680305,
            "unit": "ns",
            "range": "± 0.0046129518324551895"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6791540442832878,
            "unit": "ns",
            "range": "± 0.003314814181361483"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 475.11210950215656,
            "unit": "ns",
            "range": "± 2.6764307987723583"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 779.91805006663,
            "unit": "ns",
            "range": "± 9.144944237854101"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 9.705716447035472,
            "unit": "ns",
            "range": "± 0.05155840970438144"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 9.586269554037314,
            "unit": "ns",
            "range": "± 0.04307594437750284"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 9.779899741212526,
            "unit": "ns",
            "range": "± 0.0821782298092661"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.692302175362904,
            "unit": "ns",
            "range": "± 0.09167363972654073"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.818742557366688,
            "unit": "ns",
            "range": "± 0.046898280947452936"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.90555579428162,
            "unit": "ns",
            "range": "± 0.09147979252472316"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "8f77765a763d744a147fbaa6e2dbdce13d84cea6",
          "message": "fix(templates): generated kck-webapi project builds and tracks release (#177)\n\n## Summary\nRoadmap D5 (developer experience): the project generated by `dotnet new\nkck-webapi` did not compile. Three fixes:\n\n1. **Stale package pin** — `Kck.Bundle.WebApi` was hard-coded to 3.0.0\n(unlisted). The template project now has\n`Version=\"KCK_PACKAGE_VERSION\"`; a pack-time target in\n`Kck.Templates.csproj` replaces it with the template package's own\nversion (3.6.0 template → 3.6.0 bundle). Pack fails if the placeholder\nis missing or survives.\n2. **Swagger without a package** — `Program.cs` used\n`AddSwaggerGen`/`UseSwagger`/`UseSwaggerUI` with no Swashbuckle\nreference (CS1061). Switched to `Microsoft.AspNetCore.OpenApi` `10.0.*`\n(`AddOpenApi()`, `MapOpenApi()` in Development) — Microsoft's built-in\napproach since .NET 9. Currency check:\n`decisions/2026-10-27-template-openapi.md`.\n3. **`--IncludeSample false`** — `Features/` was excluded but\n`Program.cs` still referenced the sample namespace and service (CS0246).\nBoth lines are now template-conditional.\n\n## Test plan\n- [x] RED: Kck.Templates packed as 3.6.0 referenced `Kck.Bundle.WebApi`\n3.0.0\n- [x] GREEN: packed 3.6.0 → 3.6.0, 3.5.1-alpha.0.11 → 3.5.1-alpha.0.11\n- [x] Guard probe: replacing the placeholder with a fixed version fails\npack\n- [x] End to end (template packed as 3.5.0, custom hive): `dotnet new\nkck-webapi` with and without sample → `dotnet build` 0 warnings / 0\nerrors; app in Development serves `GET /openapi/v1.json` → 200, OpenAPI\n3.1.1, path `/api/WeatherForecast`\n- [ ] CI green (template end-to-end is not in CI yet; it needs a local\nfeed for unpublished Kck packages)\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T17:58:29+03:00",
          "tree_id": "4129d7616a3388408f90a626eac1223573391695",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/8f77765a763d744a147fbaa6e2dbdce13d84cea6"
        },
        "date": 1793113768287,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.613524354812576,
            "unit": "ns",
            "range": "± 0.2618460190583743"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 6.318424009037821,
            "unit": "ns",
            "range": "± 0.4816623595470146"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.1646331087327921,
            "unit": "ns",
            "range": "± 0.006614523030243652"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5362790077924728,
            "unit": "ns",
            "range": "± 0.02371985851347226"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 276.09485758267914,
            "unit": "ns",
            "range": "± 7.611387260792296"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 502.0929594454558,
            "unit": "ns",
            "range": "± 11.058707704165155"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 8.785645433162388,
            "unit": "ns",
            "range": "± 0.3778518001778265"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 8.463266867658366,
            "unit": "ns",
            "range": "± 0.26030355819085743"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 8.779382210926734,
            "unit": "ns",
            "range": "± 0.5419782567921543"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 9.980381752942737,
            "unit": "ns",
            "range": "± 0.7043021764465435"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 8.414200669036635,
            "unit": "ns",
            "range": "± 0.5563352916877553"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 8.709868461626474,
            "unit": "ns",
            "range": "± 0.48544097047810425"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "a035496f784fb45b7b5b9d657849e1fd6c2bb3b8",
          "message": "feat(filestorage-s3): support S3-compatible endpoints (#178)\n\n## Summary\nRoadmap H6: `Kck.FileStorage.AwsS3` can target S3-compatible services.\n\nNew `AwsS3Options` (all optional; defaults keep today's Amazon S3\nbehaviour):\n\n| Option | Effect |\n|---|---|\n| `ServiceUrl` | Endpoint of an S3-compatible service (MinIO, Cloudflare\nR2, Wasabi). `Region` then only signs requests (`AuthenticationRegion`).\nMust be an absolute http(s) URL (validated at startup). |\n| `ForcePathStyle` | Path-style URLs `host/bucket/key` |\n| `ChecksumWhenRequired` | `RequestChecksumCalculation` /\n`ResponseChecksumValidation` = `WHEN_REQUIRED`, for services that reject\nthe AWS SDK v4 default CRC32 checksums |\n\n- Client configuration moved to `internal\nCreateClientConfig(AwsS3Options)`; new public members added to\n`PublicAPI.Unshipped.txt`.\n- README: `UploadAsync` sample fixed to the real `FileUploadRequest`\nAPI, non-existent `PublicReadAccess` option removed, new options and an\nS3-compatible example documented.\n- API source: aws/aws-sdk-net (context7, 2026-10-27) — `ServiceURL` +\n`ForcePathStyle` + `AuthenticationRegion`; `WHEN_REQUIRED` skips\nnon-required checksums.\n\n## Test plan\n- [x] RED (members missing) → GREEN: `Kck.FileStorage.AwsS3.Tests` 28/28\n(8 new: client config mapping, ServiceUrl validation)\n- [x] Mutation probe: removing `AuthenticationRegion` / response\nchecksum mapping fails 2 tests\n- [x] Behaviour against a local fake S3 endpoint: `PUT\n/demo-bucket/docs/...` (path-style), signed `us-east-1`; checksum header\nsent by default, none with `ChecksumWhenRequired = true`\n- [x] Solution Release build 0 errors; package validation vs 3.5.0\npasses; `dotnet restore --locked-mode` clean\n- [ ] Not run: real MinIO/R2 (no Docker locally)\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T18:07:47+03:00",
          "tree_id": "2f91f166cb701c7e83d91141581ca1ad5e66a4e7",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/a035496f784fb45b7b5b9d657849e1fd6c2bb3b8"
        },
        "date": 1793114217703,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.8221724340144325,
            "unit": "ns",
            "range": "± 0.16888347340748158"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 6.859471036919525,
            "unit": "ns",
            "range": "± 0.08763153614528349"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.6716696502019962,
            "unit": "ns",
            "range": "± 0.1281291117412256"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.7858197288587689,
            "unit": "ns",
            "range": "± 0.0008974085757643118"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 304.5523271220071,
            "unit": "ns",
            "range": "± 3.0127489063861037"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 559.8192948341369,
            "unit": "ns",
            "range": "± 4.85909367567248"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 8.252397742654596,
            "unit": "ns",
            "range": "± 0.08968314712807923"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 8.21224985619386,
            "unit": "ns",
            "range": "± 0.06557703259599862"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 7.979582240680854,
            "unit": "ns",
            "range": "± 0.05118052666855309"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 7.894469647535256,
            "unit": "ns",
            "range": "± 0.10742729261670278"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 9.055096044459127,
            "unit": "ns",
            "range": "± 0.2847380049158348"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 8.03251130346741,
            "unit": "ns",
            "range": "± 0.04404527974861683"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "a9a32e6c8a73514f48714be976e4edd6e3f3325a",
          "message": "docs(changelog): add 3.6.0 release notes (#179)\n\n## Summary\nCHANGELOG section for 3.6.0 (roadmap H9), ahead of the `v3.6.0` tag.\nCovers #164–#178: S3-compatible endpoints, package validation, working\n`dotnet new kck-webapi` template, dependency updates, newest-per-major\nlisting, licence gate, and the 4.0.0 removal list (KCK0200, KCK0001,\nKCK0100, legacy InMemory event bus registration).\n\nAll linked docs (`docs/licensing.md`, `docs/policies/deprecation.md`,\n`docs/migrations/mediatR-to-mediator.md`) exist.\n\nKnown gap (not in this PR): sections for 3.2–3.5 were never written.\n\n## Test plan\n- [ ] CI green (docs-only change)\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T18:18:38+03:00",
          "tree_id": "7e79359f7ef2318d21293cb6c319aa95cae7a6d4",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/a9a32e6c8a73514f48714be976e4edd6e3f3325a"
        },
        "date": 1793114796614,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.735975085063414,
            "unit": "ns",
            "range": "± 0.4245466780022556"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 14.344684117857147,
            "unit": "ns",
            "range": "± 0.5410518444698635"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6374460426824433,
            "unit": "ns",
            "range": "± 0.004100304553608093"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.6934620408075196,
            "unit": "ns",
            "range": "± 0.00760265838640364"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 484.9813875198364,
            "unit": "ns",
            "range": "± 4.45154825198345"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 794.5193175633748,
            "unit": "ns",
            "range": "± 2.6438619227554265"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.080953027193363,
            "unit": "ns",
            "range": "± 0.14996227553073407"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.386885409659527,
            "unit": "ns",
            "range": "± 0.4972880992936721"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.288403332765613,
            "unit": "ns",
            "range": "± 0.38135940521041456"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 11.685313395327992,
            "unit": "ns",
            "range": "± 0.3178420045029355"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.963691233524255,
            "unit": "ns",
            "range": "± 0.19698761129579592"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.77513531781733,
            "unit": "ns",
            "range": "± 0.36460747643443897"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "9599a7d3a81bdea76d4fc8236baf9f34405ec8c4",
          "message": "build: validate packages against released 3.6.0 (#180)\n\n## Summary\nPost-release step for v3.6.0: `PackageValidationBaselineVersion` 3.5.0 →\n3.6.0, so package validation compares every PR with the version now on\nnuget.org.\n\nv3.6.0 verified on nuget.org: 59/59 packages indexed, only 3.6.0 listed\nper package (3.5.0 unlisted by the release workflow).\n\n## Test plan\n- [x] `dotnet restore --locked-mode` (no lock file changes)\n- [x] Release build + solution pack: 59 packages validated against\ndownloaded 3.6.0 baselines, 0 errors\n- [ ] CI green (Build & Test ubuntu runs the pack step)\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T19:31:21+03:00",
          "tree_id": "1be962b304d9dfb2b99fa89058120152117312ef",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/9599a7d3a81bdea76d4fc8236baf9f34405ec8c4"
        },
        "date": 1793119102508,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.885011891523997,
            "unit": "ns",
            "range": "± 0.14963776273281937"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 10.98536866047868,
            "unit": "ns",
            "range": "± 0.602955044032501"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6340568478290851,
            "unit": "ns",
            "range": "± 0.0031610059774062615"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5845094782610735,
            "unit": "ns",
            "range": "± 0.0028121937751221415"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 481.74316678728377,
            "unit": "ns",
            "range": "± 1.320106752405715"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 808.8357134546552,
            "unit": "ns",
            "range": "± 2.4467809815345167"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 11.542013110592961,
            "unit": "ns",
            "range": "± 0.28908072493274567"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 11.725331324338914,
            "unit": "ns",
            "range": "± 0.22064633050491908"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.142803559700647,
            "unit": "ns",
            "range": "± 0.2516671215813653"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 12.254098779388837,
            "unit": "ns",
            "range": "± 0.19290382546805862"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.221455987294515,
            "unit": "ns",
            "range": "± 0.1966206349208649"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 11.136481259266535,
            "unit": "ns",
            "range": "± 0.13713088162106277"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "6422927021bcdf6005afb7b8f301579d56206344",
          "message": "docs(adr): ADR-0028 v4.0.0 scope and v4 plan (#181)\n\n## Summary\nv4 step PR 1 (roadmap H12): architecture decision and plan for the 4.0.0\nmajor, approved by the maintainer.\n\n- `docs/adr/0028-v4-scope.md` — ten decisions: remove\n`Kck.Pipeline.MediatR` + licence exception, KCK0001, KCK0100\n(QueryOptions methods move into `IReadRepository`; old bool-flag members\nand `ReadRepositoryExtensions` removed), legacy InMemory event bus\nregistration; WorkerService defaults to Quartz and fails fast on\n`\"Provider\": \"Hangfire\"`; ImageSharp rejects unknown formats; major\ndependencies only when their compatibility tests pass;\n`CompatibilitySuppressions.xml` for intentional breaks; `release/3.x`\nmaintenance branch; migration guide required before release.\n- `docs/plans/2026-10-27-v4-plan.md` — PR sequence, acceptance examples\nV1–V10, impact map, premortem, threat model.\n- ADR index row for 0028.\n\nNo code changes.\n\n## Test plan\n- [x] Referenced ADRs (0010, 0016, 0021, 0027) exist\n- [ ] CI green (docs only)\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T19:50:12+03:00",
          "tree_id": "10fd60435b3a9df5f07b808ec784d25d4c47feb9",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/6422927021bcdf6005afb7b8f301579d56206344"
        },
        "date": 1793120217976,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.191003354696127,
            "unit": "ns",
            "range": "± 0.07812274479026483"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 8.905587460845709,
            "unit": "ns",
            "range": "± 0.27473800911409824"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6363200369690145,
            "unit": "ns",
            "range": "± 0.007227247952741277"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5881983935832977,
            "unit": "ns",
            "range": "± 0.0066327392511975735"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 475.2424740791321,
            "unit": "ns",
            "range": "± 4.027359468822855"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 787.6844073075515,
            "unit": "ns",
            "range": "± 2.3785018492128027"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.030887686212857,
            "unit": "ns",
            "range": "± 0.08609976442153007"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 12.48060701574598,
            "unit": "ns",
            "range": "± 0.07457250414737727"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.69207663834095,
            "unit": "ns",
            "range": "± 0.12192637992221111"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.051263206203778,
            "unit": "ns",
            "range": "± 0.08560165115146862"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.18184411128362,
            "unit": "ns",
            "range": "± 0.15397761431101642"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.704916803964547,
            "unit": "ns",
            "range": "± 0.08354920632198627"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "b8ee085194db6753af5715a8fa9d86ff1699c9bc",
          "message": "feat(backgroundjobs-quartz)!: upgrade to Quartz.NET 4.6 (#182)\n\n## Summary\nv4 plan PR 2 (ADR-0028 K7): `Kck.BackgroundJobs.Quartz` moves to\nQuartz.NET 4.6.2. Supersedes Dependabot #173.\n\n- Quartz 3.22.5 → 4.6.2; `Quartz.Extensions.DependencyInjection` /\n`.Hosting` removed (merged into `Quartz` in 4.x, migration guide: \"Still\nremove them\").\n- Adapter: `ValueTask Execute(IJobExecutionContext, CancellationToken)`;\n`ScheduleJob(job, trigger, ScheduleJobOptions, ct)`; `CheckExists` →\n`Exists`.\n- **Breaking (4.0):**\n`AddKckBackgroundJobsQuartz(Action<IQuartzBuilder>?)` replaces\n`Action<IServiceCollectionQuartzConfigurator>?` — the only entry in the\nnew `CompatibilitySuppressions.xml`.\n- README rewritten to the real API; package description fixed.\n- Currency check and Quartz 4 behaviour changes for the migration guide:\n`decisions/2026-10-27-quartz-4.md`.\n\n## Test plan\n- [x] RED after the bump: CS0246\n(`IServiceCollectionQuartzConfigurator`), CS0535 (`IJob.Execute`)\n- [x] `Kck.BackgroundJobs.Quartz.Tests` 8/8, including a new\nreal-scheduler test (job fires through DI); mutation probe (adapter not\ncalling the job) fails it\n- [x] Solution: `restore --locked-mode`, Release build 0 errors, unit\ntests 0 failures, package validation passes with the single documented\nCP0002, licence gate PASS (227 packages)\n- [x] README sample run as an app: 4 executions in 3.5 s (1 enqueue +\nper-second cron)\n- [ ] Not run: Quartz persistent (ADO) job store\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T20:07:09+03:00",
          "tree_id": "7364b024aa542dffc560c496f7654011ca8c1809",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/b8ee085194db6753af5715a8fa9d86ff1699c9bc"
        },
        "date": 1793121329811,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.163824028482563,
            "unit": "ns",
            "range": "± 0.17487817889374563"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 6.396050460063494,
            "unit": "ns",
            "range": "± 0.13122361027576931"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.7783406990979399,
            "unit": "ns",
            "range": "± 0.11884594061909458"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.7878000226337463,
            "unit": "ns",
            "range": "± 0.004184940972912783"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 307.7201016289847,
            "unit": "ns",
            "range": "± 3.212576113706388"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 564.4248368399484,
            "unit": "ns",
            "range": "± 4.267158475889477"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 7.820618785917759,
            "unit": "ns",
            "range": "± 0.07073852609347084"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 8.522327877305171,
            "unit": "ns",
            "range": "± 0.3087950774793273"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 8.761519576112429,
            "unit": "ns",
            "range": "± 0.24093143777775802"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 8.21608163842133,
            "unit": "ns",
            "range": "± 0.05891752828126254"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 8.527153696005161,
            "unit": "ns",
            "range": "± 0.11247270528261"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 9.316081077307462,
            "unit": "ns",
            "range": "± 0.7849832745338686"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "466ce8b4962c17e8b44e07b2ce34f161035f0030",
          "message": "security(filestorage-ftp): FluentFTP 55.1.0; CI on release branches (#183)\n\n## Summary\nSecurity fix: FluentFTP below 55.1.0 is affected by a **high**-severity\nadvisory (FTP data exfiltration and spoofing through man-in-the-middle\nattacks, published 2026-10-18, patched in 55.1.0).\n`Kck.FileStorage.FluentFtp` 3.6.0 depends on 54.0.2. The NuGet audit in\nCI did not flag it yet.\n\n- FluentFTP 54.0.2 → 55.1.0 (no Kck API change; package validation\nagainst 3.6.0 passes).\n- `build-test`, `codeql`, `license-audit` also run for `release/**`, so\nthe 3.x maintenance branch gets CI.\n- CHANGELOG 3.6.1 section. The same fix goes to `release/3.x` in a\nseparate PR and ships as 3.6.1.\n\n## Test plan\n- [x] `Kck.FileStorage.FluentFtp.Tests` 21/21 with 55.1.0\n- [x] `pack` validation against 3.6.0 passes\n- [x] Workflow YAML parses\n- [ ] CI green\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T20:16:45+03:00",
          "tree_id": "8a31cd5a51fa23b73d60097cada1aa37c439477c",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/466ce8b4962c17e8b44e07b2ce34f161035f0030"
        },
        "date": 1793121793083,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 3.888929754992326,
            "unit": "ns",
            "range": "± 0.1745762620790662"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 4.876693976422151,
            "unit": "ns",
            "range": "± 0.15502667996237496"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.5125989750600778,
            "unit": "ns",
            "range": "± 0.027666947704737266"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.1833502770616458,
            "unit": "ns",
            "range": "± 0.01943521818745812"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 265.736562983195,
            "unit": "ns",
            "range": "± 2.98995498496087"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 439.09027767181396,
            "unit": "ns",
            "range": "± 5.146106146672154"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 5.545188883940379,
            "unit": "ns",
            "range": "± 0.0929519537907519"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 5.441507793217897,
            "unit": "ns",
            "range": "± 0.16874808262917326"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 5.5999520754112915,
            "unit": "ns",
            "range": "± 0.15463634467469453"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 5.424669045954943,
            "unit": "ns",
            "range": "± 0.11323851264244592"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 5.5832167385981,
            "unit": "ns",
            "range": "± 0.22566174058864916"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 5.553616815322155,
            "unit": "ns",
            "range": "± 0.2581636039572025"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "7c740f2fa874ce2b3d79e978fe370890abe06fa5",
          "message": "chore(deps): YamlDotNet 18.1 and OpenTelemetry 1.20.1 (#185)\n\n## Summary\nv4 plan PR 2 (small majors, ADR-0028 K7). Supersedes Dependabot #167.\n\n| Package | Before → After |\n|---|---|\n| YamlDotNet | 16.3.0 → 18.1.0 |\n| OpenTelemetry.Api, .Extensions.Hosting,\n.Exporter.OpenTelemetryProtocol | 1.15.3 → 1.20.1 |\n| OpenTelemetry.Instrumentation.AspNetCore / .Http | 1.15.2 / 1.15.1 →\n1.20.1 |\n| OpenTelemetry.Instrumentation.EntityFrameworkCore | 1.15.1-beta.1 →\n1.20.1-beta.1 |\n\n- No code change, no Kck API change (package validation clean, no\nsuppressions).\n- YamlDotNet 18's only breaking change (`ITypeInspector`) is not used by\nKck; new default recursion limit (130) hardens YAML parsing.\n- OpenTelemetry consumer-visible changes (OTLP exporter no longer\nfollows redirects, 64 MiB max request, attribute limits) are recorded\nfor the v3→v4 migration guide in\n`decisions/2026-10-27-yamldotnet-18-otel-1.20.md`.\n- Advisories checked: YamlDotNet none; all 8 OpenTelemetry advisories\naffect ≤ 1.15.2 only.\n\n## Test plan\n- [x] `Kck.Localization.Yaml.Tests` 13/13,\n`Kck.Observability.OpenTelemetry.Tests` 10/10\n- [x] Solution: `restore --locked-mode`, Release build 0 errors, unit\ntests 0 failures, pack/package validation clean, licence gate PASS\n- [ ] Not run: end-to-end export to a real OTLP collector\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T21:01:17+03:00",
          "tree_id": "31f6bc912d11c2a3a709408607a0c23103a7e9dc",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/7c740f2fa874ce2b3d79e978fe370890abe06fa5"
        },
        "date": 1793124426753,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.483454236388207,
            "unit": "ns",
            "range": "± 0.12791942783523147"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 7.235588032220091,
            "unit": "ns",
            "range": "± 0.0905334340468948"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.5456896568338077,
            "unit": "ns",
            "range": "± 0.009461491483088669"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.7076792865991592,
            "unit": "ns",
            "range": "± 0.028009888539570125"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 486.2249849955241,
            "unit": "ns",
            "range": "± 1.181323920723973"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 844.5518529074533,
            "unit": "ns",
            "range": "± 1.5393497171225692"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 15.019780478874843,
            "unit": "ns",
            "range": "± 0.11496286017826403"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 11.968385124206543,
            "unit": "ns",
            "range": "± 0.09011286288819348"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 12.150605628887812,
            "unit": "ns",
            "range": "± 0.16398686711744734"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 11.752801038821538,
            "unit": "ns",
            "range": "± 0.20904036280213753"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 12.047007564703623,
            "unit": "ns",
            "range": "± 0.21202438329311915"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 12.234617167711258,
            "unit": "ns",
            "range": "± 0.2582121632511022"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "eb3c8fa40176c694e4c3128752ff8d6fa089b353",
          "message": "fix(caching-hybrid): GetAsync reads values written by SetAsync (#186)\n\n## Summary\nBug fix in `Kck.Caching.Hybrid` (all released 3.x versions):\n**`GetAsync` threw `JsonException` for every value written with\n`SetAsync`.**\n\n- `SetAsync` writes through Microsoft HybridCache, which stores L2\nentries in its own payload format (header + key + value — 38 bytes for\n`\"shipped\"`). `GetAsync` read the L2 entry directly from\n`IDistributedCache` and parsed it as JSON → `'0x03' is an invalid start\nof a value`.\n- Fix: `GetAsync` reads through `HybridCache.GetOrCreateAsync` with\n`DisableLocalCacheWrite | DisableDistributedCacheWrite` — returns a\nstored value (L1, then L2), stores nothing on a miss.\n- The now-unused `IDistributedCache` constructor parameter stays for 3.x\nbinary compatibility (CS9113 suppressed with a note; remove in 4.0).\n- The two old `GetAsync` tests mocked L2 as plain JSON and encoded the\nbug; replaced by `HybridCacheRoundTripTests` against the real\nHybridCache (dictionary-backed L2, since HybridCache ignores\n`MemoryDistributedCache`).\n- CHANGELOG 3.6.2. Found while validating StackExchange.Redis 3 (v4 plan\nPR 3); independent of the Redis version.\n\n## Test plan\n- [x] RED: new round-trip tests fail with the exact `JsonException`\nbefore the fix\n- [x] GREEN: `Kck.Caching.Hybrid.Tests` 8/8 (set→get for DTO/string/int,\nread from another instance via L2, miss returns default and stores\nnothing)\n- [x] Solution: `restore --locked-mode`, Release build, unit tests 0\nfailures, package validation vs 3.6.0 clean (no API change)\n- [ ] CI green\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T21:12:56+03:00",
          "tree_id": "5dc605ba0b88b9b4dc50d9c083683aa1330508b1",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/eb3c8fa40176c694e4c3128752ff8d6fa089b353"
        },
        "date": 1793125253468,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 3.716110262823732,
            "unit": "ns",
            "range": "± 0.12750928230311145"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 4.678546460202107,
            "unit": "ns",
            "range": "± 0.09548228805065892"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.4687305883719371,
            "unit": "ns",
            "range": "± 0.028361955714678172"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.21773254578667028,
            "unit": "ns",
            "range": "± 0.01895574299348585"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 242.03410363197327,
            "unit": "ns",
            "range": "± 2.497007287833731"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 474.70306126276654,
            "unit": "ns",
            "range": "± 11.31789002025423"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 5.976964722861323,
            "unit": "ns",
            "range": "± 0.34497394777300466"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 6.016571366041899,
            "unit": "ns",
            "range": "± 0.2905511327159327"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 6.358955255294421,
            "unit": "ns",
            "range": "± 0.4530902665369778"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 6.205354708184799,
            "unit": "ns",
            "range": "± 0.37153430259516335"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 5.556563256042344,
            "unit": "ns",
            "range": "± 0.13659250441763524"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 5.563913485076692,
            "unit": "ns",
            "range": "± 0.2950046455520383"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "0d0babd79ae2551edaa008a2d2cea448c673a752",
          "message": "chore(deps)!: StackExchange.Redis 3.4.20 (#188)\n\n## Summary\nv4 plan PR 3 (ADR-0028 K7): StackExchange.Redis 2.13.17 → 3.4.20.\n**Merge only if the integration tests (V8) are green**; otherwise Plan B\n(stay on 2.x).\n\n- 3.0 has no API changes vs 2.13.17 (new IO core);\n`Microsoft.Extensions.Caching.StackExchangeRedis` 10.0.x allows\n`[2.7.27, )`.\n- New transitive dependency `RESPite` 3.4.20 (MIT).\n- New `HybridCacheIntegrationTests`: HybridCache → MECSER → SE.Redis 3\nagainst a real `redis:7-alpine` container (read from another instance\nvia L2, `RemoveByPrefixAsync`). Together with the existing Redis and\nTokenBlacklist integration tests this is acceptance example V8.\n- No Kck API change (package validation clean). Behaviour changes for\nconsumers (RESP3 default, `AllowAdmin` enforced for admin commands,\nseparate pub/sub connection under RESP3) are in\n`decisions/2026-10-27-stackexchange-redis-3.md` for the migration guide.\n\n## Test plan\n- [x] Local: `restore --locked-mode`, Release build 0 errors, unit tests\n0 failures, pack/package validation clean, licence gate PASS (227)\n- [ ] CI `Test (integration, ubuntu-only — Docker required)`: Redis,\nTokenBlacklist and Hybrid integration tests green\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T21:23:09+03:00",
          "tree_id": "5b588133feecb0807885511f90d92fb991a4f183",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/0d0babd79ae2551edaa008a2d2cea448c673a752"
        },
        "date": 1793125819868,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.3590537468592325,
            "unit": "ns",
            "range": "± 0.16466613469871585"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 11.073632078038322,
            "unit": "ns",
            "range": "± 0.7073824963216728"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6391446903347968,
            "unit": "ns",
            "range": "± 0.004519873225320334"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5825933997447674,
            "unit": "ns",
            "range": "± 0.002499774794760692"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 467.19772035734997,
            "unit": "ns",
            "range": "± 3.1715068584692547"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 787.9791022618612,
            "unit": "ns",
            "range": "± 4.402239327920678"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 10.875957229733467,
            "unit": "ns",
            "range": "± 0.21660964734433671"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 10.415117448568344,
            "unit": "ns",
            "range": "± 0.09483940396523075"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 10.259272528546196,
            "unit": "ns",
            "range": "± 0.10606436275985119"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 10.112456139922141,
            "unit": "ns",
            "range": "± 0.12874675363856092"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 10.705406247692949,
            "unit": "ns",
            "range": "± 0.2841859306060528"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 10.477822233851139,
            "unit": "ns",
            "range": "± 0.11463086638688409"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "73e2cfee94ad2704b1b8db720cf9fd36d6ad01f6",
          "message": "feat(pipeline)!: remove Kck.Pipeline.MediatR (#189)\n\n## Summary\nv4 plan PR 4 (ADR-0028 K1): remove `Kck.Pipeline.MediatR` (KCK0200) and\nthe temporary MediatR licence exception. MediatR 13+ requires a\ncommercial licence; removal was announced in 3.6.0 and the package is\ndeprecated on nuget.org.\n\n- Removed: `src/Providers/Kck.Pipeline.MediatR`,\n`tests/Kck.Pipeline.MediatR.Tests`, their solution entries, `MediatR` in\n`Directory.Packages.props`, `KCK0200` in `WarningsNotAsErrors`, the\nMediatR/Mediator duplication exclusion in `.sonarcloud.properties`.\n- `.github/license-policy.json`: MediatR exception deleted.\n- Docs: `docs/providers/pipeline.md` rewritten for\n`Kck.Pipeline.Mediator` (the old page showed a non-existent\n`AddKckPipelineMediatR`); FAQ pipeline example fixed\n(`AddKckPipeline().AddBehavior<>()` did not exist); bundle READMEs now\nsay \"Mediator pipeline\" (that is what `Kck.Bundle.WebApi` references);\nlicensing, deprecation table, known-issues K2 (closed), README,\nCHANGELOG.\n- Kept on purpose: ADRs, decision records and\n`docs/migrations/mediatR-to-mediator.md`.\n\n## Test plan\n- [x] Licence gate without the MediatR exception: 222 packages, 0\nviolations — PASS (it failed while the deleted package's lock file was\nstill on disk, proving the exception is gone)\n- [x] `restore --locked-mode`, Release build 0 errors, unit tests 0\nfailures (37 test assemblies), pack 58 packages with package validation\n- [ ] CI green\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T21:42:08+03:00",
          "tree_id": "effe7103f4836fe21b7cbc9a0da08d1d8b42a40a",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/73e2cfee94ad2704b1b8db720cf9fd36d6ad01f6"
        },
        "date": 1793126968962,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 5.114380077543584,
            "unit": "ns",
            "range": "± 0.18170717749437124"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 6.867826311903841,
            "unit": "ns",
            "range": "± 0.18757297322506558"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.8449656051498923,
            "unit": "ns",
            "range": "± 0.1349631754998051"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5171878666927417,
            "unit": "ns",
            "range": "± 0.004156409791739551"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 299.00835241590227,
            "unit": "ns",
            "range": "± 1.321675555237581"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 542.1725506048936,
            "unit": "ns",
            "range": "± 2.507126488467407"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 7.722263100055548,
            "unit": "ns",
            "range": "± 0.06676396821333272"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 8.422352480888367,
            "unit": "ns",
            "range": "± 0.3260146279313065"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 8.050545581181844,
            "unit": "ns",
            "range": "± 0.08514878326211554"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 7.8046705562334795,
            "unit": "ns",
            "range": "± 0.03861681942280538"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 8.126224519312382,
            "unit": "ns",
            "range": "± 0.12428529924373252"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 7.998664757609367,
            "unit": "ns",
            "range": "± 0.026938359597812142"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "434b1c264f60fc419ec0d4d75e5e057af3371e8e",
          "message": "feat!: remove KCK0001, KCK0100 and legacy InMemory registration (#190)\n\n## Summary\nv4 plan PR 5 (ADR-0028 K2–K4): remove the APIs deprecated in 3.x.\n\n- **KCK0100 (D7a):** the six `QueryOptions` read methods become\n`IReadRepository<T,TId>` members (same parameter order as the 3.x\nextensions); bool-flag overloads and `ReadRepositoryExtensions` removed.\n`options` is `QueryOptions?`: when omitted, Get/GetList/GetListByDynamic\ntrack and Any/GetById/Count do not — exactly the 3.x defaults, so\n`GetAsync(p)` + `SaveChanges` keeps working.\n- **KCK0001:** `GetClaimsFromToken` (claims without signature\nvalidation) removed from `ITokenService` / `JwtTokenService`.\n- **InMemory:** both `AddKckEventBusInMemory` overloads + dead helper\nremoved; use `AddKckEventBus(b => b.UseInMemory())`.\n- Empty `Kck.Core.Abstractions/Pipeline/*.cs` placeholders removed.\n- `PublicAPI.Unshipped.txt` (15 `*REMOVED*`, 6 new),\n`CompatibilitySuppressions.xml` in 5 projects (23 entries, intentional\nbreaks only).\n- CHANGELOG with a 3.x → 4.0 mapping table (incl. the `withDeleted:\ntrue` → `new QueryOptions(IncludeDeleted: true, AsTracking: true)`\ntracking trap), deprecation table, `event-bus.md` and Persistence README\nsamples fixed to the real API.\n\n## Test plan\n- [x] RED first: new tests failed in all three projects\n(compile/removed-API asserts)\n- [x] GREEN: Persistence 121/121 (unit), Jwt 33/33, InMemory 7/7;\nsolution unit tests 0 failures\n- [x] Mutation probe: changing GetAsync's default to no-tracking fails\nthe defaults test\n- [x] Package validation only reports the suppressed intentional breaks;\nlicence gate PASS\n- [x] Independent adversarial review: no blockers; 1 medium + 3 low\nfindings fixed in this PR\n- [ ] CI green (incl. EF integration tests)\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T21:56:42+03:00",
          "tree_id": "b1680c2a21fc85dde95ede87a993b74d7a175c3e",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/434b1c264f60fc419ec0d4d75e5e057af3371e8e"
        },
        "date": 1793127852281,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 6.707277460892995,
            "unit": "ns",
            "range": "± 0.10688445361229998"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 12.356560118794441,
            "unit": "ns",
            "range": "± 0.9767987153556721"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 1.6356643688465868,
            "unit": "ns",
            "range": "± 0.006177408662937421"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.5859211976329486,
            "unit": "ns",
            "range": "± 0.0012544681446350335"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 476.83705128942216,
            "unit": "ns",
            "range": "± 1.1702706146501936"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 797.0373069127401,
            "unit": "ns",
            "range": "± 4.086483385926313"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 11.26349598368009,
            "unit": "ns",
            "range": "± 0.1211525840964507"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 11.394667663744517,
            "unit": "ns",
            "range": "± 0.12892141086537834"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 11.271651063362757,
            "unit": "ns",
            "range": "± 0.1422026821708885"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 11.173384128014247,
            "unit": "ns",
            "range": "± 0.15153552415523155"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 11.05966527547155,
            "unit": "ns",
            "range": "± 0.08380284137212102"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 13.714331513643264,
            "unit": "ns",
            "range": "± 0.18378602739259456"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "4a5b40689f771319a0f73ea1538cfd5f68999003",
          "message": "feat!: WorkerService bundles Quartz only; ImageSharp format rules (#191)\n\n## Summary\nv4 plan PR 6 (ADR-0028 K5–K6).\n\n**Kck.Bundle.WorkerService (D3 + D8a)**\n- Quartz is the only bundled scheduler and the default;\n`Kck.BackgroundJobs.Hangfire` reference and `HangfireWorkerCount`\nremoved (bundle lock file has no Hangfire).\n- `\"Kck:BackgroundJobs:Provider\": \"Hangfire\"` (3.x default) and unknown\nnames fail at startup with instructions — no silent scheduler switch.\n- New `tests/Kck.Bundle.WorkerService.Tests` (5 tests); README rewritten\n(it described non-existent `IJob`/`JobContext`).\n\n**Kck.Documents.ImageSharp**\n| Source | Format | Result |\n|---|---|---|\n| PNG | empty / `null` | PNG — keeps source format, as the\n`OutputFormat` XML contract says (3.x produced JPEG) |\n| TIFF (not encodable by Kck) | empty | JPEG |\n| any | `jpg` / `jpeg` | JPEG |\n| any | `avif`, other names | `NotSupportedException` listing supported\nformats (3.x silently produced JPEG) |\n\nREADME and `docs/providers/documents.md` updated (README described\nnon-existent `DefaultOutputFormat` etc.).\n\n## Test plan\n- [x] RED → GREEN: WorkerService 4 failing → 5/5; ImageSharp\nunknown-format and keep-source tests failing → 20/20\n- [x] Solution: `restore --locked-mode`, Release build, unit tests 0\nfailures, package validation (only the suppressed `HangfireWorkerCount`\nbreak), licence gate PASS\n- [ ] CI green\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T22:06:42+03:00",
          "tree_id": "a545b56569b4e3d1d58f36297458dd4f4f10413d",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/4a5b40689f771319a0f73ea1538cfd5f68999003"
        },
        "date": 1793128406493,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 3.393971315984215,
            "unit": "ns",
            "range": "± 0.037582012883971054"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 4.420017785827318,
            "unit": "ns",
            "range": "± 0.07727347834740608"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.47393269805858534,
            "unit": "ns",
            "range": "± 0.010219903807507912"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.43520583326999956,
            "unit": "ns",
            "range": "± 0.009540913535884687"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 258.84746485490064,
            "unit": "ns",
            "range": "± 0.7648116080735473"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 385.2882261616843,
            "unit": "ns",
            "range": "± 1.7470695207931444"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 5.195477118094762,
            "unit": "ns",
            "range": "± 0.05868107758391416"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 5.253005026174443,
            "unit": "ns",
            "range": "± 0.07246418473107093"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 5.252489804157189,
            "unit": "ns",
            "range": "± 0.07708070327183847"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 5.2611250923230095,
            "unit": "ns",
            "range": "± 0.08763706264107257"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 5.228479614215238,
            "unit": "ns",
            "range": "± 0.0524467351449771"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 5.154596803975957,
            "unit": "ns",
            "range": "± 0.05776829050637215"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "106805727+omerkck41@users.noreply.github.com",
            "name": "Ömer KÜÇÜK",
            "username": "omerkck41"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "ad0398a1595bc8b1958c4ec5f2670cd69d4d255c",
          "message": "docs!: v3-to-v4 migration guide; docs match the real API (#192)\n\n## Summary\nv4 plan PR 7 (ADR-0028 K10): migration guide and a documentation pass\nagainst the real API.\n\n- **New** `docs/migrations/v3-to-v4.md` — summary table, before/after\ncode for every removal, KCK0100 mapping table (incl. the `withDeleted:\ntrue` → `new QueryOptions(IncludeDeleted: true, AsTracking: true)`\ntracking trap), WorkerService/Hangfire steps, ImageSharp format rules,\nQuartz 4 / StackExchange.Redis 3 / OpenTelemetry 1.20 behaviour notes,\nchecklist. Linked from `docs/migrations/README.md` and the CHANGELOG.\n- `docs/providers/persistence.md` + Persistence README rewritten: they\ndescribed `FindAsync`, `ListAsync`, `PaginateAsync`,\n`SoftDeleteInterceptor`, `AddKckEntityFramework`,\n`AddKckPersistenceEntityFramework` (none exist), injected repositories\nthat are not registered, and called `SaveChangesAsync(ct)` (does not\ncompile).\n- README WorkerService sample uses `Kck.Bundle.WorkerService`;\n`background-jobs.md` drops Quartz-3-only\n`UseMicrosoftDependencyInjectionJobFactory()`; `file-storage.md` lists\nAzure Blob and S3 (and FluentFTP is FTP/FTPS, not SFTP).\n- `Kck.Caching.Hybrid`: `HybridCacheService` drops the unused\n`IDistributedCache` constructor parameter (CP0002 suppressed).\n\n## Test plan\n- [x] V9: all 4.0 code from the guide, persistence.md, README and\ntouched package READMEs compiled against the sources (one doc bug found\nand fixed: `repo.Query(QueryOptions.None)` is not on the interface)\n- [x] `restore --locked-mode`, Release build, unit tests 0 failures,\npack/package validation, licence gate PASS\n- [ ] CI green\n\nKnown (not in this PR, tracked in tasks/todo.md):\n`AddAuditInterceptor()` registers but does not attach the interceptor.\n\n🤖 Generated with [Claude Code](https://claude.com/claude-code)\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-27T22:16:26+03:00",
          "tree_id": "05567327013a8b2b551cccc33864ff9cd5356e3f",
          "url": "https://github.com/omerkck41/OmerkckArchitecture/commit/ad0398a1595bc8b1958c4ec5f2670cd69d4d255c"
        },
        "date": 1793128940075,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Success_Factory",
            "value": 3.3561625725456645,
            "unit": "ns",
            "range": "± 0.06297433271207807"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Failure_Factory",
            "value": 4.497939567816885,
            "unit": "ns",
            "range": "± 0.1393313718269912"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Success",
            "value": 0.5059240930355512,
            "unit": "ns",
            "range": "± 0.03459575747378836"
          },
          {
            "name": "Kck.Benchmarks.ResultBenchmarks.Match_Failure",
            "value": 0.20503737032413483,
            "unit": "ns",
            "range": "± 0.020202899076447692"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Serialize_Reflection",
            "value": 252.961832009829,
            "unit": "ns",
            "range": "± 1.2906553459513221"
          },
          {
            "name": "Kck.Benchmarks.JsonSerializationBenchmarks.Deserialize_Reflection",
            "value": 433.9515917301178,
            "unit": "ns",
            "range": "± 1.447384792506611"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 0)",
            "value": 5.504039524921349,
            "unit": "ns",
            "range": "± 0.08355275028414258"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 10, Index: 5)",
            "value": 5.960644227954058,
            "unit": "ns",
            "range": "± 0.038115787437199074"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 0)",
            "value": 5.61437005084008,
            "unit": "ns",
            "range": "± 0.14807015323455117"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 100, Index: 5)",
            "value": 5.7136334409316385,
            "unit": "ns",
            "range": "± 0.10230091933641497"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 0)",
            "value": 5.590502899672304,
            "unit": "ns",
            "range": "± 0.1355699853044832"
          },
          {
            "name": "Kck.Benchmarks.PaginateCreateBenchmarks.Create(Size: 1000, Index: 5)",
            "value": 5.449295189231634,
            "unit": "ns",
            "range": "± 0.05734697601793344"
          }
        ]
      }
    ]
  }
}