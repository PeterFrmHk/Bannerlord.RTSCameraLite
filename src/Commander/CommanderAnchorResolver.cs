using System;
using Bannerlord.RTSCameraLite.Adapters;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Computes a preferred commander anchor behind a formation (Slice 9 — no movement, no orders).
    /// </summary>
    public sealed class CommanderAnchorResolver
    {
        private readonly FormationDataAdapter _adapter;

        public CommanderAnchorResolver(FormationDataAdapter adapter)
        {
            _adapter = adapter ?? new FormationDataAdapter();
        }

        public CommanderAnchorState ResolveAnchor(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation formation,
            CommanderPresenceResult commander,
            CommanderAnchorSettings settings)
        {
            _ = mission;
            CommanderAnchorSettings s = settings ?? CommanderAnchorSettings.FromConfig(null);

            if (formation == null)
            {
                return CommanderAnchorState.None("formation is null");
            }

            if (commander == null || !commander.HasCommander || commander.Commander?.CommanderAgent == null)
            {
                return CommanderAnchorState.None(
                    "no commander for formation",
                    isCertain: commander?.IsCertain ?? true);
            }

            try
            {
                FormationDataResult centerResult = _adapter.TryGetFormationCenter(formation);
                if (!centerResult.Success)
                {
                    return CommanderAnchorState.None(
                        string.IsNullOrEmpty(centerResult.Message) ? "formation center unavailable" : centerResult.Message);
                }

                FormationDataResult facingResult = _adapter.TryGetFormationFacing(formation);
                Vec2 forward;
                bool facingOk = facingResult.Success
                    && (facingResult.Vec3.x * facingResult.Vec3.x + facingResult.Vec3.y * facingResult.Vec3.y) > 1e-8f;
                if (facingOk)
                {
                    forward = new Vec2(facingResult.Vec3.x, facingResult.Vec3.y);
                }
                else
                {
                    forward = new Vec2(0f, 1f);
                }

                FormationDataResult roleResult = _adapter.TryDetectFormationRole(formation);
                float backOffset = SelectBackOffset(s, roleResult.RoleKind);
                Vec3 center = centerResult.Vec3;
                Vec3 preferred = new Vec3(
                    center.x - forward.x * backOffset,
                    center.y - forward.y * backOffset,
                    center.z);

                FormationDataResult agentPos = _adapter.TryGetAgentPosition(commander.Commander.CommanderAgent);
                if (!agentPos.Success)
                {
                    return CommanderAnchorState.None(
                        string.IsNullOrEmpty(agentPos.Message) ? "commander position unavailable" : agentPos.Message);
                }

                float dist = PlanarDistance(agentPos.Vec3, preferred);
                float radius = Math.Max(0.1f, s.AnchorAllowedRadius);
                bool inside = dist <= radius + 1e-3f;

                bool isCertain = centerResult.Success
                    && facingOk
                    && commander.IsCertain
                    && roleResult.Success
                    && roleResult.RoleKind != FormationRoleKind.Unknown;

                return new CommanderAnchorState(
                    true,
                    preferred,
                    forward,
                    radius,
                    inside,
                    dist,
                    "anchor resolved",
                    isCertain);
            }
            catch (Exception ex)
            {
                return CommanderAnchorState.None("ResolveAnchor threw: " + ex.Message, isCertain: false);
            }
        }

        private static float SelectBackOffset(CommanderAnchorSettings s, FormationRoleKind role)
        {
            switch (role)
            {
                case FormationRoleKind.ShieldWall:
                    return s.ShieldWallCommanderBackOffset;
                case FormationRoleKind.Archer:
                    return s.ArcherCommanderBackOffset;
                case FormationRoleKind.Cavalry:
                    return s.CavalryCommanderBackOffset;
                case FormationRoleKind.Skirmisher:
                    return s.SkirmisherCommanderBackOffset;
                default:
                    return s.DefaultCommanderBackOffset;
            }
        }

        private static float PlanarDistance(Vec3 a, Vec3 b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            return (float)Math.Sqrt((dx * dx) + (dy * dy));
        }
    }
}
