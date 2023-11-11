// <copyright file="Mod.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace FiveTwentyNineTiles
{
    using Colossal.IO.AssetDatabase;
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
        /// Gets the mod's active settings configuration.
        /// </summary>
        internal static ModSettings ActiveSettings { get; private set; }

        /// <summary>
        /// Called by the game when the mod is loaded.
        /// </summary>
        public void OnLoad()
        {
            Log.Info("loading");

            // Apply harmony patches.
            new Patcher("algernon-529Tiles");

            // Register mod settings to game options UI.
            ActiveSettings = new (this);
            ActiveSettings.RegisterInOptionsUI();

            // Load translations.
            Localization.LoadTranslations(ActiveSettings);

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
            if (!Patcher.PatchesApplied)
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
