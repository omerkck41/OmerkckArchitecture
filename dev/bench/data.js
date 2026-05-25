window.BENCHMARK_DATA = {
  "lastUpdate": 1779741608985,
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
      }
    ]
  }
}