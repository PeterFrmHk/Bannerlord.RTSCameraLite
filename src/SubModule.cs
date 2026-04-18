using Bannerlord.RTSCameraLite.Core;

using Bannerlord.RTSCameraLite.Mission;

using TaleWorlds.MountAndBlade;



namespace Bannerlord.RTSCameraLite

{

    /// <summary>

    /// Module entry: Slice 2 registers the commander mission shell on supported battles only.

    /// </summary>

    public sealed class SubModule : MBSubModuleBase

    {

        protected override void OnSubModuleLoad()

        {

            base.OnSubModuleLoad();



            ModLogger.LogDebug(

                $"{ModConstants.ModuleId} loaded. Version {ModConstants.Version} ({ModConstants.LegacyShortName} codebase)."

            );

        }



        protected override void OnBeforeInitialModuleScreenSetAsRoot()

        {

            base.OnBeforeInitialModuleScreenSetAsRoot();



            ModLogger.MarkUiReady();



            ModLogger.LogDebug($"{ModConstants.ModuleId} UI ready.");



            ModLogger.SafeStartupLog(

                $"{ModConstants.DisplayName} v{ModConstants.Version} — foundation active."

            );

        }



        public override void OnMissionBehaviorInitialize(TaleWorlds.MountAndBlade.Mission mission)

        {

            base.OnMissionBehaviorInitialize(mission);



            if (!CommanderMissionModeGate.IsSupportedMission(mission))

            {

                return;

            }



            mission.AddMissionBehavior(new CommanderMissionView());

        }

    }

}

