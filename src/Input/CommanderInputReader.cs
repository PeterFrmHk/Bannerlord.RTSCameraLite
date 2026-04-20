using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Core;
using TaleWorlds.InputSystem;

namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// Centralized key reads for RTS Commander Mode. Key names come from JSON and are parsed defensively (Slice 6).
    /// </summary>
    public sealed class CommanderInputReader
    {
        private InputKey _modeActivationKey = InputKey.BackSpace;
        private InputKey _debugFallbackToggleKey = InputKey.F10;
        private bool _debugFallbackEnabled = true;
        private InputKey _moveForwardKey = InputKey.W;
        private InputKey _moveBackKey = InputKey.S;
        private InputKey _moveLeftKey = InputKey.A;
        private InputKey _moveRightKey = InputKey.D;
        private InputKey _rotateLeftKey = InputKey.Q;
        private InputKey _rotateRightKey = InputKey.E;
        private InputKey _fastMoveKey = InputKey.LeftShift;
        private InputKey _zoomInKey = InputKey.R;
        private InputKey _zoomOutKey = InputKey.F;

        private InputKey _diagnosticsToggleKey = InputKey.F9;

        public InputKey ModeActivationKey => _modeActivationKey;

        public InputKey DebugFallbackToggleKey => _debugFallbackToggleKey;

        public bool DebugFallbackEnabled => _debugFallbackEnabled;

        public InputKey MoveForwardKey => _moveForwardKey;

        public InputKey MoveBackKey => _moveBackKey;

        public InputKey MoveLeftKey => _moveLeftKey;

        public InputKey MoveRightKey => _moveRightKey;

        public InputKey RotateLeftKey => _rotateLeftKey;

        public InputKey RotateRightKey => _rotateRightKey;

        public InputKey FastMoveKey => _fastMoveKey;

        public InputKey ZoomInKey => _zoomInKey;

        public InputKey ZoomOutKey => _zoomOutKey;

        /// <summary>
        /// Applies JSON config strings; invalid names fall back to <see cref="CommanderConfigDefaults"/> key names.
        /// </summary>
        public void ApplyConfig(CommanderConfig config)
        {
            CommanderConfig d = CommanderConfigDefaults.CreateDefault();

            _modeActivationKey = ParseBinding(config?.ModeActivationKey, d.ModeActivationKey, InputKey.BackSpace);
            _debugFallbackEnabled = config?.EnableDebugFallbackToggle ?? d.EnableDebugFallbackToggle;
            _debugFallbackToggleKey = ParseBinding(config?.DebugFallbackToggleKey, d.DebugFallbackToggleKey, InputKey.F10);

            _moveForwardKey = ParseBinding(config?.MoveForwardKey, d.MoveForwardKey, InputKey.W);
            _moveBackKey = ParseBinding(config?.MoveBackKey, d.MoveBackKey, InputKey.S);
            _moveLeftKey = ParseBinding(config?.MoveLeftKey, d.MoveLeftKey, InputKey.A);
            _moveRightKey = ParseBinding(config?.MoveRightKey, d.MoveRightKey, InputKey.D);
            _rotateLeftKey = ParseBinding(config?.RotateLeftKey, d.RotateLeftKey, InputKey.Q);
            _rotateRightKey = ParseBinding(config?.RotateRightKey, d.RotateRightKey, InputKey.E);
            _fastMoveKey = ParseBinding(config?.FastMoveKey, d.FastMoveKey, InputKey.LeftShift);
            _zoomInKey = ParseBinding(config?.ZoomInKey, d.ZoomInKey, InputKey.R);
            _zoomOutKey = ParseBinding(config?.ZoomOutKey, d.ZoomOutKey, InputKey.F);
            _diagnosticsToggleKey = ParseBinding(config?.DiagnosticsToggleKey, d.DiagnosticsToggleKey, InputKey.F9);

            if (_debugFallbackEnabled && _debugFallbackToggleKey == _modeActivationKey)
            {
                ModLogger.LogWarningOnce(
                    "commander_debug_toggle_shadows_activation",
                    $"{ModConstants.ModuleId}: debug fallback toggle key matches mode activation key; disabling fallback to avoid ownership conflict.");
                _debugFallbackEnabled = false;
            }
        }

        private static InputKey ParseBinding(string candidate, string defaultName, InputKey hardFallback)
        {
            if (CommanderInputKeyParser.TryParse(candidate, out InputKey parsed))
            {
                return parsed;
            }

            if (CommanderInputKeyParser.TryParse(defaultName, out InputKey fromDefault))
            {
                return fromDefault;
            }

            return hardFallback;
        }

        public bool TryConsumeCommanderModeToggle(IInputContext input)
        {
            if (input == null)
            {
                return false;
            }

            try
            {
                return input.IsKeyReleased(_modeActivationKey);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// DEBUG / DEVELOPMENT ONLY — optional emergency toggle when the primary activation key is unavailable.
        /// </summary>
        public bool TryConsumeEmergencyDebugCommanderToggle(IInputContext input)
        {
            if (!_debugFallbackEnabled || input == null)
            {
                return false;
            }

            try
            {
                return input.IsKeyReleased(_debugFallbackToggleKey);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Slice 20 — portfolio diagnostics text toggle (configurable, default F9).</summary>
        public bool TryConsumeDiagnosticsToggle(IInputContext input)
        {
            if (input == null)
            {
                return false;
            }

            try
            {
                return input.IsKeyReleased(_diagnosticsToggleKey);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// TW-1 number-key selection input. Returns slots 1-8 only; no order input is read here.
        /// </summary>
        public bool TryConsumeFormationSelectionSlot(IInputContext input, out int slot)
        {
            slot = 0;
            if (input == null)
            {
                return false;
            }

            try
            {
                if (input.IsKeyReleased(InputKey.D1) || input.IsKeyReleased(InputKey.Numpad1)) { slot = 1; return true; }
                if (input.IsKeyReleased(InputKey.D2) || input.IsKeyReleased(InputKey.Numpad2)) { slot = 2; return true; }
                if (input.IsKeyReleased(InputKey.D3) || input.IsKeyReleased(InputKey.Numpad3)) { slot = 3; return true; }
                if (input.IsKeyReleased(InputKey.D4) || input.IsKeyReleased(InputKey.Numpad4)) { slot = 4; return true; }
                if (input.IsKeyReleased(InputKey.D5) || input.IsKeyReleased(InputKey.Numpad5)) { slot = 5; return true; }
                if (input.IsKeyReleased(InputKey.D6) || input.IsKeyReleased(InputKey.Numpad6)) { slot = 6; return true; }
                if (input.IsKeyReleased(InputKey.D7) || input.IsKeyReleased(InputKey.Numpad7)) { slot = 7; return true; }
                if (input.IsKeyReleased(InputKey.D8) || input.IsKeyReleased(InputKey.Numpad8)) { slot = 8; return true; }
            }
            catch
            {
                slot = 0;
                return false;
            }

            return false;
        }

        /// <summary>
        /// Reads held-state camera movement keys for the current tick. All <see cref="IInputContext"/> polling for
        /// these bindings stays in this class (Slice 4).
        /// </summary>
        public CommanderInputSnapshot ReadCameraSnapshot(IInputContext input)
        {
            if (input == null)
            {
                return default;
            }

            try
            {
                bool fast =
                    IsKeyDown(input, InputKey.LeftShift)
                    || IsKeyDown(input, InputKey.RightShift)
                    || IsKeyDown(input, _fastMoveKey);

                float zoom = 0f;
                if (IsKeyDown(input, _zoomInKey))
                {
                    zoom -= 1f;
                }

                if (IsKeyDown(input, _zoomOutKey))
                {
                    zoom += 1f;
                }

                return new CommanderInputSnapshot(
                    IsKeyDown(input, _moveForwardKey),
                    IsKeyDown(input, _moveBackKey),
                    IsKeyDown(input, _moveLeftKey),
                    IsKeyDown(input, _moveRightKey),
                    IsKeyDown(input, _rotateLeftKey),
                    IsKeyDown(input, _rotateRightKey),
                    fast,
                    zoom);
            }
            catch
            {
                return default;
            }
        }

        private static bool IsKeyDown(IInputContext input, InputKey key)
        {
            try
            {
                return input.IsKeyDown(key);
            }
            catch
            {
                return false;
            }
        }
    }
}
