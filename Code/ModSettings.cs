// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System.Xml.Serialization;
    using Colossal.IO.AssetDatabase;
    using Game.Modding;
    using Game.Settings;
    using Game.UI;

    /// <summary>
    /// The mod's settings.
    /// </summary>
    [FileLocation(Mod.ModName)]
    public class ModSettings : ModSetting
    {
        private bool _unlockNone = true;
        private bool _unlockAll = false;
        private bool _extraAtStart = false;
        private bool _extraAtEnd = false;
        private bool _milestones = false;
        private float _upkeepModifier = 1f;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModSettings"/> class.
        /// </summary>
        /// <param name="mod"><see cref="IMod"/> instance.</param>
        public ModSettings(IMod mod)
            : base(mod)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether no additional unlocks should be provided.
        /// </summary>
        [SettingsUISection("UnlockMode")]
        public bool UnlockNone
        {
            get => _unlockNone;

            set
            {
                _unlockNone = value;

                // Clear conflicting settings.
                if (value)
                {
                    _unlockAll = false;
                    _extraAtStart = false;
                    _extraAtEnd = false;
                    _milestones = false;
                }
                else
                {
                    // Ensure correct state (default option selected if all others are disabled).
                    EnsureState();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entire map should be unlocked on load.
        /// </summary>
        [SettingsUISection("UnlockMode")]
        public bool UnlockAll
        {
            get => _unlockAll;

            set
            {
                _unlockAll = value;

                // Clear conflicting settings.
                if (value)
                {
                    _unlockNone = false;
                    _extraAtStart = false;
                    _extraAtEnd = false;
                    _milestones = false;
                }
                else
                {
                    // Ensure correct state (default option selected if all others are disabled).
                    EnsureState();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entire map should be unlocked on load.
        /// </summary>
        [SettingsUISection("UnlockMode")]
        public bool ExtraTilesAtStart
        {
            get => _extraAtStart;

            set
            {
                _extraAtStart = value;

                // Clear conflicting settings.
                if (value)
                {
                    _unlockNone = false;
                    _unlockAll = false;
                    _extraAtEnd = false;
                    _milestones = false;
                }
                else
                {
                    // Ensure correct state (default option selected if all others are disabled).
                    EnsureState();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the extra tiles should be allocated to the final milestone.
        /// </summary>
        [SettingsUISection("UnlockMode")]
        public bool ExtraTilesAtEnd
        {
            get => _extraAtEnd;

            set
            {
                _extraAtEnd = value;

                // Clear conflicting settings.
                if (value)
                {
                    _unlockNone = false;
                    _unlockAll = false;
                    _milestones = false;
                    _extraAtStart = false;
                }
                else
                {
                    // Ensure correct state (default option selected if all others are disabled).
                    EnsureState();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entire map should be unlocked on load.
        /// </summary>
        [SettingsUISection("UnlockMode")]
        public bool AssignToMilestones
        {
            get => _milestones;

            set
            {
                _milestones = value;

                // Clear conflicting settings.
                if (value)
                {
                    _unlockNone = false;
                    _unlockAll = false;
                    _extraAtStart = false;
                    _extraAtEnd = false;
                }
                else
                {
                    // Ensure correct state (default option selected if all others are disabled).
                    EnsureState();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether there should be no unlocked starting tiles when starting a new map.
        /// </summary>
        [SettingsUIHideByCondition(typeof(ModSettings), nameof(StartingTilesHidden))]
        [SettingsUISection("StartingOptions")]
        public bool NoStartingTiles { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether there should be no unlocked starting tiles when starting a new map.
        /// </summary>
        [SettingsUIHideByCondition(typeof(ModSettings), nameof(StartingTilesHidden))]
        [SettingsUISection("StartingOptions")]
        public bool RelockAllTiles { get; set; } = false;

        /// <summary>
        /// Gets or sets the tile upkeep multiplier.
        /// </summary>
        [SettingsUISlider(min = 0f, max = 200f, step = 5f, scalarMultiplier = 100f, unit = Unit.kPercentage)]
        [SettingsUISection("Upkeep")]
        public float UpkeepMultiplier
        {
            get => _upkeepModifier;
            set
            {
                _upkeepModifier = value;
                MapTilePurchaseSystemPatches.UpkeepModifier = value;
            }
        }

        /// <summary>
        /// Sets a value indicating whether the mod's settings should be reset.
        /// </summary>
        [XmlIgnore]
        [SettingsUIButton]
        [SettingsUISection("ResetModSettings")]
        [SettingsUIConfirmation]
        public bool ResetModSettings
        {
            set
            {
                // Apply defaults.
                SetDefaults();

                // Save.
                ApplyAndSave();
            }
        }

        /// <summary>
        /// Restores mod settings to default.
        /// </summary>
        public override void SetDefaults()
        {
            _unlockNone = true;
            _unlockAll = false;
            _extraAtStart = false;
            _extraAtEnd = false;
            _milestones = false;

            RelockAllTiles = false;
            NoStartingTiles = false;
            UpkeepMultiplier = 1f;
        }

        /// <summary>
        /// Returns a value indicating whether the no starting tiles option should be hidden.
        /// </summary>
        /// <returns><c>true</c> (hide starting tiles option) if 'Unlock all tiles' is selected, <c>false</c> (don't hide) otherwise.</returns>
        public bool StartingTilesHidden() => UnlockAll;

        /// <summary>
        /// Enables Unlock None as the default option if no other unlock options are selected.
        /// </summary>
        private void EnsureState()
        {
            if (!_unlockAll && !_extraAtStart && !_extraAtEnd && !_milestones)
            {
                UnlockNone = true;
            }
        }
    }
}