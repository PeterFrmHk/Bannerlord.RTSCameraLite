namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Outcome of commander presence detection for one formation (Slice 8).
    /// </summary>
    public sealed class CommanderPresenceResult
    {
        private CommanderPresenceResult(
            bool hasCommander,
            FormationCommander commander,
            string reason,
            bool isCertain)
        {
            HasCommander = hasCommander;
            Commander = commander;
            Reason = reason ?? string.Empty;
            IsCertain = isCertain;
        }

        public bool HasCommander { get; }

        public FormationCommander Commander { get; }

        public string Reason { get; }

        public bool IsCertain { get; }

        public static CommanderPresenceResult Found(FormationCommander commander, bool isCertain, string reason)
        {
            return new CommanderPresenceResult(true, commander, reason, isCertain);
        }

        public static CommanderPresenceResult Missing(string reason)
        {
            return new CommanderPresenceResult(false, null, reason, isCertain: true);
        }

        public static CommanderPresenceResult Uncertain(string reason, FormationCommander commander = null)
        {
            return new CommanderPresenceResult(commander != null, commander, reason, isCertain: false);
        }
    }
}
