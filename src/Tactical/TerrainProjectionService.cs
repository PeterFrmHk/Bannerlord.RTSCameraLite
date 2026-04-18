using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Tactical
{
    /// <summary>
    /// Projects a horizontal forward point from the RTS camera pose and optionally snaps Z using scene terrain APIs.
    /// All engine access is defensive; failures fall back to camera height (flat projection).
    /// </summary>
    internal sealed class TerrainProjectionService
    {
        internal const float DefaultForwardDistance = 42f;

        /// <summary>
        /// Computes a world point in front of the camera on the XZ plane, then refines Z when a public scene API is available.
        /// </summary>
        public GroundTargetResult TryProjectCameraForwardGround(
            TaleWorlds.MountAndBlade.Mission mission,
            Vec3 cameraWorld,
            float yawRadians,
            float pitchDegrees,
            float forwardDistance)
        {
            try
            {
                if (!IsFiniteVec3(cameraWorld))
                {
                    return GroundTargetResult.Failure("Camera position is not finite.");
                }

                if (forwardDistance <= 0f || float.IsNaN(forwardDistance) || float.IsInfinity(forwardDistance))
                {
                    forwardDistance = DefaultForwardDistance;
                }

                float sinY = (float)Math.Sin(yawRadians);
                float cosY = (float)Math.Cos(yawRadians);
                float x = cameraWorld.x + sinY * forwardDistance;
                float y = cameraWorld.y + cosY * forwardDistance;
                float z = cameraWorld.z;

                float? terrainZ = TryGetTerrainHeightSafe(mission, x, y, z);
                if (terrainZ.HasValue && IsFiniteFloat(terrainZ.Value))
                {
                    z = terrainZ.Value;
                }

                Vec3 ground = new Vec3(x, y, z);
                if (!IsFiniteVec3(ground))
                {
                    return GroundTargetResult.Failure("Projected ground position is not finite.");
                }

                return GroundTargetResult.SuccessAt(ground);
            }
            catch (Exception ex)
            {
                return GroundTargetResult.Failure($"Terrain projection failed: {ex.Message}");
            }
        }

        private static float? TryGetTerrainHeightSafe(TaleWorlds.MountAndBlade.Mission mission, float x, float y, float fallbackZ)
        {
            if (mission == null)
            {
                return null;
            }

            try
            {
                object scene = mission.Scene;
                if (scene == null)
                {
                    return null;
                }

                Vec2 xy = new Vec2(x, y);

                // Reference assemblies vary; try common public shapes without Harmony.
                // ILSpy (TaleWorlds.Engine.Scene): GetHeightAtPosition / GetGroundHeightAtPosition naming differs by game build.
                System.Type sceneType = scene.GetType();
                System.Reflection.MethodInfo getHeight = sceneType.GetMethod(
                    "GetHeightAtPosition",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
                    null,
                    new[] { typeof(Vec2) },
                    null);
                if (getHeight != null && getHeight.ReturnType == typeof(float))
                {
                    object hz = getHeight.Invoke(scene, new object[] { xy });
                    if (hz is float f && IsFiniteFloat(f))
                    {
                        return f;
                    }
                }

                System.Reflection.MethodInfo getGround = sceneType.GetMethod(
                    "GetGroundHeightAtPosition",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
                    null,
                    new[] { typeof(Vec2) },
                    null);
                if (getGround != null && getGround.ReturnType == typeof(float))
                {
                    object hz = getGround.Invoke(scene, new object[] { xy });
                    if (hz is float f && IsFiniteFloat(f))
                    {
                        return f;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static bool IsFiniteVec3(Vec3 v)
        {
            return IsFiniteFloat(v.x) && IsFiniteFloat(v.y) && IsFiniteFloat(v.z);
        }

        private static bool IsFiniteFloat(float f)
        {
            return !(float.IsNaN(f) || float.IsInfinity(f));
        }
    }
}
