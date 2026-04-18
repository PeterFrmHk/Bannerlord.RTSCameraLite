# Version support — RTS Commander Doctrine

Values below mirror the **repository** at slice 25; update this file when pins change.

## Supported Bannerlord branch (design intent)

| Field | Value |
| --- | --- |
| **Primary pin (reference assemblies)** | **1.2.12.66233** — see `Bannerlord.RTSCameraLite.csproj` → `BannerlordRefsVersion` |
| **Target framework** | **net472** (Windows client) |

The **exact** retail Bannerlord build string on disk may differ from the NuGet ref package label; treat the **ref assembly pin** as the **contract** the C# code compiles against.

## Known tested versions

| Context | Version note |
| --- | --- |
| Local dev compile | `dotnet build` against NuGet `Bannerlord.ReferenceAssemblies` matching `BannerlordRefsVersion`, or local `BANNERLORD_INSTALL` / `BannerlordGameFolder` refs |
| Slice 0 research (repo) | Documents under `docs/research/` reference installed **1.3.x** layouts for **research** — **not** a promise this module binary runs on 1.3 without re-pin and retest |

**Maintainers:** after validating on a specific Steam build, add a row here with **date + game build + commit SHA**.

## Unsupported / unknown

- **Older** than the pinned ref major line without rebuild — expect **missing API** or **behavior drift**.
- **Future** game updates — require **recompile + manual regression** (`docs/tests/regression-matrix.md`).
- **Console / non-Windows** targets — **not** supported by this repo layout.

## How to report a version mismatch

1. Note **game version** (main menu or Steam properties) and **this mod** git tag / `InformationalVersion`.
2. Paste **first exception** from `rgl_log` (MissingMethodException / TypeLoadException are common).
3. Confirm whether you built from source with a **different** `BannerlordRefsVersion` than upstream.
4. File using `docs/tests/bug-report-template.md`.

## `SubModule.xml` vs C# pin

- **`SubModule.xml`** `Version` is user-facing module metadata — update on release.
- **`BannerlordRefsVersion`** is the **compile** pin — must stay aligned with tested APIs.
