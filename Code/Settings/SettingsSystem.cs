// <copyright file="SettingsSystem.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System.Collections.Generic;
    using System.Reflection;
    using Colossal.IO.AssetDatabase;
    using Game.UI;
    using Game.UI.Menu;
    using HarmonyLib;

    /// <summary>
    /// The mod's settings UI system.
    /// </summary>
    public partial class SettingsSystem : UISystemBase
    {
        /// <summary>
        /// Gets the current settings.
        /// </summary>
        internal static ModSettings ActiveSettings { get; private set; } = new ();

        /// <summary>
        /// Called when the system is created.
        /// </summary>
        protected override void OnCreate()
        {
            Logging.LogDebug("ModSettings OnCreate");
            base.OnCreate();

            // Load saved settings.
            AssetDatabase.global.LoadSettings("529TileSettings", ActiveSettings, new ModSettings());

            // Attach mod settings to game options UI.
            OptionsUISystem optionsUISystem = World.GetOrCreateSystemManaged<OptionsUISystem>();
            PropertyInfo optionsField = AccessTools.Property(typeof(OptionsUISystem), "options");
            if (optionsUISystem != null && optionsField?.GetValue(optionsUISystem) is List<OptionsUISystem.Page> optionsList)
            {
                optionsList.Add(new OptionsUISystem.Page
                {
                    id = "algernon.529Tiles",
                    sections =
                    {
                        new OptionsUISystem.Section
                        {
                            id = "algernon.529Tiles.Main",
                            items = AutomaticSettings.BuildSettingsSection(ActiveSettings),
                            defaultSection = true,
                        },
                    },
                });
                optionsField.SetValue(optionsUISystem, optionsList);
            }
            else
            {
                Logging.LogError("unable to access game options UI system");
            }
        }
    }
}
