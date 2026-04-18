using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// Non-destructive migration for on-disk commander JSON: version stamp, absent-field merge, structural warnings (Slice 23).
    /// </summary>
    public static class CommanderConfigMigration
    {
        /// <summary>Alias for <see cref="CommanderConfigSchema.CurrentConfigVersion"/>.</summary>
        public const int CurrentConfigVersion = CommanderConfigSchema.CurrentConfigVersion;

        /// <summary>Outcome of migration passes that may require persisting JSON.</summary>
        public sealed class MigrationOutcome
        {
            public MigrationOutcome(bool needsPersist, List<string> warnings)
            {
                NeedsPersist = needsPersist;
                Warnings = warnings ?? new List<string>();
            }

            public bool NeedsPersist { get; }

            public List<string> Warnings { get; }
        }

        /// <param name="rawJson">Original file text (may be empty).</param>
        /// <param name="target">Deserialized and slice-merged config to update in place.</param>
        /// <param name="defaults">Canonical defaults for absent keys.</param>
        public static MigrationOutcome Apply(string rawJson, CommanderConfig target, CommanderConfig defaults)
        {
            var warnings = new List<string>();
            bool needsPersist = false;

            if (target == null || defaults == null)
            {
                return new MigrationOutcome(false, warnings);
            }

            HashSet<string> rootKeys = null;
            if (!TryScanRootKeys(rawJson, out rootKeys, out List<string> duplicateWarnings))
            {
                warnings.Add("Could not scan JSON root object for migration (file may be empty or invalid).");
            }
            else
            {
                warnings.AddRange(duplicateWarnings);
                WarnUnknownRootKeys(rootKeys, warnings);
                int merged = MergeAbsentPropertiesFromDefaults(rootKeys, target, defaults);
                if (merged > 0)
                {
                    needsPersist = true;
                    warnings.Add($"Slice 23 migration: filled {merged} absent root properties from defaults.");
                }
            }

            EnsureConfigFileVersion(target, rootKeys, warnings, ref needsPersist);

            return new MigrationOutcome(needsPersist, warnings);
        }

        private static void EnsureConfigFileVersion(
            CommanderConfig target,
            HashSet<string> rootKeys,
            List<string> warnings,
            ref bool needsPersist)
        {
            bool hadKey = rootKeys != null && rootKeys.Contains(nameof(CommanderConfig.ConfigFileVersion));
            if (target.ConfigFileVersion < CurrentConfigVersion)
            {
                if (!hadKey || target.ConfigFileVersion <= 0)
                {
                    warnings.Add(
                        !hadKey
                            ? $"ConfigFileVersion was absent or unreadable; set to {CurrentConfigVersion}."
                            : $"ConfigFileVersion was invalid ({target.ConfigFileVersion}); set to {CurrentConfigVersion}.");
                }
                else
                {
                    warnings.Add($"ConfigFileVersion {target.ConfigFileVersion} is below current {CurrentConfigVersion}; upgraded.");
                }

                target.ConfigFileVersion = CurrentConfigVersion;
                needsPersist = true;
            }
        }

        private static int MergeAbsentPropertiesFromDefaults(HashSet<string> rootKeys, CommanderConfig target, CommanderConfig defaults)
        {
            if (rootKeys == null)
            {
                return 0;
            }

            int merged = 0;
            Type t = typeof(CommanderConfig);
            foreach (PropertyInfo prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite)
                {
                    continue;
                }

                if (!rootKeys.Contains(prop.Name))
                {
                    prop.SetValue(target, prop.GetValue(defaults));
                    merged++;
                }
            }

            return merged;
        }

        private static void WarnUnknownRootKeys(HashSet<string> rootKeys, List<string> warnings)
        {
            if (rootKeys == null)
            {
                return;
            }

            foreach (string key in rootKeys)
            {
                if (!CommanderConfigSchema.KnownRootPropertyNames.Contains(key))
                {
                    warnings.Add($"Unknown JSON property ignored (not mapped to CommanderConfig): '{key}'.");
                }
            }
        }

        private static bool TryScanRootKeys(string rawJson, out HashSet<string> rootKeys, out List<string> duplicateWarnings)
        {
            rootKeys = null;
            duplicateWarnings = new List<string>();
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                rootKeys = null;
                return false;
            }

            try
            {
                byte[] utf8 = Encoding.UTF8.GetBytes(rawJson);
                var reader = new Utf8JsonReader(utf8, new JsonReaderOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                int depth = 0;
                rootKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var seenAtRoot = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.StartObject:
                        case JsonTokenType.StartArray:
                            depth++;
                            break;
                        case JsonTokenType.EndObject:
                        case JsonTokenType.EndArray:
                            depth--;
                            break;
                        case JsonTokenType.PropertyName:
                            if (depth == 1)
                            {
                                string name = reader.GetString();
                                if (string.IsNullOrEmpty(name))
                                {
                                    continue;
                                }

                                rootKeys.Add(name);
                                if (!seenAtRoot.Add(name))
                                {
                                    duplicateWarnings.Add(
                                        $"Duplicate root JSON property '{name}'. Last value wins during deserialization; consider removing duplicates.");
                                }
                            }

                            break;
                    }
                }

                return true;
            }
            catch
            {
                rootKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                return false;
            }
        }
    }
}
