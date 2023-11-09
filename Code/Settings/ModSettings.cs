// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System.Xml.Serialization;
    using Colossal.IO.AssetDatabase;
    using Game.Settings;

    /// <summary>
    /// The mod's settings.
    /// </summary>
    [FileLocation("529 Tiles")]
    public class ModSettings : Setting
    {
        private bool _unlockAll = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModSettings"/> class.
        /// </summary>
        public ModSettings()
        {
            // Set initial defaults.
            SetDefaults();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entire map should be unlocked on load.
        /// </summary>
        public bool UnlockAll
        {
            get => _unlockAll;

            set
            {
                _unlockAll = value;

                // Assign contra value to ensure that JSON contains at least one non-default value.
                Contra = !value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether, well, nothing really.
        /// This is just the inverse of <see cref="UnlockAll"/>, to ensure the the JSON contains at least one non-default value.
        /// This is to workaround a bug where the settings file isn't overwritten when there are no non-default settings.
        /// </summary>
        [SettingsUIHidden]
        public bool Contra { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mod's settings should be reset.
        /// </summary>
        [XmlIgnore]
        [SettingsUIButton(confirm = true)]
        public bool ResetModSettings
        {
            // Dummy getter.
            get => false;

            set
            {
                // Apply defaults.
                SetDefaults();

                // Ensure contra is set correctly.
                Contra = !UnlockAll;

                // Save.
                ApplyAndSave();
            }
        }

        /// <summary>
        /// Restores mod settings to default.
        /// </summary>
        public override void SetDefaults()
        {
            _unlockAll = false;
        }
    }
}