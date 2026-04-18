using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Read-only formation probes. Version-sensitive calls stay here (Slice 0 / Slice 8).
    /// </summary>
    public sealed class FormationDataAdapter
    {
        public FormationDataResult TryGetFormationCenter(Formation formation)
        {
            if (formation == null)
            {
                return FormationDataResult.Failure("formation is null");
            }

            try
            {
                if (formation.CountOfUnits <= 0)
                {
                    return FormationDataResult.Failure("formation has no units");
                }

                Vec2 planar = formation.SmoothedAverageUnitPosition;
                if (planar.LengthSquared < 1e-8f)
                {
                    planar = formation.CurrentPosition;
                }

                if (planar.LengthSquared < 1e-8f && formation.OrderPositionIsValid)
                {
                    planar = formation.OrderPosition;
                }

                Vec3 ground = formation.OrderGroundPosition;
                Vec3 center = new Vec3(planar.x, planar.y, ground.z);

                return new FormationDataResult(true, string.Empty, center);
            }
            catch
            {
                return FormationDataResult.Failure("formation center read threw");
            }
        }

        public FormationDataResult TryGetFormationFacing(Formation formation)
        {
            if (formation == null)
            {
                return FormationDataResult.Failure("formation is null");
            }

            try
            {
                if (formation.CountOfUnits <= 0)
                {
                    return FormationDataResult.Failure("formation has no units");
                }

                Vec2 dir = formation.Direction;
                if (dir.LengthSquared < 1e-8f)
                {
                    return FormationDataResult.Failure("Formation.Direction is near zero");
                }

                float len = (float)Math.Sqrt(dir.LengthSquared);
                if (len < 1e-6f)
                {
                    return FormationDataResult.Failure("Formation.Direction length underflow");
                }

                Vec2 unit = new Vec2(dir.x / len, dir.y / len);
                return new FormationDataResult(true, string.Empty, new Vec3(unit.x, unit.y, 0f));
            }
            catch (Exception ex)
            {
                return FormationDataResult.Failure("TryGetFormationFacing threw: " + ex.Message);
            }
        }

        /// <summary>
        /// Best-effort formation role for anchor back-offset selection. All version-sensitive reads stay here.
        /// </summary>
        public FormationDataResult TryDetectFormationRole(Formation formation)
        {
            if (formation == null)
            {
                return FormationDataResult.Failure("formation is null");
            }

            try
            {
                if (formation.CountOfUnits <= 0)
                {
                    return FormationDataResult.Failure("formation has no units");
                }

                try
                {
                    ArrangementOrder arrangement = formation.ArrangementOrder;
                    string arrangementName = arrangement.ToString();
                    if (arrangementName.IndexOf("Shield", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return new FormationDataResult(true, arrangementName, roleKind: FormationRoleKind.ShieldWall);
                    }
                }
                catch
                {
                    // ArrangementOrder not available — continue.
                }

                FormationQuerySystem qs = formation.QuerySystem;
                if (qs != null)
                {
                    try
                    {
                        if (qs.IsRangedFormation)
                        {
                            return new FormationDataResult(true, "QuerySystem.IsRangedFormation", roleKind: FormationRoleKind.Archer);
                        }
                    }
                    catch
                    {
                        // Member missing on some builds.
                    }

                    try
                    {
                        if (qs.IsCavalryFormation)
                        {
                            return new FormationDataResult(true, "QuerySystem.IsCavalryFormation", roleKind: FormationRoleKind.Cavalry);
                        }
                    }
                    catch
                    {
                        // Member missing on some builds.
                    }

                    try
                    {
                        if (qs.IsInfantryFormation)
                        {
                            return new FormationDataResult(true, "QuerySystem.IsInfantryFormation", roleKind: FormationRoleKind.Infantry);
                        }
                    }
                    catch
                    {
                        // Member missing on some builds.
                    }
                }

                if (formation.HasAnyMountedUnit)
                {
                    return new FormationDataResult(
                        true,
                        "fallback: HasAnyMountedUnit (coarse — may include mounted infantry)",
                        roleKind: FormationRoleKind.Cavalry);
                }

                return new FormationDataResult(true, "fallback: foot default", roleKind: FormationRoleKind.Infantry);
            }
            catch (Exception ex)
            {
                return FormationDataResult.Failure("TryDetectFormationRole threw: " + ex.Message);
            }
        }

        public FormationDataResult TryGetAgentPosition(Agent agent)
        {
            if (agent == null)
            {
                return FormationDataResult.Failure("agent is null");
            }

            try
            {
                return new FormationDataResult(true, string.Empty, agent.Position);
            }
            catch (Exception ex)
            {
                return FormationDataResult.Failure("TryGetAgentPosition threw: " + ex.Message);
            }
        }

        /// <summary>Enumerates living simulation agents in the formation (best-effort).</summary>
        public FormationDataResult TryGetFormationAgents(Formation formation)
        {
            return TryGetAgents(formation);
        }

        /// <summary>Legacy name; delegates to <see cref="TryGetFormationAgents"/>.</summary>
        public FormationDataResult TryGetAgents(Formation formation)
        {
            if (formation == null)
            {
                return FormationDataResult.Failure("formation is null");
            }

            try
            {
                var list = new List<Agent>();
                formation.ApplyActionOnEachUnit(a =>
                {
                    if (a != null)
                    {
                        list.Add(a);
                    }
                });

                return new FormationDataResult(true, string.Empty, default, 0f, list);
            }
            catch
            {
                return FormationDataResult.Failure("Enumerate agents threw");
            }
        }

        /// <summary>Engine captain slot when exposed as <see cref="Formation.Captain"/> (confirmed on pinned ref assemblies).</summary>
        public FormationDataResult TryGetFormationCaptain(Formation formation)
        {
            if (formation == null)
            {
                return FormationDataResult.Failure("formation is null");
            }

            try
            {
                Agent captain = formation.Captain;
                if (captain == null)
                {
                    return FormationDataResult.Failure("Formation.Captain is null");
                }

                var list = new List<Agent> { captain };
                return new FormationDataResult(true, "Formation.Captain", default, 0f, list, commanderLikely: true);
            }
            catch (Exception ex)
            {
                return FormationDataResult.Failure("Formation.Captain read threw: " + ex.Message);
            }
        }

        /// <summary>Maps to the engine formation captain slot (same as <see cref="TryGetFormationCaptain"/>; distinct name for policy layers).</summary>
        public FormationDataResult TryGetFormationCommander(Formation formation)
        {
            FormationDataResult inner = TryGetFormationCaptain(formation);
            if (!inner.Success)
            {
                return inner;
            }

            return new FormationDataResult(true, "Formation.Captain (commander slot)", default, 0f, inner.Agents, commanderLikely: true);
        }

        public FormationDataResult TryGetHeroAgents(Formation formation)
        {
            FormationDataResult all = TryGetFormationAgents(formation);
            if (!all.Success)
            {
                return all;
            }

            try
            {
                var heroes = new List<Agent>();
                for (int i = 0; i < all.Agents.Count; i++)
                {
                    Agent a = all.Agents[i];
                    FormationDataResult heroProbe = TryGetAgentHeroFlag(a);
                    if (heroProbe.Success && heroProbe.CommanderLikely)
                    {
                        heroes.Add(a);
                    }
                }

                if (heroes.Count == 0)
                {
                    return FormationDataResult.Failure("no heroes in formation");
                }

                return new FormationDataResult(true, string.Empty, default, 0f, heroes);
            }
            catch (Exception ex)
            {
                return FormationDataResult.Failure("TryGetHeroAgents threw: " + ex.Message);
            }
        }

        public FormationDataResult TryGetAgentHeroFlag(Agent agent)
        {
            if (agent == null)
            {
                return FormationDataResult.Failure("agent is null");
            }

            try
            {
                bool hero = agent.IsHero;
                return new FormationDataResult(true, string.Empty, default, 0f, null, commanderLikely: hero);
            }
            catch (Exception ex)
            {
                return FormationDataResult.Failure("IsHero read threw: " + ex.Message);
            }
        }

        /// <summary>Uses <see cref="BasicCharacterObject.Level"/> as a coarse rank proxy (UNCERTAIN vs true troop tier).</summary>
        public FormationDataResult TryGetAgentTierOrRank(Agent agent)
        {
            if (agent == null)
            {
                return FormationDataResult.Failure("agent is null");
            }

            try
            {
                if (agent.Character is BasicCharacterObject basic)
                {
                    return new FormationDataResult(true, "BasicCharacterObject.Level", default, basic.Level);
                }

                return FormationDataResult.Failure("character is not BasicCharacterObject");
            }
            catch (Exception ex)
            {
                return FormationDataResult.Failure("TryGetAgentTierOrRank threw: " + ex.Message);
            }
        }

        /// <summary>
        /// Raw leadership / tactics skill integers in <see cref="FormationDataResult.FloatValue"/> / <see cref="FormationDataResult.FloatValueB"/>.
        /// UNCERTAIN on some builds if <see cref="DefaultSkills"/> mapping changes — callers should treat failure as "skills unknown".
        /// </summary>
        public FormationDataResult TryGetAgentLeadershipTactics(Agent agent)
        {
            if (agent == null)
            {
                return FormationDataResult.Failure("agent is null");
            }

            try
            {
                if (!(agent.Character is BasicCharacterObject basic))
                {
                    return FormationDataResult.Failure("character is not BasicCharacterObject");
                }

                int lead = basic.GetSkillValue(DefaultSkills.Leadership);
                int tac = basic.GetSkillValue(DefaultSkills.Tactics);
                return new FormationDataResult(true, "DefaultSkills.Leadership/Tactics", default, lead, null, false, tac);
            }
            catch (Exception ex)
            {
                return FormationDataResult.Failure("TryGetAgentLeadershipTactics threw: " + ex.Message);
            }
        }

        public FormationDataResult TryDetectCommander(Formation formation)
        {
            return TryGetFormationCaptain(formation);
        }

        public FormationDataResult TryDetectMountedRatio(Formation formation)
        {
            if (formation == null)
            {
                return FormationDataResult.Failure("formation is null");
            }

            try
            {
                if (formation.CountOfUnits <= 0)
                {
                    return FormationDataResult.Failure("formation has no units");
                }

                if (!formation.HasAnyMountedUnit)
                {
                    return new FormationDataResult(true, string.Empty, default, 0f, null, false);
                }

                return FormationDataResult.Failure(
                    "Mounted ratio not wired — HasAnyMountedUnit is coarse; per-agent mount scan deferred.");
            }
            catch
            {
                return FormationDataResult.Failure("mounted probe threw");
            }
        }
    }
}
