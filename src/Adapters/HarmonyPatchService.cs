using System;
using Bannerlord.RTSCameraLite.Core;
using HarmonyLib;

namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Opt-in Harmony holder: no patches are registered in scaffold v1; runtime 0Harmony is supplied by the Bannerlord.Harmony module.
    /// </summary>
    public sealed class HarmonyPatchService
    {
        public const string HarmonyInstanceId = "Bannerlord.RTSCameraLite";

        private Harmony _harmony;
        private bool _applyCompleted;

        public bool IsApplied { get; private set; }

        public bool HasFailed { get; private set; }

        public string LastMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Creates the Harmony instance and marks applied; does not register patches (future slices).
        /// Idempotent: subsequent calls are no-ops after the first completion (success or failure).
        /// </summary>
        public void ApplyPatches(bool enableDiagnostics)
        {
            if (_applyCompleted)
            {
                return;
            }

            try
            {
                _harmony = new Harmony(HarmonyInstanceId);
                if (enableDiagnostics)
                {
                    ModLogger.LogDebug($"{ModConstants.ModuleId}: Harmony scaffold active (Harmony id={HarmonyInstanceId}); zero patches registered.");
                }

                IsApplied = true;
                HasFailed = false;
                LastMessage = "Harmony scaffold initialized; no patches.";
                _applyCompleted = true;
            }
            catch (Exception ex)
            {
                HasFailed = true;
                IsApplied = false;
                LastMessage = ex.Message;
                _applyCompleted = true;
                ModLogger.Warn($"{ModConstants.ModuleId}: HarmonyPatchService.ApplyPatches failed (fail-closed): {ex}");
            }
        }

        /// <summary>
        /// Removes patches for this Harmony id if any were applied. Safe to call multiple times.
        /// </summary>
        public void UnpatchAll()
        {
            if (_harmony == null)
            {
                return;
            }

            try
            {
                _harmony.UnpatchAll(HarmonyInstanceId);
            }
            catch (Exception ex)
            {
                ModLogger.Warn($"{ModConstants.ModuleId}: HarmonyPatchService.UnpatchAll guarded failure ({ex.Message})");
            }
            finally
            {
                _harmony = null;
                IsApplied = false;
                _applyCompleted = false;
                HasFailed = false;
            }
        }
    }
}
