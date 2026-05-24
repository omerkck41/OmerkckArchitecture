window.BENCHMARK_DATA = {
  "lastUpdate": 1779662796089,
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
      }
    ]
  }
}