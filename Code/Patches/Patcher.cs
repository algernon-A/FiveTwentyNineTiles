// <copyright file="Patcher.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System;
    using HarmonyLib;

    /// <summary>
    /// A basic Harmony patching class.
    /// </summary>
    internal class Patcher
    {
        private readonly string _harmonyID;

        /// <summary>
        /// Initializes a new instance of the <see cref="Patcher"/> class.
        /// Doing so applies all annotated patches.
        /// </summary>
        /// <param name="harmonyID">Harmony ID to use.</param>
        internal Patcher(string harmonyID)
        {
            // Dispose of any existing instance.
            if (Instance != null)
            {
                Logging.LogError("existing Patcher instance detected with ID ", Instance._harmonyID, "; reverting");
                Instance.UnPatchAll();
            }

            // Set instance reference.
            Instance = this;
            _harmonyID = harmonyID;

            // Apply annotated patches.
            PatchAnnotations();
        }

        /// <summary>
        /// Gets the active instance.
        /// </summary>
        internal static Patcher Instance { get; private set; }

        /// <summary>
        /// Gets a value indicating whether patches were successfully applied.
        /// </summary>
        internal static bool PatchesApplied { get; private set; } = false;

        /// <summary>
        /// Reverts all applied patches.
        /// </summary>
        internal void UnPatchAll()
        {
            if (!string.IsNullOrEmpty(_harmonyID))
            {
                Logging.LogInfo("reverting all applied patches for ", _harmonyID);
                Harmony harmonyInstance = new (_harmonyID);

                try
                {
                    harmonyInstance.UnpatchAll("_harmonyID");

                    // Clear applied flag.
                    PatchesApplied = false;
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "exception reverting patches for ", _harmonyID);
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// Applies Harmony patches.
        /// </summary>
        private void PatchAnnotations()
        {
            Logging.LogInfo("applying annotated Harmony patches for ", _harmonyID);
            Harmony harmonyInstance = new (_harmonyID);

            try
            {
                harmonyInstance.PatchAll();
                Logging.LogInfo("patching complete");

                // Set applied flag.
                PatchesApplied = true;
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception applying annotated Harmony patches; reverting");
                harmonyInstance.UnpatchAll(_harmonyID);
            }
        }
    }
}
