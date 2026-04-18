using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Holds commander camera pose state. Slice 3: no movement/delta logic — seeding only.
    /// </summary>
    public sealed class CommanderCameraController
    {
        private CommanderCameraPose _pose;
        private bool _hasPose;

        public bool HasPose => _hasPose;

        public void InitializeFromMission(TaleWorlds.MountAndBlade.Mission mission)
        {
            Reset();
            if (mission?.MainAgent != null)
            {
                InitializeFromAgent(mission.MainAgent);
                return;
            }

            _pose = default;
            _hasPose = false;
        }

        public void InitializeFromAgent(Agent agent)
        {
            if (agent == null)
            {
                _hasPose = false;
                return;
            }

            Vec3 p = agent.Position;
            _pose = new CommanderCameraPose
            {
                Position = p,
                Yaw = 0f,
                Pitch = 0f,
                Height = p.z
            };
            _hasPose = true;
        }

        public CommanderCameraPose GetPose()
        {
            return _pose;
        }

        public void Reset()
        {
            _pose = default;
            _hasPose = false;
        }
    }
}
