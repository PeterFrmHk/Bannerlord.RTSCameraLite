using System;
using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Adapters;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Read-only commander presence for a formation (Slice 8). All engine-sensitive reads go through <see cref="FormationDataAdapter"/>.
    /// </summary>
    public sealed class CommanderAssignmentService
    {
        private readonly FormationDataAdapter _adapter;
        private CommanderDetectionSettings _settings = CommanderDetectionSettings.FromConfig(null);

        public CommanderAssignmentService()
            : this(new FormationDataAdapter())
        {
        }

        public CommanderAssignmentService(FormationDataAdapter adapter)
        {
            _adapter = adapter ?? new FormationDataAdapter();
        }

        public void ApplyDetectionSettings(CommanderDetectionSettings settings)
        {
            _settings = settings ?? CommanderDetectionSettings.FromConfig(null);
        }

        public CommanderDetectionSettings DetectionSettings => _settings;

        public CommanderPresenceResult DetectCommander(TaleWorlds.MountAndBlade.Mission mission, Formation formation)
        {
            TaleWorlds.MountAndBlade.Mission resolved = mission ?? ResolveMission(formation);
            return DetectCommanderCore(resolved, formation);
        }

        public CommanderPresenceResult DetectCommanderForFormation(Formation formation)
        {
            return DetectCommander(ResolveMission(formation), formation);
        }

        private static TaleWorlds.MountAndBlade.Mission ResolveMission(Formation formation)
        {
            if (formation == null)
            {
                return null;
            }

            try
            {
                Agent captain = formation.Captain;
                if (captain?.Mission != null)
                {
                    return captain.Mission;
                }

                TaleWorlds.MountAndBlade.Mission found = null;
                formation.ApplyActionOnEachUnit(a =>
                {
                    if (found == null && a?.Mission != null)
                    {
                        found = a.Mission;
                    }
                });

                if (found != null)
                {
                    return found;
                }
            }
            catch
            {
                // Fall through to Mission.Current.
            }

            try
            {
                return TaleWorlds.MountAndBlade.Mission.Current;
            }
            catch
            {
                return null;
            }
        }

        private CommanderPresenceResult DetectCommanderCore(TaleWorlds.MountAndBlade.Mission mission, Formation formation)
        {
            if (formation == null)
            {
                return CommanderPresenceResult.Missing("formation is null");
            }

            if (formation.CountOfUnits <= 0)
            {
                return CommanderPresenceResult.Missing("formation has no units");
            }

            FormationDataResult agentsResult = _adapter.TryGetFormationAgents(formation);
            if (!agentsResult.Success || agentsResult.Agents.Count == 0)
            {
                return CommanderPresenceResult.Missing(agentsResult.Message.Length > 0 ? agentsResult.Message : "no agents enumerated");
            }

            bool skillsCertain = true;
            float minScore = _settings.MinimumCommandAuthorityScore;

            // A — explicit formation captain (engine slot).
            FormationDataResult captainSlot = _adapter.TryGetFormationCaptain(formation);
            if (captainSlot.Success && captainSlot.Agents.Count > 0)
            {
                Agent captain = captainSlot.Agents[0];
                if (IsAgentViableCommanderBody(captain))
                {
                    bool hero = IsHeroViaAdapter(captain);
                    bool skipCaptainSlot = _settings.RequireHeroCommanderForAdvancedFormations && !hero;
                    if (!skipCaptainSlot && (_settings.AllowCaptainCommander || hero))
                    {
                        FormationCommander built = BuildCommander(
                            captain,
                            isCaptain: true,
                            source: "Formation.Captain",
                            formation,
                            ref skillsCertain);
                        if (built.CommandAuthorityScore >= minScore)
                        {
                            return CommanderPresenceResult.Found(
                                built,
                                isCertain: skillsCertain,
                                reason: "Engine formation captain");
                        }
                    }
                }
            }

            // B — hero in formation.
            FormationDataResult heroes = _adapter.TryGetHeroAgents(formation);
            if (heroes.Success && heroes.Agents.Count > 0)
            {
                Agent heroPick = PickPreferredHero(mission, heroes.Agents);
                if (heroPick != null && IsAgentViableCommanderBody(heroPick))
                {
                    FormationCommander built = BuildCommander(
                        heroPick,
                        isCaptain: IsCaptainAgent(formation, heroPick),
                        source: "Formation.Hero",
                        formation,
                        ref skillsCertain);
                    if (built.CommandAuthorityScore >= minScore)
                    {
                        return CommanderPresenceResult.Found(
                            built,
                            isCertain: skillsCertain,
                            reason: "Hero agent in formation");
                    }
                }
            }

            // C — captain-like (can lead formations remotely).
            foreach (Agent agent in agentsResult.Agents)
            {
                if (agent == null || !IsAgentViableCommanderBody(agent))
                {
                    continue;
                }

                if (!agent.CanLeadFormationsRemotely)
                {
                    continue;
                }

                if (!_settings.AllowCaptainCommander && !IsHeroViaAdapter(agent))
                {
                    continue;
                }

                if (_settings.RequireHeroCommanderForAdvancedFormations && !IsHeroViaAdapter(agent))
                {
                    continue;
                }

                FormationCommander built = BuildCommander(
                    agent,
                    isCaptain: IsCaptainAgent(formation, agent),
                    source: "CanLeadFormationsRemotely",
                    formation,
                    ref skillsCertain);
                if (built.CommandAuthorityScore >= minScore)
                {
                    return CommanderPresenceResult.Found(
                        built,
                        isCertain: skillsCertain,
                        reason: "Captain-like lead capability");
                }
            }

            // D — sergeant fallback (player sergeant in this formation).
            if (_settings.AllowSergeantFallback && mission != null)
            {
                Team team = GetTeamForFormation(mission, formation);
                if (team != null && team.IsPlayerSergeant && mission.MainAgent != null)
                {
                    Agent main = mission.MainAgent;
                    if (main.Formation == formation && IsAgentViableCommanderBody(main))
                    {
                        FormationCommander built = BuildCommander(
                            main,
                            isCaptain: IsCaptainAgent(formation, main),
                            source: "Sergeant.MainAgent",
                            formation,
                            ref skillsCertain);
                        if (built.CommandAuthorityScore >= minScore)
                        {
                            return CommanderPresenceResult.Found(
                                built,
                                isCertain: false,
                                reason: "Player sergeant fallback");
                        }
                    }
                }
            }

            // D — highest tier / level among living agents.
            if (_settings.AllowHighestTierFallback)
            {
                Agent best = null;
                float bestTier = -1f;
                foreach (Agent agent in agentsResult.Agents)
                {
                    if (agent == null || !IsAgentViableCommanderBody(agent))
                    {
                        continue;
                    }

                    FormationDataResult tierProbe = _adapter.TryGetAgentTierOrRank(agent);
                    float tier = tierProbe.Success ? tierProbe.FloatValue : 0f;
                    if (tier > bestTier)
                    {
                        bestTier = tier;
                        best = agent;
                    }
                }

                if (best != null)
                {
                    FormationCommander built = BuildCommander(
                        best,
                        isCaptain: IsCaptainAgent(formation, best),
                        source: "HighestTierFallback",
                        formation,
                        ref skillsCertain);
                    if (built.CommandAuthorityScore >= minScore)
                    {
                        return CommanderPresenceResult.Found(
                            built,
                            isCertain: false,
                            reason: "Highest tier fallback");
                    }
                }
            }

            // Optional: team general / leader not guaranteed to be in formation — uncertain path for diagnostics only.
            if (mission != null)
            {
                Team team = GetTeamForFormation(mission, formation);
                if (team != null)
                {
                    Agent general = team.GeneralAgent;
                    if (general != null && IsAgentViableCommanderBody(general) && general.Formation != formation)
                    {
                        return CommanderPresenceResult.Uncertain(
                            "Team.GeneralAgent exists but is not in this formation (commander anchor UNCERTAIN for this slice).",
                            commander: null);
                    }
                }
            }

            return CommanderPresenceResult.Missing("No commander candidate met policy and score thresholds");
        }

        private static Team GetTeamForFormation(TaleWorlds.MountAndBlade.Mission mission, Formation formation)
        {
            try
            {
                foreach (Team team in mission.Teams)
                {
                    if (team == null)
                    {
                        continue;
                    }

                    foreach (Formation f in team.FormationsIncludingEmpty)
                    {
                        if (f == formation)
                        {
                            return team;
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        private static Agent PickPreferredHero(TaleWorlds.MountAndBlade.Mission mission, IReadOnlyList<Agent> heroes)
        {
            if (heroes == null || heroes.Count == 0)
            {
                return null;
            }

            if (mission?.MainAgent != null)
            {
                for (int i = 0; i < heroes.Count; i++)
                {
                    if (heroes[i] == mission.MainAgent)
                    {
                        return heroes[i];
                    }
                }
            }

            return heroes[0];
        }

        private static bool IsCaptainAgent(Formation formation, Agent agent)
        {
            if (formation == null || agent == null)
            {
                return false;
            }

            try
            {
                return formation.Captain == agent;
            }
            catch
            {
                return false;
            }
        }

        private FormationCommander BuildCommander(
            Agent agent,
            bool isCaptain,
            string source,
            Formation formation,
            ref bool skillsCertain)
        {
            bool hero = IsHeroViaAdapter(agent);
            bool alive = IsAgentViableCommanderBody(agent);
            bool mounted = false;
            try
            {
                mounted = agent.HasMount;
            }
            catch
            {
                mounted = false;
            }

            float leadership01;
            float tactics01;
            ReadNormalizedSkills(agent, out leadership01, out tactics01, ref skillsCertain);

            float authority = ComputeAuthorityScore(agent, hero, isCaptain, leadership01, tactics01, source);

            string debugName = SafeAgentName(agent);
            return new FormationCommander(
                agent,
                hero,
                isCaptain,
                alive,
                mounted,
                leadership01,
                tactics01,
                authority,
                source,
                debugName);
        }

        private bool IsHeroViaAdapter(Agent agent)
        {
            FormationDataResult r = _adapter.TryGetAgentHeroFlag(agent);
            return r.Success && r.CommanderLikely;
        }

        private void ReadNormalizedSkills(Agent agent, out float leadership01, out float tactics01, ref bool skillsCertain)
        {
            leadership01 = 0.5f;
            tactics01 = 0.5f;

            FormationDataResult skills = _adapter.TryGetAgentLeadershipTactics(agent);
            if (!skills.Success)
            {
                skillsCertain = false;
                return;
            }

            leadership01 = NormalizeSkill(skills.FloatValue);
            tactics01 = NormalizeSkill(skills.FloatValueB);
        }

        private static float NormalizeSkill(float raw)
        {
            if (raw <= 0f)
            {
                return 0f;
            }

            return MBMath.ClampFloat(raw / 330f, 0f, 1f);
        }

        private float ComputeAuthorityScore(
            Agent agent,
            bool isHero,
            bool isCaptain,
            float leadership01,
            float tactics01,
            string source)
        {
            float score = 0.2f;
            if (isHero)
            {
                score += 0.25f;
            }

            if (isCaptain)
            {
                score += 0.15f;
            }

            try
            {
                if (agent.CanLeadFormationsRemotely)
                {
                    score += 0.1f;
                }
            }
            catch
            {
                // ignore
            }

            score += leadership01 * 0.15f;
            score += tactics01 * 0.15f;

            if (string.Equals(source, "HighestTierFallback", StringComparison.Ordinal))
            {
                FormationDataResult tier = _adapter.TryGetAgentTierOrRank(agent);
                if (tier.Success)
                {
                    score += MBMath.ClampFloat(tier.FloatValue / 40f, 0f, 1f) * 0.1f;
                }
            }

            return MBMath.ClampFloat(score, 0f, 1f);
        }

        private static string SafeAgentName(Agent agent)
        {
            try
            {
                if (agent?.Name != null)
                {
                    return agent.Name.ToString();
                }
            }
            catch
            {
                // ignored
            }

            return "unknown";
        }

        private static bool IsAgentViableCommanderBody(Agent agent)
        {
            if (agent == null)
            {
                return false;
            }

            try
            {
                if (!agent.IsActive())
                {
                    return false;
                }

                return agent.Health > 0.01f;
            }
            catch
            {
                return false;
            }
        }
    }
}
