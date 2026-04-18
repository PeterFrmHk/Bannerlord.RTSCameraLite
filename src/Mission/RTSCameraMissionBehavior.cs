using Bannerlord.RTSCameraLite.Camera;
using Bannerlord.RTSCameraLite.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Bannerlord.RTSCameraLite.Mission
{
    public sealed class RTSCameraMissionBehavior : MissionView
    {
        private readonly RTSCameraState _state = new RTSCameraState();
        private readonly RTSCameraController _controller = new RTSCameraController();
        private bool _loggedBridgeForCurrentEnable;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            ModLogger.SafeStartupLog("RTS camera mission behavior attached.");
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            HandleF10Toggle();
            UpdateRtsCameraBridge();
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
                if (Mission?.MainAgent != null)
                {
                    _controller.InitializeFromAgent(Mission.MainAgent);
                }

                _loggedBridgeForCurrentEnable = false;
            }
            else
            {
                _controller.Reset();
                _loggedBridgeForCurrentEnable = false;
            }

            string status = _state.Enabled ? "enabled" : "disabled";
            ModLogger.SafeStartupLog($"RTS Camera {status}. Toggle count: {_state.ToggleCount}");
        }

        private void UpdateRtsCameraBridge()
        {
            if (!_state.Enabled)
            {
                return;
            }

            if (!_controller.IsInitialized && Mission?.MainAgent != null)
            {
                _controller.InitializeFromAgent(Mission.MainAgent);
            }

            if (!_controller.IsInitialized)
            {
                return;
            }

            CameraBridgeResult result = CameraBridge.TryApply(Mission, _controller.GetPose());

            if (!_loggedBridgeForCurrentEnable)
            {
                ModLogger.Info(result.Message);
                _loggedBridgeForCurrentEnable = true;
            }
        }
    }
}
