// <copyright file="Localization.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Colossal.IO.AssetDatabase;
    using Colossal.Localization;
    using Colossal.Logging;
    using Game.Modding;
    using Game.SceneFlow;

    /// <summary>
    /// Translation handling.
    /// </summary>
    public static class Localization
    {
        // Mod assembly path cache.
        private static string s_assemblyPath = null;

        /// <summary>
        /// Gets the mod directory file path of the currently executing mod assembly.
        /// </summary>
        public static string AssemblyPath
        {
            get
            {
                // Update cached path if the existing one is invalid.
                if (string.IsNullOrWhiteSpace(s_assemblyPath))
                {
                    // No path cached - find current executable asset.
                    string assemblyName = Assembly.GetExecutingAssembly().FullName;
                    ExecutableAsset modAsset = AssetDatabase.global.GetAsset(SearchFilter<ExecutableAsset>.ByCondition(x => x.definition?.FullName == assemblyName));
                    if (modAsset is null)
                    {
                        Mod.Log.Error("mod executable asset not found");
                        return null;
                    }

                    // Update cached path.
                    s_assemblyPath = Path.GetDirectoryName(modAsset.GetMeta().path);
                }

                // Return cached path.
                return s_assemblyPath;
            }
        }

        /// <summary>
        /// Loads settings translations from tab-separated l10n file.
        /// </summary>
        /// <param name="settings">Settings file to use.</param>
        /// <param name="log">Log to use.</param>
        public static void LoadTranslations(ModSetting settings, ILog log)
        {
            try
            {
                log.Debug("attempting to load settings translations");

                string translationFile = Path.Combine(AssemblyPath, "l10n.csv");
                if (File.Exists(translationFile))
                {
                    // Parse file.
                    log.Debug("parsing translation file " + translationFile);
                    IEnumerable<string[]> fileLines = File.ReadAllLines(translationFile).Select(x => x.Split('\t'));

                    // Iterate through each game locale.
                    foreach (string localeID in GameManager.instance.localizationManager.GetSupportedLocales())
                    {
                        try
                        {
                            // Find matching column in file.
                            int valueColumn = Array.IndexOf(fileLines.First(), localeID);

                            // Make sure a valid column has been found (column 0 is the context and column 1 is the translation key).
                            if (valueColumn > 1)
                            {
                                log.Debug("found translation for " + localeID);

                                // Add translations to game locales.
                                MemorySource language = new (fileLines.Skip(1).ToDictionary(x => GenerateOptionsKey(x[0], x[1], settings), x => x.ElementAtOrDefault(valueColumn)));
                                GameManager.instance.localizationManager.AddSource(localeID, language);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error(e, "exception reading localization for locale " + localeID);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e, "exception reading settings localization file");
            }
        }

        /// <summary>
        /// Generates a settings option localization key.
        /// </summary>
        /// <param name="context">Key context.</param>
        /// <param name="key">Key.</param>
        /// <param name="settings">Settings instance.</param>
        /// <returns>Full option localization key.</returns>
        private static string GenerateOptionsKey(string context, string key, ModSetting settings)
        {
            return context switch
            {
                "Options.OPTION" => settings.GetOptionLabelLocaleID(key),
                "Options.OPTION_DESCRIPTION" => settings.GetOptionDescLocaleID(key),
                "Options.WARNING" => "Options.WARNING[" + key + "]",
                _ => settings.GetSettingsLocaleID(),
            };
        }
    }
}
