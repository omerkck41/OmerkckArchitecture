window.BENCHMARK_DATA = {
  "lastUpdate": 1780101694193,
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
      }
    ]
  }
}