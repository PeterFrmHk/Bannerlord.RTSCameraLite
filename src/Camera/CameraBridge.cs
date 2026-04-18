using System;
using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Bannerlord.RTSCameraLite.Camera
{
    internal sealed class CameraBridge
    {
        private static readonly string[] RestoreMethodNames =
        {
            "ActivateMainAgentCamera",
            "ResetCameraMode",
            "SwitchToDefaultCameraMode",
            "ActivateMainAgentSpectatorCamera"
        };

        public CameraBridgeResult TryApply(TaleWorlds.MountAndBlade.Mission mission, RTSCameraPose pose)
        {
            if (mission == null || pose == null)
            {
                return new CameraBridgeResult(false, "Mission or pose missing");
            }

            return new CameraBridgeResult(false, "Use TryApply(MissionView missionView, TaleWorlds.MountAndBlade.Mission mission, RTSCameraPose pose, float dt) overload.");
        }

        public CameraBridgeResult TryApply(MissionView missionView, TaleWorlds.MountAndBlade.Mission mission, RTSCameraPose pose, float dt)
        {
            _ = dt;

            if (missionView == null || mission == null || pose == null)
            {
                return new CameraBridgeResult(false, "MissionView, mission, or pose missing");
            }

            object missionScreen = missionView.MissionScreen;
            if (missionScreen == null)
            {
                return new CameraBridgeResult(false, "MissionScreen was null");
            }

            object camera = ResolveCameraInstance(missionScreen);
            if (camera == null)
            {
                return new CameraBridgeResult(false, "MissionCamera/CombatCamera not resolved on MissionScreen");
            }

            MatrixFrame frame = BuildCameraFrame(pose);
            string applied = TryApplyCameraFrame(camera, frame);
            if (applied != null)
            {
                return new CameraBridgeResult(true, applied);
            }

            return new CameraBridgeResult(false, "No SetCameraFrame/SetFrame/Frame setter matched on " + camera.GetType().FullName);
        }

        public CameraBridgeResult TryRestore(MissionView missionView, TaleWorlds.MountAndBlade.Mission mission)
        {
            if (mission == null)
            {
                return new CameraBridgeResult(false, "Mission missing");
            }

            if (missionView == null)
            {
                return new CameraBridgeResult(false, "MissionView missing");
            }

            object missionScreen = missionView.MissionScreen;
            if (missionScreen == null)
            {
                return new CameraBridgeResult(false, "MissionScreen was null");
            }

            Type screenType = missionScreen.GetType();
            foreach (string name in RestoreMethodNames)
            {
                MethodInfo method = screenType.GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (method == null)
                {
                    continue;
                }

                method.Invoke(missionScreen, null);
                return new CameraBridgeResult(true, "TryRestore invoked " + name);
            }

            return new CameraBridgeResult(false, "No restore method invoked from known MissionScreen names");
        }

        private static object ResolveCameraInstance(object missionScreen)
        {
            Type type = missionScreen.GetType();
            object missionCamera = GetPublicPropertyValue(missionScreen, type, "MissionCamera");
            if (missionCamera != null)
            {
                return missionCamera;
            }

            return GetPublicPropertyValue(missionScreen, type, "CombatCamera");
        }

        private static object GetPublicPropertyValue(object target, Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(target);
        }

        private static MatrixFrame BuildCameraFrame(RTSCameraPose pose)
        {
            float pitchRadians = pose.Pitch * ((float)Math.PI / 180f);
            Mat3 rotation = BuildCameraOrientation(pose.Yaw, pitchRadians);
            return new MatrixFrame(rotation, pose.Position);
        }

        private static Mat3 BuildCameraOrientation(float yaw, float pitchRadians)
        {
            // TaleWorlds.Library Mat3 on 1.2.x reference builds does not ship Mat3.CreateRotationZ/CreateRotationX;
            // compose the same Z-then-side (pitch) intent with RotateAboutUp + RotateAboutSide.
            Mat3 rotation = Mat3.Identity;
            rotation.RotateAboutUp(yaw);
            rotation.RotateAboutSide(pitchRadians);
            return rotation;
        }

        private static string TryApplyCameraFrame(object camera, MatrixFrame frame)
        {
            Type type = camera.GetType();
            Type matrixFrame = typeof(MatrixFrame);

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.Name != "SetCameraFrame" && method.Name != "SetFrame")
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 1)
                {
                    continue;
                }

                Type parameterType = parameters[0].ParameterType;
                if (parameterType != matrixFrame && parameterType != matrixFrame.MakeByRefType())
                {
                    continue;
                }

                object[] args = new object[] { frame };
                method.Invoke(camera, args);
                return method.Name + " applied";
            }

            PropertyInfo frameProperty = type.GetProperty("Frame", BindingFlags.Public | BindingFlags.Instance);
            if (frameProperty != null && frameProperty.CanWrite)
            {
                frameProperty.SetValue(camera, frame, null);
                return "Frame property set";
            }

            return null;
        }
    }
}
