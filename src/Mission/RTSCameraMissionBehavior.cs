using Bannerlord.RTSCameraLite.Camera;
using Bannerlord.RTSCameraLite.Core;
using Bannerlord.RTSCameraLite.Input;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Bannerlord.RTSCameraLite.Mission
{
    public sealed class RTSCameraMissionBehavior : MissionView
    {
        private readonly RTSCameraState _state = new RTSCameraState();
        private readonly RTSCameraController _controller = new RTSCameraController();
        private readonly CameraBridge _cameraBridge = new CameraBridge();
        private bool _loggedBridgeFailureOnce;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            ModLogger.SafeStartupLog("RTS camera mission behavior attached.");
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            HandleF10Toggle();
            UpdateRtsCameraBridge(dt);
        }

        public override bool UpdateOverridenCamera(float dt)
        {
            if (!_state.Enabled || !_controller.HasSeededPose)
            {
                return false;
            }

            TaleWorlds.MountAndBlade.Mission mission = Mission;
            CameraBridgeResult result = _cameraBridge.TryApply(this, mission, _controller.GetPose(), dt);
            if (!result.Applied && !_loggedBridgeFailureOnce)
            {
                ModLogger.Info(result.Message);
                _loggedBridgeFailureOnce = true;
            }

            return result.Applied;
        }

        public override void OnRemoveBehavior()
        {
            TaleWorlds.MountAndBlade.Mission mission = Mission;
            _cameraBridge.TryRestore(this, mission);
            base.OnRemoveBehavior();
        }

        private void HandleF10Toggle()
        {
            if (Input == null)
            {
                return;
            }

            if (!Input.IsKeyReleased(InputKey.F10))
            {
                return;
            }

            _state.Toggle("F10 pressed");

            if (_state.Enabled)
            {
                _controller.Reset();
                _loggedBridgeFailureOnce = false;
            }
            else
            {
                _controller.Reset();
                TaleWorlds.MountAndBlade.Mission mission = Mission;
                _cameraBridge.TryRestore(this, mission);
                _loggedBridgeFailureOnce = false;
            }

            string status = _state.Enabled ? "enabled" : "disabled";
            ModLogger.SafeStartupLog($"RTS Camera {status}. Toggle count: {_state.ToggleCount}");
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

            InputSnapshot snapshot = RTSCameraInput.Read(Input);
            _controller.Tick(snapshot, dt);
        }
    }
}
