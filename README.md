# gc-SimplCommerce Graft Port Runbook

This branch ports a focused business slice of SimplCommerce (`Core` geography operations) to Graftcode Gateway.

## Testing note

We intentionally rolled back the recent attempt to restore legacy in-repo unit tests for this port. In this GRAFT shape, public entry points are static methods, and the original .NET test architecture does not isolate or mock static boundaries well without intrusive refactoring.

That restoration produced brittle tests with high maintenance cost and low confidence gain. For this port we prioritize integration and E2E verification of the exposed GRAFT surface, where business behavior is validated end-to-end.

The exposed path removes HTTP transport and publishes static business operations directly from:

- `SimplCommerce.Module.Core.Areas.Core.Controllers.CountryApiController`
- `SimplCommerce.Module.Core.Areas.Core.Controllers.StateOrProvinceApiController`
- `SimplCommerce.Module.Core.Areas.Core.Controllers.DistrictApiController`

## Exposed public surface

Module:

- `SimplCommerce.Module.Core.dll`

Types and methods:

- `SimplCommerce.Module.Core.Areas.Core.Controllers.CountryApiController`
  - `ListCountries`
  - `GetCountryCard`
  - `CreateCountry`
  - `UpdateCountry`
  - `DeleteCountry`
- `SimplCommerce.Module.Core.Areas.Core.Controllers.StateOrProvinceApiController`
  - `ListAllStatesOrProvinces`
  - `ListStatesOrProvinces`
  - `GetStateOrProvinceCard`
  - `CreateStateOrProvince`
  - `UpdateStateOrProvince`
  - `DeleteStateOrProvince`
- `SimplCommerce.Module.Core.Areas.Core.Controllers.DistrictApiController`
  - `ListDistricts`
  - `GetDistrictCard`
  - `CreateDistrict`

## Business parity notes and remaining gaps

Implemented parity from upstream Core controllers (`CountryApiController`, `StateOrProvinceApiController`, `DistrictApiController`):

- Added missing update/delete flows for country and state/province.
- Added state/province global list operation (`ListAllStatesOrProvinces`) matching upstream read surface.
- Preserved referential delete failures as stable business errors (`...can't not be deleted because it is referenced by other tables`).

Remaining intentional gaps in this graft demo:

- SmartTable grid endpoints (`CountryApiController.List`, `StateOrProvinceApiController.List`) are not exposed because they depend on transport-oriented paging/search payloads (`SmartTableParam`) and are not part of the primitive graft contract used by the external verifier.
- HTTP-only semantics are mapped to graft equivalents: role authorization attributes (`[Authorize(Roles = "admin")]`) are not represented, and HTTP status codes (`NotFound`, `BadRequest`, `Accepted`, `NoContent`) become either return values (`string.Empty`/`false`) or `InvalidOperationException`.

## Legacy cleanup - second pass

Removed with high confidence (outside active GRAFT demo path):

- Legacy in-repo test surface under `test/`.
- Orphaned test project folder `SimplCommerce.Module.WishList.Tests.Controllers/`.
- Related test project entries from `SimplCommerce.sln`.

Residual candidates intentionally left after second pass:

- No additional high-confidence candidates were found on the active geography GRAFT path.

## Start Graft stack

From `ports/gc-SimplCommerce/src`:

```powershell
docker compose -p simplcommerce-graft -f docker-compose.graft.yml up --build -d
```

Services:

- Gateway WebSocket: `ws://localhost:8290/ws`
- Graft Vision: `http://localhost:8291`
- SQL Server: `localhost,1436`

The compose file starts only what is required for the exposed path:

- SQL Server
- idempotent seed (`Database/graft-core-seed.sql`)
- `SimplCommerce.Core.GraftHost`

## Current generated package IDs

Current generation (without `GG_PROJECT_KEY`):

- `graft.nuget.simplcommerce.module.core_6309vk` `1.0.0`
- NuGet source: `https://grft.dev/b5e7e286-eb92-4b32-a011-0e3e549ab425__free`

Install:

```powershell
dotnet add package graft.nuget.simplcommerce.module.core_6309vk -v 1.0.0 --source https://grft.dev/b5e7e286-eb92-4b32-a011-0e3e549ab425__free
```

## Client configuration

```powershell
$env:GRAFTCODE_CONFIG_NUGET_SIMPLCOMMERCE_MODULE_CORE = "name=graft.nuget.SimplCommerce.Module.Core; runtime=netcore; modules=SimplCommerce.Module.Core.dll; host=ws://localhost:8290/ws; stateless=true"
```

Without `GG_PROJECT_KEY`, each gateway generation can produce a new feed GUID and package suffix.
When that happens, update:

- `gc-simplcommerce-graft-e2e/NuGet.Config` (feed URL)
- `gc-simplcommerce-graft-e2e/gc-simplcommerce-graft-e2e.csproj` (package ID)

## External e2e verifier

External verifier project (outside this repository):

- `C:\Coding\Marketing\gc-simplcommerce-graft-e2e`

Run:

```powershell
dotnet run --project C:\Coding\Marketing\gc-simplcommerce-graft-e2e\gc-simplcommerce-graft-e2e.csproj -c Release -- `
  --host "ws://localhost:8290/ws" `
  --stateless true `
  --report "C:\Coding\Marketing\gc-simplcommerce-graft-e2e\reports\simplcommerce-graft-e2e.json"
```

Expected exit codes:

- `0` all checks passed
- `1` at least one assertion failed
- `2` blocked before assertions

## Stop stack

```powershell
docker compose -p simplcommerce-graft -f docker-compose.graft.yml down
```
