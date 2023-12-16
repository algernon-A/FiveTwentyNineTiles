// <copyright file="Mod.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FiveTwentyNineTiles
{
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game;
    using Game.Modding;
    using Game.Serialization;

    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public sealed class Mod : IMod
    {
        /// <summary>
        /// The mod's default name.
        /// </summary>
        public const string ModName = "529 Tiles";

        /// <summary>
        /// Gets the active instance reference.
        /// </summary>
        public static Mod Instance { get; private set; }

        /// <summary>
        /// Gets the mod's active log.
        /// </summary>
        internal ILog Log { get; private set; }

        /// <summary>
        /// Gets the mod's active settings configuration.
        /// </summary>
        internal ModSettings ActiveSettings { get; private set; }

        /// <summary>
        /// Called by the game when the mod is loaded.
        /// </summary>
        public void OnLoad()
        {
            // Set instance reference.
            Instance = this;

            // Initialize logger.
            Log = LogManager.GetLogger(ModName);
            Log.Info("setting logging level to Debug");
            Log.effectivenessLevel = Level.Debug;
            Log.Info("loading");

            // Apply harmony patches.
            new Patcher("algernon-529Tiles", Log);

            // Register mod settings to game options UI.
            ActiveSettings = new (this);
            ActiveSettings.RegisterInOptionsUI();

            // Load translations.
            Localization.LoadTranslations(ActiveSettings, Log);

            // Load saved settings.
            AssetDatabase.global.LoadSettings("529TileSettings", ActiveSettings, new ModSettings(this));
        }

        /// <summary>
        /// Called by the game when the game world is created.
        /// </summary>
        /// <param name="updateSystem">Game update system.</param>
        public void OnCreateWorld(UpdateSystem updateSystem)
        {
            Log.Info("starting OnCreateWorld");

            // Don't do anything if Harmony patches weren't applied.
            if (Patcher.Instance is null || !Patcher.Instance.PatchesApplied)
            {
                Log.Critical("Harmony patches not applied; aborting system activation");
                return;
            }

            // Activate systems.
            updateSystem.UpdateAfter<FiveTwentyNineSystem>(SystemUpdatePhase.Deserialize);
            updateSystem.UpdateAfter<PostDeserialize<FiveTwentyNineSystem>>(SystemUpdatePhase.Deserialize);
        }

        /// <summary>
        /// Called by the game when the mod is disposed of.
        /// </summary>
        public void OnDispose()
        {
            Log.Info("disposing");

            // Revert harmony patches.
            Patcher.Instance?.UnPatchAll();
        }
    }
}
