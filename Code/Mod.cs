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
        /// Called by the game when the mod is loaded.
        /// </summary>
        public void OnLoad()
        {
            Logging.LogInfo("loading");

            // Apply harmony patches.
            Logging.LogInfo("applying Harmony patches");
            Harmony harmonyInstance = new ("algernon-529Tiles");
            harmonyInstance.PatchAll();
            Logging.LogInfo("patching complete");
        }

        /// <summary>
        /// Called by the game when the game world is created.
        /// </summary>
        /// <param name="updateSystem">Game update system.</param>
        public void OnCreateWorld(UpdateSystem updateSystem)
        {
            Logging.LogInfo("starting OnCreateWorld");
            Localization.LoadTranslations();

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
        }
    }
}
