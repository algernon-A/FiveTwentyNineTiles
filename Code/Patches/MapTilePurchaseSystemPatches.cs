// <copyright file="MapTilePurchaseSystemPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using Game.Simulation;
    using HarmonyLib;
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    /// Harmony patches for <see cref="MapTilePurchaseSystem"/> to implement per-tile cost limits.
    /// </summary>
    [HarmonyPatch(typeof(MapTilePurchaseSystem))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony patching syntax")]
    internal static class MapTilePurchaseSystemPatches
    {
        private static float _upkeepModifier = 1f;

        /// <summary>
        /// Gets or sets the tile upkeep modifier to apply.
        /// </summary>
        internal static float UpkeepModifier
        {
            get => _upkeepModifier;
            set => _upkeepModifier = value;
        }

        /// <summary>
        /// Harmony transpiler for <c>MapTilePurchaseSystem.UpdateStatus</c> to cap the cost for tiles beyond 441 and to update the displayed tile purchase upkeep cost.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateStatus")]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> UpdateStatusTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Mod.Instance.Log.Info("transpiling " + original.DeclaringType + '.' + original.Name);

            // Lower bounds check for free first nine tiles.
            FieldInfo m_Cost = AccessTools.Field(typeof(MapTilePurchaseSystem), "m_Cost");
            bool firstCost = false;

            // Tile upkeep cost field (used for UI display).
            FieldInfo m_Upkeep = AccessTools.Field(typeof(MapTilePurchaseSystem), "m_Upkeep");

            // Parse instructions.
            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Override number of owned tiles - stored as local var 8 (to cap tile cost scaling).
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 8)
                {
                    // Insert call to math.min(x, 441).
                    yield return new CodeInstruction(OpCodes.Ldc_I4, 441);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(math), nameof(math.min), new Type[] { typeof(int), typeof(int) }));
                }

                // Otherwise, looking for second store to MapTilePurchaseSystem.m_Cost (to make first nine tiles free).
                else if (instruction.StoresField(m_Cost))
                {
                    if (!firstCost)
                    {
                        firstCost = true;
                    }
                    else
                    {
                        // Insert call to our custom method.
                        Mod.Instance.Log.Debug("found second m_Cost store");
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 7);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 8);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MapTilePurchaseSystemPatches), nameof(CheckFreeTiles)));
                    }
                }

                // Otherwise, looking for the store to m_Upkeep - need to change this with our multiplier to ensure the UI is in sync with the updated upkeep.
                else if (instruction.StoresField(m_Upkeep))
                {
                    // Multiply calculated value by our multiplier.
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(MapTilePurchaseSystemPatches), nameof(_upkeepModifier)));
                    yield return new CodeInstruction(OpCodes.Mul);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony postfix for <see cref="MapTilePurchaseSystem.UnlockTile"/> to adjust for selection of custom starting tiles.
        /// </summary>
        /// <param name="entityManager">EntityManager instance.</param>
        /// <param name="area">Tile being unlocked.</param>
        [HarmonyPatch(nameof(MapTilePurchaseSystem.UnlockTile))]
        [HarmonyPostfix]
        internal static void UnlockTilePostfix(EntityManager entityManager, Entity area)
        {
            // Add via the active FiveTwentyNineSystem instance.
            entityManager.World.GetOrCreateSystemManaged<FiveTwentyNineSystem>().AddStartingTile(area);
        }

        /// <summary>
        /// Harmony postfix for <see cref="MapTilePurchaseSystem.CalculateOwnedTilesUpkeep"/> to apply custom tile upkeep multipliers.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        [HarmonyPatch(nameof(MapTilePurchaseSystem.CalculateOwnedTilesUpkeep))]
        [HarmonyPostfix]
        internal static void GetMapTileUpkeepCostMultiplierPostfix(ref int __result)
        {
            __result = (int)(__result * UpkeepModifier);
        }

        /// <summary>
        /// Checks to see if this tile is one of the first nine; if so, the cost is free.
        /// </summary>
        /// <param name="cost">Calculated tile cost.</param>
        /// <param name="numTiles">Number of selected tiles processed this update.</param>
        /// <param name="ownedTiles">Number of already owned tiles.</param>
        /// <returns>0 if this tile is one of the first nine, otherwise returns the calculated cost.</returns>
        private static float CheckFreeTiles(float cost, int numTiles, int ownedTiles)
        {
            // Check tile count.
            if (numTiles + ownedTiles <= 9)
            {
                // First nine tiles - return free tile.
                return 0f;
            }

            // Otherwise, not free - return the calculated cost.
            return cost;
        }
    }
}
