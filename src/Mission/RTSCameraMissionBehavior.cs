using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Camera;
using Bannerlord.RTSCameraLite.Commands;
using Bannerlord.RTSCameraLite.Tactical;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Core;
using Bannerlord.RTSCameraLite.Input;
using Bannerlord.RTSCameraLite.UX;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Bannerlord.RTSCameraLite.Mission
{
    public sealed class RTSCameraMissionBehavior : MissionView
    {
        private readonly RTSCameraState _state = new RTSCameraState();
        private readonly RTSCameraController _controller = new RTSCameraController();
        private readonly CameraBridge _cameraBridge = new CameraBridge();
        private readonly NativeInputGuard _nativeInputGuard = new NativeInputGuard();
        private readonly FeedbackThrottle _feedbackThrottle = new FeedbackThrottle();
        private readonly TacticalFeedbackService _tacticalFeedback;
        private readonly CommandMarkerService _commandMarkerService;
        private bool _missionEndCleanupDone;
        private ConfigService.CameraKeyBindings _keyBindings;
        private static bool _loggedCameraConfigOnce;
        private readonly FormationQueryService _formationQuery = new FormationQueryService();
        private readonly FormationSelectionState _formationSelection = new FormationSelectionState();
        private readonly CommandRouter _commandRouter = new CommandRouter();
        private readonly NativeOrderExecutor _nativeOrderExecutor = new NativeOrderExecutor();
        private readonly GroundTargetResolver _groundTargetResolver = new GroundTargetResolver();
        private GroundTargetResult _currentGroundTarget = GroundTargetResult.Failure("not initialized");
        private bool _lastGroundFeedbackWasSuccess = true;
        private int _formationLogicTick;

        public RTSCameraMissionBehavior()
        {
            _tacticalFeedback = new TacticalFeedbackService(_feedbackThrottle);
            _commandMarkerService = new CommandMarkerService(_tacticalFeedback, () => Mission);
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            string moduleRoot = ConfigService.TryGetModuleRootFromAssembly();
            RTSCameraConfig cameraConfig = ConfigService.LoadOrCreate(moduleRoot, out string diagnostics);
            _keyBindings = ConfigService.BuildKeyBindings(cameraConfig, ConfigDefaults.CreateDefault());
            _controller.ApplyCameraSettings(cameraConfig);

            if (!_loggedCameraConfigOnce)
            {
                _loggedCameraConfigOnce = true;
                string pathNote = string.IsNullOrEmpty(moduleRoot)
                    ? "path unknown"
                    : ConfigService.GetConfigFilePath(moduleRoot);
                _tacticalFeedback.ShowDebugLine(
                    $"RTS camera config ({pathNote}): move={cameraConfig.MoveSpeed:0.###}, rot={cameraConfig.RotationSpeedDegrees:0.###} deg/s, toggle={cameraConfig.ToggleKey}. {diagnostics}");
            }

            _tacticalFeedback.ShowDebugLine("RTS camera mission behavior attached.");
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            _nativeInputGuard.Tick(dt);
            TryMissionEndCleanup();
            HandleF10Toggle();
            UpdateRtsCameraBridge(dt);
            _commandMarkerService.Tick(dt);
        }

        public override bool UpdateOverridenCamera(float dt)
        {
            if (!_state.Enabled || !_controller.HasSeededPose)
            {
                return false;
            }

            TaleWorlds.MountAndBlade.Mission mission = Mission;
            CameraBridgeResult result = _cameraBridge.TryApply(this, mission, _controller.GetPose(), dt);
            if (!result.Applied)
            {
                _tacticalFeedback.ShowWarning(
                    result.Message,
                    throttleKey: "camera-bridge",
                    cooldownSeconds: 25.0,
                    forceImmediate: false);
            }

            return result.Applied;
        }

        public override void OnRemoveBehavior()
        {
            TaleWorlds.MountAndBlade.Mission mission = Mission;
            _nativeInputGuard.ExitRtsMode("behavior removed");
            _state.ForceDisabled("behavior removed");
            _formationSelection.Clear();
            _formationQuery.InvalidateCache();
            _tacticalFeedback.ResetSession();
            _commandMarkerService.Clear();
            _cameraBridge.TryRestore(this, mission);
            base.OnRemoveBehavior();
        }

        private void HandleF10Toggle()
        {
            if (Input == null)
            {
                return;
            }

            bool toggleReleased;
            try
            {
                toggleReleased = Input.IsKeyReleased(_keyBindings.Toggle);
            }
            catch
            {
                return;
            }

            if (!toggleReleased)
            {
                return;
            }

            _state.Toggle("RTS toggle key");

            if (_state.Enabled)
            {
                _nativeInputGuard.EnterRtsMode("F10 RTS on");
                _tacticalFeedback.OnRtsEnabled();
                _formationSelection.Clear();
                _formationQuery.InvalidateCache();
                _controller.Reset();
                _currentGroundTarget = GroundTargetResult.Failure("resampling");
                _lastGroundFeedbackWasSuccess = true;
                _commandMarkerService.Clear();
                _tacticalFeedback.ShowModeEnabled(_state.ToggleCount);
            }
            else
            {
                _nativeInputGuard.ExitRtsMode("F10 RTS off");
                _tacticalFeedback.OnRtsDisabled();
                _formationSelection.Clear();
                _formationQuery.InvalidateCache();
                _currentGroundTarget = GroundTargetResult.Failure("RTS mode off");
                _lastGroundFeedbackWasSuccess = true;
                _commandMarkerService.Clear();
                _controller.Reset();
                TaleWorlds.MountAndBlade.Mission mission = Mission;
                _cameraBridge.TryRestore(this, mission);
                _tacticalFeedback.ShowModeDisabled(_state.ToggleCount);
            }
        }

        private void UpdateRtsCameraBridge(float dt)
        {
            if (!_state.Enabled)
            {
                return;
            }

            _controller.InitializeFromAgent(Mission?.MainAgent);

            if (!_controller.HasSeededPose)
            {
                return;
            }

            InputSnapshot snapshot = RTSCameraInput.Read(Input, _nativeInputGuard.Ownership, _keyBindings);
            _formationLogicTick++;
            HandleFormationInputAndFocus(snapshot);
            PublishFormationSelectionFeedback();
            UpdateGroundTargetSample();
            TryHandleDebugCommandKeys();
            _controller.Tick(snapshot, dt);
        }

        private void TryHandleDebugCommandKeys()
        {
            if (Input == null || !_state.Enabled || !_controller.HasSeededPose)
            {
                return;
            }

            try
            {
                if (Input.IsKeyPressed(InputKey.H))
                {
                    ProcessDebugCommand(CommandType.HoldPosition);
                }
                else if (Input.IsKeyPressed(InputKey.C))
                {
                    ProcessDebugCommand(CommandType.Charge);
                }
                else if (Input.IsKeyPressed(InputKey.G))
                {
                    ProcessDebugCommand(CommandType.MoveToPosition);
                }
            }
            catch
            {
                // Defensive: input stack must not break the mission tick.
            }
        }

        private void ProcessDebugCommand(CommandType type)
        {
            CommandContext context = BuildCommandContext();
            CommandIntent intent = BuildDebugIntent(type, context);
            CommandExecutionResult result = _commandRouter.ExecuteValidated(intent, context, _nativeOrderExecutor);
            _tacticalFeedback.ShowCommandExecutionResult(result);
            if (result != null && result.Executed && result.MarkerWorldPosition.HasValue)
            {
                _commandMarkerService.AddMarker(
                    result.MarkerWorldPosition.Value,
                    result.Type,
                    "Move");
            }
            else if (result != null
                && result.Executed
                && (result.Type == CommandType.Charge || result.Type == CommandType.HoldPosition))
            {
                _tacticalFeedback.ShowNonPositionalCommandMarkerNotice(result.Type);
            }
        }

        private CommandContext BuildCommandContext()
        {
            Vec3 fallback = Vec3.Zero;
            if (_controller.HasSeededPose)
            {
                fallback = _controller.GetPose().Position;
            }
            else if (Mission?.MainAgent != null)
            {
                fallback = Mission.MainAgent.Position;
            }

            return new CommandContext(
                Mission,
                _formationSelection.SelectedFormation,
                _state.Enabled,
                fallback,
                _currentGroundTarget);
        }

        private void UpdateGroundTargetSample()
        {
            if (Mission == null || !_controller.HasSeededPose)
            {
                return;
            }

            if (_formationLogicTick % 4 != 0)
            {
                return;
            }

            RTSCameraPose pose = _controller.GetPose();
            GroundTargetResult r = _groundTargetResolver.TryResolveFromCamera(Mission, pose);
            _currentGroundTarget = r;

            if (r.Success)
            {
                if (!_lastGroundFeedbackWasSuccess)
                {
                    _tacticalFeedback.ShowGroundTargetResolved(r.Position, "sampler");
                }

                _lastGroundFeedbackWasSuccess = true;
            }
            else
            {
                _tacticalFeedback.ShowGroundTargetFailed(r.Message);
                _lastGroundFeedbackWasSuccess = false;
            }
        }

        private static CommandIntent BuildDebugIntent(CommandType type, CommandContext context)
        {
            Formation formation = context.SelectedFormation;
            switch (type)
            {
                case CommandType.HoldPosition:
                    return new CommandIntent
                    {
                        Type = CommandType.HoldPosition,
                        TargetFormation = formation,
                        Source = "debug-H",
                        RequiresPosition = false,
                        RequiresDirection = false
                    };
                case CommandType.Charge:
                    return new CommandIntent
                    {
                        Type = CommandType.Charge,
                        TargetFormation = formation,
                        Source = "debug-C",
                        RequiresPosition = false,
                        RequiresDirection = false
                    };
                case CommandType.MoveToPosition:
                    Vec3? pos = context.ResolvedGroundPosition;

                    return new CommandIntent
                    {
                        Type = CommandType.MoveToPosition,
                        TargetFormation = formation,
                        TargetPosition = pos,
                        Source = "debug-G",
                        RequiresPosition = true,
                        RequiresDirection = false,
                        FromResolvedGroundTarget = pos.HasValue
                    };
                default:
                    return new CommandIntent
                    {
                        Type = CommandType.None,
                        Source = "debug-unknown",
                        RequiresPosition = false,
                        RequiresDirection = false
                    };
            }
        }

        private static bool IsFiniteVec3(Vec3 v)
        {
            return !(float.IsNaN(v.x) || float.IsInfinity(v.x)
                || float.IsNaN(v.y) || float.IsInfinity(v.y)
                || float.IsNaN(v.z) || float.IsInfinity(v.z));
        }

        private void PublishFormationSelectionFeedback()
        {
            if (_formationSelection.TryGetSelectionDetails(out int idx, out int units, out string cls))
            {
                _tacticalFeedback.ShowFormationSelected(_formationSelection.SelectedFormation, idx, units, cls);
            }
            else
            {
                _tacticalFeedback.ShowFormationSelected(null, -1, 0, string.Empty);
            }
        }

        private void HandleFormationInputAndFocus(InputSnapshot snapshot)
        {
            TaleWorlds.MountAndBlade.Mission mission = Mission;
            Agent main = Mission?.MainAgent;
            IReadOnlyList<TaleWorlds.MountAndBlade.Formation> formations = _formationQuery.GetFriendlyFormations(mission, main, _formationLogicTick);
            _formationSelection.ClearIfInvalid(formations);

            if (snapshot.NextFormation || snapshot.PreviousFormation)
            {
                _tacticalFeedback.InvalidateFormationAnnouncement();
            }

            if (snapshot.NextFormation)
            {
                _formationSelection.NextFormation(formations);
            }

            if (snapshot.PreviousFormation)
            {
                _formationSelection.PreviousFormation(formations);
            }

            if (snapshot.FocusSelectedFormation)
            {
                TaleWorlds.MountAndBlade.Formation selected = _formationSelection.SelectedFormation;
                if (selected == null)
                {
                    _tacticalFeedback.ShowFocusResult(false, "no formation selected.");
                }
                else if (!FormationFocusController.TryGetFocusWorldPosition(mission, selected, out Vec3 focus))
                {
                    _tacticalFeedback.ShowFocusResult(false, "could not resolve position.");
                }
                else
                {
                    _controller.FocusAt(focus);
                    _tacticalFeedback.ShowFocusResult(true, "moved toward formation.");
                }
            }
        }

        private void TryMissionEndCleanup()
        {
            if (Mission == null || !Mission.MissionEnded || _missionEndCleanupDone)
            {
                return;
            }

            _missionEndCleanupDone = true;
            TaleWorlds.MountAndBlade.Mission mission = Mission;
            _nativeInputGuard.ExitRtsMode("mission ended");
            _state.ForceDisabled("mission ended");
            _formationSelection.Clear();
            _formationQuery.InvalidateCache();
            _tacticalFeedback.ResetSession();
            _commandMarkerService.Clear();
            _controller.Reset();
            _cameraBridge.TryRestore(this, mission);
            _tacticalFeedback.ShowWarning(
                "RTS camera released (battle ended).",
                throttleKey: "battle-end",
                cooldownSeconds: 5.0,
                forceImmediate: true);
        }
    }
}
