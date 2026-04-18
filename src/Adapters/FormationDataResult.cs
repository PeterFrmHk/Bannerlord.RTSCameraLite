using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>Coarse formation classification for anchor offsets (Slice 9).</summary>
    public enum FormationRoleKind
    {
        Unknown = 0,
        Infantry = 1,
        Archer = 2,
        Cavalry = 3,
        Skirmisher = 4,
        ShieldWall = 5
    }

    /// <summary>
    /// Unified read result for <see cref="FormationDataAdapter"/> (Slice 3+). Extra fields are optional by scenario.
    /// </summary>
    public readonly struct FormationDataResult
    {
        public FormationDataResult(
            bool success,
            string message,
            Vec3 vec3 = default,
            float floatValue = 0f,
            IReadOnlyList<Agent> agents = null,
            bool commanderLikely = false,
            float floatValueB = 0f,
            FormationRoleKind roleKind = FormationRoleKind.Unknown)
        {
            Success = success;
            Message = message ?? string.Empty;
            Vec3 = vec3;
            FloatValue = floatValue;
            Agents = agents ?? (IReadOnlyList<Agent>)Array.Empty<Agent>();
            CommanderLikely = commanderLikely;
            FloatValueB = floatValueB;
            RoleKind = roleKind;
        }

        public bool Success { get; }

        public string Message { get; }

        public Vec3 Vec3 { get; }

        /// <summary>Semantic depends on query (e.g. mounted ratio 0..1).</summary>
        public float FloatValue { get; }

        /// <summary>Secondary scalar (e.g. paired skill read).</summary>
        public float FloatValueB { get; }

        public IReadOnlyList<Agent> Agents { get; }

        public bool CommanderLikely { get; }

        public FormationRoleKind RoleKind { get; }

        public static FormationDataResult Failure(string message)
        {
            return new FormationDataResult(false, message);
        }
    }
}
