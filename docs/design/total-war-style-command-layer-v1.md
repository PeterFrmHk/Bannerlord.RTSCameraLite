# Total War-style command layer v1

**Status:** design/spec slice only.  
**Scope:** Planned in-mission command control layer for RTS Commander Doctrine.  
**Runtime rule:** No gameplay behavior is added by this document.

## 1. Purpose and Non-goals

Purpose: define a Total War-inspired formation command layer for Bannerlord missions. The target experience is RTS camera control, formation selection, ground and enemy targets, drag previews, queued orders, command feedback, and doctrine-gated validation.

This is inspiration, not imitation. Total War supplies the user-control grammar; Bannerlord native formations remain the execution substrate.

Non-goals:

- Do not clone Total War exactly.
- Do not implement per-agent RTS puppeteering.
- Do not require Harmony for the baseline command layer.
- Do not add a new UI overlay in the first implementation slice.
- Do not add gameplay in this design slice.
- Do not bypass crash-quarantine defaults or make mission runtime default-live.

## 2. Player Control Grammar

The command layer should support this user grammar when explicitly enabled:

- **Commander camera mode:** player enters an RTS-style camera posture for reading the field and issuing formation-level commands. This remains gated by mission runtime and camera config.
- **Formation selection:** player selects one or more friendly formations. Existing `FormationSelectionState` and `FormationQueryService` are confirmed repo facts, but the current state is single-selection oriented and should be expanded only in a bounded TW-1 slice.
- **Number keys:** number keys map to friendly formation groups in the player team's ordered formation list. Selection must be read-only in TW-1 and must not issue orders.
- **Click ground move:** a ground click resolves to a world position and produces a `CommandIntent` of type `AdvanceOrMove`. Preview-only comes before native execution.
- **Right-drag facing/width:** drag start anchors a target position; drag vector sets facing; drag length may preview width or frontage. First implementation should render preview/markers only.
- **Enemy formation click target:** enemy formation selection resolves a `TargetFormation` and a representative target position. It should produce a charge/attack-style intent only after target resolving is guarded and doctrine validation exists.
- **Shift queue:** shift-modified commands append to a queue instead of replacing the current pending command list.
- **Clear queue:** explicit key clears pending queued commands for selected formation(s); no implicit clearing from unrelated input.
- **Command feedback markers:** markers communicate target point, facing line, invalid command reason, or queued command order. Markers must degrade to text feedback when visual marker rendering is unavailable.

## 3. Doctrine Mapping

Doctrine is the filter between player intent and execution. It should not be embedded in raw input or camera paths.

Doctrine should filter commands by:

- **Commander presence:** advanced commands can require a commander/captain/hero presence signal. Existing commander presence and anchor services are candidate inputs.
- **Morale:** low morale should reduce command eligibility or execution cleanliness when a stable Bannerlord signal exists; until then it must remain an explicit unknown/fallback.
- **Training:** training can raise discipline thresholds for complex shapes, reforming, or queued order reliability.
- **Equipment:** shield, polearm, ranged, mounted, and armor hints should influence what formations can cleanly use shield wall, square, loose, mounted-wide, or charge behavior.
- **Cohesion:** scattered or high-pressure formations should reject precise width/facing commands or downgrade to simpler movement.
- **Rank:** higher-rank troops may tolerate more complex maneuver requests.
- **Casualty pressure:** shock or degraded health ratios should block or degrade aggressive or complex orders.

Doctrine output should be explicit:

- `valid`: the command may proceed to preview or execution.
- `blocked`: the command is structurally understood but forbidden by doctrine.
- `degraded`: the command is accepted with a simpler native primitive or narrower preview.
- `unknown`: required signals are unavailable; default should be conservative.

## 4. Architecture

Proposed flow:

```text
Input
  -> SelectionState
  -> CommandGestureParser
  -> TargetResolver
  -> Preview/Markers
  -> CommandIntent
  -> CommandRouter
  -> DoctrineValidator
  -> NativeOrderPrimitiveExecutor
  -> Feedback
```

Layer responsibilities:

- **Input:** raw keys, mouse buttons, modifier keys, and cursor state. Must not call Bannerlord order APIs.
- **SelectionState:** selected friendly formations and optional queued command ownership.
- **CommandGestureParser:** translates click, right-drag, drag release, shift queue, and clear queue into typed command drafts.
- **TargetResolver:** resolves ground positions and enemy formation targets using guarded mission/team queries.
- **Preview/Markers:** displays transient target/facing/width/queue feedback. Must be optional and fail closed.
- **CommandIntent:** existing command payload shape; may need future fields for selected formation set, width, queue metadata, and preview-only mode.
- **CommandRouter:** existing validation/decision choke point. Should remain the only path toward native execution.
- **DoctrineValidator:** proposed extraction or extension for doctrine-specific accept/block/degrade decisions before execution.
- **NativeOrderPrimitiveExecutor:** existing native-order boundary. It remains disabled unless explicit execution gates are true.
- **Feedback:** text and markers for accepted, blocked, degraded, queued, or executed commands.

## 5. Proposed Classes

Candidate classes and responsibilities:

- **`FormationSelectionState`:** extend or wrap current single-selection state into selected formation set, active group, and selection version. Must remain read-only until command slices require mutation.
- **`FormationSelectionService`:** owns safe player-team formation discovery, number-key binding, selection cycling, selection clearing, and multi-select semantics.
- **`CommandGestureParser`:** parses click, right-drag, drag release, shift queue, and clear queue into command drafts. No mission enumeration and no native execution.
- **`EnemyFormationTargetResolver`:** resolves cursor/camera target to enemy `Formation` plus representative world position. Must be version-sensitive and adapter-backed.
- **`CommandQueue`:** per-formation pending command queue, bounded in size, cleared safely on mission end, selection invalidation, or runtime fault.
- **`QueuedCommand`:** immutable command record containing `CommandIntent`, queue index, creation time, preview marker ids, and validation state.
- **`FormationGhostLine`:** preview model for target line, facing, width, and selected formations. Data only; rendering belongs to marker/feedback surfaces.
- **`FormationPreviewMarker`:** visual/text marker state for ghost lines, target points, invalid indicators, and queue numbers.

Naming note: some class names already exist in partial form. Prefer extending current purpose where it matches instead of replacing module identity with generic abstractions.

## 6. Runtime Gates

Every runtime behavior in this design stays behind explicit config gates. Default must remain dormant.

Required/desired gates:

- **`EnableMissionRuntimeHooks`:** existing hard gate. If false, no mission command layer attaches.
- **`EnableCommanderCamera`:** proposed gate for camera control surfaces. Do not infer from runtime hooks alone.
- **`EnableFormationSelection`:** proposed gate for selection state and formation list reads.
- **`EnableCommandPreview`:** proposed gate for ground/drag/enemy preview markers without native execution.
- **`EnableCommandRouter`:** existing gate for command validation/decision.
- **`EnableNativePrimitiveOrderExecution`:** existing gate for primitive execution through the command router.
- **`EnableCommandQueue`:** proposed gate for shift queue state and queued-command tick/dispatch.
- **`EnableHarmonyPatches`:** existing gate; only relevant if a future slice proves native UI conflict integration needs patches.

Gate order for runtime paths:

```text
runtime fault? -> mission null/ended? -> EnableMissionRuntimeHooks? -> feature-specific gate? -> player team/formation query -> intent/preview/execution
```

No path should touch `Mission.PlayerTeam.FormationsIncludingEmpty` unless the relevant feature gate is enabled and mission state is valid.

## 7. Slice Plan

Planned implementation slices:

- **TW-1 Formation selection state, no orders:** extend selection state/service, number-key selection, safe feedback. No command intents, no native execution, no enemy targeting.
- **TW-2 Ground command intent + marker, no native execution:** parse ground click into `AdvanceOrMove` preview, resolve ground target, show marker/text. CommandRouter may validate only if `EnableCommandRouter` is explicitly enabled; native execution remains off.
- **TW-3 Native move/hold execution:** route validated ground move/hold through `NativeOrderPrimitiveExecutor` only when native execution gates are true.
- **TW-4 Drag facing/width preview:** parse right-drag into facing and width preview. No formation shape execution until native-safe API path is proven.
- **TW-5 Enemy formation targeting:** resolve enemy formation target and create charge/attack-style intents. Execution remains gated and doctrine-filtered.
- **TW-6 Shift queued orders:** bounded per-formation command queues, visible queue markers, clear queue command. No background/default-live dispatcher.
- **TW-7 Optional Harmony/native UI conflict integration:** only after research proves native input/UI conflicts cannot be solved through public APIs and existing guards.

## 8. Acceptance Checks per Slice

Every TW slice must include:

- `dotnet build -c Release` passes.
- Runtime remains dormant by default; no default-live mission command layer.
- Main menu load does not crash.
- Custom battle load does not crash with default config.
- No unguarded `Mission.PlayerTeam` or `FormationsIncludingEmpty` enumeration.
- No native order execution until `EnableNativeOrderExecution` and `EnableNativePrimitiveOrderExecution` are both explicitly true.
- Feature-specific config gate exists and defaults false.
- Exceptions are caught/logged at mission boundaries and do not escape into Bannerlord.
- Manual checklist distinguishes main menu load, battle load, preview behavior, validation behavior, and execution behavior.

Additional TW-1 checks:

- Selection state can clear invalid selections.
- Number-key selection does not issue command intents.
- Missing or null player team produces no crash and no selection.

Additional TW-2 checks:

- Ground click preview can fail without crash when terrain/cursor resolution is unavailable.
- Marker/text feedback is optional and fail-closed.
- No native order primitive is invoked.

## 9. Risk Notes

- **Bannerlord version drift:** `Formation`, `OrderController`, camera, and mission APIs may shift across versions; public-reference research must be refreshed before execution slices.
- **Native order API fragility:** move/facing/width semantics may not map cleanly to public `OrderController` calls. Keep one executor boundary.
- **Camera/input conflicts:** native order UI and RTS camera may both want mouse/keyboard ownership. Keep input ownership explicit.
- **Existing RTSCamera mod conflicts:** avoid assuming this mod owns all RTS camera inputs if other camera mods are installed.
- **Harmony dependency risk:** Harmony remains optional and gated. Do not add patches just to simplify a slice.
- **Duplicate mod install risk:** Steam/workshop/local duplicate module folders may cause wrong config or DLL load. Deployment audit remains a separate validation stage.
- **Preview/render risk:** marker visual effects may be unavailable or build-specific; text fallback is required.
- **Queue complexity:** queued command execution can become a hidden background system. Keep queue dispatch explicit, bounded, and gated.

## 10. Audit

Confirmed current repo facts:

- README describes the default install as a load-safe foundation with `CommanderMissionView` dormant unless `EnableMissionRuntimeHooks` is true.
- `config/commander_config.json` has `EnableMissionRuntimeHooks`, `StartBattlesInCommanderMode`, diagnostics, markers, router, native execution, and Harmony gates defaulted false.
- `CommanderMissionView` contains commander mode, camera bridge, formation scans, command router debug keys, markers, diagnostics, and runtime fault guards.
- Existing tactical surfaces include `FormationSelectionState`, `FormationQueryService`, `GroundTargetResolver`, and `FormationTargetResult`.
- Existing command surfaces include `CommandIntent`, `CommandRouter`, and `NativeOrderPrimitiveExecutor`.
- Existing UX surfaces include `CommandMarkerService` and tactical feedback.
- Existing adapter surfaces include `FormationDataAdapter`, `CameraBridge`, and `HarmonyPatchService`.

Assumptions:

- Total War-style control should be formation-level and single-player oriented.
- Existing `CommandIntent` can be extended rather than replaced.
- Cursor-to-world and enemy formation picking need additional research before execution.
- `EnableCommanderCamera`, `EnableFormationSelection`, `EnableCommandPreview`, and `EnableCommandQueue` are proposed gates, not confirmed current config fields.

Candidate implementation path:

- Start with TW-1 because selection is observable, low risk, and does not touch native order execution.
- Keep TW-2 preview-only so ground targeting and marker behavior can be validated before commands can move troops.
- Add TW-3 execution only after TW-1/TW-2 manual checks pass on the target Bannerlord build.

Forbidden shortcuts:

- Do not enumerate player formations before feature gates pass.
- Do not issue native orders from input parsing or preview code.
- Do not make mission runtime, command preview, queue dispatch, or Harmony patches default-live.
- Do not add per-agent movement puppeteering.
- Do not claim stable gameplay from design docs, compile success, or unverified debug behavior.
