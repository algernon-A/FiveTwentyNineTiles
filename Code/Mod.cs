// <copyright file="Mod.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace FiveTwentyNineTiles
{
    using Game;
    using Game.Modding;
    using Game.Serialization;
    using HarmonyLib;

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
        /// Called by the game when the mod is loaded.
        /// </summary>
        public void OnLoad()
        {
            Logging.LogInfo("loading");

            // Apply harmony patches.
            new Patcher("algernon-529Tiles");
        }

        /// <summary>
        /// Called by the game when the game world is created.
        /// </summary>
        /// <param name="updateSystem">Game update system.</param>
        public void OnCreateWorld(UpdateSystem updateSystem)
        {
            Logging.LogInfo("starting OnCreateWorld");

            // Don't do anything if Harmony patches weren't applied.
            if (!Patcher.PatchesApplied)
            {
                Logging.LogCritical("Harmony patches not applied; aborting system activation");
                return;
            }

            // Load translations.
            Localization.LoadTranslations();

            // Activate systems.
            updateSystem.UpdateAfter<FiveTwentyNineSystem>(SystemUpdatePhase.Deserialize);
            updateSystem.UpdateAfter<PostDeserialize<FiveTwentyNineSystem>>(SystemUpdatePhase.Deserialize);
            updateSystem.UpdateAt<SettingsSystem>(SystemUpdatePhase.UIUpdate);
        }

        /// <summary>
        /// Called by the game when the mod is disposed of.
        /// </summary>
        public void OnDispose()
        {
            Logging.LogInfo("disposing");

            // Revert harmony patches.
            Patcher.Instance?.UnPatchAll();
        }
    }
}
